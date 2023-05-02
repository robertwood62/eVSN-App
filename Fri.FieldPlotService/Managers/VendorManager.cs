using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Managers
{
    /// <summary>
    /// Provides an interface to manage vendors, users and projects.
    /// </summary>
    public class VendorManager
    {
        readonly DbManager dbManager;

        /// <summary>
        /// Creates the VendorManager with dependencies.
        /// </summary>
        /// <param name="dbManager"></param>
        public VendorManager(DbManager dbManager)
        {
            this.dbManager = dbManager;
        }

        /// <summary>
        /// Creates a new vendor in the system.
        /// </summary>
        /// <param name="vendorInfo"></param>
        /// <returns></returns>
        public async Task<Vendor> CreateVendorAsync(VendorInfo vendorInfo)
        {
            // Validates the required field information.
            vendorInfo.Validate();

            // Check the vendor doesn't already exist.
            var existingVendor = await GetVendorAsync(vendorInfo?.Code);
            if(existingVendor != null)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VendorAlreadyExists);
            }

            // Create the vendor in the DB.
            var vendor = new Vendor
            {
                Id = Guid.NewGuid(),
                Info = vendorInfo
            };
            await dbManager.CreateAsync(vendor);

            return vendor;
        }

        /// <summary>
        /// Updates the vendor information.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="vendorInfo"></param>
        /// <returns></returns>
        public async Task<Vendor> UpdateVendorInfoAsync(Guid vendorId, VendorInfo vendorInfo)
        {
            // Validates the required field information.
            vendorInfo.Validate();

            // Get the vendor from the database.
            var vendor = await dbManager.GetAsync<Vendor>(vendorId) ?? throw new FieldPlotServiceException(FieldPlotServiceError.VendorNotFound);

            // Check the vendor doesn't already exist (and not the same vendor being edited).
            var existingVendor = await GetVendorAsync(vendorInfo?.Code);
            if (existingVendor != null && vendor.Id != existingVendor.Id)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VendorAlreadyExists);
            }

            // Update the vendor information.
            vendor.Info = vendorInfo;
            await dbManager.UpdateAsync(vendor);
            return vendor;
        }

        /// <summary>
        /// Gets a vendor from the database.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        /// <exception cref="FieldPlotServiceException"></exception>
        public async Task<Vendor> GetVendorAsync(Guid vendorId)
        {
            // Get the vendor from the database.
            var vendor = await dbManager.GetAsync<Vendor>(vendorId) ?? throw new FieldPlotServiceException(FieldPlotServiceError.VendorNotFound);
            return vendor;
        }

        /// <summary>
        /// Gets a vendor by code.
        /// </summary>
        /// <param name="vendorCode"></param>
        /// <returns></returns>
        /// <exception cref="FieldPlotServiceException"></exception>
        public async Task<Vendor?> GetVendorAsync(string? vendorCode)
        {
            if(string.IsNullOrWhiteSpace(vendorCode))
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VendorCodeInvalid);
            }

            // Get the vendor from the database.
            var vendors = await dbManager.QueryAsync<Vendor>(
                $"select * from {nameof(Vendor)} v where v.deleted = null and v.{nameof(Vendor.Info)}.{nameof(VendorInfo.Code)} = @vendorCode",
                new Dictionary<string, object>
                {
                    { "@vendorCode", vendorCode.Trim() }
                });

            var vendor = vendors.FirstOrDefault();
            return vendor;
        }

        /// <summary>
        /// Deletes a vendor from the system.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        public async Task DeleteVendorAsync(Guid vendorId)
        {
            // Get the vendor first
            var vendor = await GetVendorAsync(vendorId);

            // Prevent deletion of the vendor has data already
            var projects = await dbManager.QueryAsync<Project>($"select * from {nameof(Project)} p where p.deleted = null and p.{nameof(IVendorEntity.VendorId)} = '{vendor.Id}'");
            if(projects.Count != 0)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VendorDeleteFailed, $"Vendor {vendorId} currently has {projects.Count} projects.");
            }

            // Get the list of vendor users
            var users = await SearchVendorUsersAsync(null, vendorId, null);
            foreach(var user in users)
            {
                await dbManager.DeleteAsync(user);
            }

            await dbManager.DeleteAsync(vendor);
        }

        /// <summary>
        /// Searches for vendors in the system using a keyword.
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<List<Vendor>> SearchVendorsAsync(string? keyword, VendorStatusType? status)
        {
            var query = $"select * from {nameof(Vendor)} v where v.deleted = null";
            var parameters = new Dictionary<string, object>();

            // If provided, filter by keyword.
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query += $" and ( CONTAINS(v.{nameof(Vendor.Info)}.{nameof(VendorInfo.Name)}, @keyword, true) or CONTAINS(v.{nameof(Vendor.Info)}.{nameof(VendorInfo.Code)}, @keyword, true))";
                parameters.Add("@keyword", keyword);
            }

            // If provided, filter by status.
            if(status != null)
            {
                query += $" and v.{nameof(Vendor.Info)}.{nameof(VendorInfo.Status)} = @status";
                parameters.Add("@status", (int)status);
            }

            return await dbManager.QueryAsync<Vendor>(query, parameters);
        }


        /// <summary>
        /// Search for vendor users in the system.
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="vendorId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<List<VendorUser>> SearchVendorUsersAsync(string? keyword, Guid? vendorId, UserStatusType? status)
        {
            var query = $"select * from {nameof(VendorUser)} v where v.deleted = null";
            var parameters = new Dictionary<string, object>();

            // If provided, filter by keyword.
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query += $" and CONTAINS(v.{nameof(VendorUser.Info)}.{nameof(UserInfo.Username)}, @keyword, true)";
                parameters.Add("@keyword", keyword);
            }

            // If provided, filter by vendorId.
            if (vendorId != null)
            {
                query += $" and v.{nameof(VendorUser.VendorId)} = @vendor";
                parameters.Add("@vendor", vendorId);
            }

            // If provided, filter by status.
            if (status != null)
            {
                query += $" and v.{nameof(VendorUser.Info)}.{nameof(UserInfo.Status)} = @status";
                parameters.Add("@status", (int)status);
            }

            return await dbManager.QueryAsync<VendorUser>(query, parameters);
        }

        /// <summary>
        /// Gets a vendor user by username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>Null if the username is not found.</returns>
        public async Task<VendorUser?> GetVendorUserAsync(string? username)
        {
            if(string.IsNullOrWhiteSpace(username))
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.UsernameInvalid);
            }

            // Get the list of username (should be just one).
            var users = await dbManager.QueryAsync<VendorUser>(
                $"select * from {nameof(VendorUser)} v where v.deleted = null and STRINGEQUALS(v.{nameof(VendorUser.Info)}.{nameof(UserInfo.Username)}, @username, true)",
                new Dictionary<string, object>
                {
                    {"@username", username.Trim()},
                });
            var user = users.FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Creates a new vendor user in the system.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="userInfo"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="FieldPlotServiceException"></exception>
        public async Task<VendorUser> CreateVendorUserAsync(Guid vendorId, UserInfo? userInfo, string? password)
        {
            // Validate the information
            if(userInfo == null)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.UserInfoMissing);
            }
            if(string.IsNullOrWhiteSpace(password))
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.PasswordMissing);
            }
            userInfo.Validate();

            // Get the vendor from the database.
            _ = await dbManager.GetAsync<Vendor>(vendorId) ?? throw new FieldPlotServiceException(FieldPlotServiceError.VendorNotFound);

            // Verify the username is unique.
            var vendorUser = await GetVendorUserAsync(userInfo.Username);
            if(vendorUser != null)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.UsernameExists);
            }

            // Create the password hash with some salt.
            var passwordSalt = Guid.NewGuid().ToString("N");
            var passwordHash = Passwords.GetPasswordHash(password, passwordSalt);

            // Create the new vendor user.
            vendorUser = new VendorUser
            {
                Info = userInfo,
                VendorId = vendorId,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                LastLogin = null,
            };

            await dbManager.CreateAsync(vendorUser);
            return vendorUser;
        }

        /// <summary>
        /// Updates an existing vendor user's information.
        /// </summary>
        /// <param name="vendorUserId"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public async Task<VendorUser> UpdateVendorUserInfoAsync(Guid vendorUserId, UserInfo userInfo)
        {
            userInfo.Validate();

            // Get the vendor user
            var vendorUser = await dbManager.GetAsync<VendorUser>(vendorUserId) ?? throw new FieldPlotServiceException(FieldPlotServiceError.VendorUserNotFound);

            // Check if the username has changed and still unique (ignore if same user)
            var existingUser = await GetVendorUserAsync(userInfo.Username);
            if(existingUser != null && existingUser.Id != vendorUserId)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.UsernameExists);
            }

            // Update the user information.
            vendorUser.Info = userInfo;
            await dbManager.UpdateAsync(vendorUser);
            return vendorUser;
        }

        /// <summary>
        /// Resets the password for a vendor user.
        /// </summary>
        /// <param name="vendorUserId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task ResetVendorUserPasswordAsync(Guid vendorUserId, string password)
        {
            var vendorUser = await dbManager.GetAsync<VendorUser>(vendorUserId) ?? throw new FieldPlotServiceException(FieldPlotServiceError.VendorUserNotFound);
            
            // Create the password hash with some salt.
            var passwordSalt = Guid.NewGuid().ToString("N");
            var passwordHash = Passwords.GetPasswordHash(password, passwordSalt);

            vendorUser.PasswordHash = passwordHash;
            vendorUser.PasswordSalt = passwordSalt;
            await dbManager.UpdateAsync(vendorUser);
        }

        /// <summary>
        /// Gets the vendor user information.
        /// </summary>
        /// <param name="vendorUserId"></param>
        /// <returns></returns>
        public async Task<VendorUser> GetVendorUserAsync(Guid vendorUserId)
        {
            var vendorUser = await dbManager.GetAsync<VendorUser>(vendorUserId) ?? throw new FieldPlotServiceException(FieldPlotServiceError.VendorUserNotFound);
            return vendorUser;
        }

        /// <summary>
        /// Deletes a vendor user from the database.
        /// </summary>
        /// <param name="vendorUserId"></param>
        /// <returns></returns>
        public async Task DeleteVendorUserAsync(Guid vendorUserId)
        {
            var vendorUser = await dbManager.GetAsync<VendorUser>(vendorUserId) ?? throw new FieldPlotServiceException(FieldPlotServiceError.VendorUserNotFound);
            await dbManager.DeleteAsync(vendorUser);
        }


    }
}
