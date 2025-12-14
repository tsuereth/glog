using System;
using System.Collections.Generic;
using System.Linq;

namespace GlogGenerator.Data
{
    public class GlogReferenceableLookup<T> where T : class, IGlogReferenceable
    {
        private Dictionary<string, T> dataById = new Dictionary<string, T>();

        private Dictionary<string, string> idsByReferenceableKey = new Dictionary<string, string>();

        public GlogReferenceableLookup() { }

        public GlogReferenceableLookup(IEnumerable<T> initialData)
        {
            foreach (T data in initialData)
            {
                this.AddData(data);
            }
        }

        public void AddData(T data)
        {
            var dataId = data.GetDataId();
            if (string.IsNullOrEmpty(dataId))
            {
                throw new ArgumentException($"Cannot add a data item with an empty ID");
            }
            if (this.dataById.ContainsKey(dataId))
            {
                throw new ArgumentException($"A data item with ID {dataId} already exists");
            }

            var normalizedKey = UrlizedString.Urlize(data.GetReferenceableKey());
            if (string.IsNullOrEmpty(normalizedKey))
            {
                throw new ArgumentException($"Cannot add a data item with an empty referenceable key");
            }
            if (this.idsByReferenceableKey.ContainsKey(normalizedKey))
            {
                throw new ArgumentException($"A data item with normalized referenceable key {normalizedKey} already exists");
            }

            this.dataById.Add(dataId, data);
            this.idsByReferenceableKey.Add(normalizedKey, dataId);
        }

        public void RemoveDataById(string id)
        {
            if (!this.dataById.TryGetValue(id, out var data))
            {
                throw new ArgumentException($"No {typeof(T).Name} found with ID {id}");
            }

            var normalizedKey = UrlizedString.Urlize(data.GetReferenceableKey());
            this.idsByReferenceableKey.Remove(normalizedKey);
            this.dataById.Remove(id);
        }

        public void RemoveDataByReferenceableKey(string key)
        {
            var normalizedKey = UrlizedString.Urlize(key);
            if (!this.idsByReferenceableKey.TryGetValue(normalizedKey, out var id))
            {
                throw new ArgumentException($"No {typeof(T).Name} found with key {normalizedKey}");
            }

            this.idsByReferenceableKey.Remove(normalizedKey);
            this.dataById.Remove(id);
        }

        public bool TryGetDataById(string id, out T data)
        {
            return this.dataById.TryGetValue(id, out data);
        }

        public T GetDataById(string id)
        {
            if (!this.dataById.TryGetValue(id, out var data))
            {
                throw new ArgumentException($"No {typeof(T).Name} found with ID {id}");
            }

            return data;
        }

        public bool HasDataByReferenceableKey(string key)
        {
            var normalizedKey = UrlizedString.Urlize(key);
            return this.idsByReferenceableKey.ContainsKey(normalizedKey);
        }

        public bool TryGetDataByReferenceableKey(string key, out T data)
        {
            var normalizedKey = UrlizedString.Urlize(key);
            if (!this.idsByReferenceableKey.TryGetValue(normalizedKey, out var id))
            {
                data = null;
                return false;
            }

            data = this.dataById[id];
            return true;
        }

        public T GetDataByReferenceableKey(string key)
        {
            var normalizedKey = UrlizedString.Urlize(key);
            if (!this.idsByReferenceableKey.TryGetValue(normalizedKey, out var id))
            {
                throw new ArgumentException($"No {typeof(T).Name} found with key {key}");
            }

            return this.GetDataById(id);
        }

        public ISet<string> GetIds()
        {
            return this.dataById.Keys.ToHashSet();
        }

        public List<T> GetValues()
        {
            return this.dataById.Values.ToList();
        }

        public void Clear()
        {
            dataById.Clear();
            idsByReferenceableKey.Clear();
        }
    }
}
