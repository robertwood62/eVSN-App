using Fri.FieldPlotService.Common;
using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Tools;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


namespace Fri.FieldPlotService.Managers
{
    /// <summary>
    /// The Sync manager provides an interface to insert and update VSN data collected by the app.
    /// </summary>
    public class SyncManager
    {
        readonly DbManager dbManager;
        readonly VendorManager vendorManager;
        readonly FieldPlotServiceOptions options;
        readonly ILogger<SyncManager>? logger;

        /// <summary>
        /// Creates the SyncManager with dependencies.
        /// </summary>
        /// <param name="dbManager"></param>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <param name="vendorManager"></param>
        public SyncManager(DbManager dbManager, ILogger<SyncManager>? logger, FieldPlotServiceOptions options, VendorManager vendorManager)
        {
            this.dbManager = dbManager;
            this.vendorManager = vendorManager;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Signs a user into the system and generates a user token.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<UserTokenInfo> SignInAsync(string? username, string? password)
        {
            logger?.LogInformation($"Sign-in for {username}");

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new UnauthorizedAccessException();
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new UnauthorizedAccessException();
            }

            // Get user by username.
            var user = await vendorManager.GetVendorUserAsync(username) ?? throw new UnauthorizedAccessException();

            // Validate the password
            var passwordHash = Passwords.GetPasswordHash(password.Trim(), user.PasswordSalt);
            if(passwordHash != user.PasswordHash)
            {
                throw new UnauthorizedAccessException();
            }

            // Verify the user active.
            switch(user?.Info?.Status)
            {
                case UserStatusType.Active:
                    break;
                default:
                    throw new FieldPlotServiceException(FieldPlotServiceError.VendorUserBlocked);
            }

            // Get the vendor information to ensure the vendor isn't blocked
            var vendor = await dbManager.GetAsync<Vendor>(user.VendorId);
            switch(vendor?.Info?.Status)
            {
                case VendorStatusType.Active:
                    break;
                default:
                    throw new FieldPlotServiceException(FieldPlotServiceError.VendorBlocked);
            }

            // Update the database with the last sign-in
            user.LastLogin = DateTime.UtcNow;
            await dbManager.UpdateAsync(user);

            var token = new UserToken
            {
                UserId = user.Id,
                Username = user.Info.Username,
                VendorId = user.VendorId
            };

            logger?.LogInformation($"Sign-in completed for {username}, {user.Id}");

            return new UserTokenInfo(token.ToEncryptedString(options.EncryptionKey), username, user.Id, user.VendorId);
        }

        /// <summary>
        /// Authorizes the user by checking they exist, are active and associated with the vendor that's active.
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        /// <exception cref="FieldPlotServiceException"></exception>
        public async Task<VendorUser> AuthorizeVendorUserAsync(UserToken userToken)
        {
            logger?.LogInformation($"Checking authorization for {userToken.Username} - {userToken.UserId}");

            // Verify the user active.
            var user = await dbManager.GetAsync<VendorUser>(userToken.UserId);
            switch (user?.Info?.Status)
            {
                case UserStatusType.Active:
                    break;
                default:
                    throw new FieldPlotServiceException(FieldPlotServiceError.VendorUserBlocked);
            }

            // Get the vendor information to ensure the vendor isn't blocked
            var vendor = await dbManager.GetAsync<Vendor>(user.VendorId);
            switch (vendor?.Info?.Status)
            {
                case VendorStatusType.Active:
                    break;
                default:
                    throw new FieldPlotServiceException(FieldPlotServiceError.VendorBlocked);
            }

            return user;
        }

        /// <summary>
        /// Adds a new synchronization entry to the log table.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="userToken"></param>
        /// <param name="syncRequest"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        async Task LogSynchronizationAsync(SyncActionType action, UserToken userToken, SyncRequest syncRequest, IEnumerable<Dictionary<string, object?>> records)
        {
            var syncLog = new SyncLog
            {
                Action = action,
                Request = syncRequest,
                Username = userToken.Username,
                VendorId = userToken.VendorId,
                VendorUserId = userToken.UserId,
                Records = records.ToArray(),
                Id = Guid.NewGuid(),
            };

            await dbManager.CreateAsync(syncLog);
        }

