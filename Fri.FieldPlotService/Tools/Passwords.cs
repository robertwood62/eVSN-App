using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Fri.FieldPlotService.Tools
{
    /// <summary>
    /// Utility to generate an MD5 Password Hash.
    /// </summary>
    public class Passwords
    {
        static readonly SHA512 sha512 = SHA512.Create();

        /// <summary>
        /// Generates a password hash using some salt.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordSalt"></param>
        /// <returns></returns>
        public static string GetPasswordHash(string? password, string? passwordSalt)
        {
            var encryptedSHA512 = sha512.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, passwordSalt)));
            return Convert.ToBase64String(encryptedSHA512);
        }
    }
}
