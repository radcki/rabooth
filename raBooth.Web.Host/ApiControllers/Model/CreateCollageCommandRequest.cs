namespace raBooth.Web.Host.ApiControllers.Model
{
    public class CreateCollageCommandRequest
    {
        public DateTime CaptureDate { get; set; }
        public IFormFile Image { get; set; }
    }
}
