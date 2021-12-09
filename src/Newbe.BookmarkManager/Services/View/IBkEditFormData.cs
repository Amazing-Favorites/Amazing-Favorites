using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services;

public interface IBkEditFormData
{
    public string Url { get; set; }
    public string Title { get; set; }
    public HashSet<string> Tags { get; set; }
    public string[] AllTags { get; set; }

    Task LoadAsync(string url, string title, string[] tags);
    Task SaveAsync();
    Task RemoveAsync();
}