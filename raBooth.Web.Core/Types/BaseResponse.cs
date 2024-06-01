namespace raBooth.Web.Core.Types
{
    public abstract class BaseResponse
    {
        public DateTime ServerTime => DateTime.Now;
    }
}