﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// Plot data to import.
    /// </summary>
    public class ImportPlotData
    {
        /// <summary>
        /// VSN Plot Name
        /// </summary>
        public string? PlotName { get; set; }

        /// <summary>
        /// Measurement Year
        /// </summary>
        public int? MeasureYear { get; set; }

        /// <summary>
        /// VSN Plot type code.
        /// </summary>
        public string? PlotTypeCode { get; set; }

        /// <summary>
        /// UTM Zone
        /// </summary>
        public int UtmZone { get; set; }

        /// <summary>
        /// Easting
        /// </summary>
        public int Easting { get; set; }

        /// <summary>
        /// Northing
        /// </summary>
        public int Northing { get; set; }

    }
}
