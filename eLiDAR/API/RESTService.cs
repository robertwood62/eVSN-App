using eLiDAR.Models;
using eLiDAR.Views;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace eLiDAR.API
{
    public class UserTokenModel
    {
        public string AccessToken { get; set; }
        public string Username { get; set; }
        public Guid UserId { get; set; }
        public Guid VendorId { get; set; }
    }

    public class ApiErrorModel
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }


    public interface IRestService
	{
        Task<UserTokenModel> SignInAsync(string username, string password);
        Task<List<T>> GetSyncDataAsync<T>(DateTime? lastSyncTime, string plotId) where T : new();
        Task PushSyncDataAsync<T>(List<T> items, bool isNewItem = false) where T : new();
        Task DeleteSyncDataAsync<T>(string ItemId);
    }

    public class RestService : IRestService
    {
        static HttpClient _client;
        readonly Utilities.Utils util = new Utilities.Utils();
        readonly  string baseUrl;
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }

        public RestService()
        {
            // Determine the environment to use
            baseUrl = EnvironmentSelector.Current.BaseUrl;

            // Setup handler to allow for invalid certificates for localhost (enables testing).
            var handler = new HttpClientHandler();
#if DEBUG
            if (baseUrl.Contains("localhost"))
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };
            }
#endif
            _client = new HttpClient(handler);
        }

        /// <summary>
        /// Signs the user into the system and get's a token back from the server.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<UserTokenModel> SignInAsync(string username, string password)
        {
            var url = $"{baseUrl}api/Field/sign-in";
            var json = JsonConvert.SerializeObject(new { Username = username, Password = password });
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(url, requestContent);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<UserTokenModel>(responseContent);
                IsSuccess = true;
                return data;
            }
            else if(response.StatusCode == HttpStatusCode.BadRequest)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiError = JsonConvert.DeserializeObject<ApiErrorModel>(responseContent);
                throw new ApplicationException($"Error {apiError.ErrorCode} : {apiError.Message}");
            }
            else if(response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }
            else
            {
                throw new ApplicationException($"HTTP Error {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Creates a new request message with the current auth-token.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        HttpRequestMessage CreateRequestMessage(HttpMethod method, string requestUri, HttpContent content)
        {
            HttpRequestMessage message = new HttpRequestMessage(method, requestUri);
            if(content != null)
            {
                message.Content = content;
            }
            var token = util.LoginToken;

            if(!string.IsNullOrWhiteSpace(token))
            {
                message.Headers.Add("user_token", token);
                message.Headers.Add("user_device_type", util.DeviceType);
                message.Headers.Add("user_device_id", util.DeviceId.ToString());
            }
            return message;
        }

        /// <summary>
        /// Method to get data from the server based on various filters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lastSyncTime"></param>
        /// <param name="plotId"></param>
        /// <returns></returns>
        public async Task<List<T>> GetSyncDataAsync<T>(DateTime? lastSyncTime, string plotId) where T: new()
        {
            // Build the get URL
            var url = $"{baseUrl}api/Field/sync?table={typeof(T).Name.ToLower()}";
            if(lastSyncTime != null)
            {
                url += $"&lastSyncTime={WebUtility.UrlEncode(lastSyncTime.Value.ToString())}";
            }
            if(!string.IsNullOrWhiteSpace(plotId))
            {
                url += $"&plotId={WebUtility.UrlEncode(plotId)}";
            }

            try
            {
                var request = CreateRequestMessage(HttpMethod.Get, url, null);
                var response = await _client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<T>>(content);
                    IsSuccess = true;
                    return data;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiError = JsonConvert.DeserializeObject<ApiErrorModel>(responseContent);
                    throw new ApplicationException($"Error {apiError.ErrorCode} : {apiError.Message}");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ApplicationException($"Authorized failed, please logout and login again.");
                }
                else
                {
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
                IsSuccess = false;
                Msg = ex.Message;
            }
            return new List<T>();
        }

        /// <summary>
        /// Pushes new data to the server (either new date or updates to existing data).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="isNewItem"></param>
        /// <returns></returns>
        public async Task PushSyncDataAsync<T>(List<T> items, bool isNewItem = false) where T : new()
        {
            try
            {
                var url = $"{baseUrl}api/Field/sync?table={typeof(T).Name.ToLower()}";
                var json = JsonConvert.SerializeObject(items);
                Trace.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;

                if (isNewItem)
                {
                    var request = CreateRequestMessage(HttpMethod.Post, url, content);
                    response = await _client.SendAsync(request);
                }
                else
                {
                    var request = CreateRequestMessage(HttpMethod.Put, url, content);
                    response = await _client.SendAsync(request);
                }
                if (response.IsSuccessStatusCode)
                {
                    IsSuccess = true;
                    Debug.WriteLine(@"\Items successfully saved.");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiError = JsonConvert.DeserializeObject<ApiErrorModel>(responseContent);
                    throw new ApplicationException($"Error {apiError.ErrorCode} : {apiError.Message}");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ApplicationException($"Authorized failed, please logout and login again.");
                }
                else
                {
                    IsSuccess = false;
                    Msg = "Project data did not push";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
                IsSuccess = false;
                Msg = ex.Message;
            }
        }

        /// <summary>
        /// Deletes an entity from the backend.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteSyncDataAsync<T>(string id)
        {
            // Build the get URL
            var url = $"{baseUrl}api/Field/sync?table={typeof(T).Name.ToLower()}";
            url += $"&id={WebUtility.UrlEncode(id)}";

            try
            {
                var request = CreateRequestMessage(HttpMethod.Delete, url, null);
                var response = await _client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine(@"\tItems successfully deleted.");
                    IsSuccess = true;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiError = JsonConvert.DeserializeObject<ApiErrorModel>(responseContent);
                    throw new ApplicationException($"Error {apiError.ErrorCode} : {apiError.Message}");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ApplicationException($"Authorized failed, please logout and login again.");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
            }
        }

    }

}