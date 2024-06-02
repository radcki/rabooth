using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raBooth.Core.Services.Web
{
    public interface IWebHostApiClient
    {
        Task<CreateCollage.Result> CreateCollage(CreateCollage.Command command, CancellationToken cancellationToken);
        Task<AddSourceCollagePhoto.Result> AddSourceCollagePhoto(AddSourceCollagePhoto.Command command, CancellationToken cancellationToken);
    }

    public class CreateCollage
    {
        public record Command(DateTime CaptureDate, byte[] Image);
        public record Result(Guid CollageId, string PageUrl);
    }
    public class AddSourceCollagePhoto
    {
        public record Command(Guid CollageId, DateTime CaptureDate, byte[] Image);
        public record Result();
    }

}
