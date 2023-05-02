using eLiDAR.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace eLiDAR.API
{
    /// <summary>
    /// Current environment information.
    /// </summary>
    public class EnvironmentConfig
    {
        /// <summary>
        /// Constructs the current environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="baseUrl"></param>
        /// <param name="isDefault"></param>
        public EnvironmentConfig(string name, string baseUrl, bool isDefault = false)
        {
            Name = name;
            BaseUrl = baseUrl;
            IsDefault = isDefault;
        }

        /// <summary>
        /// Name of the environment.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Base URL to the backend.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// True if this is the default environment to use.
        /// </summary>
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Provides a method to get/set and list the environments configured.
    /// </summary>
    public class EnvironmentSelector
    {
        /// <summary>
        /// List of all the current environments.
        /// </summary>
        readonly static public EnvironmentConfig[] Environments = new EnvironmentConfig[]
        {
#if DEBUG
            new EnvironmentConfig("Development", "https://dev-fri-field-app-service-api.azurewebsites.net/", true),
            new EnvironmentConfig("Local", "https://localhost:7133/"),
            new EnvironmentConfig("Production", "https://beta-fri-field-app-service-api.azurewebsites.net/"),
#else
            new EnvironmentConfig("Production", "https://beta-fri-field-app-service-api.azurewebsites.net/", true),
            new EnvironmentConfig("Development", "https://dev-fri-field-app-service-api.azurewebsites.net/", false),
#endif
        };

        /// <summary>
        /// Gets the currently configured environment to use.
        /// </summary>
        public static EnvironmentConfig Current
        {
            get
            {
                var util = new Utils();
                var environmentName = util.EnvironmentName;
                if (!string.IsNullOrWhiteSpace(environmentName))
                {
                    var environment = Environments.FirstOrDefault(e => e.Name == environmentName);
                    if(environment != null)
                    {
                        return environment;
                    }
                }
                return Environments.FirstOrDefault(e => e.IsDefault);
            }
        }

        /// <summary>
        /// Sets the current environment to use.
        /// </summary>
        /// <param name="environmentName"></param>
        public static void SetEnvironment(string environmentName)
        {
            var environment = Environments.FirstOrDefault(e => e.Name == environmentName);
            if(environment != null)
            {
                var util = new Utils
                {
                    EnvironmentName = environment.Name
                };
            }
        }
    }
}
