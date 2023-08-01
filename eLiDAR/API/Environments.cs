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

            if(BaseUrl == null)
            {
                var util = new Utils();
                BaseUrl = util.CustomEnvironmentUrl;
            }
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
        public const string CustomEnvironmentName = "Custom";

        /// <summary>
        /// List of all the current environments.
        /// </summary>
        public readonly static EnvironmentConfig[] Environments = new EnvironmentConfig[]
        {
#if DEBUG
            new EnvironmentConfig("Development", "https://dev-fri-partners-api.azurewebsites.net/", true),
            new EnvironmentConfig("Local", "https://localhost:7133/"),
            new EnvironmentConfig("Production", "https://beta-fri-partners-api.azurewebsites.net/"),
            new EnvironmentConfig("IST", "https://dev.partner.fri.mnrf.gov.on.ca/api/", false),
#else
            new EnvironmentConfig("Production", "https://beta-fri-partners-api.azurewebsites.net/", false),
            new EnvironmentConfig("Development", "https://dev-fri-partners-api.azurewebsites.net/", false),
            new EnvironmentConfig("IST", "https://dev.partner.fri.mnrf.gov.on.ca/api/", true),
#endif
            new EnvironmentConfig(CustomEnvironmentName, null)
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

        /// <summary>
        /// Set's the custom environment URL to use.  If a blank string is used, the custom option won't be available.
        /// </summary>
        /// <param name="serverUrl"></param>
        public static void SetCustomEnvironment(string serverUrl)
        {
            // Update the preferences.
            var util = new Utils
            {
                CustomEnvironmentUrl = serverUrl
            };

            // Update the current in-memory environment setting.
            foreach(var environment in  Environments)
            {
                if(environment.Name == CustomEnvironmentName)
                {
                    environment.BaseUrl = serverUrl;
                }
            }
        }
    }
}
