using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// Represents all the name/value pair details for a plot.
    /// </summary>
    public class PlotDetails
    {
        /// <summary>
        /// VSN Plot Data.
        /// </summary>
        public IVendorEntity? Plot { get; set; }

        /// <summary>
        /// Project Information.
        /// </summary>
        public IVendorEntity? Project { get; set; }

        /// <summary>
        /// Soil information.
        /// </summary>
        public List<IVendorEntity>? Soils { get; set; }

        /// <summary>
        /// People information.
        /// </summary>
        public List<IVendorEntity>? People { get; set; }

        /// <summary>
        /// Photos.
        /// </summary>
        public List<IVendorEntity>? Photos { get; set; }

        /// <summary>
        /// Small trees.
        /// </summary>
        public List<IVendorEntity>? SmallTrees { get; set; }

        /// <summary>
        /// Small tree tallies.
        /// </summary>
        public List<IVendorEntity>? SmallTreeTallies { get; set; }

        /// <summary>
        /// Ecosite.
        /// </summary>
        public List<IVendorEntity>? EcoSites { get; set; }

        /// <summary>
        /// DWDs
        /// </summary>
        public List<IVendorEntity>? Dwds { get; set; }

        /// <summary>
        /// List of trees.
        /// </summary>
        public List<TreeDetail>? Trees { get; set; }

        /// <summary>
        /// Vegetation List.
        /// </summary>
        public List<IVendorEntity>? Vegetations { get; set; }

        /// <summary>
        /// Vegetation census list.
        /// </summary>
        public List<IVendorEntity>? VegetationCensuses { get; set; }
    }

    /// <summary>
    /// Tree details.
    /// </summary>
    public class TreeDetail
    {
        /// <summary>
        /// Tree information.
        /// </summary>
        public IVendorEntity? Tree { get; set; }

        /// <summary>
        /// List of deformities
        /// </summary>
        public List<IVendorEntity>? Deformities { get; set; }

        /// <summary>
        /// List of stem maps.
        /// </summary>
        public List<IVendorEntity>? StemMaps { get; set; }
    }
}
