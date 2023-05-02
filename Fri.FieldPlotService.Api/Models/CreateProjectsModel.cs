using Fri.FieldPlotService.Entities;

namespace Fri.FieldPlotService.Api.Models
{
    /// <summary>
    /// Model used to create/update a project for multiple vendors.
    /// </summary>
    public class CreateProjectsModel
    {
        /// <summary>
        /// List of vendors to ensure this project exists for.
        /// </summary>
        public Guid[]? VendorIds { get; set; }

        /// <summary>
        /// Project information.
        /// </summary>
        public ProjectInfo? Info { get; set; }

        /// <summary>
        /// The VSN Project Id.
        /// </summary>
        public string? VsnProjectId { get; set; }
    }
}
