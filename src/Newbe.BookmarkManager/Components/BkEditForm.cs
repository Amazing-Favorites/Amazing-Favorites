using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Newbe.BookmarkManager.Components
{
    public partial class BkEditForm
    {
        [Parameter] public string Url { get; set; } = null!;
        [Parameter] public string Title { get; set; } = null!;
        [Parameter] public HashSet<string> Tags { get; set; } = null!;
        [Parameter] public string[] AllTags { get; set; } = Array.Empty<string>();

        [Parameter] public EventCallback<string> TitleChanged { get; set; }

        private void OnRemovingTag(string tag)
        {
            Tags.Remove(tag);
        }

        private void OnNewTagsAddAsync(TagInput.NewTagArgs args)
        {
            foreach (var tag in args.Tags)
            {
                Tags.Add(tag);
            }
        }
    }
}