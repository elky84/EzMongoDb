﻿using EzMongoDb.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EzMongoDb.Util
{
    public class MongoDbUtil<T> where T : MongoDbHeader
    {
        public IMongoCollection<T> Collection { get; set; }

        public MongoDbUtil(IMongoDatabase database)
        {
            Collection = database.GetCollection<T>(typeof(T).Name);
        }

        public MongoDbUtil(IMongoDatabase database, string name)
        {
            Collection = database.GetCollection<T>(name);
        }

        public async Task<List<T>> All() =>
                   (await Collection.FindAsync(t => true)).ToList();

        public async Task<long> CountAsync() => await Collection.CountDocumentsAsync(x => true);

        public async Task<long> CountAsync(FilterDefinition<T> filter) => await Collection.CountDocumentsAsync(filter);

        public async Task<List<T>> Page(FilterDefinition<T> filter, int limit, int offset, string sort = "", bool asc = false)
        {
            return await Collection.Find(filter)
                .Sort(string.IsNullOrEmpty(sort) ? null : new BsonDocument(sort, asc ? 1 : -1))
                .Skip(offset)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<List<T>> Page(FilterDefinition<T> filter, int limit, int offset, Dictionary<string, int> dict)
        {
            return await Collection.Find(filter)
                .Sort(new BsonDocument(dict))
                .Skip(offset)
                .Limit(limit)
                .ToListAsync();
        }

        public T FindOne(FilterDefinition<T> filter) =>
            Collection.Find(filter).FirstOrDefault();

        // ReSharper disable once MemberCanBePrivate.Global
        public async Task<T> FindOneAsync(FilterDefinition<T> filter) =>
            await Collection.Find(filter).FirstOrDefaultAsync();

        public async Task<T> FindOneAsyncById(string id) =>
            await Collection.Find(GetIdFilter(id)).FirstOrDefaultAsync();

        public async Task<List<T>> FindAsync() =>
            (await Collection.FindAsync(t => true)).ToList();

        public async Task<List<T>> FindAsync(FilterDefinition<T> filter) =>
            (await Collection.FindAsync(filter)).ToList();

        public List<T> Find(FilterDefinition<T> filter) =>
             Collection.Find(filter).ToList();

        public List<T> List(FilterDefinition<T> filter) => Collection.Find(filter).ToList();

        public T Create(T t)
        {
            Collection.InsertOne(t);
            return t;
        }

        public async Task<T> CreateAsync(T t)
        {
            await Collection.InsertOneAsync(t);
            return t;
        }

        public async Task<List<T>> CreateManyAsync(List<T> t)
        {
            await Collection.InsertManyAsync(t);
            return t;
        }

        public List<T> Create(List<T> t)
        {
            try
            {
                Collection.InsertMany(t);
            }
            catch (MongoBulkWriteException ex)
            {
                Console.Write(ex.Message);
            }
            return t;
        }

        public void Clear()
        {
            Collection.DeleteMany(x => true);
        }

        public T Update(string id, T t)
        {
            t.Id = id;
            t.Updated = DateTime.Now;
            Collection.ReplaceOne(GetIdFilter(id), t);
            return t;
        }

        public T Update(FilterDefinition<T> filter, T t)
        {
            t.Updated = DateTime.Now;
            Collection.ReplaceOne(filter, t);
            return t;
        }

        public async Task<T> UpsertAsync(FilterDefinition<T> filter, T t, Action<T> createAction = null)
        {
            var origin = await FindOneAsync(filter);
            if (origin != null)
            {
                t.Id = origin.Id;
                t.Created = origin.Created;
                t.Updated = DateTime.Now;
                return await UpdateAsync(origin.Id, t);
            }
            else
            {
                try
                {
                    var createdItem = await CreateAsync(t);
                    createAction?.Invoke(createdItem);
                    return createdItem;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("CreateAsync() Failed (in UpsertAsync)", ex);
                }
            }
        }

        public async Task<T> UpdateAsync(FilterDefinition<T> filter, T t)
        {
            t.Updated = DateTime.Now;
            var result = await Collection.ReplaceOneAsync(filter, t);
            return result.ModifiedCount > 0 ? t : null;
        }

        public async Task UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            await Collection.UpdateManyAsync(filter, update);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public async Task<T> UpdateAsync(string id, T t)
        {
            t.Id = id;
            t.Updated = DateTime.Now;
            var result = await Collection.ReplaceOneAsync(GetIdFilter(id), t);
            return result.ModifiedCount > 0 ? t : null;
        }

        public async Task<T> UpdateGetAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            return await Collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<T>
            {
                ReturnDocument = ReturnDocument.After,
            });
        }

        public async Task<bool> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            var result = await Collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<T> UpdateGetAsync(string id, UpdateDefinition<T> update)
        {
            return await Collection.FindOneAndUpdateAsync(GetIdFilter(id), update, new FindOneAndUpdateOptions<T>
            {
                ReturnDocument = ReturnDocument.After,
            });
        }

        public async Task<bool> UpdateAsync(string id, UpdateDefinition<T> update)
        {
            var result = await Collection.UpdateOneAsync(GetIdFilter(id), update);
            return result.ModifiedCount > 0;
        }

        private FilterDefinition<T> GetIdFilter(string id)
        {
            return Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        }

        public void Remove(string id) => Collection.DeleteOne(GetIdFilter(id));

        public void Remove(FilterDefinition<T> filter) => Collection.DeleteOne(filter);

        public async Task RemoveAsync(string id) => await Collection.DeleteOneAsync(GetIdFilter(id));

        public async Task RemoveAsync(FilterDefinition<T> filter) => await Collection.DeleteOneAsync(filter);

        public async Task<T> RemoveGetAsync(string id) => await Collection.FindOneAndDeleteAsync(GetIdFilter(id));

        public async Task<T> RemoveGetAsync(FilterDefinition<T> filter) => await Collection.FindOneAndDeleteAsync(filter);

        public async Task<bool> RemoveManyAsync(FilterDefinition<T> filter)
        {
            var result = await Collection.DeleteManyAsync(filter);
            return result.DeletedCount > 0;
        }

    }
}
