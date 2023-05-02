using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fri.FieldPlotService.Tools;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// Status of the vendor.
    /// </summary>
    public enum VendorStatusType
    {
        /// <summary>
        /// The vendor is currently active.
        /// </summary>
        Active,

        /// <summary>
        /// The vendor is blocked from using the system.
        /// </summary>
        Blocked,
    }

    /// <summary>
    /// Information about the vendor.
    /// </summary>
    public class VendorInfo
    {
        /// <summary>
        /// Name of the vendor.  This should be a full vendor name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Vendor code (ex: 'kbm')
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// General contact information.
        /// </summary>
        public string? ContactInfo { get; set; }

        /// <summary>
        /// Vendor comment.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Status of the vendor.
        /// </summary>
        public VendorStatusType Status { get; set; }

        /// <summary>
        /// Validates the vendor information.
        /// </summary>
        /// <exception cref="FieldPlotServiceException"></exception>
        public void Validate()
        {
            if(string.IsNullOrWhiteSpace(Name))
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VendorNameInvalid);
            }
            if (string.IsNullOrWhiteSpace(Code))
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VendorCodeInvalid);
            }
        }
    }

    /// <summary>
    /// Vendor entity in the database.
    /// </summary>
    public class Vendor : EntityBase
    {
        /// <summary>
        /// Vendor information.
        /// </summary>
        public VendorInfo? Info { get; set; }
    }
}
