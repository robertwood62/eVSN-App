using Fri.FieldPlotService.Common;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Tools;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.RecordIO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using CsvHelper;
using CsvHelper.Configuration;

namespace Fri.FieldPlotService.Managers
{
    /// <summary>
    /// Manages vendor projects in the system.
    /// </summary>
    public class ProjectManager
    {
        readonly DbManager dbManager;
        readonly VendorManager vendorManager;

        /// <summary>
        /// Creates the SyncManager with dependencies.
        /// </summary>
        /// <param name="dbManager"></param>
        /// <param name="vendorManager"></param>
        public ProjectManager(DbManager dbManager, VendorManager vendorManager)
        {
            this.dbManager = dbManager;
            this.vendorManager = vendorManager;
        }


        /// <summary>
        /// Creates a new project for a list of vendors.
        /// </summary>
        /// <param name="vendors"></param>
        /// <param name="vsnProjectId"></param>
        /// <param name="projectInfo"></param>
        /// <returns></returns>
        public async Task CreateProjectsAsync(Guid[]? vendors, string? vsnProjectId, ProjectInfo? projectInfo)
        {
            // Check at least one vendor was provided.
            if(vendors == null || vendors.Length == 0)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VendorNotFound);
            }

            // Validate the project info.
            if(projectInfo == null)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.ProjectInfoMissing);
            }
            projectInfo.Validate();

            // Check the project name is valid and doesn't already exist.
            if(string.IsNullOrWhiteSpace(vsnProjectId))
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VSNProjectIdInvalid);
            }

            var data = new Dictionary<string, object?>
            {
                { Constants.VsnProjectId, vsnProjectId },
                { Constants.VsnProjectDescription, projectInfo.Description },
                { Constants.VsnProjectName, projectInfo.Name },
                { Constants.VsnProjectDate, projectInfo.ProjectDate },
                { Constants.VsnCreated, DateTime.UtcNow },
                { Constants.VsnIsDeleted, Constants.VsnActiveFlag },
                { Constants.VsnLastModified, DateTime.UtcNow }
            };

            // Create the project for each vendor (if not already created)
            foreach (var vendorId in vendors)
            {
                // See if there is an existing project
                var project = await GetProjectAsync(vendorId, vsnProjectId);
                if (project == null)
                {
                    // Create the project.
                    project = new Project
                    {
                        VendorId = vendorId,
                        Id = Guid.NewGuid(),
                        Data = data
                    };
                    await dbManager.CreateAsync(project);
                }
                else
                {
                    // Update the existing project.
                    project.Data = data;
                    await dbManager.UpdateAsync(project);
                }
            }
        }

        /// <summary>
        /// Creates a new project for a vendor.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="vsnProjectId"></param>
        /// <param name="projectInfo"></param>
        /// <returns></returns>
        public async Task<Project> CreateProjectAsync(Guid vendorId, string? vsnProjectId, ProjectInfo? projectInfo)
        {
            // Validate the project info.
            if (projectInfo == null)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.ProjectInfoMissing);
            }
            projectInfo.Validate();

            // Check the project name is valid and doesn't already exist.
            if (string.IsNullOrWhiteSpace(vsnProjectId))
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VSNProjectIdInvalid);
            }

            // See if there is an existing project
            var project = await GetProjectAsync(vendorId, vsnProjectId);
            if(project != null)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VsnProjectIdAlreadyExists);
            }

            // Create the project.
            project = new Project
            {
                VendorId = vendorId,
                Id = Guid.NewGuid(),
                Data = new Dictionary<string, object?>
                {
                    { Constants.VsnProjectId, vsnProjectId },
                    { Constants.VsnProjectDescription, projectInfo.Description },
                    { Constants.VsnProjectName, projectInfo.Name },
                    { Constants.VsnProjectDate, projectInfo.ProjectDate },
                    { Constants.VsnCreated, DateTime.UtcNow },
                    { Constants.VsnIsDeleted, Constants.VsnActiveFlag },
                    { Constants.VsnLastModified, DateTime.UtcNow }
                }
            };
            await dbManager.CreateAsync(project);
            return project;
        }



        /// <summary>
        /// Updates a vendor project.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectInfo"></param>
        /// <returns></returns>
        public async Task<Project> UpdateProjectAsync(Guid projectId, ProjectInfo? projectInfo)
        {
            // Validate the project info.
            if (projectInfo == null)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.ProjectInfoMissing);
            }
            projectInfo.Validate();

            // Check that the project name is unique (within the vendor)
            var existingProject = await GetProjectAsync(projectId);
            if (existingProject.Data != null)
            {
                existingProject.Data[Constants.VsnProjectDate] = projectInfo.ProjectDate;
                existingProject.Data[Constants.VsnProjectName] = projectInfo.Name;
                existingProject.Data[Constants.VsnProjectDescription] = projectInfo.Description;
                existingProject.Data[Constants.VsnLastModified] = DateTime.UtcNow;
            }

            await dbManager.UpdateAsync(existingProject);
            return existingProject;
        }

        /// <summary>
        /// Deletes 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="hardDelete"></param>
        /// <returns></returns>
        public async Task DeleteProjectAsync(Guid projectId, bool hardDelete = false)
        {
            // Check that the project name is unique (within the vendor)
            var existingProject = await GetProjectAsync(projectId);

            if(hardDelete)
            {

                await dbManager.DeleteAsync(existingProject);

            }
            else
            {
                if (existingProject.Data != null)
                {
                    existingProject.Data[Constants.VsnIsDeleted] = Constants.VsnDeletedFlag;
                }
                await dbManager.UpdateAsync(existingProject);
            }
        }

        /// <summary>
        /// Gets a project from the database.
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public async Task<Project> GetProjectAsync(Guid projectId)
        {
            var project = await dbManager.GetAsync<Project>(projectId);
            return project ?? throw new FieldPlotServiceException(FieldPlotServiceError.ProjectNotFound);
        }

        /// <summary>
        /// Gets a project from the database.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="vsnProjectId"></param>
        /// <returns></returns>
        public async Task<Project?> GetProjectAsync(Guid vendorId, string? vsnProjectId)
        {
            if(string.IsNullOrWhiteSpace(vsnProjectId))
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VSNProjectIdInvalid);
            }

            var results = await dbManager.QueryAsync<Project>(
                $"select * from {nameof(Project)} p where p.deleted = null and p.{nameof(IVendorEntity.VendorId)} = @vendorId and p.{nameof(Project.Data)}.{Constants.VsnProjectId} = @vsnProjectId",
                new Dictionary<string, object>
                {
                    { "@vendorId", vendorId },
                    { "@vsnProjectId", vsnProjectId }
                });

            var project = results.FirstOrDefault();
            return project;
        }

        /// <summary>
        /// Searches for projects.
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="vendorId"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public async Task<List<Project>> SearchProjectsAsync(string? keyword, Guid? vendorId, bool includeDeleted)
        {
            var query = $"select * from {nameof(Project)} p where p.deleted = null ";
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query += $" and (  Contains(p.{nameof(Project.Data)}.{Constants.VsnProjectName}, @keyword, true) ";
                query += $" or Contains(p.{nameof(Project.Data)}.{Constants.VsnProjectId}, @keyword, true) ) ";
                parameters.Add("@keyword", keyword);
            }

            if (vendorId != null)
            {
                query += $" and p.{nameof(IVendorEntity.VendorId)} = @vendorId ";
                parameters.Add("@vendorId", vendorId);
            }

            if(!includeDeleted)
            {
                query += $" and p.{nameof(Project.Data)}.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}' ";
            }
            
            var results = await dbManager.QueryAsync<Project>(query, parameters);
            return results;
        }

        /// <summary>
        /// Gets a plot name using an active VSN Plot Name which should be unique in the system.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="vsnPlotName"></param>
        /// <param name="measureYear"></param>
        /// <param name="measureTypeCode"></param>
        /// <returns></returns>
        async Task<Plot?> GetPlotAsync(Guid vendorId, string? vsnPlotName, int measureYear, string? measureTypeCode)
        {
            var query = $"select * from {nameof(Plot)} p where p.deleted = null and p.Data.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}'";
            query += $" and p.VendorId = @vendorId";
            query += $" and p.Data.{Constants.VsnPlotName} = @vsnPlotName";
            query += $" and p.Data.{Constants.VsnMeasureTypeCode} = @measureTypeCode";
            query += $" and p.Data.{Constants.VsnMeasureYear} = @measureYear";

            var parameters = new Dictionary<string, object>
            {
                { "@vendorId", vendorId },
                { "@vsnPlotName", vsnPlotName ?? string.Empty },
                { "@measureYear", measureYear },
                { "@measureTypeCode", measureTypeCode ?? string.Empty},
            };

            var existingVsnPlots = await dbManager.QueryAsync<Plot>(query, parameters);
                
            return existingVsnPlots.FirstOrDefault();
        }

        /// <summary>
        /// Search for VSN Plots
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="vendorId"></param>
        /// <param name="vsnProjectId"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public async Task<List<Plot>> SearchPlotsAsync(string? keyword, Guid? vendorId, string? vsnProjectId, bool includeDeleted)
        {
            var query = $"select * from {nameof(Plot)} p where p.deleted = null ";
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query += $" and Contains(p.{nameof(Project.Data)}.{Constants.VsnPlotName}, @keyword, true) ";
                parameters.Add("@keyword", keyword);
            }

            if(!string.IsNullOrWhiteSpace(vsnProjectId))
            {
                query += $" and p.{nameof(Project.Data)}.{Constants.VsnProjectId} = @vsnProjectId";
                parameters.Add("@vsnProjectId", vsnProjectId);
            }

            if (vendorId != null)
            {
                query += $" and p.{nameof(IVendorEntity.VendorId)} = @vendorId ";
                parameters.Add("@vendorId", vendorId);
            }

            if (!includeDeleted)
            {
                query += $" and p.{nameof(Project.Data)}.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}' ";
            }

            var results = await dbManager.QueryAsync<Plot>(query, parameters);
            return results;
        }

        /// <summary>
        /// Gets all the plot details.
        /// </summary>
        /// <param name="plotId"></param>
        /// <param name="includedDeleted"></param>
        /// <returns></returns>
        public async Task<PlotDetails> GetPlotDetailsAsync(Guid plotId, bool includedDeleted)
        {
            // Get the plot data first.
            var plot = await dbManager.GetAsync<Plot>(plotId) ?? throw new FieldPlotServiceException(FieldPlotServiceError.PlotNotFound);

            // Get the list of trees for the plot
            var treeDetails = new List<TreeDetail>();
            var trees = await GetVsnDataAsync<Tree>(plot, Constants.VsnPlotId, includedDeleted);
            if (trees != null)
            {
                foreach (var tree in trees)
                {
                    var stemMaps = await GetVsnDataAsync<StemMap>(tree, Constants.VsnTreeId, includedDeleted);
                    var deformities = await GetVsnDataAsync<Deformity>(tree, Constants.VsnTreeId, includedDeleted);
                    treeDetails.Add(new TreeDetail
                    {
                        Tree = tree,
                        StemMaps = stemMaps,
                        Deformities = deformities,
                    });
                }
            }

            // Start to build up the details.
            var details = new PlotDetails
            {
                Plot = plot,
                SmallTrees = await GetVsnDataAsync<SmallTree>(plot, Constants.VsnPlotId, includedDeleted),
                SmallTreeTallies = await GetVsnDataAsync<SmallTreeTally>(plot, Constants.VsnPlotId, includedDeleted),
                Dwds = await GetVsnDataAsync<Dwd>(plot, Constants.VsnPlotId, includedDeleted),
                EcoSites = await GetVsnDataAsync<Ecosite>(plot, Constants.VsnPlotId, includedDeleted),
                Soils = await GetVsnDataAsync<Soil>(plot, Constants.VsnPlotId, includedDeleted),
                People = await GetVsnDataAsync<Person>(plot, Constants.VsnPlotId, includedDeleted),
                Photos = await GetVsnDataAsync<Photo>(plot, Constants.VsnPlotId, includedDeleted),
                Project = (await GetVsnDataAsync<Project>(plot, Constants.VsnPlotId, includedDeleted))?.FirstOrDefault(),
                Vegetations = await GetVsnDataAsync<Vegetation>(plot, Constants.VsnPlotId, includedDeleted),
                VegetationCensuses = await GetVsnDataAsync<VegetationCensus>(plot, Constants.VsnPlotId, includedDeleted),
            };

            return details;
        }

        /// <summary>
        /// Method to get the VSN entity data for a given entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="vsnIdName"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        async Task<List<IVendorEntity>?> GetVsnDataAsync<T>(IVendorEntity entity, string vsnIdName, bool includeDeleted) where T: IVendorEntity
        {
            if(entity.Data == null || entity.GetString(vsnIdName) == null)
            {
                return null;
            }

            var vsnIdValue = entity.GetString(vsnIdName) ?? throw new FieldPlotServiceException(FieldPlotServiceError.TableDataIdMissing, $"The {vsnIdName} is null. for entity {typeof(T).Name} with ID {entity.Id}");

            // Build the query for selecting the entity.
            var query = $"select * from {typeof(T).Name} e where e.deleted = null and e.VendorId = @vendorId";
            query += $" and e.Data.{vsnIdName} = @vsnId";

            var parameters = new Dictionary<string, object>()
            {
                { "@vendorId", entity.VendorId },
                { "@vsnId", vsnIdValue }
            };

            if(!includeDeleted)
            {
                query += $" and e.Data.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}' ";
            }

            var results = await dbManager.QueryAsync<T>(query, parameters, null, DbManager.GetContainerName<T>());
            return results.Select(t=> t as IVendorEntity).ToList();
        }

        /// <summary>
        /// Imports plots into the system.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="vendorId"></param>
        /// <param name="plotList"></param>
        /// <returns></returns>
        public async Task ImportPlotsAsync(Guid projectId, Guid vendorId, List<ImportPlotData> plotList)
        {
            var dataItems = new List<Dictionary<string, object?>>();
            if (plotList != null)
            {
                foreach (var plot in plotList)
                {
                    dataItems.Add(new Dictionary<string, object?>
                    {
                        { Constants.VsnPlotName, plot.PlotName },
                        { Constants.VsnMeasureTypeCode, plot.PlotTypeCode },
                        { Constants.VsnMeasureYear, plot.MeasureYear },
                    });
                }
            }

            await ImportPlotsAsync(projectId, vendorId, dataItems.ToArray());
        }


        /// <summary>
        /// Import a list of VSN plot names into the system for vendors to 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="vendorId"></param>
        /// <param name="plotList"></param>
        /// <returns></returns>
        public async Task ImportPlotsAsync(Guid projectId, Guid vendorId, Dictionary<string, object?>[]? plotList)
        {
            if (plotList == null || plotList.Length == 0)
            {
                return;
            }

            // Ensure the project exists, which also indicates the vendor exists.
            var project = await GetProjectAsync(projectId);

            // Get the list of 'active' VNS plots in the system (all of them)
            var existingVsnPlots = await dbManager.QueryAsync<Plot>(
                $"select * from Plot p where p.deleted = null and p.Data.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}'",
                null, null, DbManager.GetContainerName<Plot>());

            // Iterate through the plots to create.
            foreach (var plotData in plotList)
            {
                // Check if the plot currently exists.
                var plotName = plotData[Constants.VsnPlotName]?.ToString();
                int.TryParse(plotData[Constants.VsnMeasureYear]?.ToString(), null, out int measureYear);
                var measureTypeCode = plotData[Constants.VsnMeasureTypeCode]?.ToString();

                var existingPlot = await GetPlotAsync(vendorId, plotName, measureYear, measureTypeCode);

                // If it doesn't exist, create it.
                if (existingPlot == null)
                {
                    var plot = new Plot
                    {
                        VendorId = vendorId,
                        Id = Guid.NewGuid(),
                        Data = plotData
                    };
                    await dbManager.CreateAsync(plot);
                }
                else
                {
                    existingPlot.Data = plotData;
                    await dbManager.UpdateAsync(existingPlot);
                }
            }

        }

        /// <summary>
        /// Gets the headings for a dictionary object (used for CSV exporting)
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        static List<string> GetHeadings(List<Dictionary<string, object?>> entityList)
        {
            var headings = new List<string>();

            // Iterate through each of the entities to gather all the possible keys
            foreach (var entity in entityList)
            {
                foreach (var key in entity.Keys)
                {
                    if (!headings.Contains(key))
                    {
                        headings.Add(key);
                    }
                }
            }

            return headings;
        }


        /// <summary>
        /// Generates a CSV file (in temp folder) for the data table requested.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="vendorCode"></param>
        /// <param name="lastChanged"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public async Task<string> GenerateCsvFile(string? table, string? vendorCode, DateTime? lastChanged, bool includeDeleted)
        {
            // Get the data from the system.
            var results = await ExportVsnDataAsync(table, vendorCode, lastChanged, includeDeleted);

            var reportFile = Path.Combine(Path.GetTempPath(), $"Report_{Guid.NewGuid()}");

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Mode = CsvMode.Escape,
            };

            using var writer = File.CreateText(reportFile);
            using var csv = new CsvWriter(writer, csvConfig);

            // Write out the header first.
            var headings = GetHeadings(results);
            foreach(var heading in headings)
            {
                csv.WriteField(heading);
            }
            await csv.NextRecordAsync();

            // Write out each row
            foreach(var item in results)
            {
                foreach (var heading in headings)
                {
                    if(item.TryGetValue(heading, out object? value))
                    {
                        csv.WriteField(value);
                    }
                    else
                    {
                        csv.WriteField(string.Empty);
                    }
                }
                await csv.NextRecordAsync();
            }

            await csv.DisposeAsync();
            writer.Close();
            await writer.DisposeAsync();

            return reportFile;
        }

        /// <summary>
        /// Exports VSN data from the system based on the provided filter criteria.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="vendorCode"></param>
        /// <param name="lastChanged"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, object?>>> ExportVsnDataAsync(string? table, string? vendorCode, DateTime? lastChanged, bool includeDeleted)
        {
            switch (table?.ToUpper())
            {
                case "DEFORMITY": return await ExportVsnDataAsync<Deformity>(vendorCode, lastChanged, includeDeleted);
                case "DWD": return await ExportVsnDataAsync<Dwd>(vendorCode, lastChanged, includeDeleted);
                case "ECOSITE": return await ExportVsnDataAsync<Ecosite>(vendorCode, lastChanged, includeDeleted);
                case "PERSON": return await ExportVsnDataAsync<Person>(vendorCode, lastChanged, includeDeleted);
                case "PHOTO": return await ExportVsnDataAsync<Photo>(vendorCode, lastChanged, includeDeleted);
                case "PROJECT": return await ExportVsnDataAsync<Project>(vendorCode, lastChanged, includeDeleted);
                case "PLOT": return await ExportVsnDataAsync<Plot>(vendorCode, lastChanged, includeDeleted);
                case "SMALLTREE": return await ExportVsnDataAsync<SmallTree>(vendorCode, lastChanged, includeDeleted);
                case "SMALLTREETALLY": return await ExportVsnDataAsync<SmallTreeTally>(vendorCode, lastChanged, includeDeleted);
                case "SOIL": return await ExportVsnDataAsync<Soil>(vendorCode, lastChanged, includeDeleted);
                case "STEMMAP": return await ExportVsnDataAsync<StemMap>(vendorCode, lastChanged, includeDeleted);
                case "TREE": return await ExportVsnDataAsync<Tree>(vendorCode, lastChanged, includeDeleted);
                case "VEGETATION": return await ExportVsnDataAsync<Vegetation>(vendorCode, lastChanged, includeDeleted);
                case "VEGETATIONCENSUS": return await ExportVsnDataAsync<VegetationCensus>(vendorCode, lastChanged, includeDeleted);
                default:
                    throw new FieldPlotServiceException(FieldPlotServiceError.TableAccessDenied, $"The table '{table}' is not valid.");
            }
        }

        /// <summary>
        /// Method get the list of entities from the last sync date based on an optional plot ID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vendorCode"></param>
        /// <param name="lastChanged"></param>
        /// <param name="includeDeleted"></param>
        /// <returns>JSON</returns>
        async Task<List<Dictionary<string, object?>>> ExportVsnDataAsync<T>(string? vendorCode, DateTime? lastChanged, bool includeDeleted) where T : EntityBase, IVendorEntity, new()
        {
            // Build the query for selecting the entity.
            var query = $"select value e.Data from {typeof(T).Name} e where e.deleted = null ";
            var parameters = new Dictionary<string, object>();

            if(lastChanged != null)
            {
                query += $" and (e.created > @lastChanged or e.updated > @lastChanged) ";
                parameters.Add("@lastChanged", lastChanged.Value.ToString(DbManager.CosmosDbDateTimeFormat));
            }

            if(vendorCode != null)
            {
                var vendor = await vendorManager.GetVendorAsync(vendorCode) ?? throw new FieldPlotServiceException(FieldPlotServiceError.VendorNotFound);
                query += $" and e.VendorId = @vendorId";
                parameters.Add("@vendorId", vendor.Id);
            }

            if(!includeDeleted)
            {
                query += $" and e.Data.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}' ";
            }

            var results = await dbManager.QueryAsync<Dictionary<string, object?>>(query, parameters, null, DbManager.GetContainerName<T>());
            return results;
        }

        /// <summary>
        /// Gets a count of plots by project.
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, int>> GetPlotsByProjectReportAsync(int? year)
        {
            var report = new Dictionary<string, int>();

            // Count the plots per project
            var query = $"select {{ '{Constants.VsnProjectId}' : p.Data.{Constants.VsnProjectId}, 'Total' : count(1) }} from Plot p where p.Data.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}' ";
            if(year != null)
            {
                query += $" and p.Data.{Constants.VsnMeasureYear} = {year}";
            }
            query += $" group by p.Data.{Constants.VsnProjectId}";
            var projectTotals = await dbManager.QueryAsync<Dictionary<string, object?>>(query, null, null, DbManager.GetContainerName<Plot>());

            // Get the list of project (so we can update the names)
            var projects = await SearchProjectsAsync(null, null, false);
            foreach(var projectTotal in projectTotals)
            {
                var projectId = projectTotal[Constants.VsnProjectId]?.ToString();
                var project = projects.Find(p=> p.GetString(Constants.VsnProjectId) == projectId);
                report.Add(project?.GetString(Constants.VsnProjectName) ?? "Unknown Project", (int)(projectTotal["Total"] ?? 0));
            }

            return report;
        }

        /// <summary>
        /// Gets a list of plots by vendor.
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, int>> GetPlotsByVendorReportAsync(int? year)
        {
            var report = new Dictionary<string, int>();

            // Count the plots per project
            var query = $"select {{ VendorId : p.VendorId, 'Total' : count(1) }} from Plot p where p.Data.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}' ";
            if (year != null)
            {
                query += $" and p.Data.{Constants.VsnMeasureYear} = {year}";
            }
            query += $" group by p.VendorId";
            var vendorTotals = await dbManager.QueryAsync<Dictionary<string, object?>>(query, null, null, DbManager.GetContainerName<Plot>());

            // Get the list of vendors (so we can update the names)
            var vendors = await vendorManager.SearchVendorsAsync(null, null);
            foreach (var vendorTotal in vendorTotals)
            {
                var vendorId = Guid.Parse(vendorTotal[Constants.VsnProjectId]?.ToString() ?? Guid.Empty.ToString());
                var project = vendors.Find(p => p.Id == vendorId);
                report.Add(project?.Info?.Name ?? "Unknown Vendor", (int)(vendorTotal["Total"] ?? 0));
            }

            return report;
        }

        /// <summary>
        /// Gets a dashboard view of the current progress in the system.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="projectId"></param>
        /// <param name="year"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<DashboardData> GetDashboardDataAsync(Guid? vendorId,  Guid? projectId, int? year, DateTime? startDate, DateTime? endDate)
        {
            // Get a list of vendors and projects to do fast lookups.
            var vendors = await dbManager.QueryAsync<Vendor>($"select * from {DbManager.GetContainerName<Vendor>()} p where p.deleted = null");
            var projects = await dbManager.QueryAsync<Project>($"select * from {DbManager.GetContainerName<Project>()} p where p.deleted = null");

            // Build the query to collect the plot data.
            var query = $"select * from {DbManager.GetContainerName<Plot>()} p where p.deleted = null and p.{nameof(IVendorEntity.Data)}.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}'";
            var parameters = new Dictionary<string, object>();

            // Add the vendor filter
            if (vendorId != null)
            {
                query += $" and p.{nameof(IVendorEntity.VendorId)} = @vendorId";
                parameters.Add("@vendorId", vendorId.Value);
            }

            // Add the project filter
            if (projectId != null)
            {
                var project = await GetProjectAsync(projectId.Value);
                query += $" and p.{nameof(IVendorEntity.Data)}.{Constants.VsnProjectId} = @vsnProjectId";
                parameters.Add("@vsnProjectId", project.GetString(Constants.VsnProjectId) ?? string.Empty);
            }

            // Add the project year
            if (year != null)
            {
                query += $" and IS_DEFINED(p.{nameof(IVendorEntity.Data)}.{Constants.VsnMeasureYear}) and p.{nameof(IVendorEntity.Data)}.{Constants.VsnMeasureYear} = @year";
                parameters.Add("@year", year);
            }

            if (startDate != null)
            {
                query += $" and IS_DEFINED(p.{nameof(IVendorEntity.Data)}.{Constants.VsnPlotOverviewDate}) and p.{nameof(IVendorEntity.Data)}.{Constants.VsnPlotOverviewDate} >= @startDate";
                parameters.Add("@startDate", startDate.Value.Date.ToString(Constants.VsnDateFormat));
            }

            if (endDate != null)
            {
                query += $" and IS_DEFINED(p.{nameof(IVendorEntity.Data)}.{Constants.VsnPlotOverviewDate}) and p.{nameof(IVendorEntity.Data)}.{Constants.VsnPlotOverviewDate} <= @endDate";
                parameters.Add("@endDate", endDate.Value.Date.ToString(Constants.VsnDateFormat));
            }

            // Execute the query based the filters provided.
            var results = await dbManager.QueryAsync<Plot>(query, parameters);

            // Get the plot summary information
            var plots = results.Select(p => new PlotSummary(p, vendors.Find(v=>v.Id == p.VendorId), projects.Find(j=>j.GetString(Constants.VsnProjectId) == p.GetString(Constants.VsnProjectId))))
                .OrderBy(p => p.PlotOverviewDate)
                .ThenBy(p => p.Status)
                .ToList();

            // Calculate the monthly activity.
            var monthlyActivity = plots.Where(p => p.Status == PlotStatusType.Completed && p.PlotOverviewDate != null)
                .OrderBy(p=>p.PlotOverviewDate)
                .GroupBy(p => p.PlotOverviewDate?.ToString("MM/yy") ?? "(None)", (date, p) => new SimpleMetric(date, p.Count()))
                .ToList();

            return new DashboardData
            {
                MonthlyActivity = monthlyActivity,
                Plots = plots,
                PlotStatus = new PlotMetrics(
                    plots.Count(p => p.Status == PlotStatusType.Completed),
                    plots.Count(p => p.Status == PlotStatusType.NoStarted),
                    plots.Count(p => p.Status == PlotStatusType.InProgress)
                ),
                ProjectId = projectId,
                VendorId = vendorId,
                Year = year,
            };

        }


        /// <summary>
        /// Gets a dashboard view of the current progress in the system.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="projectId"></param>
        /// <param name="year"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<string> GetDashboardPlotReportAsync(Guid? vendorId, Guid? projectId, int? year, DateTime? startDate, DateTime? endDate)
        {
            // Execute the query based the filters provided.
            var data = await GetDashboardDataAsync(vendorId, projectId, year, startDate, endDate);

            var reportFile = Path.Combine(Path.GetTempPath(), $"Report_{Guid.NewGuid()}");

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Mode = CsvMode.Escape,
            };

            using var writer = File.CreateText(reportFile);
            using var csv = new CsvWriter(writer, csvConfig);

            // Write out the header first.
            csv.WriteHeader<PlotSummary>();
            await csv.NextRecordAsync();

            // Write out each row
            if (data.Plots != null)
            {
                foreach (var plot in data.Plots)
                {
                    csv.WriteRecord(plot);
                    await csv.NextRecordAsync();
                }
            }

            await csv.DisposeAsync();
            writer.Close();
            await writer.DisposeAsync();

            return reportFile;
        }

    }
}
