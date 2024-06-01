using MediatR;
using raBooth.Web.Core.Entities;
using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Features.Collage.Commands;

public class CreateCollage
{
    public record Command(DateTime CaptureDate, FileEnvelope Image) : IRequest<Result>;

    public class Result() : BaseResponse
    {
        public Guid CollageId { get; init;  }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            return new Result { CollageId = Guid.NewGuid() };
        }
    }
}