﻿namespace raBooth.Web.Host.ApiControllers.Model;

public class AddSourcePhotoCommandRequest
{
    public IFormFile Image { get; set; }
    public DateTime CaptureDate { get; set; }
}