        /// <summary>
        /// Gets a list of entities based on the last Sync date and plot ID (optional).
        /// </summary>
        /// <param name="userToken"></param>
        /// <param name="syncRequest"></param>
        /// <returns>JSON string</returns>
        public async Task<List<Dictionary<string, object?>>> GetEntitiesAsync(UserToken userToken, SyncRequest syncRequest)
        {
            // Validate the user and vendor information.
            await AuthorizeVendorUserAsync(userToken);

            logger?.LogInformation($"Processing GetEntitiesAsync for {userToken.Username} - {syncRequest}");

            switch (syncRequest.Table?.ToUpper())
            {
                case "DEFORMITY": return await GetEntitiesAsync<Deformity>(userToken, syncRequest);
                case "DWD": return await GetEntitiesAsync<Dwd>(userToken, syncRequest);
                case "ECOSITE": return await GetEntitiesAsync<Ecosite>(userToken, syncRequest);
                case "PERSON": return await GetEntitiesAsync<Person>(userToken, syncRequest);
                case "PHOTO": return await GetEntitiesAsync<Photo>(userToken, syncRequest);
                case "PROJECT": return await GetEntitiesAsync<Project>(userToken, syncRequest);
                case "PLOT": return await GetEntitiesAsync<Plot>(userToken, syncRequest);
                case "SMALLTREE": return await GetEntitiesAsync<SmallTree>(userToken, syncRequest);
                case "SMALLTREETALLY": return await GetEntitiesAsync<SmallTreeTally>(userToken, syncRequest);
                case "SOIL": return await GetEntitiesAsync<Soil>(userToken, syncRequest);
                case "STEMMAP": return await GetEntitiesAsync<StemMap>(userToken, syncRequest);
                case "TREE": return await GetEntitiesAsync<Tree>(userToken, syncRequest);
                case "VEGETATION": return await GetEntitiesAsync<Vegetation>(userToken, syncRequest);
                case "VEGETATIONCENSUS": return await GetEntitiesAsync<VegetationCensus>(userToken, syncRequest);
                default:
                    throw new FieldPlotServiceException(FieldPlotServiceError.TableAccessDenied, $"The table '{syncRequest.Table}' is not valid.");
            }
        }

        /// <summary>
        /// Method get the list of entities from the last sync date based on an optional plot ID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userToken"></param>
        /// <param name="syncRequest"></param>
        /// <returns>JSON</returns>
        async Task<List<Dictionary<string, object?>>> GetEntitiesAsync<T>(UserToken userToken, SyncRequest syncRequest) where T : EntityBase, IVendorEntity, new()
        {
            // Build the query for selecting the entity.
            var query = $"select value e.Data from {typeof(T).Name} e where e.deleted = null and e.{nameof(IVendorEntity.VendorId)} = @vendorId ";
            var parameters = new Dictionary<string, object>
            {
                { "@vendorId", userToken.VendorId },
            };

            // Apply the last sync date filter.
            if (syncRequest.LastSyncTime != null)
            {
                query += $" and e.created > @lastSyncTime and e.updated > @lastSyncTime ";
                parameters.Add("@lastSyncTime", syncRequest.LastSyncTime.Value.ToString(DbManager.CosmosDbDateTimeFormat));
            }

            // Apply the plot filter if provided
            if (!string.IsNullOrWhiteSpace(syncRequest.PlotId))
            {
                query += $" and e.PLOTID = @plotId ";
                parameters.Add("@plotId", syncRequest.PlotId.Trim());
            }

            // Get the list of results and return as JSON (This just get's the DATA element)
            var results = await dbManager.QueryAsync<Dictionary<string, object?>>(query, parameters, null, DbManager.GetContainerName<T>());

            // Log the synchronization.
            await LogSynchronizationAsync(SyncActionType.Read, userToken, syncRequest, results);

            return results;
        }

