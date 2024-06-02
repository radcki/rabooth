using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Entities;

public class CollageSourcePhoto : BaseEntity, IPhoto
{
    public int CollageSourcePhotoId { get; set; }
    public Guid PhotoId { get; set; }
    public Guid CollageId { get; set; }
    public int Index { get; set; }
    public DateTime CaptureDateTime { get; set; }
    public DateTime AddedDateTime { get; set; } = DateTime.UtcNow;
}