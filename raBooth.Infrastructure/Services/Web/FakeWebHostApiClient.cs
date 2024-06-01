using raBooth.Core.Services.Web;

namespace raBooth.Infrastructure.Services.Web;

public class FakeWebHostApiClient : IWebHostApiClient
{
    public async Task<CreateCollage.Result> CreateCollage(CreateCollage.Command command, CancellationToken cancellationToken)
    {
        await Task.Delay(200);
        return new CreateCollage.Result(Guid.NewGuid());
    }

    public async Task<AddSourceCollagePhoto.Result> AddSourceCollagePhoto(AddSourceCollagePhoto.Command command, CancellationToken cancellationToken)
    {
        await Task.Delay(200);
        return new AddSourceCollagePhoto.Result();
    }
}