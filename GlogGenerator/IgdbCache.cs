using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GlogGenerator.Data;
using GlogGenerator.IgdbApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlogGenerator
{
    public class IgdbCache : IIgdbCache
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        private Dictionary<int, IgdbCollection> collectionsById = new Dictionary<int, IgdbCollection>();

        private Dictionary<int, IgdbCompany> companiesById = new Dictionary<int, IgdbCompany>();

        private Dictionary<int, IgdbFranchise> franchisesById = new Dictionary<int, IgdbFranchise>();

        private Dictionary<int, IgdbGame> gamesById = new Dictionary<int, IgdbGame>();

        private Dictionary<int, IgdbGameMode> gameModesById = new Dictionary<int, IgdbGameMode>();

        private Dictionary<int, IgdbGameType> gameTypesById = new Dictionary<int, IgdbGameType>();

        private Dictionary<int, IgdbGenre> genresById = new Dictionary<int, IgdbGenre>();

        private Dictionary<int, IgdbInvolvedCompany> involvedCompaniesById = new Dictionary<int, IgdbInvolvedCompany>();

        private Dictionary<int, IgdbKeyword> keywordsById = new Dictionary<int, IgdbKeyword>();

        private Dictionary<int, IgdbPlatform> platformsById = new Dictionary<int, IgdbPlatform>();

        private Dictionary<int, IgdbPlayerPerspective> playerPerspectivesById = new Dictionary<int, IgdbPlayerPerspective>();

        private Dictionary<int, IgdbReleaseDate> releaseDatesById = new Dictionary<int, IgdbReleaseDate>();

        private Dictionary<int, IgdbTheme> themesById = new Dictionary<int, IgdbTheme>();

        private Dictionary<int, HashSet<int>> gamesParentGameIds = new Dictionary<int, HashSet<int>>();

        private Dictionary<int, HashSet<int>> gamesOtherReleaseGameIds = new Dictionary<int, HashSet<int>>();

        private Dictionary<int, HashSet<int>> gamesChildGameIds = new Dictionary<int, HashSet<int>>();

        private Dictionary<int, HashSet<int>> gamesRelatedGameIds = new Dictionary<int, HashSet<int>>();

        public T GetEntity<T>(int id)
            where T : IgdbEntity
        {
            Dictionary<int, T> entitiesById;
            var entityType = typeof(T);
            if (entityType == typeof(IgdbCollection))
            {
                entitiesById = this.collectionsById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbCompany))
            {
                entitiesById = this.companiesById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbFranchise))
            {
                entitiesById = this.franchisesById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbGame))
            {
                entitiesById = this.gamesById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbGameMode))
            {
                entitiesById = this.gameModesById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbGameType))
            {
                entitiesById = this.gameTypesById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbGenre))
            {
                entitiesById = this.genresById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbInvolvedCompany))
            {
                entitiesById = this.involvedCompaniesById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbKeyword))
            {
                entitiesById = this.keywordsById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbPlatform))
            {
                entitiesById = this.platformsById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbPlayerPerspective))
            {
                entitiesById = this.playerPerspectivesById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbReleaseDate))
            {
                entitiesById = this.releaseDatesById as Dictionary<int, T>;
            }
            else if (entityType == typeof(IgdbTheme))
            {
                entitiesById = this.themesById as Dictionary<int, T>;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (entitiesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbCollection GetCollection(int id)
        {
            if (this.collectionsById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbCompany GetCompany(int id)
        {
            if (this.companiesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbFranchise GetFranchise(int id)
        {
            if (this.franchisesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbGame GetGame(int id)
        {
            if (this.gamesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public List<IgdbGame> GetAllGames()
        {
            return this.gamesById.Values.ToList();
        }

        public IgdbGameMode GetGameMode(int id)
        {
            if (this.gameModesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbGameType GetGameType(int id)
        {
            if (this.gameTypesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbGenre GetGenre(int id)
        {
            if (this.genresById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbInvolvedCompany GetInvolvedCompany(int id)
        {
            if (this.involvedCompaniesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbKeyword GetKeyword(int id)
        {
            if (this.keywordsById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbPlatform GetPlatform(int id)
        {
            if (this.platformsById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public List<IgdbPlatform> GetAllPlatforms()
        {
            return this.platformsById.Values.ToList();
        }

        public IgdbPlayerPerspective GetPlayerPerspective(int id)
        {
            if (this.playerPerspectivesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbReleaseDate GetReleaseDate(int id)
        {
            if (this.releaseDatesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbTheme GetTheme(int id)
        {
            if (this.themesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public List<IgdbEntity> GetAllGameMetadata()
        {
            var allMetadata = new List<IgdbEntity>();

            allMetadata.AddRange(this.collectionsById.Values);
            allMetadata.AddRange(this.companiesById.Values);
            allMetadata.AddRange(this.franchisesById.Values);
            allMetadata.AddRange(this.gameModesById.Values);
            allMetadata.AddRange(this.gameTypesById.Values);
            allMetadata.AddRange(this.genresById.Values);
            allMetadata.AddRange(this.keywordsById.Values);
            allMetadata.AddRange(this.playerPerspectivesById.Values);
            allMetadata.AddRange(this.themesById.Values);

            return allMetadata;
        }

        public IEnumerable<int> GetParentGameIds(int gameId)
        {
            if (this.gamesParentGameIds.TryGetValue(gameId, out var parentGameIds))
            {
                return parentGameIds;
            }

            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> GetOtherReleaseGameIds(int gameId)
        {
            if (this.gamesOtherReleaseGameIds.TryGetValue(gameId, out var otherReleaseGameIds))
            {
                return otherReleaseGameIds;
            }

            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> GetChildGameIds(int gameId)
        {
            if (this.gamesChildGameIds.TryGetValue(gameId, out var childGameIds))
            {
                return childGameIds;
            }

            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> GetRelatedGameIds(int gameId)
        {
            if (this.gamesRelatedGameIds.TryGetValue(gameId, out var relatedGameIds))
            {
                return relatedGameIds;
            }

            return Enumerable.Empty<int>();
        }

        private static IEnumerable<int> IgdbGameAllAssociatedGameIds(IgdbGame game)
        {
            return game.GetParentGameIds()
                .Union(game.GetOtherReleaseGameIds())
                .Union(game.GetChildGameIds())
                .Union(game.GetRelatedGameIds());
        }

        private static void AppendIdToDictionarySet(Dictionary<int, HashSet<int>> dictionary, int key, int appendId)
        {
            AppendIdsToDictionarySet(dictionary, key, new[] { appendId });
        }

        private static void AppendIdsToDictionarySet(Dictionary<int, HashSet<int>> dictionary, int key, IEnumerable<int> appendIds)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, new HashSet<int>(appendIds));
            }
            else
            {
                dictionary[key].UnionWith(appendIds);
            }
        }

        private void RebuildAssociatedGamesIndexes()
        {
            this.gamesParentGameIds.Clear();
            this.gamesOtherReleaseGameIds.Clear();
            this.gamesChildGameIds.Clear();
            this.gamesRelatedGameIds.Clear();

            foreach (var game in this.gamesById.Values)
            {
                var gameIsBundle = this.GetGameType(game.GameTypeId)?.IsBundle() ?? false;

                var parentGameIds = game.GetParentGameIds();
                AppendIdsToDictionarySet(this.gamesParentGameIds, game.Id, parentGameIds);
                foreach (var parentGameId in parentGameIds)
                {
                    AppendIdToDictionarySet(this.gamesChildGameIds, parentGameId, game.Id);
                }

                var otherReleaseGameIds = game.GetOtherReleaseGameIds();
                foreach (var otherReleaseGameId in otherReleaseGameIds)
                {
                    if (this.gamesById.TryGetValue(otherReleaseGameId, out var otherRelease))
                    {
                        var otherReleaseIsBundle = this.GetGameType(otherRelease.GameTypeId)?.IsBundle() ?? false;

                        // Bundles and non-bundles shouldn't be considered "other releases" of one another.
                        if (gameIsBundle == otherReleaseIsBundle)
                        {
                            AppendIdToDictionarySet(this.gamesOtherReleaseGameIds, game.Id, otherReleaseGameId);
                            AppendIdToDictionarySet(this.gamesOtherReleaseGameIds, otherReleaseGameId, game.Id);
                        }
                    }
                }

                var childGameIds = game.GetChildGameIds();
                AppendIdsToDictionarySet(this.gamesChildGameIds, game.Id, childGameIds);
                foreach (var childGameId in childGameIds)
                {
                    AppendIdToDictionarySet(this.gamesParentGameIds, childGameId, game.Id);
                }

                var relatedGameIds = game.GetRelatedGameIds();
                AppendIdsToDictionarySet(this.gamesRelatedGameIds, game.Id, relatedGameIds);
                foreach (var relatedGameId in relatedGameIds)
                {
                    AppendIdToDictionarySet(this.gamesRelatedGameIds, relatedGameId, game.Id);
                }
            }
        }

        public async Task UpdateFromApiClient(IgdbApiClient client)
        {
            // Update games first, to get current IDs for metadata references.
            var gameIds = this.gamesById.Keys.ToList();
            var gamesCurrent = await client.GetGamesAsync(gameIds);

            // Extract associated game IDs (remasters, expansions, etc) and do one more update.
            var associatedGameIds = gamesCurrent.SelectMany(g => IgdbGameAllAssociatedGameIds(g)).Distinct();
            var newAssociatedGameIds = associatedGameIds.Except(gameIds).ToList();
            var associatedGames = await client.GetGamesAsync(newAssociatedGameIds);

            // To avoid problems integrating this update, DON'T use games which are missing important metadata.
            associatedGames = associatedGames.Where(g => g.GetFirstReleaseDate(this) != null).ToList();
            gamesCurrent.AddRange(associatedGames);

            var gamesCurrentById = gamesCurrent.ToDictionary(o => o.Id, o => o);
            this.gamesById = gamesCurrentById;

            // Update involvedCompanies to get most-current company IDs.
            var involvedCompanyIds = gamesCurrent.SelectMany(g => g.InvolvedCompanyIds).Distinct().ToList();
            var involvedCompaniesCurrent = await client.GetInvolvedCompaniesAsync(involvedCompanyIds);
            this.involvedCompaniesById = involvedCompaniesCurrent.ToDictionary(o => o.Id, o => o);

            var companyIds = involvedCompaniesCurrent.Select(i => i.CompanyId).Distinct().ToList();
            var companiesCurrent = await client.GetCompaniesAsync(companyIds);
            this.companiesById = companiesCurrent.ToDictionary(o => o.Id, o => o);

            // Now, update the rest of the ID-driven metadata.

            var collectionIds = gamesCurrent.SelectMany(g => g.CollectionIds).Distinct().ToList();
            var collectionsCurrent = await client.GetCollectionsAsync(collectionIds.Distinct().ToList());
            this.collectionsById = collectionsCurrent.ToDictionary(o => o.Id, o => o);

            var franchiseIds = gamesCurrent.Where(g => g.MainFranchiseId != IgdbFranchise.IdNotFound).Select(g => g.MainFranchiseId).Distinct().ToList();
            franchiseIds.AddRange(gamesCurrent.SelectMany(g => g.FranchiseIds).Distinct());
            var franchisesCurrent = await client.GetFranchisesAsync(franchiseIds.Distinct().ToList());
            this.franchisesById = franchisesCurrent.ToDictionary(o => o.Id, o => o);

            var gameModeIds = gamesCurrent.SelectMany(g => g.GameModeIds).Distinct().ToList();
            var gameModesCurrent = await client.GetGameModesAsync(gameModeIds);
            this.gameModesById = gameModesCurrent.ToDictionary(o => o.Id, o => o);

            var gameTypeIds = gamesCurrent.Where(g => g.GameTypeId != IgdbGameType.IdNotFound).Select(g => g.GameTypeId).Distinct().ToList();
            var gameTypesCurrent = await client.GetGameTypesAsync(gameTypeIds);
            this.gameTypesById = gameTypesCurrent.ToDictionary(o => o.Id, o => o);

            var genreIds = gamesCurrent.SelectMany(g => g.GenreIds).Distinct().ToList();
            var genresCurrent = await client.GetGenresAsync(genreIds);
            this.genresById = genresCurrent.ToDictionary(o => o.Id, o => o);

            // NOTE: As of writing, "keywords" are way too abundant and vague to be useful; ignore 'em.
#if false
            var keywordIds = gamesCurrent.SelectMany(g => g.KeywordIds).Distinct().ToList();
            var keywordsCurrent = await client.GetKeywordsAsync(keywordIds);
#else
            var keywordsCurrent = new List<IgdbKeyword>();
#endif
            this.keywordsById = keywordsCurrent.ToDictionary(o => o.Id, o => o);

            var platformIds = gamesCurrent.SelectMany(g => g.PlatformIds).Distinct().ToList();
            var platformsCurrent = await client.GetPlatformsAsync(platformIds);

            var platformsCurrentById = platformsCurrent.ToDictionary(o => o.Id, o => o);
            this.platformsById = platformsCurrentById;

            var playerPerspectiveIds = gamesCurrent.SelectMany(g => g.PlayerPerspectiveIds).Distinct().ToList();
            var playerPerspectivesCurrent = await client.GetPlayerPerspectivesAsync(playerPerspectiveIds);
            this.playerPerspectivesById = playerPerspectivesCurrent.ToDictionary(o => o.Id, o => o);

            // Note: we only care about IgdbReleaseDate data for games which are missing a direct FirstReleasedDate.
            var releaseDateIds = gamesCurrent.Where(g => g.FirstReleaseDateTimestamp == 0).SelectMany(g => g.ReleaseDateIds).Distinct().ToList();
            var releaseDatesCurrent = await client.GetReleaseDatesAsync(releaseDateIds);
            this.releaseDatesById = releaseDatesCurrent.ToDictionary(o => o.Id, o => o);

            var themeIds = gamesCurrent.SelectMany(g => g.ThemeIds).Distinct().ToList();
            var themesCurrent = await client.GetThemesAsync(themeIds);
            this.themesById = themesCurrent.ToDictionary(o => o.Id, o => o);

            this.RebuildAssociatedGamesIndexes();
        }

        private void RemoveEntityByIdInternal<T>(Dictionary<int, T> entitiesById, int id)
            where T : IgdbEntity
        {
            if (!entitiesById.TryGetValue(id, out var entity))
            {
                throw new ArgumentException($"No {typeof(T).Name} found with ID {id}");
            }

            if (!entity.ShouldForcePersistInCache())
            {
                entitiesById.Remove(id);
            }
        }

        public void RemoveEntityById(Type entityType, int id)
        {
            if (entityType == typeof(IgdbCollection))
            {
                this.RemoveEntityByIdInternal(this.collectionsById, id);
            }
            else if (entityType == typeof(IgdbCompany))
            {
                this.RemoveEntityByIdInternal(this.companiesById, id);
            }
            else if (entityType == typeof(IgdbFranchise))
            {
                this.RemoveEntityByIdInternal(this.franchisesById, id);
            }
            else if (entityType == typeof(IgdbGame))
            {
                this.RemoveEntityByIdInternal(this.gamesById, id);
            }
            else if (entityType == typeof(IgdbGameMode))
            {
                this.RemoveEntityByIdInternal(this.gameModesById, id);
            }
            else if (entityType == typeof(IgdbGameType))
            {
                this.RemoveEntityByIdInternal(this.gameTypesById, id);
            }
            else if (entityType == typeof(IgdbGenre))
            {
                this.RemoveEntityByIdInternal(this.genresById, id);
            }
            else if (entityType == typeof(IgdbInvolvedCompany))
            {
                this.RemoveEntityByIdInternal(this.involvedCompaniesById, id);
            }
            else if (entityType == typeof(IgdbKeyword))
            {
                this.RemoveEntityByIdInternal(this.keywordsById, id);
            }
            else if (entityType == typeof(IgdbPlatform))
            {
                this.RemoveEntityByIdInternal(this.platformsById, id);
            }
            else if (entityType == typeof(IgdbPlayerPerspective))
            {
                this.RemoveEntityByIdInternal(this.playerPerspectivesById, id);
            }
            else if (entityType == typeof(IgdbReleaseDate))
            {
                this.RemoveEntityByIdInternal(this.releaseDatesById, id);
            }
            else if (entityType == typeof(IgdbTheme))
            {
                this.RemoveEntityByIdInternal(this.themesById, id);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static string JsonFilePathForEntityType(string directoryPath, string typeName)
        {
            return Path.Combine(directoryPath, $"igdb_cache_{typeName}.json");
        }

        private static void WriteEntityTypeToJsonFile<T>(IEnumerable<T> entities, string directoryPath, string typeName)
            where T : IgdbEntity
        {
            var jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);
            var entitiesArray = JArray.FromObject(entities, jsonSerializer);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var jsonFilePath = JsonFilePathForEntityType(directoryPath, typeName);
            using (var fileStream = new FileStream(jsonFilePath, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream) { NewLine = "\n" })
            using (var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented })
            {
                jsonSerializer.Serialize(jsonWriter, entitiesArray);
                streamWriter.Flush();
            }
        }

        public void WriteToJsonFiles(string directoryPath)
        {
            WriteEntityTypeToJsonFile(this.collectionsById.Values.OrderBy(o => o.Id), directoryPath, "collections");
            WriteEntityTypeToJsonFile(this.companiesById.Values.OrderBy(o => o.Id), directoryPath, "companies");
            WriteEntityTypeToJsonFile(this.franchisesById.Values.OrderBy(o => o.Id), directoryPath, "franchises");
            WriteEntityTypeToJsonFile(this.gameModesById.Values.OrderBy(o => o.Id), directoryPath, "gameModes");
            WriteEntityTypeToJsonFile(this.gameTypesById.Values.OrderBy(o => o.Id), directoryPath, "gameTypes");
            WriteEntityTypeToJsonFile(this.gamesById.Values.OrderBy(o => o.Id), directoryPath, "games");
            WriteEntityTypeToJsonFile(this.genresById.Values.OrderBy(o => o.Id), directoryPath, "genres");
            WriteEntityTypeToJsonFile(this.involvedCompaniesById.Values.OrderBy(o => o.Id), directoryPath, "involvedCompanies");
            WriteEntityTypeToJsonFile(this.keywordsById.Values.OrderBy(o => o.Id), directoryPath, "keywords");
            WriteEntityTypeToJsonFile(this.platformsById.Values.OrderBy(o => o.Id), directoryPath, "platforms");
            WriteEntityTypeToJsonFile(this.playerPerspectivesById.Values.OrderBy(o => o.Id), directoryPath, "playerPerspectives");
            WriteEntityTypeToJsonFile(this.releaseDatesById.Values.OrderBy(o => o.Id), directoryPath, "releaseDates");
            WriteEntityTypeToJsonFile(this.themesById.Values.OrderBy(o => o.Id), directoryPath, "themes");
        }

        public static bool JsonFilesExist(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return false;
            }

            var checkForTypeNames = new List<string>() { "games", "platforms" };
            foreach (var typeName in checkForTypeNames)
            {
                var jsonFilePath = JsonFilePathForEntityType(directoryPath, typeName);
                if (!File.Exists(jsonFilePath))
                {
                    return false;
                }
            }

            return true;
        }

        private static List<T> ReadEntityTypeFromJsonFile<T>(string directoryPath, string typeName)
            where T : IgdbEntity
        {
            var jsonFilePath = JsonFilePathForEntityType(directoryPath, typeName);
            if (!File.Exists(jsonFilePath))
            {
                return new List<T>();
            }

            var cacheEntities = JArray.Parse(File.ReadAllText(jsonFilePath));
            return cacheEntities?.ToObject<List<T>>() ?? new List<T>();
        }

        public static IgdbCache FromJsonFiles(string directoryPath)
        {
            var cache = new IgdbCache();

            cache.collectionsById = ReadEntityTypeFromJsonFile<IgdbCollection>(directoryPath, "collections").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbCollection>();
            cache.companiesById = ReadEntityTypeFromJsonFile<IgdbCompany>(directoryPath, "companies").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbCompany>();
            cache.franchisesById = ReadEntityTypeFromJsonFile<IgdbFranchise>(directoryPath, "franchises").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbFranchise>();
            cache.gameModesById = ReadEntityTypeFromJsonFile<IgdbGameMode>(directoryPath, "gameModes").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbGameMode>();
            cache.gameTypesById = ReadEntityTypeFromJsonFile<IgdbGameType>(directoryPath, "gameTypes").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbGameType>();
            cache.gamesById = ReadEntityTypeFromJsonFile<IgdbGame>(directoryPath, "games").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbGame>();
            cache.genresById = ReadEntityTypeFromJsonFile<IgdbGenre>(directoryPath, "genres").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbGenre>();
            cache.involvedCompaniesById = ReadEntityTypeFromJsonFile<IgdbInvolvedCompany>(directoryPath, "involvedCompanies").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbInvolvedCompany>();
            cache.keywordsById = ReadEntityTypeFromJsonFile<IgdbKeyword>(directoryPath, "keywords").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbKeyword>();
            cache.platformsById = ReadEntityTypeFromJsonFile<IgdbPlatform>(directoryPath, "platforms").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbPlatform>();
            cache.playerPerspectivesById = ReadEntityTypeFromJsonFile<IgdbPlayerPerspective>(directoryPath, "playerPerspectives").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbPlayerPerspective>();
            cache.releaseDatesById = ReadEntityTypeFromJsonFile<IgdbReleaseDate>(directoryPath, "releaseDates").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbReleaseDate>();
            cache.themesById = ReadEntityTypeFromJsonFile<IgdbTheme>(directoryPath, "themes").ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbTheme>();

            cache.RebuildAssociatedGamesIndexes();

            return cache;
        }
    }
}
