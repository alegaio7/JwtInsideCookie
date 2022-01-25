using Api.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using UI.Helpers;

namespace UI.Pages
{
    public class PageModelBase<T> : PageModel
    {
        protected IHttpClientFactory _clientFactory;

        public PageModelBase(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        protected HttpClient GetClient(HttpContext context, bool dontAuthenticate = false)
        {
            string jwt = default;
            if (!dontAuthenticate)
                if (context != null)
                    jwt = context.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, JwtAuthTicketFormat.TokenConstants.TokenName).Result;

            if (jwt is null && !dontAuthenticate)
            {
                throw new Exception("Token expired");
            }

            var client = _clientFactory.CreateClient("ApiClient");
            if (!dontAuthenticate)
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
            return client;
        }

        protected async Task<TContent> GetContents<TContent>(HttpContext context, string url, string method = "GET", HttpContent content = null, bool dontAuthenticate = false) where TContent : class
        {
            HttpResponseMessage result = default;
            try
            {
                var client = GetClient(context, dontAuthenticate);
                if (method == HttpMethods.Get)
                    result = await client.GetAsync(url);
                else if (method == HttpMethods.Post)
                    result = await client.PostAsync(url, content);
                else if (method == HttpMethods.Put)
                    result = await client.PutAsync(url, content);
                else if (method == HttpMethods.Delete)
                    result = await client.DeleteAsync(url);
                else
                    throw new Exception("Http method not supported in Api Service Wrapper");

                var err = await GetExceptionFromResultAsync(result);
                if (err != null)
                    throw err;

                return await GetObjectFromJsonResultAsync<TContent>(result);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new UIGenericHttpException(System.Net.HttpStatusCode.RequestTimeout);
            }
            catch (UIGenericHttpException)
            {
                throw;
            }
            catch (Exception)
            {
                // log the ex.
                throw new UIGenericHttpException(System.Net.HttpStatusCode.InternalServerError);
            }
        }

        public async Task<TResult> GetObjectFromJsonResultAsync<TResult>(HttpResponseMessage result)
        {
            var s = await result.Content.ReadAsStringAsync();
            if (typeof(TResult) == typeof(string))
                return (TResult)Convert.ChangeType(s, typeof(TResult));

            return JsonSerializer.Deserialize<TResult>(s);
        }

        /// <summary>
        /// Gets an exception according to the status code of a response, and adds the content of the message to the
        /// exception if its available.
        /// If the message has a successful status, null is returned.
        /// </summary>
        /// <param name="message"></param>
        public async Task<Exception> GetExceptionFromResultAsync(HttpResponseMessage message)
        {
            if (message.IsSuccessStatusCode)
                return null;

            var messageContents = default(string);
            if (message.Content != null)
                messageContents = await message.Content.ReadAsStringAsync();

            messageContents = StringHelper.FormatJsonString(messageContents);

            if (message.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return new UIUnauthorizedException();

            if (string.IsNullOrEmpty(messageContents))
                return new UIGenericHttpException(message.StatusCode);
            else
                return new UIGenericHttpException(message.StatusCode, messageContents);
        }

        /// <summary>
        /// Boxes an exception into a json (GenericJsonResponse) object, suitable for using by clients, with a Result = false property set.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected GenericJsonResponse BoxError(UIGenericHttpException ex)
        {
            var m = ex.Message;
            var ret = new GenericJsonResponse() { Result = false, Message = m, StatusCode = (int)HttpStatusCode.InternalServerError };

            var hre = ex as UIGenericHttpException;
            if (hre != null)
                ret.StatusCode = (int)hre.StatusCode;

            return ret;
        }

        /// <summary>
        /// Creates a GenericJsonResponse object which contains the content to be returned to the client (i.e.: js scripts), 
        /// plus a Result property set, which indicates whether the result represents a successful operation or not.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        protected GenericJsonResponse BoxContents(bool result, object content = null, string message = null)
        {
            var resp = new GenericJsonResponse() { Result = result, Contents = content, Message = message };
            if (result)
                resp.StatusCode = (int)System.Net.HttpStatusCode.OK;
            else
                resp.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            return resp;
        }

        protected IActionResult JsonResult(bool result, object content = null, string message = null)
        {
            return new OkObjectResult(BoxContents(result, content, message));
        }
    }
}