        /// <summary>
        /// Method to determine the class types for each table.
        /// </summary>
        /// <param name="userToken"></param>
        /// <param name="syncRequest"></param>
        /// <param name="dataItems"></param>
        /// <param name="insertOnly">True means only insert, false means update, null indicates merge.</param>
        /// <param name="checkVsnPlotName"></param>
        public async Task SyncEntitiesAsync(UserToken userToken, SyncRequest syncRequest, List<Dictionary<string, object?>>? dataItems, bool? insertOnly, bool checkVsnPlotName)
        {
            // Validate the user and vendor information.
            await AuthorizeVendorUserAsync(userToken);

            logger?.LogInformation($"Processing SyncEntitiesAsync for {userToken.Username} - {syncRequest}");

            switch (syncRequest.Table?.ToUpper())
            {
                case "DEFORMITY": await SyncEntitiesAsync<Deformity>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "DWD": await SyncEntitiesAsync<Dwd>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "ECOSITE": await SyncEntitiesAsync <Ecosite>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "PERSON": await SyncEntitiesAsync<Person>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "PHOTO": await SyncEntitiesAsync<Photo>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "PROJECT": await SyncEntitiesAsync<Project>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "PLOT": await SyncEntitiesAsync<Plot>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "SMALLTREE": await SyncEntitiesAsync<SmallTree>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "SMALLTREETALLY": await SyncEntitiesAsync<SmallTreeTally>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "SOIL": await SyncEntitiesAsync<Soil>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "STEMMAP": await SyncEntitiesAsync<StemMap>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "TREE": await SyncEntitiesAsync<Tree>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "VEGETATION": await SyncEntitiesAsync<Vegetation>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                case "VEGETATIONCENSUS": await SyncEntitiesAsync<VegetationCensus>(userToken, syncRequest, dataItems, insertOnly, checkVsnPlotName); break;
                default:
                    throw new FieldPlotServiceException(FieldPlotServiceError.TableAccessDenied, $"The table {syncRequest.Table} is not valid.");
            }
        }

        /// <summary>
        /// Main processing method to synchronize data from the client app with the backend.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userToken"></param>
        /// <param name="syncRequest"></param>
        /// <param name="dataItems"></param>
        /// <param name="insertOnly">True means only insert, false means update, null indicates merge.</param>
        /// <param name="checkVsnPlotName"></param>
        /// <returns></returns>
        /// <exception cref="FieldPlotServiceException"></exception>
        async Task SyncEntitiesAsync<T>(UserToken userToken, SyncRequest syncRequest, List<Dictionary<string, object?>>? dataItems, bool? insertOnly, bool checkVsnPlotName) where T: EntityBase, IVendorEntity, new()
        {
            logger?.LogInformation($"Processing SyncEntitiesAsync for {userToken.Username} - {syncRequest.Table} - {dataItems?.Count} items.");

            if (dataItems != null && dataItems.Count > 0)
            {
                // Get the <entity>ID of the record
                var dataIdName = syncRequest.Table?.ToUpper() + "ID";

                // Iterate through each of the items to insert.
                foreach (var dataItem in dataItems)
                {
                    if(dataItem == null)
                    {
                        continue;
                    }

                    // Validate the entity type
                    VsnValidator.Validate<T>(dataItem);

                    // ----------------------------------
                    // Check for duplicate VSN Plot Names
                    // ----------------------------------
                    if (syncRequest.Table?.ToUpper() == "PLOT")
                    {
                        try
                        {
                            await CheckVsnPlotNameAsync(userToken, dataItem);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex.Message);
                            if(checkVsnPlotName)
                            {
                                throw;
                            }
                        }
                    }

                    // Remove any invalid columns (legacy data)
                    dataItem.Remove("@odata.etag");
                    dataItem.Remove("ItemInternalId");
                    dataItem.Remove("ID");
                    dataItem.Remove("CreatedAtServer");
                    dataItem.Remove("LastModifiedAtServer");

                    // Get the item's ID (which should be the called <table name>ID.
                    var dataId = dataItem[dataIdName]?.ToString()
                        ?? throw new FieldPlotServiceException(FieldPlotServiceError.TableDataIdMissing, $"The data entity does not contain a {dataIdName} property.");

                    // Get the existing record (if any)
                    var existingEntities = await dbManager.QueryAsync<T>(
                        $"select * from {typeof(T).Name} e where e.deleted = null and e.{nameof(IVendorEntity.VendorId)} = @vendorId and STRINGEQUALS(e.Data.{dataIdName}, @dataId, true)",
                        new Dictionary<string, object>
                        {
                        { "@vendorId", userToken.VendorId },
                        { "@dataId", dataId }
                        });
                    var existingEntity = existingEntities.FirstOrDefault();

                    
                    if(insertOnly == null)
                    {
                        // Merge the new data (insert or update)
                        if (existingEntity != null)
                        {
                            // Update the existing record.
                            existingEntity.Data = dataItem;
                            await dbManager.UpdateAsync<T>(existingEntity);
                        }
                        else
                        {
                            // Insert the new record.
                            var entity = new T
                            {
                                Id = Guid.NewGuid(),
                                VendorId = userToken.VendorId,
                                Data = dataItem
                            };
                            await dbManager.CreateAsync<T>(entity);
                        }
                    }
                    else if (insertOnly.Value)
                    {
                        // Verify the record doesn't already exist.
                        if (existingEntity != null)
                        {
                            throw new FieldPlotServiceException(FieldPlotServiceError.TableDataExists, $"The {syncRequest.Table} with {dataIdName} of '{dataId}' already exists.");
                        }

                        // Insert the new record.
                        var entity = new T
                        {
                            Id = Guid.NewGuid(),
                            VendorId = userToken.VendorId,
                            Data = dataItem
                        };
                        await dbManager.CreateAsync<T>(entity);
                    }
                    else
                    {
                        // Verify the record does exist already.
                        if (existingEntity == null)
                        {
                            throw new FieldPlotServiceException(FieldPlotServiceError.TableDataNotFound, $"The {syncRequest.Table} with {dataIdName} of '{dataId}' does not exist.");
                        }

                        // Update the existing record.
                        existingEntity.Data = dataItem;
                        await dbManager.UpdateAsync<T>(existingEntity);
                    }
                }

                // Log the synchronization.
                await LogSynchronizationAsync((insertOnly ?? false) ? SyncActionType.Insert : SyncActionType.Update, userToken, syncRequest, dataItems);
            }
        }

