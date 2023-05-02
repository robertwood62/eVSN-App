using Fri.DownloadService.Tests;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Managers;
using Fri.FieldPlotService.Tools;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Security.Cryptography.X509Certificates;

namespace Fri.FieldPlotService.Tests
{
    [TestClass]
    public class VendorTests
    {
        [TestMethod]
        public async Task TestVendorsAsync()
        {
            var dbManager = TestHelper.Db;
            var vendorManager = new VendorManager(dbManager);

            // Create vendor 1
            var vendor1 = await vendorManager.CreateVendorAsync(new VendorInfo
            {
                Code = Guid.NewGuid().ToString(),
                Status = VendorStatusType.Active,
                Comment = "comment",
                ContactInfo = "Robert Wood",
                Name = "Vendor 1"
            });
            Assert.IsNotNull(vendor1);
            Assert.IsNotNull(vendor1.Info);
            Assert.IsTrue(vendor1.Info.Name == "Vendor 1");
            Assert.IsTrue(vendor1.Info.Comment == "comment");
            Assert.IsTrue(vendor1.Info.Status == VendorStatusType.Active);

            // Attempt to read the vendor information back against (by ID)
            var existing1 = await vendorManager.GetVendorAsync(vendor1.Id);
            Assert.IsNotNull(existing1);
            Assert.IsTrue(existing1.Id == vendor1.Id);

            // Attempt to read the vendor information back using the code.
            existing1 = await vendorManager.GetVendorAsync(vendor1.Info.Code);
            Assert.IsNotNull(existing1);
            Assert.IsTrue(existing1.Id == vendor1.Id);

            // Create a second vendor
            var vendor2 = await vendorManager.CreateVendorAsync(new VendorInfo
            {
                Code = Guid.NewGuid().ToString(),
                Status = VendorStatusType.Blocked,
                Comment = "comment",
                ContactInfo = "Robert Wood",
                Name = "Vendor 2"
            });
            Assert.IsNotNull(vendor2);
            Assert.IsNotNull(vendor2.Info);
            Assert.IsTrue(vendor2.Info.Name == "Vendor 2");
            Assert.IsTrue(vendor2.Info.Comment == "comment");
            Assert.IsTrue(vendor2.Info.Status == VendorStatusType.Blocked);

            // Read vendor 2 back again.
            var existing2 = await vendorManager.GetVendorAsync(vendor2.Id);
            Assert.IsTrue(existing2.Id == vendor2.Id);

            // Update the vendor comment (should work)
            vendor1.Info.Comment = "updated comment";
            var updated1 = await vendorManager.UpdateVendorInfoAsync(vendor1.Id, vendor1.Info);
            Assert.IsNotNull(updated1);
            Assert.IsTrue(updated1.Id == vendor1.Id);
            Assert.IsTrue(updated1.Info?.Comment == "updated comment");

            // Change the Vendor 1 code (should work)
            vendor1.Info.Code = Guid.NewGuid().ToString();
            updated1 = await vendorManager.UpdateVendorInfoAsync(vendor1.Id, vendor1.Info);

            // Change the Vendor 1 code to Vendor 2's code (should fail)
            var ex = await Assert.ThrowsExceptionAsync<FieldPlotServiceException>(async () =>
            {
                await vendorManager.UpdateVendorInfoAsync(vendor1.Id, vendor2.Info);
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.ErrorCode == FieldPlotServiceError.VendorAlreadyExists);

            // Search for anything called "vendor" and there should be at least two.
            var results = await vendorManager.SearchVendorsAsync("vendor", null);
            Assert.IsTrue(results.Count > 2);
            Assert.IsNotNull(results.Find(v => v.Id == vendor1.Id));
            Assert.IsNotNull(results.Find(v => v.Id == vendor2.Id));

            // Search for anything called "vendor" and status of blocked (vendor 1 should not return)
            results = await vendorManager.SearchVendorsAsync("vendor", VendorStatusType.Blocked);
            Assert.IsTrue(results.Count > 0);
            Assert.IsNull(results.Find(v => v.Id == vendor1.Id));
            Assert.IsNotNull(results.Find(v => v.Id == vendor2.Id));

            // search for vendor 1 (by code) which should return back a single result
            results = await vendorManager.SearchVendorsAsync(vendor1.Info.Code, null);
            Assert.IsTrue(results.Count == 1);
            Assert.IsNotNull(results.Find(v => v.Id == vendor1.Id));

            // Delete both the vendors
            await vendorManager.DeleteVendorAsync(vendor1.Id);
            await vendorManager.DeleteVendorAsync(vendor2.Id);
        }

        [TestMethod]
        public async Task TestVendorUsersAsync()
        {
            var dbManager = TestHelper.Db;
            var vendorManager = new VendorManager(dbManager);

            // Create vendor 1
            var vendor1 = await vendorManager.CreateVendorAsync(new VendorInfo
            {
                Code = Guid.NewGuid().ToString(),
                Status = VendorStatusType.Active,
                Comment = "comment",
                ContactInfo = "Robert Wood",
                Name = "Vendor 1"
            });

            var username1 = Guid.NewGuid().ToString();
            var username2 = Guid.NewGuid().ToString();
            var password = "UnitTest1234!@#$";

            // Create user 1
            var user1 = await vendorManager.CreateVendorUserAsync(vendor1.Id, new UserInfo
            {
                FirstName = "Robert",
                LastName = "Wood",
                Status = UserStatusType.Active,
                Username = username1
            }, password);
            Assert.IsNotNull(user1);
            Assert.IsNotNull(user1.Info);
            Assert.IsTrue(user1.Info.Username == username1);

            // Read the user information back using ID
            var existing1 = await vendorManager.GetVendorUserAsync(user1.Id);
            Assert.IsNotNull(existing1);
            Assert.IsTrue(existing1.Id == user1.Id);

            // Read the user information back using username
            existing1 = await vendorManager.GetVendorUserAsync(user1.Info.Username);
            Assert.IsNotNull(existing1);
            Assert.IsTrue(existing1.Id == user1.Id);

            // Update the user information
            user1.Info.FirstName = "Rob";
            var updated1 = await vendorManager.UpdateVendorUserInfoAsync(user1.Id, user1.Info);
            Assert.IsTrue(updated1.Info?.FirstName == "Rob");

            // Create a new user
            var user2 = await vendorManager.CreateVendorUserAsync(vendor1.Id, new UserInfo
            {
                FirstName = "Homer",
                LastName = "Simpson",
                Status = UserStatusType.Blocked,
                Username = username2
            }, password);
            Assert.IsNotNull(user2);
            Assert.IsTrue(user2.Info?.Username == username2);

            // Update the user1 to the same username as user2 (should fail)
            var ex = await Assert.ThrowsExceptionAsync<FieldPlotServiceException>(async () =>
            {
                await vendorManager.UpdateVendorUserInfoAsync(user1.Id, new UserInfo
                {
                    Username = username2
                });
            });
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.ErrorCode == FieldPlotServiceError.UsernameExists);

            // Search for both users in the system
            var users = await vendorManager.SearchVendorUsersAsync(null, vendor1.Id, null);
            Assert.IsTrue(users.Count == 2);

            // Search for the first user only
            users = await vendorManager.SearchVendorUsersAsync(username1, vendor1.Id, null);
            Assert.IsTrue(users.Count == 1);
            Assert.IsNotNull(users.Find(u => u.Id == user1.Id));

            // Delete a user
            await vendorManager.DeleteVendorUserAsync(user1.Id);
            Assert.IsNull(await vendorManager.GetVendorUserAsync(user1.Info.Username));

            // Delete the vendor (which should delete the users also)
            await vendorManager.DeleteVendorAsync(vendor1.Id);
            Assert.IsNull(await vendorManager.GetVendorAsync(vendor1.Info?.Code));
            Assert.IsNull(await vendorManager.GetVendorUserAsync(user2.Info.Username));

        }
    }
}