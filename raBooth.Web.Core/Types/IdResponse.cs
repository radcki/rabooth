namespace raBooth.Web.Core.Types
{
    public abstract class IdResponse<T> : BaseResponse
    {
        public T Id { get; set; }
    }
}