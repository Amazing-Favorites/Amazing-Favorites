namespace Newbe.BookmarkManager.Services.RPC.Handlers
{
    public class SampleRequest : IRequest
    {
        public int Count { get; set; }

        public string Name { get; set; }
    }

    public class SampleResponse
    {
        public int Count { get; set; }

        public string Name { get; set; }
    }
}