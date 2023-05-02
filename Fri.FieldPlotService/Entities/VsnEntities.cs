using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// Provides a base class for all VSN Entity types.
    /// </summary>
    public class VsnEntityBase : EntityBase, IVendorEntity
    {
        /// <summary>
        /// Vendor associated with this record.
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// Entity synchronized with field app data table.
        /// </summary>
        public Dictionary<string, object?>? Data { get; set; }

        /// <summary>
        /// Safe method to get data from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object? GetObject(string key)
        {
            if(Data != null && Data.TryGetValue(key, out object? value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// Gets a string value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string? GetString(string key, string? defaultValue = null)
        {
            return GetObject(key)?.ToString() ?? defaultValue;
        }

        /// <summary>
        /// Gets an integer value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int? GetInt(string key, int? defaultValue = null)
        {
            if (int.TryParse(GetObject(key)?.ToString(), out int result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a DateTime value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public DateTime? GetDateTime(string key, DateTime? defaultValue = null)
        {
            if(DateTime.TryParse(GetObject(key)?.ToString(), out DateTime result))
            {
                return result;
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// Deformity information
    /// </summary>
    public class Deformity : VsnEntityBase { }

    /// <summary>
    /// Dwd information
    /// </summary>
    public class Dwd : VsnEntityBase { }

    /// <summary>
    /// Eco-site information.
    /// </summary>
    public class Ecosite : VsnEntityBase { }

    /// <summary>
    /// Person (crew) doing the data collection.
    /// </summary>
    public class Person : VsnEntityBase { }

    /// <summary>
    /// Photo data.
    /// </summary>
    public class Photo : VsnEntityBase { }

    /// <summary>
    /// Field plot information.
    /// </summary>
    public class Plot : VsnEntityBase { }

    /// <summary>
    /// Project information.
    /// </summary>
    public class Project : VsnEntityBase { }

    /// <summary>
    /// Small tree information.
    /// </summary>
    public class SmallTree : VsnEntityBase { }

    /// <summary>
    /// Small tree tally information.
    /// </summary>
    public class SmallTreeTally : VsnEntityBase { }

    /// <summary>
    /// Soil information.
    /// </summary>
    public class Soil : VsnEntityBase { }

    /// <summary>
    /// Stem map information.
    /// </summary>
    public class StemMap : VsnEntityBase { }

    /// <summary>
    /// Tree information.
    /// </summary>
    public class Tree : VsnEntityBase { }

    /// <summary>
    /// Vegetation information.
    /// </summary>
    public class Vegetation : VsnEntityBase { }

    /// <summary>
    /// Vegetation census information.
    /// </summary>
    public class VegetationCensus : VsnEntityBase { }

}
