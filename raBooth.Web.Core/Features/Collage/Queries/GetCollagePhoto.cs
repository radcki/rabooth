using MediatR;
using Microsoft.EntityFrameworkCore;
using raBooth.Web.Core.DataAccess;
using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Features.Collage.Queries
{
    public class GetCollagePhoto
    {
        public record Command(Guid CollageId, Guid PhotoId) : IRequest<Result>;

        public class Result() : BaseResponse
        {
            public CollagePhotoDto CollagePhoto { get; init; }
        }

        public class CollagePhotoDto
        {
            public Guid PhotoId { get; set; }
            public int Index { get; set; }
            public byte[] Data { get; set; }
        }

        public class Handler(IDatabaseContext db, IPhotoStorage photoStorage) : IRequestHandler<Command, Result>
        {

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var collagePhoto = await db.Collages
                                .AsNoTracking()
                                .Include(x => x.SourcePhotos)
                                .Where(x => x.CollageId == request.CollageId)
                                .SelectMany(x=>x.SourcePhotos)
                                .FirstOrDefaultAsync(x=>x.PhotoId == request.PhotoId, cancellationToken: cancellationToken)
                              ?? throw new KeyNotFoundException($"Not found collage with id {request.CollageId}");

                var data = await photoStorage.GetImage(collagePhoto);

                return new Result
                {
                    CollagePhoto = new CollagePhotoDto()
                                   {
                                       Index = collagePhoto.Index,
                                       PhotoId = collagePhoto.PhotoId,
                                       Data = data
                                   }
                };
            }
        }
    }
}
