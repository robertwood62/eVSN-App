using Fri.FieldPlotService.Entities;
using Fri.FieldPlotService.Tools;
using System.Reflection.Metadata;

namespace Fri.FieldPlotService.Api.Models
{
    /// <summary>
    /// Project model
    /// </summary>
    public class ProjectModel
    {
        /// <summary>
        /// Creates the project model.
        /// </summary>
        /// <param name="project"></param>
        public ProjectModel(Project project)
        {
            ProjectId = project.Id;
            VsnProjectId = project.GetString(Constants.VsnProjectId);
            VendorId = project.VendorId;
            Info = new ProjectInfo
            {
                Description = project.GetString(Constants.VsnProjectDescription),
                Name = project.GetString(Constants.VsnProjectName),
            };
            IsDeleted = project.GetString(Constants.VsnIsDeleted) == Constants.VsnDeletedFlag;
            Info.ProjectDate = project.GetDateTime(Constants.VsnProjectDate) ?? new DateTime(2000, 1, 1);
        }

        /// <summary>
        /// ID in the database.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// VSN Project Id
        /// </summary>
        public string? VsnProjectId { get; set; }

        /// <summary>
        /// True of the project is currently deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Vendor ID
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// Project information.
        /// </summary>
        public ProjectInfo Info { get; set; }
    }
}
