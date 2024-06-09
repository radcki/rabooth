namespace raBooth.Web.Core.Entities;

public interface IPhoto
{
    Guid PhotoId { get; }
    int Index { get; }
    Guid CollageId { get; }
    public DateTime CaptureDateTime { get; }
}