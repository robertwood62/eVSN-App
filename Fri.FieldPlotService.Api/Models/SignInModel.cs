namespace Fri.FieldPlotService.Api.Models
{
    /// <summary>
    /// Model used for sign-in
    /// </summary>
    public class SignInModel
    {
        /// <summary>
        /// The username.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// The password.
        /// </summary>
        public string? Password { get; set; }
    }
}
