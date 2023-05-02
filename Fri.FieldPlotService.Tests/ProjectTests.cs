using Fri.DownloadService.Tests;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Managers;
using Fri.FieldPlotService.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Tests
{
    [TestClass]
    public class ProjectTests
    {
        [TestMethod]
        public async Task TestProjectsAsync()
        {
            var dbManager = TestHelper.Db;
            var vendorManager = new VendorManager(dbManager);
            var projectManager = new ProjectManager(dbManager, vendorManager);

            // Create a new vendor and user account.
            var vendor1 = await vendorManager.CreateVendorAsync(new VendorInfo
            {
                Code = Guid.NewGuid().ToString(),
                Name = "Test Vendor 1",
                Status = VendorStatusType.Active,
            });
            var vendor2 = await vendorManager.CreateVendorAsync(new VendorInfo
            {
                Code = Guid.NewGuid().ToString(),
                Name = "Test Vendor 2",
                Status = VendorStatusType.Active,
            });

            // Create a new project for the first vendor only.
            var vsnProjectId = Guid.NewGuid().ToString();
            await projectManager.CreateProjectsAsync(new Guid[] { vendor1.Id }, vsnProjectId, new ProjectInfo
            {
                Name = "Test project 1",
                Description = "Hello World",
                ProjectDate = new DateTime(2023, 1, 1)
            });

            // Check that the project was created for the first vendor.
            var project1 = await projectManager.GetProjectAsync(vendor1.Id, vsnProjectId);
            Assert.IsNotNull(project1);
            Assert.IsNotNull(project1.Data);
            Assert.IsTrue(project1.Data[Constants.VsnProjectId]?.ToString() == vsnProjectId);
            Assert.IsTrue(project1.Data[Constants.VsnProjectName]?.ToString() == "Test project 1");

            // Create the same project again, but this time for both vendors (should update one, add it to the second)
            await projectManager.CreateProjectsAsync(new Guid[] { vendor1.Id, vendor2.Id }, vsnProjectId, new ProjectInfo
            {
                Name = "Test project 1 - updated",
                Description = "Hello World",
                ProjectDate = new DateTime(2023, 1, 1)
            });

            // Check that the project was updated for the first vendor.
            project1 = await projectManager.GetProjectAsync(vendor1.Id, vsnProjectId);
            Assert.IsNotNull(project1);
            Assert.IsNotNull(project1.Data);
            Assert.IsTrue(project1.Data[Constants.VsnProjectId]?.ToString() == vsnProjectId);
            Assert.IsTrue(project1.Data[Constants.VsnProjectName]?.ToString() == "Test project 1 - updated");

            // Check that the project was created for the second vendor.
            var project2 = await projectManager.GetProjectAsync(vendor2.Id, vsnProjectId);
            Assert.IsNotNull(project2);
            Assert.IsNotNull(project2.Data);
            Assert.IsTrue(project2.Data[Constants.VsnProjectId]?.ToString() == vsnProjectId);
            Assert.IsTrue(project2.Data[Constants.VsnProjectName]?.ToString() == "Test project 1 - updated");

            // Update the project information
            project1 = await projectManager.UpdateProjectAsync(project1.Id, new ProjectInfo
            {
                Name = "Updated 1",
                Description = "Test",
                ProjectDate = new DateTime(2022, 1, 1)
            });
            Assert.IsNotNull(project1);
            Assert.IsNotNull(project1.Data);
            Assert.IsTrue(project1.Data[Constants.VsnProjectId]?.ToString() == vsnProjectId);
            Assert.IsTrue(project1.Data[Constants.VsnProjectName]?.ToString() == "Updated 1");
            Assert.IsTrue(project1.Data[Constants.VsnProjectDate]?.ToString() == new DateTime(2022, 1, 1).ToString());

            // Search for projects (both should be in the results)
            var results = await projectManager.SearchProjectsAsync(null, null, false);
            Assert.IsTrue(results.Count >= 2);
            Assert.IsNotNull(results.Find(p => p.Id == project1.Id));
            Assert.IsNotNull(results.Find(p => p.Id == project2.Id));

            // Search for projects (project 1 should be in the results)
            results = await projectManager.SearchProjectsAsync("Updated 1", null, false);
            Assert.IsTrue(results.Count >= 1);
            Assert.IsNotNull(results.Find(p => p.Id == project1.Id));
            Assert.IsNull(results.Find(p => p.Id == project2.Id));

            // Search for projects (project 1 should be in the results)
            results = await projectManager.SearchProjectsAsync("Updated 1", vendor1.Id, false);
            Assert.IsTrue(results.Count == 1);
            Assert.IsNotNull(results.Find(p => p.Id == project1.Id));
            Assert.IsNull(results.Find(p => p.Id == project2.Id));

            // Search for projects (project 1 should be in the results)
            results = await projectManager.SearchProjectsAsync(null, vendor2.Id, false);
            Assert.IsTrue(results.Count == 1);
            Assert.IsNotNull(results.Find(p => p.Id == project2.Id));

            // Delete a project (soft delete)
            await projectManager.DeleteProjectAsync(project1.Id, false);

            // Search for projects (project 1 is deleted and should not return)
            results = await projectManager.SearchProjectsAsync("Updated 1", vendor1.Id, false);
            Assert.IsTrue(results.Count == 0);
            Assert.IsNull(results.Find(p => p.Id == project1.Id));

            // Search for projects (project 1 is deleted and should return)
            results = await projectManager.SearchProjectsAsync("Updated 1", vendor1.Id, true);
            Assert.IsTrue(results.Count == 1);
            Assert.IsNotNull(results.Find(p => p.Id == project1.Id));
        }


        [TestMethod]
        public async Task TestVendorImportExportAsync()
        {
            var dbManager = TestHelper.Db;
            var vendorManager = new VendorManager(dbManager);
            var projectManager = new ProjectManager(dbManager, vendorManager);

            // Create a new vendor and user account.
            var vendor1 = await vendorManager.CreateVendorAsync(new VendorInfo
            {
                Code = Guid.NewGuid().ToString(),
                Name = "Test Vendor 1",
                Status = VendorStatusType.Active,
            });
            var vendor2 = await vendorManager.CreateVendorAsync(new VendorInfo
            {
                Code = Guid.NewGuid().ToString(),
                Name = "Test Vendor 2",
                Status = VendorStatusType.Active,
            });

            // Create a new project for the first vendor only.
            var vsnProjectId = Guid.NewGuid().ToString();
            await projectManager.CreateProjectsAsync(new Guid[] { vendor1.Id, vendor2.Id }, vsnProjectId, new ProjectInfo
            {
                Name = "Test project 1",
                Description = "Hello World",
                ProjectDate = new DateTime(2023, 1, 1)
            });

            // Get the project that were created
            var project1 = await projectManager.GetProjectAsync(vendor1.Id, vsnProjectId);
            Assert.IsNotNull(project1);
            var project2 = await projectManager.GetProjectAsync(vendor2.Id, vsnProjectId);
            Assert.IsNotNull(project2);

            // Create a set of unique plot names for each vendor to import
            var vsnPlots1 = new List<ImportPlotData>
            { 
                new ImportPlotData { PlotName = Guid.NewGuid().ToString(), MeasureYear = 1900, PlotTypeCode = "A" },
                new ImportPlotData { PlotName = Guid.NewGuid().ToString(), MeasureYear = 1900, PlotTypeCode = "A" },
                new ImportPlotData { PlotName = Guid.NewGuid().ToString(), MeasureYear = 1900, PlotTypeCode = "A" },
            };
            var vsnPlots2 = new List<ImportPlotData>
            {
                new ImportPlotData { PlotName = Guid.NewGuid().ToString(), MeasureYear = 1900, PlotTypeCode = "A" },
                new ImportPlotData { PlotName = Guid.NewGuid().ToString(), MeasureYear = 1900, PlotTypeCode = "A" },
            };
            await projectManager.ImportPlotsAsync(project1.Id, vendor1.Id, vsnPlots1);
            await projectManager.ImportPlotsAsync(project2.Id, vendor2.Id, vsnPlots2);

            // Export the plot information for each vendor to verify
            var plots = await projectManager.ExportVsnDataAsync("plot", vendor1.Info?.Code, null, true);
            Assert.IsNotNull(plots);
            Assert.IsTrue(plots.Count == 3);
            Assert.IsNotNull(plots.Find(p => p[Constants.VsnPlotName]?.ToString() == vsnPlots1[0].PlotName));
            Assert.IsNotNull(plots.Find(p => p[Constants.VsnPlotName]?.ToString() == vsnPlots1[1].PlotName));
            Assert.IsNotNull(plots.Find(p => p[Constants.VsnPlotName]?.ToString() == vsnPlots1[2].PlotName));

            plots = await projectManager.ExportVsnDataAsync("plot", vendor2.Info?.Code, null, true);
            Assert.IsNotNull(plots);
            Assert.IsTrue(plots.Count == 2);
            Assert.IsNotNull(plots.Find(p => p[Constants.VsnPlotName]?.ToString() == vsnPlots2[0].PlotName));
            Assert.IsNotNull(plots.Find(p => p[Constants.VsnPlotName]?.ToString() == vsnPlots2[1].PlotName));

            // Reimport the plots which should result in no changes (as they already exist)
            await projectManager.ImportPlotsAsync(project1.Id, vendor1.Id, vsnPlots1);
            await projectManager.ImportPlotsAsync(project2.Id, vendor2.Id, vsnPlots2);

            plots = await projectManager.ExportVsnDataAsync("plot", vendor1.Info?.Code, null, true);
            Assert.IsNotNull(plots);
            Assert.IsTrue(plots.Count == 3);

            plots = await projectManager.ExportVsnDataAsync("plot", vendor2.Info?.Code, null, true);
            Assert.IsNotNull(plots);
            Assert.IsTrue(plots.Count == 2);

            // Try to import an existing plot for another vendor (should fail)
            var ex = await Assert.ThrowsExceptionAsync<FieldPlotServiceException>(async () =>
            {
                // Add vendor 1's plots to vendor 2
                await projectManager.ImportPlotsAsync(project2.Id, vendor2.Id, vsnPlots1);
            });

            // Check the error above didn't impact the plot data.
            plots = await projectManager.ExportVsnDataAsync("plot", vendor1.Info?.Code, null, true);
            Assert.IsNotNull(plots);
            Assert.IsTrue(plots.Count == 3);

            plots = await projectManager.ExportVsnDataAsync("plot", vendor2.Info?.Code, null, true);
            Assert.IsNotNull(plots);
            Assert.IsTrue(plots.Count == 2);

        }

        [TestMethod]
        public async Task TestPlotSearchingAsync()
        {
            var dbManager = TestHelper.Db;
            var vendorManager = new VendorManager(dbManager);
            var projectManager = new ProjectManager(dbManager, vendorManager);

            var plots = await projectManager.SearchPlotsAsync(null, null, null, true);
            foreach (var plot in plots.Take(40))
            {
                var details = await projectManager.GetPlotDetailsAsync(plot.Id, true);
                Assert.IsNotNull(details);
            }
        }

        [TestMethod]
        public async Task TestReportsAsync()
        {
            var dbManager = TestHelper.Db;
            var vendorManager = new VendorManager(dbManager);
            var projectManager = new ProjectManager(dbManager, vendorManager);

            var csvFile = await projectManager.GenerateCsvFile("plot", null, null, true);
            Console.WriteLine(csvFile);
        }
    }

}