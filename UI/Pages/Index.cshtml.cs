using Api.Common;
using Api.Common.Authorization;
using Api.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace UI.Pages
{
    [Authorize]
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(IHttpClientFactory clientFactory) : base(clientFactory)
        {
        }

        public async Task<IActionResult> OnGetGetDocument() {
            // pass true to dontAuthenticate to avoid jwt auth, since this is for getting a token
            var document = await GetContents<DocumentDTO>(HttpContext, $"api/documents/1");
            return new OkObjectResult(BoxContents(true, document));
        }

        public async Task<IActionResult> OnDeleteDeleteDocument()
        {
            var apiUser = HttpContext.User as ApiClaimsPrincipal;
            if (apiUser != null && apiUser.HasRole(RoleDTO.AdministratorRole))
            { 
                // do something
            }
            try
            {
                // pass true to dontAuthenticate to avoid jwt auth, since this is for getting a token
                // <string> is a dummy generic parameter, since we don't expect to get any results apart
                // from a status code, from GetContents
                await GetContents<string>(HttpContext, $"api/documents/2", HttpMethods.Delete); 
                return new OkObjectResult(BoxContents(true, null));
            }
            catch (UIGenericHttpException ex)
            {
                return new OkObjectResult(BoxContents(false, BoxError(ex)));
            }
        }
    }
}