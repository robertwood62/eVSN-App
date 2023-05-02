using Fri.FieldPlotService.Tools;
using Microsoft.Extensions.Configuration;

namespace Fri.FieldPlotService.Common
{
    /// <summary>
    /// Represents the list of options (configuration) for the application.
    /// </summary>
    public class FieldPlotServiceOptions
    {
        /// <summary>
        /// Constructs the field plot options.
        /// </summary>
        /// <param name="configuration"></param>
        /// <exception cref="FieldPlotServiceException"></exception>
        public FieldPlotServiceOptions(IConfiguration configuration)
        {
            DatabaseConnectionString = configuration["Database:ConnectionString"] ?? throw new FieldPlotServiceException(FieldPlotServiceError.ConfigurationError, "The Database:ConnectionString is missing.");
            DatabaseName = configuration["Database:DatabaseName"] ?? throw new FieldPlotServiceException(FieldPlotServiceError.ConfigurationError, "The Database:DatabaseName is missing.");
            EncryptionKey = configuration["EncryptionKey"] ?? throw new FieldPlotServiceException(FieldPlotServiceError.ConfigurationError, "The EncryptionKey is missing.");
            ApiKeys = (configuration["ApiKeys"] ?? throw new FieldPlotServiceException(FieldPlotServiceError.ConfigurationError, "The ApiKeys is missing.")).Split(',');
        }

        /// <summary>
        /// Connection string to the Cosmos DB.
        /// </summary>
        public string DatabaseConnectionString { get; set; }

        /// <summary>
        /// Name of the Cosmos DB database.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Encryption key used to encrypt a user token generated for the field app after sign-in.
        /// </summary>
        public string EncryptionKey { get; set; }

        /// <summary>
        /// API Keys for certain API operations
        /// </summary>
        public string[] ApiKeys { get; set; }
    }
}
