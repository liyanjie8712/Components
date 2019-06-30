﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Liyanjie.Http
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Request
    {
        internal HttpMethod Method { get; set; }

        internal string Url { get; set; }

        internal IList<KeyValuePair<string, string>> Queries { get; set; } = new List<KeyValuePair<string, string>>();

        internal IList<KeyValuePair<string, string>> Headers { get; set; } = new List<KeyValuePair<string, string>>();

        internal IList<HttpContent> Contents { get; set; } = new List<HttpContent>();

        internal Request() { }

        internal HttpClient CreateHttpClient(TimeSpan timeout)
        {
            var httpClient = new HttpClient
            {
                Timeout = timeout
            };
            if (Headers != null)
                foreach (var item in Headers)
                {
                    if (!httpClient.DefaultRequestHeaders.Contains(item.Key))
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            return httpClient;
        }

        internal Uri CreateRequestUri()
        {
            var requestUrl = Url.IndexOf('?') < 0 ? $"{Url}?" : Url;
            if (Queries != null)
                foreach (var item in Queries)
                {
                    requestUrl = $"{requestUrl}&{item.Key}={item.Value}";
                }
            return new Uri(requestUrl);
        }

        internal HttpContent CreateHttpContent()
        {
            if (Contents != null)
                if (Contents.Count == 1)
                    return Contents[0];
                else if (Contents.Count > 0)
                {
                    var content = new MultipartContent();
                    foreach (var item in Contents)
                    {
                        content.Add(item);
                    }
                    return content;
                }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeoutBySeconds"></param>
        /// <returns></returns>
        public virtual async Task<HttpResponseMessage> SendAsync(int timeoutBySeconds = 60)
        {
            using (var client = CreateHttpClient(TimeSpan.FromSeconds(timeoutBySeconds))) 
            {
                return await client.SendAsync(new HttpRequestMessage
                {
                    Content = CreateHttpContent(),
                    Method = Method,
                    RequestUri = CreateRequestUri(),
                });
            }
        }
    }
}
