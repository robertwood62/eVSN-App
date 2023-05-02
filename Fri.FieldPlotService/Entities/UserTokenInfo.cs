using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Entities
{
    /// <summary>
    /// The user token info has basic user information and the access token used for authorization.
    /// </summary>
    public class UserTokenInfo
    {
        /// <summary>
        /// Constructs the user token information.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="username"></param>
        /// <param name="userId"></param>
        /// <param name="vendorId"></param>
        public UserTokenInfo(string accessToken, string username, Guid userId, Guid vendorId)
        {
            AccessToken = accessToken;
            Username = username;
            UserId = userId;
            VendorId = vendorId;
        }

        /// <summary>
        /// Encrypted access token.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Username used to login.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The user's vendor ID.
        /// </summary>
        public Guid VendorId { get; set; }
    }
}
