using raBooth.Core.Services.Web;
using RestSharp;

namespace raBooth.Infrastructure.Services.Web
{
    public class WebHostApiClientConfiguration
    {
        public string BaseUrl { get; init; }
    }
    public class WebHostApiClient : IWebHostApiClient
    {
        private readonly WebHostApiClientConfiguration _configuration;
        private IRestClient _client;

        public WebHostApiClient(WebHostApiClientConfiguration configuration)
        {
            _configuration = configuration;
            _client = new RestClient(_configuration.BaseUrl);
        }

        public async Task<CreateCollage.Result> CreateCollage(CreateCollage.Command command, CancellationToken cancellationToken)
        {
            var request = new RestRequest(@"api/collage/create", Method.Post);
            request.AddJsonBody(command);
            var response = await _client.PostAsync<CreateCollage.Result>(request, cancellationToken);
            return response;
        }

        public async Task<AddSourceCollagePhoto.Result> AddSourceCollagePhoto(AddSourceCollagePhoto.Command command, CancellationToken cancellationToken)
        {
            var request = new RestRequest(@$"api/collage/{command.CollageId}/add-source-photo", Method.Post);
            request.AddJsonBody(command);
            var response = await _client.PostAsync<AddSourceCollagePhoto.Result>(request, cancellationToken);
            return response;
        }

    }
}
