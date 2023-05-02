using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// Defines the type of synchronization action.
    /// </summary>
    public enum SyncActionType
    {
        /// <summary>
        /// Inserted new records.
        /// </summary>
        Insert,

        /// <summary>
        /// Updated existing records.
        /// </summary>
        Update,

        /// <summary>
        /// Read new records.
        /// </summary>
        Read,
    }

    /// <summary>
    /// Entity that tracks the last synchronization by the client application.
    /// </summary>
    public class SyncLog : EntityBase
    {
        /// <summary>
        /// Defines the type of synchronization action taken.
        /// </summary>
        public SyncActionType Action { get; set; }

        /// <summary>
        /// The ID of the user who did the sync.
        /// </summary>
        public Guid VendorUserId { get; set; }

        /// <summary>
        /// The Vendor ID.
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// The users name.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// The synchronization request information.
        /// </summary>
        public SyncRequest? Request { get; set; }

        /// <summary>
        /// The records processed for the update
        /// </summary>
        public Dictionary<string, object?>[]? Records { get; set; }
    }
}
