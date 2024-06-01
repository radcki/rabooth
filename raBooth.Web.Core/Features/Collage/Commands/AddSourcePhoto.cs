using MediatR;
using raBooth.Web.Core.Entities;
using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Features.Collage.Commands;

public class AddSourcePhoto
{
    public record Command(Guid CollageId, FileEnvelope FileEnvelope) : IRequest<Result>;

    public class Result : BaseResponse
    {
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            return new Result {};
        }
    }
}