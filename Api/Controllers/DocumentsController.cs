using Api.Authorization;
using Api.DTO;
using Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        public DocumentsController()
        {
        }

        private UserState UserState
        {
            get
            {
                return (UserState)HttpContext?.Items["UserState"];
            }
        }

        [HttpGet("{docId:int}")]
        [ProducesResponseType(StatusCodes.Status404NotFound),
            ProducesResponseType(StatusCodes.Status200OK)]
        [AuthorizeRole(UserRoles.EmployeeRole, UserRoles.AdministratorRole)] // allow use by admins and employees
        public ActionResult<DocumentDTO> GetDocument(int docId)
        {
            // here you would obtains a Document object from a DB (i.e. using EF Core DBContext), which is not included in this example.
            // Then, using a tool like AutoMapper, you'd convert the Document object to a DocumentDTO object,
            // suitable for use by clients.
            // It's recommended that model objects visibility is kept private (like EF classes), and use DTO objects to expose
            // a logical model to the outside.
            var docDto = new DocumentDTO();
            docDto.Id = docId; // just copy the requested id to the Id propety, for demo purposes.

            if (docId == 1)
                docDto.Contents = "My first test document";
            else
                docDto.Contents = $"Another document with id {docId}";

            return Ok(docDto);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK),
            ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AuthorizeRole(UserRoles.AdministratorRole)] // allow use by admins only
        public ActionResult DeleteDocument(int id)
        {
            if (id > 1)
                return Ok();
            else
                return NotFound();
        }
    }
}