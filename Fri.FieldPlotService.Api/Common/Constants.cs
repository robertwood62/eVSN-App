namespace Fri.FieldPlotService.Api.Common
{
    /// <summary>
    /// List of application constants in the system.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// User Role for Admin and Reporting users (any roles)
        /// </summary>
        public const string UserRole = "Admin,User";

        /// <summary>
        /// User Role for Administrators who can manage the downloads.
        /// </summary>
        public const string AdminRole = "Admin";

        /// <summary>
        /// User role for users that can view-only and access reports.
        /// </summary>
        public const string ReportsRole = "User";
    }
}
