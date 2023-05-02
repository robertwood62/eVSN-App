using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// Defines an entity that is scoped to a specific vendor.
    /// </summary>
    public interface IVendorEntity
    {
        /// <summary>
        /// Entity ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID of the vendor for this record.
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// Contains the entity data from the front-end application.
        /// </summary>
        public Dictionary<string, object?>? Data { get; set; }

        /// <summary>
        /// Safe method to get data from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object? GetObject(string key);

        /// <summary>
        /// Gets a string value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string? GetString(string key, string? defaultValue = null);

        /// <summary>
        /// Gets an integer value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        int? GetInt(string key, int? defaultValue = null);

        /// <summary>
        /// Gets a DateTime value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        DateTime? GetDateTime(string key, DateTime? defaultValue = null);
    }
}
