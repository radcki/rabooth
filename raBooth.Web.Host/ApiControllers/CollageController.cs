using MediatR;
using Microsoft.AspNetCore.Mvc;
using raBooth.Web.Core.Features.Collage.Commands;
using raBooth.Web.Host.ApiControllers.Model;
using raBooth.Web.Host.Infrastructure;

namespace raBooth.Web.Host.ApiControllers
{
    [ApiController]
    [Route("api/collage")]
    public class CollageController(IMediator mediator, IFormFileEnvelopeMapper fileMapper) : Controller
    {


        [HttpPost("create")]
        public async Task<CreateCollage.Result> Create([FromForm] CreateCollageCommandRequest request, CancellationToken cancellationToken)
        {
            var fileEnvelope = fileMapper.MapFormFile(request.Image);
            var command = new CreateCollage.Command(request.CaptureDate, fileEnvelope);
            return await mediator.Send(command, cancellationToken);
        }

        [HttpPost("{collageId}/add-source-photo")]
        public async Task<AddSourcePhoto.Result> AddSourcePhoto([FromRoute] Guid collageId, [FromForm] AddSourcePhotoCommandRequest request, CancellationToken cancellationToken)
        {
            var fileEnvelope = fileMapper.MapFormFile(request.Image);
            var command = new AddSourcePhoto.Command(collageId, fileEnvelope, request.CaptureDate);
            return await mediator.Send(command, cancellationToken);
        }
        
    }
}
