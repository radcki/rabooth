using MediatR;
using Microsoft.EntityFrameworkCore;
using raBooth.Web.Core.DataAccess;
using raBooth.Web.Core.Entities;
using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Features.Collage.Queries
{
    public class GetCollagePhoto
    {
        public record Request(Guid CollageId, Guid PhotoId) : IRequest<Result>;

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

        public class Handler(IDatabaseContext db, IPhotoStorage photoStorage) : IRequestHandler<Request, Result>
        {

            public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
            {
                var collage = await db.Collages
                                .AsNoTracking()
                                .Include(x => x.SourcePhotos)
                                .FirstOrDefaultAsync(x => x.CollageId == request.CollageId, cancellationToken)
                              ?? throw new KeyNotFoundException($"Not found collage with id {request.CollageId}");


                IPhoto photo = collage.CollagePhoto.PhotoId == request.PhotoId 
                                   ? collage.CollagePhoto 
                                   : collage.SourcePhotos.FirstOrDefault(x => x.PhotoId == request.PhotoId)
                                     ?? throw new KeyNotFoundException($"Not found collage photo with id {request.PhotoId}");

                var data = await photoStorage.GetImage(photo);

                return new Result
                {
                    CollagePhoto = new CollagePhotoDto()
                                   {
                                       Index = photo.Index,
                                       PhotoId = photo.PhotoId,
                                       Data = data
                                   }
                };
            }
        }
    }
}
