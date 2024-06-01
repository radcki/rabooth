using MediatR;
using Microsoft.AspNetCore.Mvc;
using raBooth.Web.Core.Features.Collage.Commands;

namespace raBooth.Web.Host.ApiControllers
{
    [ApiController]
    [Route("api/collage")]
    public class CollageController(IMediator mediator) : Controller
    {

        [HttpPost("create")]
        public async Task<CreateCollage.Result> Create([FromForm] CreateCollage.Command command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken);
        }

        [HttpPost("{collageId}/add-source-photo")]
        public async Task<AddSourcePhoto.Result> AddSourcePhoto([FromForm] AddSourcePhoto.Command command, CancellationToken cancellationToken)
        {
            return await mediator.Send(command, cancellationToken);
        }
        
    }
}
