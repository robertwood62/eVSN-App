using Fri.FieldPlotService.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// Contains information about the synchronization request.
    /// </summary>
    public class SyncRequest
    {
        /// <summary>
        /// Table being synchronized.
        /// </summary>
        public string? Table { get; set; }

        /// <summary>
        /// Plot to synchronize.
        /// </summary>
        public string? PlotId { get; set; }

        /// <summary>
        /// Last sync time information.
        /// </summary>
        public DateTime? LastSyncTime { get; set; }

        /// <summary>
        /// Device Id.
        /// </summary>
        public Guid? DeviceId { get; set; }

        /// <summary>
        /// Device type.
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// Overload to display last sync time information.
        /// </summary>
        public override string ToString()
        {
            return $"Table:{Table}, Plot:{PlotId}, LastSync:{LastSyncTime}";
        }
    }
}
