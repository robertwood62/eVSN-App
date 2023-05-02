using Azure.Storage.Blobs.Models;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System.Reflection.Metadata.Ecma335;

namespace Fri.FieldPlotService.MigratePhotoTool
{
    /// <summary>
    /// Represents a single existing field photo 
    /// </summary>
    public class StorageFile
    {
        public StorageFile(string container, string name, long size)
        {
            Container = container;
            Name = name;
            Size = size;
            TargetName = new FileInfo(Name).Name.ToUpper();
        }

        public string Container { get; set; }
        public string Name { get; set; }
        public string? TargetName { get; set; }
        public long Size { get; set; }

        public override string ToString()
        {
            return $"{Name} -> {TargetName} ({Size} bytes)";
        }
    }

    /// <summary>
    /// Class that provides an abstractions accessing Azure Storage.
    /// </summary>
    public class StorageManager
    {
        readonly MigrationOptions options;
        readonly BlobServiceClient sourceClient;
        readonly BlobContainerClient sourceContainer;
        readonly BlobServiceClient targetClient;
        readonly BlobContainerClient targetContainer;

        /// <summary>
        /// Constructs the storage manager for a given storage account.
        /// </summary>
        /// <param name="options"></param>
        public StorageManager(MigrationOptions options)
        {
            this.options = options;

            // Connect to the source storage
            sourceClient = new BlobServiceClient(options.SourceConnectionString);
            sourceContainer = sourceClient.GetBlobContainerClient(options.SourceContainer);
            targetClient = new BlobServiceClient(options.TargetConnectionString);
            targetContainer = targetClient.GetBlobContainerClient(options.TargetContainer);
        }

        /// <summary>
        /// Gets a list of files that match a file expression (recursive all folders)
        /// </summary>
        /// <param name="fileExpression"></param>
        /// <returns></returns>
        public async Task<List<StorageFile>> FindFilesAsync(Regex fileExpression)
        {
            if(!await sourceContainer.ExistsAsync())
            {
                throw new ApplicationException($"The source container '{sourceContainer.Name}' does not exist.");
            }

            var files = new List<StorageFile>();

            foreach(var folder in options.SourceFolders)
            {
                Console.WriteLine($"Scanning folder '{folder}'");

                // Get the list of files (in segments of 100)
                var resultSegment = sourceContainer.GetBlobsAsync(prefix: folder).AsPages(default, 100);
                
                // Enumerate the blobs returned for each page.
                await foreach (Page<BlobItem> blobPage in resultSegment)
                {
                    foreach (BlobItem blobItem in blobPage.Values)
                    {
                        var fileInfo = new FileInfo(blobItem.Name);
                        if (fileExpression.IsMatch(fileInfo.Name))
                        {
                            var file = new StorageFile(sourceContainer.Name, blobItem.Name, blobItem.Properties.ContentLength ?? 0);
                            Console.WriteLine($"\t{file}");
                            files.Add(file);
                        }
                    }
                }
            }

            return files;
        }

        /// <summary>
        /// Copy the file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public async Task CopyFileAsync(StorageFile file, bool overwrite)
        {
            if(file.TargetName == null)
            {
                throw new ApplicationException($"The source file does not have a target name."); ;
            }

            // Ensure the container are valid.
            if (!await sourceContainer.ExistsAsync())
            {
                throw new ApplicationException($"The source container '{sourceContainer.Name}' does not exist.");
            }
            if (!await targetContainer.ExistsAsync())
            {
                throw new ApplicationException($"The target container '{targetContainer.Name}' does not exist.");
            }

            // Connect to the source file
            var sourceBlob = sourceContainer.GetBlobClient(file.Name);
            if(!await sourceBlob.ExistsAsync())
            {
                throw new ApplicationException($"The source file '{file.Name}' in container '{sourceContainer.Name}' does not exist.");
            }

            // Setup the upload location
            var targetFilename = Path.Combine(options.TargetFolder, file.TargetName);
            var targetBlob = targetContainer.GetBlobClient(targetFilename);
            if(await targetBlob.ExistsAsync())
            {
                // Check if the file is the same size or not
                var properties = await targetBlob.GetPropertiesAsync();
                if(properties.Value.ContentLength == file.Size)
                {
                    Console.WriteLine($"Skipping '{file.TargetName}' which already exists with same size.");
                    return;
                }
                else
                {
                    if (overwrite)
                    {
                        await targetBlob.DeleteIfExistsAsync();
                    }
                    else
                    {
                        throw new ApplicationException($"File '{targetBlob.Name}' ({properties.Value.ContentLength} bytes) is different from source file size {file.Size} bytes.");
                    }
                }

            }

            // Download the data locally, then upload it.
            Console.Write($"Copying '{file.Name}' ({file.Size} bytes).");
            var fileData = await sourceBlob.DownloadContentAsync();
            await targetBlob.UploadAsync(fileData.Value.Content);
            Console.WriteLine($" - Completed");
        }
    }
}
