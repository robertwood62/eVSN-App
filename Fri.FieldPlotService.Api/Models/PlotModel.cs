using Fri.FieldPlotService.Entities;

namespace Fri.FieldPlotService.Api.Models
{
    /// <summary>
    /// Plot information.
    /// </summary>
    public class PlotModel
    {
        /// <summary>
        /// Constructs the plot model.
        /// </summary>
        /// <param name="plot"></param>
        public PlotModel(Plot plot)
        {
            VendorId = plot.VendorId;
            PlotId = plot.Id;
            Data = plot.Data;
        }

        /// <summary>
        /// Vendor ID
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// Plot ID 
        /// </summary>
        public Guid PlotId { get; set; }

        /// <summary>
        /// VSN Plot data.
        /// </summary>
        public Dictionary<string, object?>? Data { get; set; }
    }
}
