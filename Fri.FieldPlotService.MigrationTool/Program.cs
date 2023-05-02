using Fri.FieldPlotService.Common;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Managers;
using Fri.FieldPlotService.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Fri.FieldPlotService.Admin
{
    public class VendorConfig
    {
        public VendorConfig(IConfigurationSection section)
        {
            Code = section["Code"] ?? throw new ApplicationException("Missing 'Code' for Vendor");
            BaseUrl = section["BaseUrl"] ?? throw new ApplicationException("Missing 'Url' for Vendor");
            Name = section["Name"] ?? throw new ApplicationException("Missing 'Name' for Vendor");

            if(!BaseUrl.EndsWith("/"))
            {
                BaseUrl += "/";
            }
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string BaseUrl { get; set; }
    }

    internal class Program
    {
        readonly static Type[] entities = { typeof(Deformity), typeof(Dwd), typeof(Ecosite), typeof(Person), typeof(Plot), typeof(Photo), typeof(Project), typeof(SmallTree), typeof(SmallTreeTally), typeof(Soil), typeof(StemMap), typeof(Tree), typeof(Vegetation), typeof(VegetationCensus) };
        readonly static HttpClient http = new();
#pragma warning disable CS8618
        static FieldPlotServiceOptions options;
        static SyncManager syncManager;
        static DbManager dbManager;
        static VendorManager vendorManager;
        static string legacyApiKey;
#pragma warning restore CS8618

        /// <summary>
        /// Migration Tool main.
        /// </summary>
        /// <param name="_"></param>
        static async Task Main(string[] _)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            var serviceProvider = new ServiceCollection()
                .AddLogging((o) => { o.AddDebug(); o.AddConsole(); o.AddSimpleConsole(); })
                .BuildServiceProvider() ?? throw new ApplicationException("Service provider not found.");

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? throw new ApplicationException("Logger factory not initialized.");
            var configuration = builder.Build();

            // Create the FRI Field Plot Services
            options = new FieldPlotServiceOptions(configuration);
            dbManager = new DbManager(new DbManagerOptions(configuration), loggerFactory.CreateLogger<DbManager>());
            vendorManager = new VendorManager(dbManager);
            syncManager = new SyncManager(dbManager, loggerFactory.CreateLogger<SyncManager>(), options, vendorManager);

            // Get the legacy API Key to use.
            legacyApiKey = configuration["LegacyApiKey"] ?? throw new ApplicationException("The 'LegacyApiKey' value is missing.");



            // Get the list of vendors to process
            var vendors = LoadVendors(configuration);
            Console.WriteLine($"Processing {vendors.Count} vendors.");

            Console.WriteLine("Totals before import");
            await DisplayTotalsAsync();

            // Import each vendors data.
            foreach (var vendor in vendors)
            {
                await ImportVendorDataAsync(vendor);
            }

            Console.WriteLine("Totals after import");
            await DisplayTotalsAsync();

            //await EnsurePlotPhotosExistAsync();

            await CheckDuplicateRecordsAsync();
        }

        class TotalData
        {
            public int Total { get; set; }
        }


        static async Task DisplayTotalsAsync()
        {
            var vendors = await vendorManager.SearchVendorsAsync(null, null);
            foreach(var vendor in vendors)
            {
                foreach (var entity in entities)
                {
                    var query = $"select count(1) as Total from (select distinct value p.Data.{entity.Name.ToUpper()}ID from (select * from c where c.Data.IsDeleted = 'N' and c.VendorId = '{vendor.Id}' ) p ) d";
                    var result = await dbManager.QueryAsync<TotalData>(query, null, null, entity.Name);
                    Console.WriteLine($"{vendor.Info?.Code}, {entity.Name}, {result[0].Total}");
                }
            }
        }

        /// <summary>
        /// Load the list of vendors from the configuration files.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        static List<VendorConfig> LoadVendors(IConfiguration configuration)
        {
            var vendors = new List<VendorConfig>();
            var vendorsSection = configuration.GetSection("Vendors") ?? throw new ApplicationException("'Vendors' is missing from the configuration file.");
            foreach(var vendorSection in vendorsSection.GetChildren())
            {
                vendors.Add(new VendorConfig(vendorSection));
            }

            return vendors;
        }

        /// <summary>
        /// Import the data for each vendor.
        /// </summary>
        /// <param name="vendorConfig"></param>
        /// <returns></returns>
        async static Task ImportVendorDataAsync(VendorConfig vendorConfig)
        {
            Console.WriteLine($"Processing vendor '{vendorConfig.Code}'.");

            // Ensure the vendor exists in the new system.
            var vendor = await vendorManager.GetVendorAsync(vendorConfig.Code);
            vendor ??= await vendorManager.CreateVendorAsync(new VendorInfo
            {
                Code = vendorConfig.Code,
                Status = VendorStatusType.Active,
                Name = vendorConfig.Name
            });

            // Ensure the migration user account exists
            var user = await vendorManager.GetVendorUserAsync("MigrationTool");
            user ??= await vendorManager.CreateVendorUserAsync(vendor.Id, new UserInfo
                {
                    Username = "MigrationTool",
                    Status = UserStatusType.Active
                }, Guid.NewGuid().ToString());

            // Create a user token
            var userToken = new UserToken
            {
                UserId = user.Id,
                Username = user.Info?.Username,
                VendorId = vendor.Id
            };

            /// Sync each of the tables in the system.
            foreach (var table in entities.Select(e=>e.Name.ToLower()))
            {
                Console.WriteLine($"Processing vendor '{vendorConfig.Code}' table '{table}'.");

                var requestUrl = $"{vendorConfig.BaseUrl}get/manual/paths/invoke/{table}?filter=CreatedAtServer gt '01-01-2000'";
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", legacyApiKey);
                
                var responseMessage = await http.SendAsync(requestMessage);
                if(!responseMessage.IsSuccessStatusCode)
                {
                    throw new ApplicationException($"Error calling '{requestUrl}': {responseMessage.StatusCode} - {responseMessage.ReasonPhrase}");
                }

                // Get the JSON data
                var json = await responseMessage.Content.ReadAsStringAsync();

                // -------------------------------------------------------
                // Check the logical IDs are unique
                // -------------------------------------------------------
                var dataItems = JsonConvert.DeserializeObject<List<Dictionary<string, object?>>>(json);

                var duplicates = dataItems?
                    .GroupBy(p => p[$"{table.ToUpper()}ID"]?.ToString(), (key, values) => new { Name = key?.ToString(), Count = values.Count() })
                    .Where(p => p.Count > 1)
                    .ToArray();

                if (duplicates != null && duplicates.Any())
                {
                    foreach (var duplicateID in duplicates)
                    {
                        Console.WriteLine($"{duplicateID.Count} duplicates {table}ID for '{duplicateID}' found.");
                    }
                    throw new ApplicationException("Abort migration due to duplicate IDs");
                }

                // Merge the data.
                await syncManager.SyncEntitiesAsync(userToken, new SyncRequest
                {
                    DeviceId = null,
                    LastSyncTime = null,
                    DeviceType = null,
                    PlotId = null,
                    Table = table
                }, dataItems, null, false);
            }

            // Remove the migration user account
            await vendorManager.DeleteVendorUserAsync(user.Id);
        }

        static async Task EnsurePlotPhotosExistAsync()
        {
            var query = $"select * from Plot p where p.deleted = null and p.Data.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}'";
            var existingPlots = await dbManager.QueryAsync<Plot>(query, null);

            Console.WriteLine($"Checking photo records for {existingPlots.Count} plots.");

            int counter = 0;
            foreach(var plot in existingPlots)
            {
                counter++;
                if(plot.Data != null && plot.Data.TryGetValue(Constants.VsnPlotId, out var id))
                {
                    var plotId = id?.ToString();
                    if(!string.IsNullOrWhiteSpace(plotId))
                    {
                        Console.WriteLine($"({counter}/{existingPlots.Count}) Checking plot {plotId} photos.");
                        await EnsurePlotPhotoExistAsync(plot.VendorId, plotId);
                    }
                }
            }
        }

        public class DuplicateEntity
        {
            public Guid Id { get; set; }
            public DateTime Created { get; set; }
            public int Count { get; set; }
        }

        /// <summary>
        /// Removes duplicate records
        /// </summary>
        /// <returns></returns>
        static async Task CheckDuplicateRecordsAsync()
        {
            foreach (var entity in entities)
            {
                var query = $"select * from (SELECT c.VendorId as VendorId, c.Data.{entity.Name.ToUpper()}ID as Id, c.Data.Created as Created, count(1) as Total from (select * from {entity.Name} p where p.deleted = null and p.Data.IsDeleted = 'N') c group by c.VendorId, c.Data.{entity.Name.ToUpper()}ID, c.Data.Created) z where z.Total > 1";
                var duplicates = await dbManager.QueryAsync<DuplicateEntity>(query, null, null, entity.Name);
                Console.WriteLine($"Entity {entity.Name} has {duplicates.Count} duplicate records");
            }
        }

        static bool IsDuplicateEntity(VsnEntityBase entity1, VsnEntityBase entity2)
        {
            if(entity1.Data == null || entity2.Data == null)
            {
                return false;
            }

            foreach(var key in entity1.Data.Keys)
            {
                var value1 = entity1.GetObject(key)?.ToString();
                var value2 = entity2.GetObject(key)?.ToString();
                Console.Write($"{value1}={value2}");
                if (value1 != value2)
                {
                    Console.WriteLine("Different");
                    return false;
                }
                Console.WriteLine("");
            }

            return true;
        }


        /// <summary>
        /// Ensure that all the plot photos record exist for a given vendor/VSN plotid
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="vsnPlotId"></param>
        /// <returns></returns>
        static async Task EnsurePlotPhotoExistAsync(Guid vendorId, string vsnPlotId)
        {
            const int DefaultPhoto1Distance = 6;
            const int DefaultPhoto2Distance = 12;

            // Create the initial photos 1 and 18 for Stand information using the logic borrowed from the
            // front-end application that can pre-create the photo records.
            var plotPhotos = new List<Photo>
            {
                new Photo
                {
                    VendorId = vendorId,
                    Data = new Dictionary<string, object?>
                    {
                        { "PHOTOTYPE", "Stand Information" },
                        { "PHOTONUMBER", 1 },
                        { "AZIMUTH", 0 },
                        { "DISTANCE", 0 },
                        { "PLOTID", vsnPlotId },
                        { "Created", DateTime.UtcNow },
                        { "LastModified", DateTime.UtcNow },
                        { "IsDeleted", Constants.VsnActiveFlag },
                    }
                },

                new Photo
                {
                    VendorId = vendorId,
                    Data = new Dictionary<string, object?>
                    {
                        { "PHOTOTYPE", "Stand Information" },
                        { "PHOTONUMBER", 18 },
                        { "AZIMUTH", 0 },
                        { "DISTANCE", 0 },
                        { "PLOTID", vsnPlotId },
                        { "Created", DateTime.UtcNow },
                        { "LastModified", DateTime.UtcNow },
                        { "IsDeleted", Constants.VsnActiveFlag },
                    }
                }
            };

            // Insert records for the Stand Infor Photos
            int j = 1;
            for (int i = 2; i < 18; i += 2)
            {
                plotPhotos.Add(new Photo
                {
                    VendorId = vendorId,
                    Data = new Dictionary<string, object?>
                    {
                        { "PHOTOTYPE", "Stand Information" },
                        { "PHOTONUMBER", i },
                        { "AZIMUTH", (i - (j + 1)) * 45 },
                        { "DISTANCE", (j % 2 == 0) ? DefaultPhoto2Distance : DefaultPhoto1Distance },
                        { "PLOTID", vsnPlotId },
                        { "Created", DateTime.UtcNow },
                        { "LastModified", DateTime.UtcNow },
                        { "IsDeleted", Constants.VsnActiveFlag },
                    }
                });
                j += 1;
            }

            j = 2;
            for (int i = 3; i < 18; i += 2)
            {
                plotPhotos.Add(new Photo
                {
                    VendorId = vendorId,
                    Data = new Dictionary<string, object?>
                    {
                        { "PHOTOTYPE", "Stand Information" },
                        { "PHOTONUMBER", i },
                        { "AZIMUTH", (i - (j + 1)) * 45 },
                        { "DISTANCE", (j % 2 == 0) ? DefaultPhoto2Distance : DefaultPhoto1Distance },
                        { "PLOTID", vsnPlotId },
                        { "Created", DateTime.UtcNow },
                        { "LastModified", DateTime.UtcNow },
                        { "IsDeleted", Constants.VsnActiveFlag },
                    }
                });
                j += 1;
            }

            var query = $"select * from Photo p where p.VendorId = @vendorId and p.deleted = null and p.Data.PLOTID = @vsnPlotId and p.Data.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}'";
            var existingPhotos = await dbManager.QueryAsync<Photo>(query, new Dictionary<string, object>
                {
                    { "@vendorId", vendorId },
                    { "@vsnPlotId", vsnPlotId},
                });

            // Use the list above to ensure each photo record exists in the DB.
            // If not, create the record.
            foreach (var plotPhoto in plotPhotos)
            {
                // Check if he phone number exist.
                var photoNumber = plotPhoto.GetInt("PHOTONUMBER");
                var existingPhoto = existingPhotos.Find(p => p.GetInt("PHOTONUMBER") == photoNumber);

                // If no record exists, create it.
                if (existingPhoto == null)
                {
                    await dbManager.CreateAsync(plotPhoto);
                }
            }
        }
    }
}