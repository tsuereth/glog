using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GlogGenerator.IgdbApi;
using Newtonsoft.Json;

namespace GlogGenerator
{
    public class IgdbCache
    {
        public static readonly string JsonFilesBaseDir = "igdb_cache";

        private Dictionary<int, IgdbCollection> collectionsById = new Dictionary<int, IgdbCollection>();

        private Dictionary<int, IgdbCompany> companiesById = new Dictionary<int, IgdbCompany>();

        private Dictionary<int, IgdbFranchise> franchisesById = new Dictionary<int, IgdbFranchise>();

        private Dictionary<int, IgdbGame> gamesById = new Dictionary<int, IgdbGame>();

        private List<IgdbGame> gamesUnidentified = new List<IgdbGame>();

        private Dictionary<int, IgdbGameMode> gameModesById = new Dictionary<int, IgdbGameMode>();

        private Dictionary<int, IgdbGenre> genresById = new Dictionary<int, IgdbGenre>();

        private Dictionary<int, IgdbInvolvedCompany> involvedCompaniesById = new Dictionary<int, IgdbInvolvedCompany>();

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

        public List<IgdbGame> GetGameByName(string name)
        {
            var results = this.gamesById.Where(kv => kv.Value.NameForGlog.Equals(name, StringComparison.Ordinal)).Select(kv => kv.Value).ToList();

            var unidentified = this.gamesUnidentified.Where(o => o.NameForGlog.Equals(name, StringComparison.Ordinal)).ToList();
            results.AddRange(unidentified);

            return results;
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
            var gameIds = this.gamesById.Select(kv => kv.Key).ToList();
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

            var collectionIds = gamesCurrent.Where(g => g.CollectionId != IgdbCollection.IdNotFound).Select(g => g.CollectionId).Distinct().ToList();
            var collectionsCurrent = await client.GetCollectionsAsync(collectionIds);
            this.collectionsById = collectionsCurrent.ToDictionary(o => o.Id, o => o);

            var franchiseIds = gamesCurrent.Where(g => g.MainFranchiseId != IgdbFranchise.IdNotFound).Select(g => g.MainFranchiseId).Distinct().ToList();
            franchiseIds.AddRange(gamesCurrent.SelectMany(g => g.OtherFranchiseIds).Distinct());
            var franchisesCurrent = await client.GetFranchisesAsync(franchiseIds);
            this.franchisesById = franchisesCurrent.ToDictionary(o => o.Id, o => o);

            var gameModeIds = gamesCurrent.SelectMany(g => g.GameModeIds).Distinct().ToList();
            var gameModesCurrent = await client.GetGameModesAsync(gameModeIds);
            this.gameModesById = gameModesCurrent.ToDictionary(o => o.Id, o => o);

            var genreIds = gamesCurrent.SelectMany(g => g.GenreIds).Distinct().ToList();
            var genresCurrent = await client.GetGenresAsync(genreIds);
            this.genresById = genresCurrent.ToDictionary(o => o.Id, o => o);

            var playerPerspectiveIds = gamesCurrent.SelectMany(g => g.PlayerPerspectiveIds).Distinct().ToList();
            var playerPerspectivesCurrent = await client.GetPlayerPerspectivesAsync(playerPerspectiveIds);
            this.playerPerspectivesById = playerPerspectivesCurrent.ToDictionary(o => o.Id, o => o);

            var themeIds = gamesCurrent.SelectMany(g => g.ThemeIds).Distinct().ToList();
            var themesCurrent = await client.GetThemesAsync(themeIds);
            this.themesById = themesCurrent.ToDictionary(o => o.Id, o => o);
        }

        public void WriteToJsonFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var collectionsFilePath = Path.Combine(directoryPath, "collections.json");
            File.WriteAllText(collectionsFilePath, JsonConvert.SerializeObject(this.collectionsById.Values.OrderBy(o => o.Id), Formatting.Indented));

            var companiesFilePath = Path.Combine(directoryPath, "companies.json");
            File.WriteAllText(companiesFilePath, JsonConvert.SerializeObject(this.companiesById.Values.OrderBy(o => o.Id), Formatting.Indented));

            var franchisesFilePath = Path.Combine(directoryPath, "franchises.json");
            File.WriteAllText(franchisesFilePath, JsonConvert.SerializeObject(this.franchisesById.Values.OrderBy(o => o.Id), Formatting.Indented));

