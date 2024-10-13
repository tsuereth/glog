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
        public static readonly string JsonFileName = "igdb_cache.json";

        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        private Dictionary<int, IgdbCollection> collectionsById = new Dictionary<int, IgdbCollection>();

        private Dictionary<int, IgdbCompany> companiesById = new Dictionary<int, IgdbCompany>();

        private Dictionary<int, IgdbFranchise> franchisesById = new Dictionary<int, IgdbFranchise>();

        private Dictionary<int, IgdbGame> gamesById = new Dictionary<int, IgdbGame>();

        private List<IgdbGame> gamesUnidentified = new List<IgdbGame>();

        private Dictionary<int, IgdbGameMode> gameModesById = new Dictionary<int, IgdbGameMode>();

        private Dictionary<int, IgdbGenre> genresById = new Dictionary<int, IgdbGenre>();

        private Dictionary<int, IgdbInvolvedCompany> involvedCompaniesById = new Dictionary<int, IgdbInvolvedCompany>();

        private Dictionary<int, IgdbKeyword> keywordsById = new Dictionary<int, IgdbKeyword>();

        private Dictionary<int, IgdbPlatform> platformsById = new Dictionary<int, IgdbPlatform>();

        private List<IgdbPlatform> platformsUnidentified = new List<IgdbPlatform>();

        private Dictionary<int, IgdbPlayerPerspective> playerPerspectivesById = new Dictionary<int, IgdbPlayerPerspective>();

        private Dictionary<int, IgdbReleaseDate> releaseDatesById = new Dictionary<int, IgdbReleaseDate>();

        private Dictionary<int, IgdbTheme> themesById = new Dictionary<int, IgdbTheme>();

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
            var results = this.gamesById.Values.ToList();

            results.AddRange(this.gamesUnidentified);

            return results;
        }

        public IgdbGameMode GetGameMode(int id)
        {
            if (this.gameModesById.TryGetValue(id, out var result))
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
            var results = this.platformsById.Values.ToList();

            results.AddRange(this.platformsUnidentified);

            return results;
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
            allMetadata.AddRange(this.genresById.Values);
            allMetadata.AddRange(this.keywordsById.Values);
            allMetadata.AddRange(this.playerPerspectivesById.Values);
            allMetadata.AddRange(this.themesById.Values);

            return allMetadata;
        }

        public List<int> GetBundledGameIds(int bundleGameId)
        {
            return this.GetAllGames().Where(g => g.BundleGameIds.Contains(bundleGameId) && g.Id != IgdbEntity.IdNotFound).Select(g => g.Id).ToList();
        }

        public async Task UpdateFromApiClient(IgdbApiClient client)
        {
            // Update games first, to get current IDs for metadata references.
            var gameIds = this.gamesById.Keys.ToList();
            var gamesCurrent = await client.GetGamesAsync(gameIds);

            var gamesCurrentById = gamesCurrent.ToDictionary(o => o.Id, o => o);

            // Re-apply overriden properties from the old cache.
            foreach (var gameId in gameIds)
            {
                if (gamesCurrentById.ContainsKey(gameId))
                {
                    var gameOverrides = this.gamesById[gameId].GetGlogOverrideValues();
                    gamesCurrentById[gameId].SetGlogOverrideValues(gameOverrides);
                }
            }

            // Update involvedCompanies to get most-current company IDs.
            var involvedCompanyIds = gamesCurrent.SelectMany(g => g.InvolvedCompanyIds).Distinct().ToList();
            var involvedCompaniesCurrent = await client.GetInvolvedCompaniesAsync(involvedCompanyIds);
            this.involvedCompaniesById = involvedCompaniesCurrent.ToDictionary(o => o.Id, o => o);

            var companyIds = involvedCompaniesCurrent.Select(i => i.CompanyId).Distinct().ToList();
            var companiesCurrent = await client.GetCompaniesAsync(companyIds);
            this.companiesById = companiesCurrent.ToDictionary(o => o.Id, o => o);

            // Now, update the rest of the ID-driven metadata.

            var collectionIds = gamesCurrent.Where(g => g.MainCollectionId != IgdbCollection.IdNotFound).Select(g => g.MainCollectionId).Distinct().ToList();
            collectionIds.AddRange(gamesCurrent.SelectMany(g => g.CollectionIds).Distinct());
            var collectionsCurrent = await client.GetCollectionsAsync(collectionIds.Distinct().ToList());
            this.collectionsById = collectionsCurrent.ToDictionary(o => o.Id, o => o);

            var franchiseIds = gamesCurrent.Where(g => g.MainFranchiseId != IgdbFranchise.IdNotFound).Select(g => g.MainFranchiseId).Distinct().ToList();
            franchiseIds.AddRange(gamesCurrent.SelectMany(g => g.FranchiseIds).Distinct());
            var franchisesCurrent = await client.GetFranchisesAsync(franchiseIds.Distinct().ToList());
            this.franchisesById = franchisesCurrent.ToDictionary(o => o.Id, o => o);

            var gameModeIds = gamesCurrent.SelectMany(g => g.GameModeIds).Distinct().ToList();
            var gameModesCurrent = await client.GetGameModesAsync(gameModeIds);
            this.gameModesById = gameModesCurrent.ToDictionary(o => o.Id, o => o);

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

            // Re-apply overridden properties from the old cache.
            foreach (var platformId in this.platformsById.Keys)
            {
                if (platformsCurrentById.ContainsKey(platformId))
                {
                    var platformOverrides = this.platformsById[platformId].GetGlogOverrideValues();
                    platformsCurrentById[platformId].SetGlogOverrideValues(platformOverrides);
                }
            }

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

            // Check game data for quirks to override/fix.

            var gamesDuplicatedUrls = gamesCurrentById.Values
                .GroupBy(g => UrlizedString.Urlize(g.GetReferenceString(this)))
                .Where(g => g.Count() > 1);
            foreach (var gamesDuplicatedUrl in gamesDuplicatedUrls)
            {
                var duplicatedUrl = gamesDuplicatedUrl.Key;

                // We can disambiguate games with the same name/URL based on their release dates.
                // Unless some release dates aren't available -- that'll be trouble.
                var gamesMissingReleaseDate = gamesDuplicatedUrl.Where(g => g.GetFirstReleaseDate(this) == null);
                if (gamesMissingReleaseDate.Any())
                {
                    throw new InvalidDataException($"Multiple games have the same URL \"{duplicatedUrl}\" and cannot be disambiguated because some are missing a release date.");
                }

                var gamesByReleaseYear = gamesDuplicatedUrl.GroupBy(g => g.GetFirstReleaseDate(this).Value.Year).ToDictionary(g => g.Key, g => g);
                var earliestReleaseYear = gamesByReleaseYear.Keys.OrderBy(y => y).First();
                foreach (var releaseYear in gamesByReleaseYear.Keys)
                {
                    var disambiguateByReleaseYear = false;
                    var disambiguateByPlatforms = false;

                    // If this release year isn't the earliest year for a game of this name, later-released games will be disambiguated by release year.
                    if (releaseYear != earliestReleaseYear)
                    {
                        disambiguateByReleaseYear = true;
                    }

                    // If multiple games with this name were released in the SAME year, then they need to be disambiguated by their release platforms.
                    if (gamesByReleaseYear[releaseYear].Count() > 1)
                    {
                        disambiguateByPlatforms = true;
                    }

                    foreach (var game in gamesByReleaseYear[releaseYear])
                    {
                        if (disambiguateByPlatforms)
                        {
                            gamesCurrentById[game.Id].NameGlogAppendPlatforms = true;
                        }

                        if (disambiguateByReleaseYear)
                        {
                            gamesCurrentById[game.Id].NameGlogAppendReleaseYear = true;
                        }
                    }
                }
            }

            // Sometimes... disambiguating game names/URLs by release-year and platform STILL isn't enough!
            // This is so rare and bizarre that we may as well just start slapping numbers on the games' names.
            gamesDuplicatedUrls = gamesCurrentById.Values
                .GroupBy(g => UrlizedString.Urlize(g.GetReferenceString(this)))
                .Where(g => g.Count() > 1);
            foreach (var gamesDuplicatedUrl in gamesDuplicatedUrls)
            {
                var duplicatedUrl = gamesDuplicatedUrl.Key;

                var gamesInReleaseOrder = gamesDuplicatedUrl.OrderBy(g => g.GetFirstReleaseDate(this)).ToList();
                for (var i = 1; i < gamesInReleaseOrder.Count; ++i)
                {
                    var gameId = gamesInReleaseOrder[i].Id;

                    // Start with release number "2"
                    gamesCurrentById[gameId].NameGlogAppendReleaseNumber = i + 1;
                }
            }

            this.gamesById = gamesCurrentById;
        }

        public void WriteToJsonFile(string directoryPath)
        {
            var jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var allGames = this.gamesUnidentified.OrderBy(o => o.GetReferenceString(this)).ToList();
            allGames.AddRange(this.gamesById.Values.OrderBy(o => o.Id));

            var allPlatforms = this.platformsUnidentified.OrderBy(o => o.GetReferenceString(this)).ToList();
            allPlatforms.AddRange(this.platformsById.Values.OrderBy(o => o.Id));

            var cacheJson = new JObject();
            cacheJson["collections"] = JArray.FromObject(this.collectionsById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["companies"] = JArray.FromObject(this.companiesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["franchises"] = JArray.FromObject(this.franchisesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["gameModes"] = JArray.FromObject(this.gameModesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["games"] = JArray.FromObject(allGames, jsonSerializer);
            cacheJson["genres"] = JArray.FromObject(this.genresById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["involvedCompanies"] = JArray.FromObject(this.involvedCompaniesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["keywords"] = JArray.FromObject(this.keywordsById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["platforms"] = JArray.FromObject(allPlatforms, jsonSerializer);
            cacheJson["playerPerspectives"] = JArray.FromObject(this.playerPerspectivesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["releaseDates"] = JArray.FromObject(this.releaseDatesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["themes"] = JArray.FromObject(this.themesById.Values.OrderBy(o => o.Id), jsonSerializer);

            var jsonFilePath = Path.Combine(directoryPath, JsonFileName);
            using (var fileStream = new FileStream(jsonFilePath, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream) { NewLine = "\n" })
            using (var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented })
            {
                jsonSerializer.Serialize(jsonWriter, cacheJson);
                streamWriter.Flush();
            }
        }

        public static IgdbCache FromJsonFile(string directoryPath)
        {
            var cacheJson = JObject.Parse(File.ReadAllText(Path.Combine(directoryPath, JsonFileName)));

            var allGames = cacheJson["games"]?.ToObject<List<IgdbGame>>() ?? new List<IgdbGame>();

            var allPlatforms = cacheJson["platforms"]?.ToObject<List<IgdbPlatform>>() ?? new List<IgdbPlatform>();

            var cache = new IgdbCache();
            cache.collectionsById = cacheJson["collections"]?.ToObject<List<IgdbCollection>>().ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbCollection>();
            cache.companiesById = cacheJson["companies"]?.ToObject<List<IgdbCompany>>().ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbCompany>();
            cache.franchisesById = cacheJson["franchises"]?.ToObject<List<IgdbFranchise>>().ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbFranchise>();
            cache.gameModesById = cacheJson["gameModes"]?.ToObject<List<IgdbGameMode>>().ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbGameMode>();
            cache.gamesById = allGames.Where(o => o.Id != IgdbGame.IdNotFound).ToDictionary(o => o.Id, o => o);
            cache.gamesUnidentified = allGames.Where(o => o.Id == IgdbGame.IdNotFound).ToList();
            cache.genresById = cacheJson["genres"]?.ToObject<List<IgdbGenre>>().ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbGenre>();
            cache.involvedCompaniesById = cacheJson["involvedCompanies"]?.ToObject<List<IgdbInvolvedCompany>>().ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbInvolvedCompany>();
            cache.keywordsById = cacheJson["keywords"]?.ToObject<List<IgdbKeyword>>().ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbKeyword>();
            cache.platformsById = allPlatforms.Where(o => o.Id != IgdbPlatform.IdNotFound).ToDictionary(o => o.Id, o => o);
            cache.platformsUnidentified = allPlatforms.Where(o => o.Id == IgdbPlatform.IdNotFound).ToList();
            cache.playerPerspectivesById = cacheJson["playerPerspectives"]?.ToObject<List<IgdbPlayerPerspective>>().ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbPlayerPerspective>();
            cache.releaseDatesById = cacheJson["releaseDates"]?.ToObject<List<IgdbReleaseDate>>().ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbReleaseDate>();
            cache.themesById = cacheJson["themes"]?.ToObject<List<IgdbTheme>>().ToDictionary(o => o.Id, o => o) ?? new Dictionary<int, IgdbTheme>();

            return cache;
        }
    }
}
