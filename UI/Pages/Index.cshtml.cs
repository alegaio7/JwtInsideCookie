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
            try
            {
                // pass true to dontAuthenticate to avoid jwt auth, since this is for getting a token
                await GetContents<string>(HttpContext, $"api/documents/2", HttpMethods.Delete); // <string> is a dummy generic parameter
                return new OkObjectResult(BoxContents(true, null));
            }
            catch (UIGenericHttpException ex)
            {
                return new OkObjectResult(BoxContents(false, BoxError(ex)));
            }
        }
    }
}