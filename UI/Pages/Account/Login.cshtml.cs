using Api.Common;
using Api.Model;
using UI.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;

namespace UI.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModelBase<LoginModel>
    {
        public LoginModel(IHttpClientFactory clientFactory) : base(clientFactory)
        {
        }

        public class InputModel
        {
            [Required]
            public string username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string password { get; set; }
        }

        public string ReturnUrl { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync([FromBody] InputModel input, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var validations = new Dictionary<string, string>();

            try
            {
                if (!ModelState.IsValid)
                    return JsonResult(false, validations, "Invalid data. Please check the form values.");

                var client = GetClient(HttpContext, true);
                
                // pass true to dontAuthenticate to avoid jwt auth, since this is for getting a token
                var authResult = await GetContents<AuthResult>(HttpContext, $"api/auth/gettoken?userName={input.username}&password={input.password}", dontAuthenticate: true);
                
                var userState = await JwtOnCookieHelper.SignInUsingJwt(authResult, HttpContext);

                return new OkObjectResult(BoxContents(true, new { returnUrl = returnUrl }));
            }
            catch (UIGenericHttpException ex)
            {
                // log ex.
                return StatusCode((int)ex.StatusCode, BoxError(ex));
            }
        }
    }
}