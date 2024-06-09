using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using raBooth.Web.Core.Features.Collage.Commands;
using raBooth.Web.Core.Features.Collage.Queries;
using raBooth.Web.Host.ApiControllers.Model;
using raBooth.Web.Host.Infrastructure;

namespace raBooth.Web.Host.ApiControllers
{
    [ApiController]
    [Route("api/collage")]
    public class CollageController(IMediator mediator, IFormFileEnvelopeMapper fileMapper) : Controller
    {


        [ApiKey]
        [HttpPost("create")]
        public async Task<CreateCollageCommandResponse> Create([FromForm] CreateCollageCommandRequest request, CancellationToken cancellationToken)
        {
            var fileEnvelope = fileMapper.MapFormFile(request.Image);
            var command = new CreateCollage.Command(request.CaptureDate, fileEnvelope);
            var result = await mediator.Send(command, cancellationToken);
            var viewUrl = Url.Page(@"/Collage", new { CollageId = result.CollageId });
            return new CreateCollageCommandResponse
            {
                CollageId = result.CollageId,
                PageUrl = viewUrl
            };
        }

        [ApiKey]
        [HttpPost("{collageId}/add-source-photo")]
        public async Task<AddSourcePhoto.Result> AddSourcePhoto([FromRoute] Guid collageId, [FromForm] AddSourcePhotoCommandRequest request, CancellationToken cancellationToken)
        {
            var fileEnvelope = fileMapper.MapFormFile(request.Image);
            var command = new AddSourcePhoto.Command(collageId, fileEnvelope, request.CaptureDate);
            return await mediator.Send(command, cancellationToken);
        }

        [AllowAnonymous]
        [HttpGet("{collageId}/photo-data/{photoId}")]
        public async Task<FileResult> GetPhotoData([FromRoute] Guid collageId, [FromRoute] Guid photoId, CancellationToken cancellationToken)
        {
            var command = new GetCollagePhoto.Request(collageId, photoId);
            var result = await mediator.Send(command, cancellationToken);
            return File(result.CollagePhoto.Data, "image/jpeg");
        }

    }
}
