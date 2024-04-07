using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager
{
    internal class RestApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUri;

        public RestApiClient(string baseUri, string token = null)
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            // 如果需要，可以在这里添加身份验证信息
            if (!string.IsNullOrEmpty(token))
            {
                handler.Credentials = new NetworkCredential("username", token);
            }

            _httpClient = new HttpClient(handler)
            {
                // 设置超时时间
                Timeout = TimeSpan.FromSeconds(5)
            };

            _baseUri = baseUri;

        }

        public async Task<string> GetAsync(string resource)
        {
            try
            {
                //Trace.TraceInformation("【GetStatus】(Before)");
                var response = await _httpClient.GetAsync(_baseUri + resource).ConfigureAwait(false);
                //Trace.TraceInformation("【GetStatus】[After]");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // 添加更详细的错误信息
                    throw new Exception($"Failed to GET data from {_baseUri + resource}. Status code: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException)
            {
                // 处理超时
                throw new Exception($"Request to {_baseUri + resource} timed out.");
            }
        }

        public async Task<string> PostAsync(string resource, string data)
        {
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            try
            {
                //Trace.TraceInformation("【PostMove】(Before)");
                var response = await _httpClient.PostAsync(_baseUri + resource, content).ConfigureAwait(false);
                //Trace.TraceInformation("【PostMove】[After]");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception($"Failed to POST data to {_baseUri + resource}. Status code: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException)
            {
                throw new Exception($"Request to {_baseUri + resource} timed out.");
            }
        }

        public async Task<string> PutAsync(string resource, string data)
        {
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PutAsync(_baseUri + resource, content).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception($"Failed to PUT data to {_baseUri + resource}. Status code: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException)
            {
                throw new Exception($"Request to {_baseUri + resource} timed out.");
            }
        }

        public async Task<string> DeleteAsync(string resource)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(_baseUri + resource).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception($"Failed to DELETE data from {_baseUri + resource}. Status code: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException)
            {
                throw new Exception($"Request to {_baseUri + resource} timed out.");
            }
        }

        public string BuildUri(string baseUri, Dictionary<string, string> parameters)
        {
            var builder = new StringBuilder(baseUri);

            if (parameters != null && parameters.Count > 0)
            {
                builder.Append("?");

                foreach (var pair in parameters)
                {
                    builder.AppendFormat("{0}={1}&", Uri.EscapeDataString(pair.Key), Uri.EscapeDataString(pair.Value));
                }

                // 移除最后一个 "&"
                builder.Length--;
            }

            return builder.ToString();
        }
    }
}
