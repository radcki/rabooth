using MediatR;
using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Features.Collage.Commands;

public class CreateCollage
{
    public record Command(DateTime CaptureDate) : IRequest<Result>;

    public class Result : IdResponse<Guid>
    {
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            return new Result { Id = Guid.NewGuid() };
        }
    }
}