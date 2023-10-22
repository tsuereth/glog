using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GlogGenerator.IgdbApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlogGenerator
{
    public class IgdbCache
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

        private Dictionary<int, IgdbPlatform> platformsById = new Dictionary<int, IgdbPlatform>();

        private List<IgdbPlatform> platformsUnidentified = new List<IgdbPlatform>();

        private Dictionary<int, IgdbPlayerPerspective> playerPerspectivesById = new Dictionary<int, IgdbPlayerPerspective>();

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

        public IgdbGame GetGameByName(string name)
        {
            var result = this.gamesById.Values.Where(g => g.NameForGlog.Equals(name, StringComparison.Ordinal));
            if (result.Count() > 0)
            {
                return result.First();
            }

            result = this.gamesUnidentified.Where(g => g.NameForGlog.Equals(name, StringComparison.Ordinal));
            if (result.Count() > 0)
            {
                return result.First();
            }

            throw new ArgumentException($"No game matches name {name}");
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

        public IgdbPlatform GetPlatform(int id)
        {
            if (this.platformsById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbPlatform GetPlatformByAbbreviation(string abbreviation)
        {
            var result = this.platformsById.Values.Where(p => p.AbbreviationForGlog.Equals(abbreviation, StringComparison.Ordinal));
            if (result.Count() > 0)
            {
                return result.First();
            }

            result = this.platformsUnidentified.Where(p => p.AbbreviationForGlog.Equals(abbreviation, StringComparison.Ordinal));
            if (result.Count() > 0)
            {
                return result.First();
            }

            throw new ArgumentException($"No platform matches abbreviation {abbreviation}");
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

        public IgdbTheme GetTheme(int id)
        {
            if (this.themesById.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public async Task UpdateFromApiClient(IgdbApiClient client)
        {
            // Update games first, to get current IDs for metadata references.
            var gameIds = this.gamesById.Keys.ToList();
            var gamesCurrent = await client.GetGamesAsync(gameIds);

            // Preserve data for glog overrides to re-override the updated cache.
            var gamesWithNameOverrides = this.gamesById.Where(kv => !string.IsNullOrEmpty(kv.Value.NameGlogOverride)).Select(kv => kv.Value).ToList();

            this.gamesById = gamesCurrent.ToDictionary(o => o.Id, o => o);
            foreach (var game in gamesWithNameOverrides)
            {
                this.gamesById[game.Id].NameGlogOverride = game.NameGlogOverride;
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

            var platformIds = this.platformsById.Keys.ToList();
            var platformsCurrent = await client.GetPlatformsAsync(platformIds);

            // Preserve data for glog overrides to re-override the updated cache.
            var platformsWithAbbreviationOverrides = this.platformsById.Where(kv => !string.IsNullOrEmpty(kv.Value.AbbreviationGlogOverride)).Select(kv => kv.Value).ToList();

            this.platformsById = platformsCurrent.ToDictionary(o => o.Id, o => o);
            foreach (var platform in platformsWithAbbreviationOverrides)
            {
                this.platformsById[platform.Id].AbbreviationGlogOverride = platform.AbbreviationGlogOverride;
            }

            var playerPerspectiveIds = gamesCurrent.SelectMany(g => g.PlayerPerspectiveIds).Distinct().ToList();
            var playerPerspectivesCurrent = await client.GetPlayerPerspectivesAsync(playerPerspectiveIds);
            this.playerPerspectivesById = playerPerspectivesCurrent.ToDictionary(o => o.Id, o => o);

            var themeIds = gamesCurrent.SelectMany(g => g.ThemeIds).Distinct().ToList();
            var themesCurrent = await client.GetThemesAsync(themeIds);
            this.themesById = themesCurrent.ToDictionary(o => o.Id, o => o);
        }

        public void WriteToJsonFile(string directoryPath)
        {
            var jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var allGames = this.gamesUnidentified.OrderBy(o => o.NameForGlog).ToList();
            allGames.AddRange(this.gamesById.Values.OrderBy(o => o.Id));

            var allPlatforms = this.platformsUnidentified.OrderBy(o => o.AbbreviationForGlog).ToList();
            allPlatforms.AddRange(this.platformsById.Values.OrderBy(o => o.Id));

            var cacheJson = new JObject();
            cacheJson["collections"] = JArray.FromObject(this.collectionsById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["companies"] = JArray.FromObject(this.companiesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["franchises"] = JArray.FromObject(this.franchisesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["gameModes"] = JArray.FromObject(this.gameModesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["games"] = JArray.FromObject(allGames, jsonSerializer);
            cacheJson["genres"] = JArray.FromObject(this.genresById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["involvedCompanies"] = JArray.FromObject(this.involvedCompaniesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["platforms"] = JArray.FromObject(allPlatforms, jsonSerializer);
            cacheJson["playerPerspectives"] = JArray.FromObject(this.playerPerspectivesById.Values.OrderBy(o => o.Id), jsonSerializer);
            cacheJson["themes"] = JArray.FromObject(this.themesById.Values.OrderBy(o => o.Id), jsonSerializer);

            File.WriteAllText(Path.Combine(directoryPath, JsonFileName), JsonConvert.SerializeObject(cacheJson, Formatting.Indented, jsonSerializerSettings));
        }

        public static IgdbCache FromJsonFile(string directoryPath)
        {
            var cacheJson = JObject.Parse(File.ReadAllText(Path.Combine(directoryPath, JsonFileName)));

            var allGames = cacheJson["games"].ToObject<List<IgdbGame>>();

            var allPlatforms = cacheJson["platforms"].ToObject<List<IgdbPlatform>>();

            var cache = new IgdbCache();
            cache.collectionsById = cacheJson["collections"].ToObject<List<IgdbCollection>>().ToDictionary(o => o.Id, o => o);
            cache.companiesById = cacheJson["companies"].ToObject<List<IgdbCompany>>().ToDictionary(o => o.Id, o => o);
            cache.franchisesById = cacheJson["franchises"].ToObject<List<IgdbFranchise>>().ToDictionary(o => o.Id, o => o);
            cache.gameModesById = cacheJson["gameModes"].ToObject<List<IgdbGameMode>>().ToDictionary(o => o.Id, o => o);
            cache.gamesById = allGames.Where(o => o.Id != IgdbGame.IdNotFound).ToDictionary(o => o.Id, o => o);
            cache.gamesUnidentified = allGames.Where(o => o.Id == IgdbGame.IdNotFound).ToList();
            cache.genresById = cacheJson["genres"].ToObject<List<IgdbGenre>>().ToDictionary(o => o.Id, o => o);
            cache.involvedCompaniesById = cacheJson["involvedCompanies"].ToObject<List<IgdbInvolvedCompany>>().ToDictionary(o => o.Id, o => o);
            cache.platformsById = allPlatforms.Where(o => o.Id != IgdbPlatform.IdNotFound).ToDictionary(o => o.Id, o => o);
            cache.platformsUnidentified = allPlatforms.Where(o => o.Id == IgdbPlatform.IdNotFound).ToList();
            cache.playerPerspectivesById = cacheJson["playerPerspectives"].ToObject<List<IgdbPlayerPerspective>>().ToDictionary(o => o.Id, o => o);
            cache.themesById = cacheJson["themes"].ToObject<List<IgdbTheme>>().ToDictionary(o => o.Id, o => o);

            return cache;
        }
    }
}
