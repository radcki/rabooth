using MediatR;
using Microsoft.EntityFrameworkCore;
using raBooth.Web.Core.DataAccess;
using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Features.Collage.Queries
{
    public class GetCollage
    {
        public record Command(Guid CollageId) : IRequest<Result>;

        public class Result() : BaseResponse
        {
            public CollageDto Collage { get; init; }
        }

        public class CollageDto
        {
            public Guid CollageId { get; set; }
            public DateTime CaptureDateTime { get; set; } = new();
            public DateTime AddedDateTime { get; set; } = new();
            public CollagePhotoDto CollagePhoto { get; set; }
            public List<CollagePhotoDto> SourcePhotos { get; set; } = new();
        }

        public class CollagePhotoDto
        {
            public Guid PhotoId { get; set; }
            public int Index { get; set; }
        }

        public class Handler(IDatabaseContext db) : IRequestHandler<Command, Result>
        {

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var collage = db.Collages
                                .AsNoTracking()
                                .Include(x => x.SourcePhotos)
                                .FirstOrDefault(x => x.CollageId == request.CollageId)
                              ?? throw new KeyNotFoundException($"Not found collage with id {request.CollageId}");

                var dto = new CollageDto()
                {
                    CollageId = collage.CollageId,
                    CaptureDateTime = collage.CaptureDateTime,
                    AddedDateTime = collage.AddedDateTime,
                    CollagePhoto = new CollagePhotoDto()
                    {
                        PhotoId = collage.CollagePhoto.PhotoId,
                        Index = collage.CollagePhoto.Index
                    },
                    SourcePhotos = collage.SourcePhotos.Select(x => new CollagePhotoDto()
                    {
                        PhotoId = x.PhotoId,
                        Index = x.Index
                    }).ToList()
                };

                return new Result
                {
                    Collage = dto
                };
            }
        }
    }
}
