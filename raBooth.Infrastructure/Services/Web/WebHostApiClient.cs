using raBooth.Core.Services.Web;
using RestSharp;

namespace raBooth.Infrastructure.Services.Web
{
    public class WebHostApiClientConfiguration
    {
        public static string SectionName = "WebHostApiConfiguration";
        public string BaseUrl { get; init; }
        public string ApiKey { get; init; }
    }
    public class WebHostApiClient : IWebHostApiClient
    {
        private readonly WebHostApiClientConfiguration _configuration;
        private readonly IRestClient _client;

        public WebHostApiClient(WebHostApiClientConfiguration configuration)
        {
            _configuration = configuration;
            _client = new RestClient(_configuration.BaseUrl);
            _client.AddDefaultHeader("X-API-KEY", _configuration.ApiKey);
        }

        public async Task<CreateCollage.Result> CreateCollage(CreateCollage.Command command, CancellationToken cancellationToken)
        {
            //var request = new RestRequest(@"api/collage/create", Method.Post);
            //request.AddJsonBody(command);

            var request = new RestRequest("api/collage/create", Method.Post) { RequestFormat = DataFormat.Json, AlwaysMultipartFormData = true };
            request.AddParameter(nameof(command.CollageId), command.CollageId.ToString());
            request.AddParameter(nameof(command.CaptureDate), command.CaptureDate.ToString("u"));
            request.AddFile(nameof(command.Image), command.Image, "collage.jpg");

            var response = await _client.PostAsync<CreateCollage.Result>(request, cancellationToken);
            return response with { PageUrl = $"{_configuration.BaseUrl}{response.PageUrl}" };
        }

        public async Task<AddSourceCollagePhoto.Result> AddSourceCollagePhoto(AddSourceCollagePhoto.Command command, CancellationToken cancellationToken)
        {
            var request = new RestRequest(@$"api/collage/{command.CollageId}/add-source-photo", Method.Post);
            request.AddFile(nameof(command.Image), command.Image, "image.jpg");
            request.AddParameter(nameof(command.CaptureDate), command.CaptureDate.ToString("u"));
            var response = await _client.PostAsync<AddSourceCollagePhoto.Result>(request, cancellationToken);
            return response;
        }

    }
}
