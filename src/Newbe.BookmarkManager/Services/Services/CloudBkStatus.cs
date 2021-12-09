using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services;

public record CloudBkStatus(bool HasChanged, GetCloudOutput? GetCloudOutput);