            var gamesFilePath = Path.Combine(directoryPath, "games.json");
            var allGames = this.gamesUnidentified.OrderBy(o => o.NameForGlog).ToList();
            allGames.AddRange(this.gamesById.Values.OrderBy(o => o.Id));
            File.WriteAllText(gamesFilePath, JsonConvert.SerializeObject(allGames, Formatting.Indented));

            var gameModesFilePath = Path.Combine(directoryPath, "gameModes.json");
            File.WriteAllText(gameModesFilePath, JsonConvert.SerializeObject(this.gameModesById.Values.OrderBy(o => o.Id), Formatting.Indented));

            var genresFilePath = Path.Combine(directoryPath, "genres.json");
            File.WriteAllText(genresFilePath, JsonConvert.SerializeObject(this.genresById.Values.OrderBy(o => o.Id), Formatting.Indented));

            var involvedCompaniesFilePath = Path.Combine(directoryPath, "involvedCompanies.json");
            File.WriteAllText(involvedCompaniesFilePath, JsonConvert.SerializeObject(this.involvedCompaniesById.Values.OrderBy(o => o.Id), Formatting.Indented));

            var playerPerspectivesFilePath = Path.Combine(directoryPath, "playerPerspectives.json");
            File.WriteAllText(playerPerspectivesFilePath, JsonConvert.SerializeObject(this.playerPerspectivesById.Values.OrderBy(o => o.Id), Formatting.Indented));

            var themesFilePath = Path.Combine(directoryPath, "themes.json");
            File.WriteAllText(themesFilePath, JsonConvert.SerializeObject(this.themesById.Values.OrderBy(o => o.Id), Formatting.Indented));
        }

        public static IgdbCache FromJsonFiles(string directoryPath)
        {
            var cache = new IgdbCache();

            var collectionsFilePath = Path.Combine(directoryPath, "collections.json");
            cache.collectionsById = JsonConvert.DeserializeObject<List<IgdbCollection>>(File.ReadAllText(collectionsFilePath)).ToDictionary(o => o.Id, o => o);

            var companiesFilePath = Path.Combine(directoryPath, "companies.json");
            cache.companiesById = JsonConvert.DeserializeObject<List<IgdbCompany>>(File.ReadAllText(companiesFilePath)).ToDictionary(o => o.Id, o => o);

            var franchisesFilePath = Path.Combine(directoryPath, "franchises.json");
            cache.franchisesById = JsonConvert.DeserializeObject<List<IgdbFranchise>>(File.ReadAllText(franchisesFilePath)).ToDictionary(o => o.Id, o => o);

            var gamesFilePath = Path.Combine(directoryPath, "games.json");
            var allGames = JsonConvert.DeserializeObject<List<IgdbGame>>(File.ReadAllText(gamesFilePath));
            cache.gamesById = allGames.Where(o => o.Id != IgdbGame.IdNotFound).ToDictionary(o => o.Id, o => o);
            cache.gamesUnidentified = allGames.Where(o => o.Id == IgdbGame.IdNotFound).ToList();

            var gameModesFilePath = Path.Combine(directoryPath, "gameModes.json");
            cache.gameModesById = JsonConvert.DeserializeObject<List<IgdbGameMode>>(File.ReadAllText(gameModesFilePath)).ToDictionary(o => o.Id, o => o);

            var genresFilePath = Path.Combine(directoryPath, "genres.json");
            cache.genresById = JsonConvert.DeserializeObject<List<IgdbGenre>>(File.ReadAllText(genresFilePath)).ToDictionary(o => o.Id, o => o);

            var involvedCompaniesFilePath = Path.Combine(directoryPath, "involvedCompanies.json");
            cache.involvedCompaniesById = JsonConvert.DeserializeObject<List<IgdbInvolvedCompany>>(File.ReadAllText(involvedCompaniesFilePath)).ToDictionary(o => o.Id, o => o);

            var playerPerspectivesFilePath = Path.Combine(directoryPath, "playerPerspectives.json");
            cache.playerPerspectivesById = JsonConvert.DeserializeObject<List<IgdbPlayerPerspective>>(File.ReadAllText(playerPerspectivesFilePath)).ToDictionary(o => o.Id, o => o);

            var themesFilePath = Path.Combine(directoryPath, "themes.json");
            cache.themesById = JsonConvert.DeserializeObject<List<IgdbTheme>>(File.ReadAllText(themesFilePath)).ToDictionary(o => o.Id, o => o);

            return cache;
        }
    }
}
