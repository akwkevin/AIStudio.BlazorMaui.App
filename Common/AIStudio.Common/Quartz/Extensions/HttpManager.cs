﻿using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;

namespace AIStudio.Common.Quartz.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpManager
    {

        /// <summary>
        /// Gets the user ip.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns></returns>
        public static string GetUserIP(IHttpContextAccessor httpContextAccessor)
        {
            var Request = httpContextAccessor.HttpContext.Request;
            string realIP = null;
            string forwarded = null;
            string remoteIpAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            if (Request.Headers.ContainsKey("X-Real-IP"))
            {
                realIP = Request.Headers["X-Real-IP"].ToString();
                if (realIP != remoteIpAddress)
                {
                    remoteIpAddress = realIP;
                }
            }
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                forwarded = Request.Headers["X-Forwarded-For"].ToString();
                if (forwarded != remoteIpAddress)
                {
                    remoteIpAddress = forwarded;
                }
            }
            return remoteIpAddress;
        }

        /// <summary>
        /// HTTPs the post asynchronous.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="timeOut">The time out.</param>
        /// <param name="headers">The headers.</param>
        /// <returns></returns>
        public static Task<string> HttpPostAsync(string url, string postData = null, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            if (!string.IsNullOrEmpty(contentType))
            {
                request.ContentType = contentType;
            }
            if (headers != null)
            {
                foreach (var header in headers)
                    request.Headers[header.Key] = header.Value;
            }

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(postData ?? "");
                using (Stream sendStream = request.GetRequestStream())
                {
                    sendStream.Write(bytes, 0, bytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream responseStream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    return streamReader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(ex.Message);
            }

        }
        /// <summary>
        /// HTTPs the get asynchronous.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="headers">The headers.</param>
        /// <returns></returns>
        public static Task<string> HttpGetAsync(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (headers != null)
                {
                    foreach (var header in headers)
                        request.Headers[header.Key] = header.Value;
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream responseStream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    return streamReader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(ex.Message);
            }
        }
    }
}
