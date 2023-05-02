namespace Fri.FieldPlotService.Admin.Common
{
    public class Constants
    {
        /// <summary>
        /// User Role for Administrators who can manage the downloads.
        /// </summary>
        public const string AdminRole = "Admin";

        /// <summary>
        /// User role for users that can view-only and access reports.
        /// </summary>
        public const string ReportsRole = "Users";

        /// <summary>
        /// Validation for .NET Timespan
        /// </summary>
        public const string TimeSpanValidation = @"^\s*-?(\d{0,7}|10[0-5]\d{0,5}|106[0-6]\d{0,4}|1067[0-4]\d{0,3}|10675[0-1]\d{0,2}|((\d{0,7}|10[0-5]\d{0,5}|106[0-6]\d{0,4}|1067[0-4]\d{0,3}|10675[0-1]\d{0,2})\.)?([0-1]?[0-9]|2[0-3]):[0-5]?[0-9](:[0-5]?[0-9](\.\d{1,7})?)?)\s*$";

        /// <summary>
        /// Default date format
        /// </summary>
        public const string DateFormat = @"MMM dd, yyyy";

        /// <summary>
        /// Default time format
        /// </summary>
        public const string TimeFormat = @"hh:mm:ss tt";

        /// <summary>
        /// GUID format (upper or lower case without brackets)
        /// </summary>
        public const string GuidFormat = @"^[A-Za-z0-9]{8}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{12}$";

        /// <summary>
        /// List of VSN tables
        /// </summary>
        public static readonly string[] VsnTables =
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
    }

}
