using Fri.FieldPlotService.Api.Common;
using Fri.FieldPlotService.Api.Models;
using Fri.FieldPlotService.Common;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace Fri.FieldPlotService.Api.Controllers
{
    /// <summary>
    /// Data control to import plots and explore data using an API Key for authorization.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : BaseController
    {
        readonly ProjectManager projectManager;

        /// <summary>
        /// Constructs the data controller.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <param name="projectManager"></param>
        public DataController(ILogger<FieldController> logger, FieldPlotServiceOptions options, ProjectManager projectManager)
            :base(logger, options)
        {
            this.projectManager = projectManager;
        }

        /// <summary>
        /// Imports a series of plots for a vendor
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="vendorId"></param>
        /// <param name="projectId"></param>
        /// <param name="plotList"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPost]
        [Route("plots/import")]
        public async Task<IActionResult> ImportVsnPlotsAsync([FromHeader(Name = "api-key")] string apiKey, [FromQuery]Guid vendorId, [FromQuery]Guid projectId, [FromBody] Dictionary<string, object?>[] plotList )
        {
            try
            {
                ValidateApiKey(apiKey);

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
        /// <param name="apiKey"></param>
        /// <param name="table"></param>
        /// <param name="vendorCode"></param>
        /// <param name="lastChanged"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<Dictionary<string, object?>>), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("export")]
        public async Task<IActionResult> ExportVsnDataAsync([FromHeader(Name = "api-key")] string apiKey, [FromQuery] string table, [FromQuery] string? vendorCode, [FromQuery] DateTime? lastChanged, [FromQuery]bool includeDeleted)
        {
            try
            {
                ValidateApiKey(apiKey);

                var data = await projectManager.ExportVsnDataAsync(table, vendorCode, lastChanged, includeDeleted);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}
