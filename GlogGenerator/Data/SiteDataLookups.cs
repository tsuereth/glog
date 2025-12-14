using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GlogGenerator.Data
{
    public class SiteDataLookups
    {
        public GlogReferenceableLookup<CategoryData> Categories { get; set; } = new GlogReferenceableLookup<CategoryData>();
        public GlogReferenceableLookup<GameData> Games { get; set; } = new GlogReferenceableLookup<GameData>();
        public GlogReferenceableLookup<PlatformData> Platforms { get; set; } = new GlogReferenceableLookup<PlatformData>();
        public GlogReferenceableLookup<RatingData> Ratings { get; set; } = new GlogReferenceableLookup<RatingData>();
        public GlogReferenceableLookup<TagData> Tags { get; set; } = new GlogReferenceableLookup<TagData>();

        public GlogReferenceableLookup<T> GetLookup<T>() where T : class, IGlogReferenceable
        {
            var dataType = typeof(T);

            if (dataType == typeof(CategoryData))
            {
                return this.Categories as GlogReferenceableLookup<T>;
            }
            else if (dataType == typeof(GameData))
            {
                return this.Games as GlogReferenceableLookup<T>;
            }
            else if (dataType == typeof(PlatformData))
            {
                return this.Platforms as GlogReferenceableLookup<T>;
            }
            else if (dataType == typeof(RatingData))
            {
                return this.Ratings as GlogReferenceableLookup<T>;
            }
            else if (dataType == typeof(TagData))
            {
                return this.Tags as GlogReferenceableLookup<T>;
            }
            else
            {
                throw new NotImplementedException($"Missing lookup for type {dataType.Name}");
            }
        }

        public void AddData<T>(T data) where T : class, IGlogReferenceable
        {
            this.GetLookup<T>().AddData(data);
        }

        public void RemoveDataById<T>(string id) where T : class, IGlogReferenceable
        {
            this.GetLookup<T>().RemoveDataById(id);
        }

        public void RemoveDataByReferenceableKey<T>(string key) where T : class, IGlogReferenceable
        {
            this.GetLookup<T>().RemoveDataByReferenceableKey(key);
        }

        public bool TryGetDataById<T>(string id, out T data) where T : class, IGlogReferenceable
        {
            return this.GetLookup<T>().TryGetDataById(id, out data);
        }

        public T GetDataById<T>(string id) where T : class, IGlogReferenceable
        {
            return this.GetLookup<T>().GetDataById(id);
        }

        public bool HasDataByReferenceableKey<T>(string key) where T : class, IGlogReferenceable
        {
            return this.GetLookup<T>().HasDataByReferenceableKey(key);
        }

        public bool TryGetDataByReferenceableKey<T>(string key, out T data) where T : class, IGlogReferenceable
        {
            return this.GetLookup<T>().TryGetDataByReferenceableKey(key, out data);
        }

        public T GetDataByReferenceableKey<T>(string key) where T : class, IGlogReferenceable
        {
            return this.GetLookup<T>().GetDataByReferenceableKey(key);
        }

        public ISet<string> GetIds<T>() where T : class, IGlogReferenceable
        {
            return this.GetLookup<T>().GetIds();
        }

        public List<T> GetValues<T>() where T : class, IGlogReferenceable
        {
            return this.GetLookup<T>().GetValues();
        }

        public void Clear<T>() where T : class, IGlogReferenceable
        {
            this.GetLookup<T>().Clear();
        }

        public void ClearAll()
        {
            this.Categories.Clear();
            this.Games.Clear();
            this.Platforms.Clear();
            this.Ratings.Clear();
            this.Tags.Clear();
        }
    }
}
