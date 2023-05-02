using Fri.FieldPlotService.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// Metrics on completion of the assigned plots.
    /// </summary>
    public class PlotMetrics
    {
        /// <summary>
        /// Constructs the plot metrics.
        /// </summary>
        /// <param name="completed"></param>
        /// <param name="inProgress"></param>
        /// <param name="notStarted"></param>
        public PlotMetrics(int completed, int inProgress, int notStarted)
        {
            Completed = completed;
            InProgress = inProgress;
            NotStarted = notStarted;
        }

        /// <summary>
        /// Number of completed plots.
        /// </summary>
        public int Completed { get; set; }

        /// <summary>
        /// Number of in progress plots.
        /// </summary>
        public int InProgress { get; set; }

        /// <summary>
        /// Number of plot not yet started.
        /// </summary>
        public int NotStarted { get; set; }

        /// <summary>
        /// Total number of plots
        /// </summary>
        public int Total
        {
            get
            {
                return Completed + InProgress + NotStarted;
            }
        }
    }

    /// <summary>
    /// Simple metric value.
    /// </summary>
    public class SimpleMetric
    {
        /// <summary>
        /// Constructs the simple metrics.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        public SimpleMetric(string label, double value)
        {
            Label = label;
            Value = value;
        }

        /// <summary>
        /// The label for the value. 
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Teh value
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Provides the status on a plot
    /// </summary>
    public enum PlotStatusType
    {
        /// <summary>
        /// The plot hasn't yet been started.
        /// </summary>
        NoStarted,

        /// <summary>
        /// The Plot is currently in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// The Plot has been completed.
        /// </summary>
        Completed,
    }

    /// <summary>
    /// Information on a specific plot.
    /// </summary>
    public class PlotSummary
    {
        /// <summary>
        /// Constructs the plot summary from a Plot record.
        /// </summary>
        /// <param name="plot"></param>
        /// <param name="vendor"></param>
        /// <param name="project"></param>
        public PlotSummary(Plot plot, Vendor? vendor, Project? project)
        {
            PlotId = plot.Id;
            VendorId = plot.VendorId;
            PlotName = plot.GetString(Constants.VsnPlotName);
            PlotTypeCode = plot.GetString(Constants.VsnPlotTypeCode);
            MeasureTypeCode = plot.GetString(Constants.VsnMeasureTypeCode);
            PlotOverviewDate = plot.GetDateTime(Constants.VsnPlotOverviewDate);
            ProjectName = project?.GetString(Constants.VsnProjectName);
            Vendor = vendor?.Info?.Code;

            // The primary fields to look at are the plot overview date & error count to determine the status
            if (PlotOverviewDate == null || PlotOverviewDate.Value.Date > DateTime.UtcNow.Date)
            {
                Status = PlotStatusType.NoStarted;
            }
            else
            {
                int? errorCount = plot.GetInt(Constants.VsnErrorCount, null);
                if (errorCount != null)
                {
                    Status = (errorCount == 0) ? PlotStatusType.Completed : PlotStatusType.InProgress;
                }
                else
                {
                    Status = PlotStatusType.InProgress;
                }
            }


        }

        /// <summary>
        /// The vendor code.
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// The project name.
        /// </summary>
        public string? ProjectName { get; set; }

        /// <summary>
        /// The ID of the Plot.
        /// </summary>
        public Guid PlotId { get; set; }

        /// <summary>
        /// The ID of the vendor.
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// The VSN Plot name.
        /// </summary>
        public string? PlotName { get; set; }

        /// <summary>
        /// The VSN Plot Type code.
        /// </summary>
        public string? PlotTypeCode { get; set; }

        /// <summary>
        /// The VSN Measure Type code.
        /// </summary>
        public string? MeasureTypeCode { get; set; }

        /// <summary>
        /// The status of the plot work.
        /// </summary>
        public PlotStatusType Status { get; set; }

        /// <summary>
        /// The date the plot sample was collected.
        /// </summary>
        public DateTime? PlotOverviewDate { get; set; }
    }


    /// <summary>
    /// Provides a dashboard summary of the current activity and progress.
    /// </summary>
    public class DashboardData
    {
        /// <summary>
        /// Optional Vendor ID.
        /// </summary>
        public Guid? VendorId { get; set; }

        /// <summary>
        /// Optional Project ID.
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Optional metrics year.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Current status.
        /// </summary>
        public PlotMetrics? PlotStatus { get; set; }

        /// <summary>
        /// List of monthly activity.
        /// </summary>
        public List<SimpleMetric>? MonthlyActivity { get; set; }

        /// <summary>
        /// The list of relevant plots.
        /// </summary>
        public List<PlotSummary>? Plots { get; set; }
    }
}
