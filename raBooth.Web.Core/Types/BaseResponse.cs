namespace raBooth.Web.Core.Types
{
    public abstract record BaseResponse
    {
        public DateTime ServerTime => DateTime.Now;
    }
}