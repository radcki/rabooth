using MediatR;
using raBooth.Web.Core.DataAccess;
using raBooth.Web.Core.Entities;
using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Features.Collage.Commands;

public class CreateCollage
{
    public record Command(Guid CollageId, DateTime CaptureDate, FileDto Image) : IRequest<Result>;

    public class Result() : BaseResponse
    {
        public Guid CollageId { get; init; }
    }

    public class Handler(IDatabaseContext db, IPhotoStorage photoStorage) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var collage = new Entities.Collage()
                          {
                              CollageId = request.CollageId,
                              CaptureDateTime = request.CaptureDate
                          };
            var collagePhoto = new CollagePhoto()
                               {
                                   PhotoId = Guid.NewGuid(),
                                   CollageId = collage.CollageId,
                                   Index = 0,
                                   CaptureDateTime = request.CaptureDate,
                               };
            collage.CollagePhoto = collagePhoto;

            db.Collages.Add(collage);
            await db.SaveChangesAsync(cancellationToken);
            await photoStorage.StoreImage(collagePhoto, collage, request.Image.Data);

            return new Result
                   {
                       CollageId = collage.CollageId
                   };
        }
    }
}