using MediatR;
using Microsoft.EntityFrameworkCore;
using raBooth.Web.Core.DataAccess;
using raBooth.Web.Core.Entities;
using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Features.Collage.Commands;

public class AddSourcePhoto
{
    public record Command(Guid CollageId, FileDto FileDto, DateTime CaptureDate) : IRequest<Result>;

    public record Result : BaseResponse
    {
    }

    public class Handler(IDatabaseContext db, IPhotoStorage photoStorage) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {

            var collage = db.Collages.Include(x => x.SourcePhotos).FirstOrDefault(x => x.CollageId == request.CollageId)
                          ?? throw new KeyNotFoundException($"Not found collage with id {request.CollageId}");

            var maxIndex = collage.SourcePhotos.Select(x => x.Index).DefaultIfEmpty(0).Max();

            var sourcePhoto = new CollageSourcePhoto()
            {
                PhotoId = Guid.NewGuid(),
                Index = maxIndex + 1,
                CaptureDateTime = request.CaptureDate,
            };
            collage.SourcePhotos.Add(sourcePhoto);
            await db.SaveChangesAsync(cancellationToken);
            await photoStorage.StoreImage(sourcePhoto, collage, request.FileDto.Data);

            return new Result { };
        }
    }
}