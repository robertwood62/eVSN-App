using Fri.FieldPlotService.Entities;

namespace Fri.FieldPlotService.Api.Models
{
    /// <summary>
    /// Model to create a new user in the system.
    /// </summary>
    public class NewUserModel
    {
        /// <summary>
        /// The password for the user.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// The vendor for this user.
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// The user's information.
        /// </summary>
        public UserInfo? Info { get; set; }
    }
}
