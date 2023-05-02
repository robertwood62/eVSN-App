using Fri.FieldPlotService.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// User status information.
    /// </summary>
    public enum UserStatusType
    {
        /// <summary>
        /// User is active (can login)
        /// </summary>
        Active,

        /// <summary>
        /// User is blocked from logging in or synchronizing data.
        /// </summary>
        Blocked,
    }

    /// <summary>
    /// User information.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// First name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Username used to sign-in to the application.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// User status (active or blocked).
        /// </summary>
        public UserStatusType Status { get; set; }

        /// <summary>
        /// Validates the vendor information.
        /// </summary>
        /// <exception cref="FieldPlotServiceException"></exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.UsernameInvalid);
            }
        }

    }

    /// <summary>
    /// User entity information.
    /// </summary>
    public class VendorUser : EntityBase
    {
        /// <summary>
        /// Vendor this user is associated with.
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// User information.
        /// </summary>
        public UserInfo? Info { get; set; }

        /// <summary>
        /// Password hash.
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Salt added to the password.
        /// </summary>
        public string? PasswordSalt { get; set; }

        /// <summary>
        /// User's last login.
        /// </summary>
        public DateTime? LastLogin { get; set; }
    }
}
