using Fri.FieldPlotService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Tools
{
    /// <summary>
    /// General validation
    /// </summary>
    public class VsnValidator
    {
        /// <summary>
        /// Checks there is a value present for a given field.  Returns the value or null.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="key"></param>
        /// <param name="errorCode">Error code to through for missing data.</param>
        /// <returns></returns>
        /// <exception cref="FieldPlotServiceException"></exception>
        static void CheckValue(IDictionary<string, object?>? entity, string key, FieldPlotServiceError errorCode)
        {
            if (entity == null)
            {
                throw new FieldPlotServiceException(FieldPlotServiceError.VSNEntityCannotBeNull);
            }

            if (!entity.ContainsKey(key))
            {
                throw new FieldPlotServiceException(errorCode);
            }

            var value = entity[key]?.ToString();
            if(string.IsNullOrWhiteSpace(value))
            {
                throw new FieldPlotServiceException(errorCode);
            }
        }

        /// <summary>
        /// Validates the VSN entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <exception cref="FieldPlotServiceException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static void Validate<T>(IDictionary<string, object?>? entity) where T: IVendorEntity
        {
            if(typeof(T) == typeof(Deformity))
            {
                CheckValue(entity, "DEFORMITYID", FieldPlotServiceError.VSNDeformityIdInvalid);
                CheckValue(entity, "TREEID", FieldPlotServiceError.VSNTreeIdInvalid);
            }
            else if(typeof(T) == typeof(Dwd))
            {
                CheckValue(entity, "DWDID", FieldPlotServiceError.VSNDwdIdInvalid);
                CheckValue(entity, "PLOTID", FieldPlotServiceError.VSNPlotIdInvalid);
            }
            else if(typeof(T) == typeof(Ecosite))
            {
                CheckValue(entity, "ECOSITEID", FieldPlotServiceError.VSNEcositeIdInvalid);
                CheckValue(entity, "PLOTID", FieldPlotServiceError.VSNPlotIdInvalid);
            }
            else if (typeof(T) == typeof(Person))
            {
                CheckValue(entity, "PERSONID", FieldPlotServiceError.VSNPersonIdInvalid);
                CheckValue(entity, "PROJECTID", FieldPlotServiceError.VSNProjectIdInvalid);
            }
            else if(typeof(T) == typeof(Photo))
            {
                CheckValue(entity, "PLOTID", FieldPlotServiceError.VSNPlotIdInvalid);
                CheckValue(entity, "PHOTOID", FieldPlotServiceError.VSNPhotoIdInvalid);
            }
            else if (typeof(T) == typeof(Plot))
            {
                CheckValue(entity, "VSNPLOTNAME", FieldPlotServiceError.VSNPlotNameInvalid);
                CheckValue(entity, "PLOTID", FieldPlotServiceError.VSNPlotIdInvalid);
                CheckValue(entity, "PROJECTID", FieldPlotServiceError.VSNProjectIdInvalid);
            }
            else if (typeof(T) == typeof(Project))
            {
                CheckValue(entity, "PROJECTID", FieldPlotServiceError.VSNProjectIdInvalid);
                CheckValue(entity, "NAME", FieldPlotServiceError.VSNProjectNameInvalid);
            }
            else if(typeof(T) == typeof(SmallTree))
            {
                CheckValue(entity, "SMALLTREEID", FieldPlotServiceError.VSNSmallTreeIdInvalid);
                CheckValue(entity, "PLOTID", FieldPlotServiceError.VSNPlotIdInvalid);
            }
            else if(typeof(T) == typeof(SmallTreeTally))
            {
                CheckValue(entity, "SMALLTREETALLYID", FieldPlotServiceError.VSNSmallTreeTallyIdInvalid);
                CheckValue(entity, "PLOTID", FieldPlotServiceError.VSNPlotIdInvalid);
            }
            else if(typeof(T) == typeof(Soil))
            {
                CheckValue(entity, "SOILID", FieldPlotServiceError.VSNSoilIdInvalid);
                CheckValue(entity, "PLOTID", FieldPlotServiceError.VSNPlotIdInvalid);
            }
            else if(typeof(T) == typeof(StemMap))
            {
                CheckValue(entity, "STEMMAPID", FieldPlotServiceError.VSNStemMapIdInvalid);
                CheckValue(entity, "TREEID", FieldPlotServiceError.VSNTreeIdInvalid);
            }
            else if (typeof(T) == typeof(Tree))
            {
                CheckValue(entity, "PLOTID", FieldPlotServiceError.VSNPlotIdInvalid);
                CheckValue(entity, "TREEID", FieldPlotServiceError.VSNTreeIdInvalid);
            }
            else if (typeof(T) == typeof(Vegetation))
            {
                CheckValue(entity, "PLOTID", FieldPlotServiceError.VSNPlotIdInvalid);
                CheckValue(entity, "VEGETATIONID", FieldPlotServiceError.VSNVegetationIdInvalid);
            }
            else if (typeof(T) == typeof(VegetationCensus))
            {
                CheckValue(entity, "PLOTID", FieldPlotServiceError.VSNPlotIdInvalid);
                CheckValue(entity, "VEGETATIONCENSUSID", FieldPlotServiceError.VSNVegetationCensusIdInvalid);
            }
            else
            {
                throw new NotImplementedException($"Missing validation checks for VSN entity type '{typeof(T).Name}'.");
            }

        }
    }
}
