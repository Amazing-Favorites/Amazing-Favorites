namespace Newbe.BookmarkManager.Services.RPC
{
    public interface IRequest : IRequest<Unit> { }
    
    public interface IRequest<out TResponse> :IBaseRequest{}
    
    public interface IBaseRequest{}
}