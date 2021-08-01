using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Newbe.BookmarkManager.Services;

namespace Newbe.BookmarkManager.Components
{
    public partial class BkEditForm
    {
        [Parameter] public IBkEditFormData BkEditFormModel { get; set; }

        private void OnRemovingTag(string tag)
        {
            BkEditFormModel.Tags.Remove(tag);
        }

        private void OnNewTagsAddAsync(TagInput.NewTagArgs args)
        {
            foreach (var tag in args.Tags)
            {
                BkEditFormModel.Tags.Add(tag);
            }
        }
    }
}