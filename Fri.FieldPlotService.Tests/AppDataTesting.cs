using Fri.DownloadService.Tests;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Tests
{
    [TestClass]
    public class AppDataTesting
    {
        [TestMethod]
        public async Task LoadTestDataAsync()
        {
            bool loadData = false;
            if(!loadData)
            {
                return;
            }

            // -------------------------------------------------------------------------
            // Ensure the vendors and users are created first
            // -------------------------------------------------------------------------

            // Load the data
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
                if (user == null)
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
    }
}
