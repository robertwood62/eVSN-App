namespace Fri.FieldPlotService.Tools
{
    /// <summary>
    /// List of system wide constants.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// List of tables that allow bulk insert/update operations from the client.
        /// </summary>
        public static readonly string[] DirectEntities =
        {
            "deformity",
            "dwd",
            "ecosite",
            "person",
            "photo",
            "plot",
            "project",
            "smalltree",
            "soil",
            "stemmap",
            "tree",
            "vegetation",
            "vegetationcensus"
        };

        /// <summary>
        /// Flag used for VSN entities to mark it as deleted.
        /// </summary>
        public const string VsnDeletedFlag = "Y";

        /// <summary>
        /// Flag used for VSN entities to mark it as NOT deleted.
        /// </summary>
        public const string VsnActiveFlag = "N";

        /// <summary>
        /// VSN Project ID name field name.
        /// </summary>
        public const string VsnProjectId = "PROJECTID";

        /// <summary>
        /// VSN Project name field name.
        /// </summary>
        public const string VsnProjectName = "NAME";

        /// <summary>
        /// VSN Project description field name.
        /// </summary>
        public const string VsnProjectDescription = "DESCRIPTION";

        /// <summary>
        /// VSN is deleted field name.
        /// </summary>
        public const string VsnIsDeleted = "IsDeleted";

        /// <summary>
        /// VSN Project date field name.
        /// </summary>
        public const string VsnProjectDate = "PROJECT_DATE";

        /// <summary>
        /// VSN Record created field name.
        /// </summary>
        public const string VsnCreated = "Created";

        /// <summary>
        /// VSN Record last modified field name.
        /// </summary>
        public const string VsnLastModified = "LastModified";

        /// <summary>
        /// VSN Record plot name.
        /// </summary>
        public const string VsnPlotName = "VSNPLOTNAME";

        /// <summary>
        /// The ID of the plot
        /// </summary>
        public const string VsnPlotId = "PLOTID";

        /// <summary>
        /// The Type of VSN Plot measure type.
        /// </summary>
        public const string VsnMeasureTypeCode = "MEASURETYPECODE";

        /// <summary>
        /// The VSN Measure year.
        /// </summary>
        public const string VsnMeasureYear = "MEASUREYEAR";

        /// <summary>
        /// The VNS Tree ID name.
        /// </summary>
        public const string VsnTreeId = "TREEID";

        /// <summary>
        /// The VSN Plot Type code.
        /// </summary>
        public const string VsnPlotTypeCode = "VSNPLOTTYPECODE";

        /// <summary>
        /// The VSN record error count.
        /// </summary>
        public const string VsnErrorCount = "ERRORCOUNT";

        /// <summary>
        /// The VSN plot overview date (date viewed)
        /// </summary>
        public const string VsnPlotOverviewDate = "PLOTOVERVIEWDATE";

        /// <summary>
        /// The VSN date format used (sortable format)
        /// </summary>
        public const string VsnDateFormat = "s";
    }
}
