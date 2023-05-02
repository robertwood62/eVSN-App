using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fri.FieldPlotService.Admin.Services
{
    /// <summary>
    /// API Client base class.
    /// </summary>
    public class FieldPlotServiceApiClient
    {
        /// <summary>
        /// Bearer token shared by all API Clients.
        /// </summary>
        public static string? BearerToken { get; set; }

        /// <summary>
        /// Creates a custom request message that adds the BearerToken to the header for identification purposes
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        protected Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var msg = new HttpRequestMessage();
            if (BearerToken != null)
            {
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
            }
            return Task.FromResult(msg);
        }

        /// <summary>
        /// Updates the JSON Serializer settings.
        /// </summary>
        /// <param name="settings"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        protected void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.ContractResolver = new SafeContractResolver();
            settings.Converters.Add(new JsonTimeSpanConverter());
        }

        class SafeContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var jsonProp = base.CreateProperty(member, memberSerialization);
                jsonProp.Required = Required.Default;
                return jsonProp;
            }
        }

        /// <summary>
        /// Overrides the request preparation.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="url"></param>
        public static void PrepareRequest(HttpRequestMessage request, string url)
        {
            Trace.WriteLine($"API Request: {request.Method}:{url}");
        }

        /// <summary>
        /// Overrides the request preparation.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="urlBuilder"></param>
        public static void PrepareRequest(HttpRequestMessage request, StringBuilder urlBuilder)
        {
            Trace.WriteLine($"API Request: {request.Method}:{urlBuilder}");
        }

        /// <summary>
        /// Overrides the response processing.
        /// </summary>
        /// <param name="response"></param>
        public static void ProcessResponse(HttpResponseMessage response)
        {
            Trace.WriteLine($"API Response: {response.StatusCode}");
        }

    }

    /// <summary>
    /// Admin Client
    /// </summary>
    public partial class AdminClient
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")] 
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url) => FieldPlotServiceApiClient.PrepareRequest(request, url);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder) => FieldPlotServiceApiClient.PrepareRequest(request, urlBuilder);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")] 
        partial void ProcessResponse(HttpClient client, HttpResponseMessage response) => FieldPlotServiceApiClient.ProcessResponse(response);
    }

}
