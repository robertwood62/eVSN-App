using Fri.FieldPlotService.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// Project Information.
    /// </summary>
    public class ProjectInfo
    {
        /// <summary>
        /// Project name.
        /// </summary>
        public string? Name {get; set; }

        /// <summary>
        /// Project description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Project date.
        /// </summary>
        public DateTime ProjectDate { get; set; }

        /// <summary>
        /// Validates the project information
        /// </summary>
        /// <exception cref="FieldPlotServiceException"></exception>
        public void Validate()
        {
            if(string.IsNullOrWhiteSpace(Name))
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VSNProjectNameInvalid);
            }
        }
    }
}
