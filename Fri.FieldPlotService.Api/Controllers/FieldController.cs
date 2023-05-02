using Fri.FieldPlotService.Api.Common;
using Fri.FieldPlotService.Api.Models;
using Fri.FieldPlotService.Common;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Managers;
using Fri.FieldPlotService.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.UI.Areas.MicrosoftIdentity.Pages.Account;
using Newtonsoft.Json;
using System.Text.Json;

namespace Fri.FieldPlotService.Api.Controllers
{
    /// <summary>
    /// Provides an API for authentication and synchronizing data between the field app and backend database.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class FieldController : BaseController
    {
        readonly SyncManager syncManager;

        /// <summary>
        /// Constructor with dependencies.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <param name="syncManager"></param>
        public FieldController(ILogger<FieldController> logger, FieldPlotServiceOptions options, SyncManager syncManager) 
            :base(logger, options)
        { 
            this.syncManager = syncManager;
        }


        /// <summary>
        /// Signs a user into the system using a username and password.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserTokenInfo), 200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPost]
        [Route("sign-in")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInModel model)
        {
            try
            {
                var userTokenInfo = await syncManager.SignInAsync(model.Username, model.Password);
                return Ok(userTokenInfo);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Gets table entities from the database as of the last sync date and (optional) plot ID.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="lastSyncTime"></param>
        /// <param name="plotId"></param>
        /// <param name="token"></param>
        /// <param name="deviceId"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpGet]
        [Route("sync")]
        public async Task<IActionResult> GetEntitiesAsync([FromQuery] string table, [FromQuery] DateTime? lastSyncTime, [FromQuery] string? plotId, [FromHeader(Name = "user_token")] string token, [FromHeader(Name = "user_device_id")] Guid? deviceId, [FromHeader(Name = "user_device_type")] string? deviceType)
        {
            try
            {
                var userToken = ValidateUserToken(token);
                var results = await syncManager.GetEntitiesAsync(userToken, new SyncRequest
                {
                    DeviceId = deviceId,
                    DeviceType = deviceType,
                    LastSyncTime = lastSyncTime,
                    PlotId = plotId,
                    Table = table
                });
                return Ok(results);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Method to insert entity data into the database.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="json"></param>
        /// <param name="token"></param>
        /// <param name="deviceId"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPost]
        [Route("sync")]
        public async Task<IActionResult> InsertEntitiesAsync([FromQuery]string table, [FromBody] JsonElement? json, [FromHeader(Name = "user_token")] string token, [FromHeader(Name = "user_device_id")] Guid? deviceId, [FromHeader(Name = "user_device_type")] string? deviceType)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, object?>>>(json?.ToString() ?? "[]");

                var userToken = ValidateUserToken(token);
                await syncManager.SyncEntitiesAsync(userToken, new SyncRequest
                {
                    DeviceId = deviceId,
                    DeviceType = deviceType,
                    LastSyncTime = null,
                    PlotId = null,
                    Table = table
                }, data, true, true);
                return Ok();
            }
            catch(Exception ex) 
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Method to update existing entity data in the database.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="json"></param>
        /// <param name="token"></param>
        /// <param name="deviceId"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorModel), 400)]
        [ProducesResponseType(401)]
        [HttpPut]
        [Route("sync")]
        public async Task<IActionResult> UpdateEntitiesAsync([FromQuery] string table, [FromBody] JsonElement? json, [FromHeader(Name = "user_token")]string token, [FromHeader(Name = "user_device_id")] Guid? deviceId, [FromHeader(Name = "user_device_type")] string? deviceType)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, object?>>>(json?.ToString() ?? "[]");

                var userToken = ValidateUserToken(token);
                await syncManager.SyncEntitiesAsync(userToken, new SyncRequest
                {
                    DeviceId  = deviceId,
                    DeviceType = deviceType,
                    LastSyncTime = null,
                    PlotId = null,
                    Table = table
                }, data, false, true);

                return Ok();
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

    }
}
