namespace Fri.FieldPlotService.Admin.Common
{
    /// <summary>
    /// Manages the Admin Context Options.
    /// </summary>
    public class AdminContextOptions
    {
        /// <summary>
        /// Constructs the Admin Context Options.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>
        /// <exception cref="ApplicationException"></exception>
        public AdminContextOptions(IConfiguration configuration, string environment)
        {
            // Get the backend service configuration.
            FieldPlotServiceApi = configuration[$"{environment}:FieldPlotServiceApi:BaseUrl"] ?? throw new ApplicationException("Missing appsettings.json entry for {environment}:FieldPlotServiceApi:BaseUrl");
        }

        /// <summary>
        /// The base URL for the download admin API.
        /// </summary>
        public string FieldPlotServiceApi { get; set; }
    }
}
