using Fri.DownloadService.Tests;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Managers;
using Fri.FieldPlotService.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Tests
{
    [TestClass]
    public class SyncManagerTests
    {
        [TestMethod]
        public async Task EnsureTestUserAsync()
        {
            var dbManager = TestHelper.Db;
            var vendorManager = new VendorManager(dbManager);

            var vendorCode = "unit-test";
            var usernames = new string[] { "user1", "user2", "user3" };
            var password = "password";

            // Check the unit test vendor exists.
            var vendor = await vendorManager.GetVendorAsync(vendorCode);

            // Create a new vendor and user account.
            vendor ??= await vendorManager.CreateVendorAsync(new VendorInfo
            {
                Code = vendorCode,
                Name = "Test Vendor",
                Status = VendorStatusType.Active
            });

            foreach (var username in usernames)
            {
                var user = await vendorManager.GetVendorUserAsync(username);
                if(user == null)
                {
                    await vendorManager.CreateVendorUserAsync(vendor.Id, new UserInfo
                    {
                        Username = username,
                        FirstName = "Robert",
                        LastName = "Wood",
                        Status = UserStatusType.Active
                    }, password);
                }
            }

        }

        [TestMethod]
        public async Task SignInTestAsync() 
        {
            var dbManager = TestHelper.Db;
            var options = TestHelper.Options;
            var vendorManager = new VendorManager(dbManager);
            var syncManager = new SyncManager(dbManager, null, options, vendorManager);

            // Create a new vendor and user account.
            var vendor1 = await vendorManager.CreateVendorAsync(new Entities.VendorInfo
            {
                Code = Guid.NewGuid().ToString(),
                Name = "Vendor 1",
                Status = Entities.VendorStatusType.Active
            });

            // Create a new user
            var password = "password";
            var user1 = await vendorManager.CreateVendorUserAsync(vendor1.Id, new Entities.UserInfo
            {
                Username = Guid.NewGuid().ToString(),
                FirstName = "Robert",
                LastName = "Wood",
                Status = Entities.UserStatusType.Active
            }, password);

            // Test the sign-in
            var token = await syncManager.SignInAsync(user1.Info?.Username, password);
            Assert.IsNotNull(token);
            Assert.IsTrue(token.UserId == user1.Id);
            Assert.IsTrue(token.Username == user1.Info?.Username);
            Assert.IsTrue(token.VendorId == vendor1.Id);

            // Verify the key can be decrypted.
            var userToken = UserToken.FromString(token.AccessToken, options.EncryptionKey);
            Assert.IsTrue(userToken.UserId == user1.Id);
            Assert.IsTrue(userToken.Username == user1.Info?.Username);
            Assert.IsTrue(userToken.VendorId == vendor1.Id);

        }

        [TestMethod]
        public async Task SimpleSyncTestAsync()
        {
            var dbManager = TestHelper.Db;
            var options = TestHelper.Options;
            var vendorManager = new VendorManager(dbManager);
            var syncManager = new SyncManager(dbManager, null, options, vendorManager);

            // Create a new vendor and user account.
            var vendor1 = await vendorManager.CreateVendorAsync(new Entities.VendorInfo
            {
                Code = Guid.NewGuid().ToString(),
                Name = "Vendor 1",
                Status = Entities.VendorStatusType.Active
            });

            // Create a new user
            var password = "password";
            var user1 = await vendorManager.CreateVendorUserAsync(vendor1.Id, new Entities.UserInfo
            {
                Username = Guid.NewGuid().ToString(),
                FirstName = "Robert",
                LastName = "Wood",
                Status = Entities.UserStatusType.Active
            }, password);

            // Test the sign-in
            var token = await syncManager.SignInAsync(user1.Info?.Username, password);
            var userToken = UserToken.FromString(token.AccessToken, options.EncryptionKey);

            // Create a new project to sync.
            var project1 = new Dictionary<string, object?>
            {
                { "PROJECTID", Guid.NewGuid().ToString() },
                { "NAME", "Project1" },
                { "DESCRIPTION", "Test Project 1" },
                { "PROJECT_DATE", new DateTime(2023, 1, 1) },
            };
            var project2 = new Dictionary<string, object?>
            {
                { "PROJECTID", Guid.NewGuid().ToString() },
                { "NAME", "Project2" },
                { "DESCRIPTION", "Test Project 2" },
                { "PROJECT_DATE", new DateTime(2023, 1, 1) },
            };

            // Attempt to insert the entities.
            DateTime? lastSyncTime = null;   // First sync should be null.
            await syncManager.SyncEntitiesAsync(userToken, new SyncRequest
            {
                Table = "project",
                DeviceId = Guid.NewGuid(),
                DeviceType = "test-device",
                PlotId = null,
                LastSyncTime = lastSyncTime
            }, new List<Dictionary<string, object?>> { project1, project2 }, true, true);
            lastSyncTime = DateTime.UtcNow;

            // Make a change and sync it again.
            project1["DESCRIPTION"] += " - Updated";
            await syncManager.SyncEntitiesAsync(userToken, new SyncRequest
            {
                Table = "project",
                DeviceId = Guid.NewGuid(),
                DeviceType = "test-device",
                PlotId = null,
                LastSyncTime = lastSyncTime
            }, new List<Dictionary<string, object?>> { project1, project2 }, false, true);

            // Read back the entities (all of them).
            var projects = await syncManager.GetEntitiesAsync(userToken, new SyncRequest
            {
                Table = "project",
                DeviceId = Guid.NewGuid(),
                DeviceType = "test-device",
                PlotId = null,
                LastSyncTime = null
            });
            Assert.IsNotNull(projects);
            Assert.IsTrue(projects.Count == 2);
            Assert.IsNotNull(projects.FirstOrDefault(p => p[Constants.VsnProjectId]?.ToString() == project1[Constants.VsnProjectId]?.ToString()));
            Assert.IsNotNull(projects.FirstOrDefault(p => p[Constants.VsnProjectId]?.ToString() == project2[Constants.VsnProjectId]?.ToString()));
        }
    }
}
