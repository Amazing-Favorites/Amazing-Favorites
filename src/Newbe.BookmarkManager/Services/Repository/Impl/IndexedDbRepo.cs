using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TG.Blazor.IndexedDB;

namespace Newbe.BookmarkManager.Services
{
    public class IndexedDbRepo<T, TKey> : IIndexedDbRepo<T, TKey> where T : IEntity<TKey>
    {
        private readonly ILogger<IndexedDbRepo<T, TKey>> _logger;
        private readonly IndexedDBManager _indexedDbManager;
        private readonly string _storeName;

        public IndexedDbRepo(
            ILogger<IndexedDbRepo<T, TKey>> logger,
            IndexedDBManager indexedDbManager)
        {
            _logger = logger;
            _indexedDbManager = indexedDbManager;
            _storeName = TableNameCache.StoreName;
        }

        private bool _cacheInvalid = true;
        private List<T> _cache = new();

        public virtual async Task<List<T>> GetAllAsync()
        {
            if (_cacheInvalid)
            {
                _cache = await _indexedDbManager.GetRecords<T>(_storeName);
                _cacheInvalid = false;
            }

            return _cache;
        }

        public virtual async Task UpsertAsync(T entity)
        {
            try
            {
                var record = await _indexedDbManager.GetRecordById<TKey, T>(_storeName, entity.Id);
                if (record == null)
                {
                    await _indexedDbManager.AddRecord(new StoreRecord<T>
                    {
                        Storename = _storeName,
                        Data = entity
                    });
                }
                else
                {
                    await _indexedDbManager.UpdateRecord(new StoreRecord<T>
                    {
                        Data = entity,
                        Storename = _storeName,
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error when upsert");
            }
            finally
            {
                _cacheInvalid = true;
            }
        }

        public virtual async Task<T?> GetAsync(TKey id)
        {
            var re = await _indexedDbManager.GetRecordById<TKey, T>(_storeName, id);
            return re;
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            try
            {
                var re = await _indexedDbManager.GetRecordById<TKey, T>(_storeName, id);
                if (re != null)
                {
                    await _indexedDbManager.DeleteRecord(_storeName, id);
                }
            }
            finally
            {
                _cacheInvalid = true;
            }
        }

        public async Task DeleteAllAsync()
        {
            try
            {
                var list = await _indexedDbManager.GetRecords<T>(_storeName);
                if (list?.Any() == true)
                {
                    foreach (var item in list)
                    {
                        await _indexedDbManager.DeleteRecord(_storeName, item.Id);
                    }
                }
            }
            finally
            {
                _cacheInvalid = true;
            }
        }

        private static class TableNameCache
        {
            static TableNameCache()
            {
                StoreName = GetStoreName();
            }

            // ReSharper disable once StaticMemberInGenericType
            public static string StoreName { get; }

            private static string GetStoreName()
            {
                var attr = typeof(T).GetCustomAttribute<TableAttribute>();
                if (attr == null)
                {
                    throw new Exception("There must be a TableAttribute on the entity");
                }

                return attr.Name;
            }
        }
    }
}