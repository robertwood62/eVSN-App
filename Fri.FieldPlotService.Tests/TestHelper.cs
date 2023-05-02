using Fri.FieldPlotService.Common;
using Fri.FieldPlotService.Managers;
using Fri.FieldPlotService.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fri.DownloadService.Tests
{
    public class TestHelper
    {
        readonly static HttpClient http = new();
        readonly static IConfiguration configuration;
        readonly static ILoggerFactory loggerFactory;
        public readonly static DbManager Db;
        public readonly static FieldPlotServiceOptions Options;

        static TestHelper()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            var serviceProvider = new ServiceCollection()
                .AddLogging((o) => { o.AddDebug(); o.AddConsole(); o.AddSimpleConsole(); })
                .BuildServiceProvider() ?? throw new ApplicationException("Service provider not found");

#pragma warning disable CS8601 // Possible null reference assignment.
            loggerFactory = serviceProvider.GetService<ILoggerFactory>();
#pragma warning restore CS8601 // Possible null reference assignment.

            if (loggerFactory == null)
            {
                throw new ApplicationException("Logger factory not initialized");
            }

            configuration = builder.Build();
            Options = new FieldPlotServiceOptions(configuration);
            Db = new DbManager(new DbManagerOptions(configuration), loggerFactory.CreateLogger<DbManager>());
        }
    }

}
