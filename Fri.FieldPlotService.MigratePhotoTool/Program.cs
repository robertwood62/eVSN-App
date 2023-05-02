using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Fri.FieldPlotService.MigratePhotoTool
{
    /// <summary>
    /// Defines the migration options.
    /// </summary>
    public class MigrationOptions
    {
        /// <summary>
        /// Constructs the migration options from the configuration.
        /// </summary>
        /// <param name="configuration"></param>
        /// <exception cref="ApplicationException"></exception>
        public MigrationOptions(IConfiguration configuration)
        {
            // Get the source information.
            SourceConnectionString = configuration["Source:ConnectionString"] ?? throw new ApplicationException("Missing setting 'Source:ConnectionString'.");
            SourceContainer = configuration["Source:Container"] ?? throw new ApplicationException("Missing setting 'Source:Container'.");
            var folderSection = configuration.GetSection("Source:Folders") ?? throw new ApplicationException("Missing setting 'Source:Folders'.");
            SourceFolders = folderSection.GetChildren().Select(c => c.Value ?? string.Empty).ToArray() ?? throw new ApplicationException("Empty setting 'Source:Folders'.");

            // Get the target information.
            TargetConnectionString = configuration["Target:ConnectionString"] ?? throw new ApplicationException("Missing settings 'Target:ConnectionString'.");
            TargetContainer = configuration["Target:Container"] ?? throw new ApplicationException("Missing settings 'Target:Container'.");
            TargetFolder = configuration["Target:Folder"] ?? throw new ApplicationException("Missing settings 'Target:Folder'.");
        }

        public string SourceConnectionString { get; set; }
        public string SourceContainer { get; set; }
        public string[] SourceFolders { get; set; }
        public string TargetConnectionString { get; set; }
        public string TargetContainer { get; set; }
        public string TargetFolder { get; set;}
    }

    /// <summary>
    /// Main program that migrates the VSN photos from source to target.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            // Setup the environment.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);

            var serviceProvider = new ServiceCollection()
                .AddLogging((o) => { o.AddDebug(); o.AddConsole(); o.AddSimpleConsole(); })
                .BuildServiceProvider() ?? throw new ApplicationException("Service provider not found.");

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? throw new ApplicationException("Logger factory not initialized.");
            var configuration = builder.Build();

            // Get the migration options.
            var options = new MigrationOptions(configuration);
            bool migrate = true;
            bool overwrite = false;

            MigrateDataAsync(options, migrate, overwrite).Wait();
        }

        /// <summary>
        /// Runs the migration process.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="migrate"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static async Task MigrateDataAsync(MigrationOptions options, bool migrate, bool overwrite)
        {
            try
            {
                var storageManager = new StorageManager(options);

                // Get the VSN Plot Photos from the source location.
                var vsnPhotoFileExpression = new Regex(@"^(VSN)((\d)*).(JPG|PNG|jpg|png)");
                Console.WriteLine("Finding files");
                var files = await storageManager.FindFilesAsync(vsnPhotoFileExpression);
                Console.WriteLine($"Total Files Found: {files.Count}");

                // Display duplicate files (that aren't the same size file).
                var duplicates = CountDifferentDuplicates(files);
                Console.WriteLine($"Total Duplicates Found: {duplicates.Count()}");

                // Apply migration fixes to files
                FixDuplicateFiles(files, duplicates);
                Console.WriteLine($"Files fixed.");

                // Display duplicate files (that aren't the same size file).
                duplicates = CountDifferentDuplicates(files);
                Console.WriteLine($"Total Duplicates Found: {duplicates.Count()}");
                if (duplicates.Count() > 0)
                {
                    Console.WriteLine("Duplicate files found - Migration aborted.");
                    return;
                }

                // Remove the files not to migrate
                files = files.Where(f => f.TargetName != null).ToList();

                // Display the list of files.
                Console.WriteLine("Listing files:");
                int counter = 0;
                foreach (var file in files)
                {
                    counter++;
                    try
                    {
                        if (migrate)
                        {
                            // Copy the file to the target location.
                            Console.Write($"({counter}/{files.Count})");
                            await storageManager.CopyFileAsync(file, overwrite);

                            // Ensure the plot record exists for this file.
                            //await EnsurePlotRecordExistsAsync(file);
                        }
                        else
                        {
                            Console.WriteLine($"{file}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Check for duplicates.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static List<StorageFile> CountDifferentDuplicates(List<StorageFile> files)
        {
            var duplicatesToFix = new List<StorageFile>();
            int duplicateCount = 0;

            // Check for duplicate files (of different size)
            var duplicateFiles = files.Where(f=>f.TargetName != null).GroupBy(f => f.TargetName, (key, f) => new { TargetName = key, Count = f.Count() }).ToList();

            Console.WriteLine($"Duplicate files in source location: {duplicateFiles.Count} found.");
            foreach (var duplicateFile in duplicateFiles)
            {
                var duplicates = files.Where(f => f.TargetName == duplicateFile.TargetName);
                var differentSizes = duplicates.GroupBy(f => f.Size, (key, f) => new { Size = key, Count = f.Count() }).ToArray();
                if (differentSizes.Count() > 1)
                {
                    duplicateCount++;
                    Console.WriteLine($"{duplicateFile.TargetName} - {duplicateFile.Count} copies found.");

                    // Check if the duplicates have the same file size.
                    Console.WriteLine($" - Multiple sizes found ({differentSizes.Count()})");
                    foreach (var file in duplicates)
                    {
                        duplicatesToFix.Add(file);
                        Console.WriteLine($"\t{file}");
                    }
                }
            }

            return duplicatesToFix;
        }


        /// <summary>
        /// Fix the duplicate files using different strategies.
        /// </summary>
        /// <param name="originalFiles"></param>
        /// <param name="duplicates"></param>
        /// <returns></returns>
        public static void FixDuplicateFiles(List<StorageFile> originalFiles, List<StorageFile> duplicates)
        {
            foreach (var file in originalFiles)
            {
                if (duplicates.Find(f => f.Name == file.Name) != null)
                {
                    // ----------------------------------------------------------------------------------
                    // Scenario 1: The filename's plot name is wrong, but the folder name is correct.
                    // Example: 2020LiDAR_Kenogami/2020LiDAR_Kenogami/VSN350055/VSN350001202001.JPG
                    // ----------------------------------------------------------------------------------
                    if (file.Name.StartsWith(@"2020LiDAR_Kenogami/2020LiDAR_Kenogami/VSN") && file.TargetName != null)
                    {
                        var fileInfo = new FileInfo(file.Name);
                        var plotIndex = fileInfo.DirectoryName?.LastIndexOf("VSN");
                        if(plotIndex != null && plotIndex != -1)
                        {
                            var plotName = fileInfo.DirectoryName?.Substring(plotIndex.Value);
                            var targetName = plotName + file.TargetName.Substring(9);
                            Console.WriteLine($"Changed {file.Name} to {targetName}");
                            file.TargetName = targetName;
                        }
                    }

                    // ----------------------------------------------------------------------------------
                    // Scenarios 2: The filename year is wrong (based on the folder structure)
                    // Example: 2021/Needaak/July submision/July submision/VSN350018/VSN350018202002.JPG
                    // ----------------------------------------------------------------------------------
                    if(file.Name.StartsWith(@"2021/Needaak/July submision/July submision/VSN") && file.TargetName != null)
                    {
                        var year = file.Name.Substring(0, 4);
                        var targetName = file.TargetName.Substring(0, 9) + year + file.TargetName.Substring(13);
                        Console.WriteLine($"Changed {file.Name} to {targetName}");
                        file.TargetName = targetName;
                    }

                    // ----------------------------------------------------------------------------------
                    // Scenarios 3: There are duplicates files in different folders that have already been
                    //              corrected, so we should ignore the original files.
                    // Example: 2022/kbm/Delivery_July2022/Photos/360Photos/Spanish/VSN210149202211.JPG
                    // Corrected version: 2022/kbm/Spanish Forest/VSN210149/VSN210149202211.JPG
                    // ----------------------------------------------------------------------------------
                    if (file.Name.StartsWith(@"2022/kbm/Delivery_July2022") && file.TargetName != null)
                    {
                        Console.WriteLine($"Excluding {file.Name}");
                        file.TargetName = null;
                    }

                    // ----------------------------------------------------------------------------------
                    // Scenario 4: Remove any zero byte files.
                    // ----------------------------------------------------------------------------------
                    if (file.Size == 0)
                    {
                        Console.WriteLine($"Excluding {file.Name} with zero bytes.");
                        file.TargetName = null;
                    }

                }
            }
        }
    }
}