using Fri.FieldPlotService.Entities;

namespace Fri.FieldPlotService.Api.Models
{
    /// <summary>
    /// Information about a vendor in the system.
    /// </summary>
    public class VendorModel
    {
        /// <summary>
        /// Constructor from internal vendor.
        /// </summary>
        /// <param name="vendor"></param>
        public VendorModel(Vendor vendor)
        {
            VendorId = vendor.Id;
            Info = vendor.Info;
        }

        /// <summary>
        /// ID of the vendor.
        /// </summary>
        public Guid VendorId {get; set;}

        /// <summary>
        /// Information about the vendor.
        /// </summary>
        public VendorInfo? Info { get; set; }

    }
}
