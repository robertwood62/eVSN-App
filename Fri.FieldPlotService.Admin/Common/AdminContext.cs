using Fri.FieldPlotService.Admin.Services;

namespace Fri.FieldPlotService.Admin.Common
{
    /// <summary>
    /// Manages interactions with the backend.
    /// </summary>
    public class AdminContext
    {
        readonly AdminContextOptions options;
        readonly HttpClient httpClient;

        /// <summary>
        /// Constructs the Admin Context with the required dependencies.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="httpClient"></param>
        public AdminContext(AdminContextOptions options, HttpClient httpClient)
        {
            this.options = options;
            this.httpClient = httpClient;

            Client = new AdminClient(this.options.FieldPlotServiceApi, this.httpClient);
        }

        /// <summary>
        /// Access to the backend Admin API for the Download Service.
        /// </summary>
        public AdminClient Client { get; set; }
    }
}
