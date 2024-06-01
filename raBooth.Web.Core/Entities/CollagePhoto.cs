using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Entities;

public class CollagePhoto : BaseEntity
{
    public Guid CollagePhotoId { get;set; }
    public Guid CollageId { get;set; }
    public DateTime CaptureDateTime { get; set; } = new();
    public DateTime AddedDateTime { get; set; } = new();
}