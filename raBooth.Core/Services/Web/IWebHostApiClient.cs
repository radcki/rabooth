using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raBooth.Core.Services.Web
{
    public interface IWebHostApiClient
    {
        Task<CreateCollage.Result> CreateCollage(CreateCollage.Command command);
        Task<AddSourceCollagePhoto.Result> AddSourceCollagePhoto(AddSourceCollagePhoto.Command command);
    }

    public class CreateCollage
    {
        public record Command();
        public record Result(Guid CollageId);
    }
    public class AddSourceCollagePhoto
    {
        public record Command(Guid CollageId);
        public record Result();
    }

}
