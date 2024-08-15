namespace raBooth.Web.Core.Types
{
    public abstract record IdResponse<T> : BaseResponse
    {
        public T Id { get; set; }
    }
}