        /// <summary>
        /// We need to ensure that the VSN Plot names unique system-wide (regardless of vendor and project).
        /// </summary>
        /// <returns></returns>
        async Task CheckVsnPlotNameAsync(UserToken userToken, Dictionary<string, object?> dataItem)
        {
            var vsnPlotId = dataItem[Constants.VsnPlotId]?.ToString();
            var vsnPlotName = dataItem[Constants.VsnPlotName]?.ToString();
            var measureType = dataItem[Constants.VsnMeasureTypeCode]?.ToString();
            var measureYear = dataItem[Constants.VsnMeasureYear]?.ToString();
            if (vsnPlotId == null || vsnPlotName == null || measureType == null || measureYear == null)
            {
                return;
            }

            // Get the existing record (if any)
            var existingEntities = await dbManager.QueryAsync<Plot>(
                $"select * from {typeof(Plot).Name} e where e.deleted = null and STRINGEQUALS(e.Data.{Constants.VsnPlotName}, @vsnPlotName, true) and STRINGEQUALS(e.Data.{Constants.VsnMeasureTypeCode}, @measureType, true) and e.Data.{Constants.VsnMeasureYear} = @measureYear and e.Data.{Constants.VsnIsDeleted} = '{Constants.VsnActiveFlag}'",
                new Dictionary<string, object>
                {
                    { "@vsnPlotName", vsnPlotName },
                    { "@measureType", measureType },
                    { "@measureYear", measureYear }
                });
            var existingEntity = existingEntities.FirstOrDefault();
            if(existingEntity != null)
            {
                if(userToken.VendorId != existingEntity.VendorId)
                {
                    throw new FieldPlotServiceException(FieldPlotServiceError.VSNPlotNameAlreadyExists, $"Vendor ID '{existingEntity.VendorId}' already has an active VSN plot name {vsnPlotName}, {measureType}, {measureYear} in the system.");
                }
                var existingVsnPlotId = existingEntity.GetString(Constants.VsnPlotId);
                if (vsnPlotId != existingVsnPlotId)
                {
                    throw new FieldPlotServiceException(FieldPlotServiceError.VSNPlotNameAlreadyExists, $"VSN Plot ID'{existingVsnPlotId}' already has an active VSN plot name {vsnPlotName}, {measureType}, {measureYear} in the system.");
                }
            }
        }
    }
}
