using Fri.FieldPlotService.Admin.Services;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Fri.FieldPlotService.Admin.Common
{
    /// <summary>
    /// Provides support to download files from the server (using auth)
    /// </summary>
    public class DownloadHelper
    {
        readonly HttpClient httpClient;
        readonly AdminContextOptions options;

        /// <summary>
        /// Constructs the download helper;
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="options"></param>
        public DownloadHelper(HttpClient httpClient, AdminContextOptions options)
        {
            this.httpClient = httpClient;
            this.options = options;
        }

        ///// <summary>
        ///// Downloads a report.
        ///// </summary>
        ///// <param name="reportType"></param>
        ///// <param name="reportFormat"></param>
        ///// <param name="startTime"></param>
        ///// <param name="endTime"></param>
        ///// <returns></returns>
        //public async Task<Stream> DownloadReportAsync(ReportType reportType, ReportFormatType reportFormat, DateTime startTime, DateTime endTime)
        //{
        //    var url = new StringBuilder();
        //    url.Append("/api/Admin/reports/csv?");
        //    url.Append(Uri.EscapeDataString("reportType") + "=").Append(Uri.EscapeDataString(ConvertToString(reportType, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
        //    url.Append(Uri.EscapeDataString("reportFormat") + "=").Append(Uri.EscapeDataString(ConvertToString(reportFormat, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
        //    url.Append(Uri.EscapeDataString("startTime") + "=").Append(Uri.EscapeDataString(startTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture))).Append("&");
        //    url.Append(Uri.EscapeDataString("endTime") + "=").Append(Uri.EscapeDataString(endTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture))).Append("&");
        //    url.Length--;

        //    // Download the file
        //    return await DownloadAsync(HttpMethod.Get, url.ToString());
        //}

        /// <summary>
        /// Converts object to string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return "";
            }

            if (value is Enum)
            {
                var name = Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                            as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null)
                        {
                            return attribute.Value != null ? attribute.Value : name;
                        }
                    }

                    var converted = Convert.ToString(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted == null ? string.Empty : converted;
                }
            }
            else if (value is bool)
            {
                return Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[])
            {
                return Convert.ToBase64String((byte[])value);
            }
            else if (value.GetType().IsArray)
            {
                var array = Enumerable.OfType<object>((Array)value);
                return string.Join(",", Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
            }

            var result = Convert.ToString(value, cultureInfo);
            return result == null ? "" : result;
        }

        /// <summary>
        /// Downloads a file from the user.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="rootRelativeUrl"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Stream> DownloadAsync(HttpMethod method, string rootRelativeUrl, object? data = null)
        {
            // Assemble the request message
            HttpRequestMessage request = AssembleRequestMessage(options.FieldPlotServiceApi, FieldPlotServiceApiClient.BearerToken, method, rootRelativeUrl, data);
            var response = await httpClient.SendAsync(request);

            // Handle the response
            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException($"API Download Request '{request.RequestUri}' failed with HTTP status code {response.StatusCode}-{response.ReasonPhrase}.", null);
            }

            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Assemble a fully configured request message.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="method"></param>
        /// <param name="rootRelativeUrl"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        HttpRequestMessage AssembleRequestMessage(string baseUrl, string? accessToken, HttpMethod method, string rootRelativeUrl, object? data = null)
        {
            string uri = baseUrl.TrimEnd('/') + rootRelativeUrl;

            Trace.WriteLine("- SendAsync() ------------------------------");
            Trace.WriteLine($"{method} - {uri}");

            HttpRequestMessage request = new HttpRequestMessage(method, uri);

            // Add the payload for the request
            if (data != null)
            {
                var jsonModel = JsonConvert.SerializeObject(data);
                Trace.WriteLine($"(Request Data) - {jsonModel}");
                request.Content = new StringContent(jsonModel, Encoding.UTF8, "application/json");
            }

            // Setup the header
            request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoStore = true,
                NoCache = true,
            };

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return request;
        }
    }
}
