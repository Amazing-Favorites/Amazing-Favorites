﻿using System.Threading.Tasks;
using Microsoft.Graph;
using Newbe.BookmarkManager.Services.Ai;
using static Newbe.BookmarkManager.Services.Ai.Events;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkSearcher
    {
        [Insight(EventName = BkSearchEvent)]
        Task<SearchResultItem[]> Search(string searchText, int limit);


        Task<SearchResultItem[]> RecentClicked(string searchText, int limit);
    }
}