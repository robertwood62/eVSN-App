using Fri.FieldPlotService.Api.Common;
using Fri.FieldPlotService.Api.Models;
using Fri.FieldPlotService.Common;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fri.FieldPlotService.Api.Controllers
{
    /// <summary>
    /// The Vendor controller manages vendors, users and projects.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : BaseController
    {
        readonly VendorManager vendorManager;
        readonly ProjectManager projectManager;

        /// <summary>
        /// Constructor with dependencies.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <param name="vendorManager"></param>
        /// <param name="projectManager"></param>
        public AdminController(ILogger<FieldController> logger, FieldPlotServiceOptions options, VendorManager vendorManager, ProjectManager projectManager)
            : base(logger, options)
        {
            this.vendorManager = vendorManager;
            this.projectManager = projectManager;
        }

        /// <summary>
        /// Creates a new vendor in the system.
        /// </summary>
        /// <param name="vendorInfo"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(VendorModel), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPost]
        [Route("vendor")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> CreateVendorAsync([FromBody] VendorInfo vendorInfo)
        {
            try
            {
                var vendor = await vendorManager.CreateVendorAsync(vendorInfo);
                return Ok(new VendorModel(vendor));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Updates an existing vendor's information.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="vendorInfo"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(VendorModel), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPut]
        [Route("vendor/{vendorId}")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> UpdateVendorInfoAsync([FromRoute]Guid vendorId, [FromBody] VendorInfo vendorInfo)
        {
            try
            {
                var vendor = await vendorManager.UpdateVendorInfoAsync(vendorId, vendorInfo);
                return Ok(new VendorModel(vendor));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Gets an existing vendor from the system.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(VendorModel), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("vendor/{vendorId}")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> GetVendorAsync([FromRoute] Guid vendorId)
        {
            try
            {
                var vendor = await vendorManager.GetVendorAsync(vendorId);
                return Ok(new VendorModel(vendor));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Deletes an existing vendor from the system.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpDelete]
        [Route("vendor/{vendorId}")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> DeleteVendorAsync([FromRoute] Guid vendorId)
        {
            try
            {
                await vendorManager.DeleteVendorAsync(vendorId);
                return Ok();
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Searches for vendors in the system.
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<VendorModel>), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("vendor/search")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> SearchVendorsAsync([FromQuery] string? keyword, [FromQuery]VendorStatusType? status)
        {
            try
            {
                var vendors = await vendorManager.SearchVendorsAsync(keyword, status);
                return Ok(vendors.Select(v=> new VendorModel(v)).ToList());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Search for vendor users in the system.
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="vendorId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<UserModel>), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("user/search")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> SearchVendorUsersAsync([FromQuery] string? keyword, [FromQuery] Guid? vendorId, [FromQuery] UserStatusType? status)
        {
            try
            {
                var users = await vendorManager.SearchVendorUsersAsync(keyword, vendorId, status);
                return Ok(users.Select(v => new UserModel(v)).ToList());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Gets a vendor user from the 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("user/{userId}")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> GetVendorUserAsync([FromRoute] Guid userId)
        {
            try
            {
                var vendorUser = await vendorManager.GetVendorUserAsync(userId);
                return Ok(new UserModel(vendorUser));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Creates a new user model in the system.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPost]
        [Route("user")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> CreateVendorUserAsync([FromBody] NewUserModel model)
        {
            try
            {
                var vendorUser = await vendorManager.CreateVendorUserAsync(model.VendorId, model.Info, model.Password);
                return Ok(new UserModel(vendorUser));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Updates the user's information.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPut]
        [Route("user/{userId}")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> UpdateVendorUserInfoAsync([FromRoute] Guid userId, [FromBody] UserInfo userInfo)
        {
            try
            {
                var vendorUser = await vendorManager.UpdateVendorUserInfoAsync(userId, userInfo);
                return Ok(new UserModel(vendorUser));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Resets the password for an existing vendor user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPut]
        [Route("user/{userId}/password")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> ResetVendorUserPasswordAsync([FromRoute] Guid userId, [FromQuery] string password)
        {
            try
            {
                await vendorManager.ResetVendorUserPasswordAsync(userId, password);
                return Ok();
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Deletes a vendor user from the system.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpDelete]
        [Route("user/{userId}")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> DeleteVendorUserAsync([FromRoute] Guid userId)
        {
            try
            {
                await vendorManager.DeleteVendorUserAsync(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Creates a new project for each of the vendors provided.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPost]
        [Route("project/ensure")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> CreateProjectsAsync([FromBody] CreateProjectsModel model)
        {
            try
            {
                await projectManager.CreateProjectsAsync(model.VendorIds, model.VsnProjectId, model.Info);
                return Ok();
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Creates a new project for a vendor.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="vsnProjectId"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ProjectModel), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPost]
        [Route("project")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> CreateProjectAsync([FromQuery] Guid vendorId, [FromQuery] string? vsnProjectId, [FromBody] ProjectInfo info)
        {
            try
            {
                var project = await projectManager.CreateProjectAsync(vendorId, vsnProjectId, info);
                return Ok(new ProjectModel(project));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Updates an existing project.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ProjectModel), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPut]
        [Route("project/{projectId}")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> UpdateProjectAsync([FromRoute] Guid projectId, [FromBody]ProjectInfo info)
        {
            try
            {
                var project = await projectManager.UpdateProjectAsync(projectId, info);
                return Ok(new ProjectModel(project));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Get a project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ProjectModel), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("project/{projectId}")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> GetProjectAsync([FromRoute] Guid projectId)
        {
            try
            {
                var project = await projectManager.GetProjectAsync(projectId);
                return Ok(new ProjectModel(project));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Delete a project.
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpDelete]
        [Route("project/{projectId}")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> DeleteProjectAsync([FromRoute] Guid projectId)
        {
            try
            {
                await projectManager.DeleteProjectAsync(projectId);
                return Ok();
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Search for projects.
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="vendorId"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ProjectModel[]), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("project/search")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> SearchProjectsAsync([FromQuery] string? keyword, [FromQuery] Guid? vendorId, [FromQuery] bool includeDeleted = false)
        {
            try
            {
                var results = await projectManager.SearchProjectsAsync(keyword, vendorId, includeDeleted);
                return Ok(results.Select(p => new ProjectModel(p)).ToArray());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Imports a series of plots for a vendor
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="projectId"></param>
        /// <param name="plotList"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPost]
        [Route("plots/import")]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<IActionResult> ImportVsnPlotsAsync([FromQuery] Guid vendorId, [FromQuery] Guid projectId, [FromBody] List<ImportPlotData> plotList)
        {
            try
            {
                await projectManager.ImportPlotsAsync(projectId, vendorId, plotList);
                return Ok();
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Exports VSN data for a given table using an optional vendor code and last changed filter.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="vendorCode"></param>
        /// <param name="lastChanged"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<Dictionary<string, object?>>), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("export")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> ExportVsnDataAsync([FromQuery] string table, [FromQuery] string? vendorCode, [FromQuery] DateTime? lastChanged)
        {
            try
            {
                var data = await projectManager.ExportVsnDataAsync(table, vendorCode, lastChanged, true);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Gets the total plots by vendor (for an optional measurement year)
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<Dictionary<string, int>>), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("report/plots-by-vendor")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> GetPlotsByVendorReportAsync([FromQuery] int? year)
        {
            try
            {
                var data = await projectManager.GetPlotsByVendorReportAsync(year);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Gets the total plots by project (for an optional measurement year)
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<Dictionary<string, int>>), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("report/plots-by-project")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> GetPlotsByProjectReportAsync([FromQuery] int? year)
        {
            try
            {
                var data = await projectManager.GetPlotsByProjectReportAsync(year);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }



        /// <summary>
        /// Search for plots in the system.
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="vendorId"></param>
        /// <param name="vsnProjectId"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<PlotModel>), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("plots/search")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> SearchPlotsAsync([FromQuery] string? keyword, [FromQuery] Guid? vendorId, [FromQuery] string? vsnProjectId, [FromQuery] bool includeDeleted)
        {
            try
            {
                var plots = await projectManager.SearchPlotsAsync(keyword, vendorId, vsnProjectId, includeDeleted);
                return Ok(plots.Select(p=>new PlotModel(p)).ToList());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Gets details for a specific plot.
        /// </summary>
        /// <param name="plotId"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(PlotDetails), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("plot")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> GetPlotDetailsAsync([FromQuery] Guid plotId, [FromQuery] bool includeDeleted)
        {
            try
            {
                var data = await projectManager.GetPlotDetailsAsync(plotId, includeDeleted);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }


        /// <summary>
        /// Downloads a CSV file for VSN data.
        /// </summary>
        /// <param name="table">VSN table to download.</param>
        /// <param name="vendorCode">(Optional) Vendor code filter.</param>
        /// <param name="lastChanged">(Optional) Last changed date filter.</param>
        /// <returns></returns>
        [Route("export/csv")]
        [HttpGet]
        [Produces("text/csv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> DownloadCsvFile([FromQuery] string table, [FromQuery] string? vendorCode, [FromQuery] DateTime? lastChanged)
        {
            try
            {
                // Log the parameters passed.
                logger.LogInformation($"Request for table '{table}', from '{lastChanged}' with vendor code '{vendorCode}'.");

                var filename = await projectManager.GenerateCsvFile(table, vendorCode, lastChanged, true);
                var downloadFilename = $"{table.ToLower()}.csv";
                return File(System.IO.File.OpenRead(filename), "text/csv", downloadFilename);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Gets a dashboard view of the current progress by vendor, project and year.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="projectId"></param>
        /// <param name="year"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(DashboardData), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("dashboard")]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> GetDashboardDataAsync([FromQuery] Guid? vendorId, [FromQuery]Guid? projectId, [FromQuery] int? year, [FromQuery] DateTime? startDate, [FromQuery]DateTime? endDate)
        {
            try
            {
                var data = await projectManager.GetDashboardDataAsync(vendorId, projectId, year, startDate, endDate);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Gets a list of plot summary information for reporting.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="projectId"></param>
        /// <param name="year"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [Route("dashboard/csv")]
        [HttpGet]
        [Produces("text/csv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [Authorize(Roles = Constants.UserRole)]
        public async Task<IActionResult> GetDashboardDataReportAsync([FromQuery] Guid? vendorId, [FromQuery] Guid? projectId, [FromQuery] int? year, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var filename = await projectManager.GetDashboardPlotReportAsync(vendorId, projectId, year, startDate, endDate);
                return File(System.IO.File.OpenRead(filename), "text/csv", "plot-summary.csv");
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}
