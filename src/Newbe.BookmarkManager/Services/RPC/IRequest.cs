namespace Newbe.BookmarkManager.Services.RPC
{
    public interface IRequest
    {
        
    }
    
    
    public interface IRequest<out TResponse> :IRequest{}
}