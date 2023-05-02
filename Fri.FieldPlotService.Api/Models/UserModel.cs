using Fri.FieldPlotService.Entities;

namespace Fri.FieldPlotService.Api.Models
{
    /// <summary>
    /// Information about vendor user in the system.
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// Constructor from internal vendor user.
        /// </summary>
        /// <param name="vendorUser"></param>
        public UserModel(VendorUser vendorUser)
        {
            UserId = vendorUser.Id;
            VendorId = vendorUser.VendorId;
            Info = vendorUser.Info;
            LastLogin = vendorUser.LastLogin;
        }
        
        /// <summary>
        /// ID of the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Information on the user's last login (if any).
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// ID of the vendor.
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// Information about the user.
        /// </summary>
        public UserInfo? Info { get; set; }
    }
}
