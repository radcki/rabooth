using MediatR;
using Microsoft.EntityFrameworkCore;
using raBooth.Web.Core.DataAccess;
using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Features.Collage.Queries
{
    public class ListCollages
    {
        public record Command(DateTime? DateFrom, DateTime? DateTo, int Page = 1, int PageSize = 10) : IRequest<Result>;

        public record Result(List<CollageDto> Data, int Page, int PageSize, int Total) : BaseResponse;


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
                var page = Math.Max(1, request.Page);
                var skip = (page - 1) * request.PageSize;
                var collageQuery = db.Collages
                                     .AsNoTracking()
                                     .Include(x => x.SourcePhotos)
                                     .OrderBy(x => x.CaptureDateTime)
                                     .AsQueryable();
                if (request.DateFrom.HasValue)
                {
                    var fromDate = request.DateFrom.Value;
                    collageQuery = collageQuery.Where(x => x.CaptureDateTime >= fromDate);
                }

                if (request.DateTo.HasValue)
                {
                    var toDate = request.DateTo.Value;
                    collageQuery = collageQuery.Where(x => x.CaptureDateTime <= toDate);
                }

                var total = collageQuery.Count();
                var dataQuery = collageQuery
                               .Skip(skip)
                               .Take(request.PageSize);
                var sql = dataQuery.ToQueryString();
                var data = dataQuery.ToList();

                var dtos = data.Select(collage =>
                                           new CollageDto()
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
                                                                                               })
                                                                     .ToList()
                                           }
                                      )
                               .ToList();


                return new Result(dtos, page, request.PageSize, total);
            }
        }
    }
}