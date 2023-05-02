using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Tools
{
    /// <summary>
    /// The access token is written to the user's browser to track requests to the download service.
    /// </summary>
    public class UserToken
    {
        /// <summary>
        /// Username.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// The unique ID generated for the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The unique ID for the vendor.
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// Parses the token from a string (encrypted json as base64).
        /// </summary>
        /// <param name="token"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static UserToken FromString(string token, string key)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var json = token.Decrypt(key);
                    var userToken = JsonConvert.DeserializeObject<UserToken>(json);
                    if (userToken == null)
                    {
                        throw new UnauthorizedAccessException();
                    }

                    return userToken;
                }
                catch
                {
                    // The token has been tampered with, so simply return null.
                    throw new UnauthorizedAccessException();
                }
            }
            throw new UnauthorizedAccessException();
        }

        /// <summary>
        /// Serializes the token as (encrypted json base64 string).
        /// </summary>
        /// <returns></returns>
        public string ToEncryptedString(string key)
        {
            var json = JsonConvert.SerializeObject(this);
            return json.Encrypt(key);
        }
    }
}
