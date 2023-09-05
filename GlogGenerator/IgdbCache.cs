using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using GlogGenerator.IgdbApi;
using Newtonsoft.Json;

namespace GlogGenerator
{
    public class IgdbCache
    {
        public static readonly string JsonFilesBaseDir = "igdb_cache";

        private Dictionary<int, IgdbCollection> collections = new Dictionary<int, IgdbCollection>();

        private Dictionary<int, IgdbCompany> companies = new Dictionary<int, IgdbCompany>();

        private Dictionary<int, IgdbFranchise> franchises = new Dictionary<int, IgdbFranchise>();

        private Dictionary<int, IgdbGame> games = new Dictionary<int, IgdbGame>();

        private Dictionary<int, IgdbGameMode> gameModes = new Dictionary<int, IgdbGameMode>();

        private Dictionary<int, IgdbGenre> genres = new Dictionary<int, IgdbGenre>();

        private Dictionary<int, IgdbInvolvedCompany> involvedCompanies = new Dictionary<int, IgdbInvolvedCompany>();

        private Dictionary<int, IgdbPlayerPerspective> playerPerspectives = new Dictionary<int, IgdbPlayerPerspective>();

        private Dictionary<int, IgdbTheme> themes = new Dictionary<int, IgdbTheme>();

        public IgdbCollection GetCollection(int id)
        {
            if (this.collections.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbCompany GetCompany(int id)
        {
            if (this.companies.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbFranchise GetFranchise(int id)
        {
            if (this.franchises.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbGame GetGame(int id)
        {
            if (this.games.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public List<IgdbGame> GetAllGames()
        {
            return this.games.Values.ToList();
        }

        public IgdbGameMode GetGameMode(int id)
        {
            if (this.gameModes.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbGenre GetGenre(int id)
        {
            if (this.genres.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbInvolvedCompany GetInvolvedCompany(int id)
        {
            if (this.involvedCompanies.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbPlayerPerspective GetPlayerPerspective(int id)
        {
            if (this.playerPerspectives.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public IgdbTheme GetTheme(int id)
        {
            if (this.themes.TryGetValue(id, out var result))
            {
                return result;
            }

            return null;
        }

        public void WriteToJsonFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var collectionsFilePath = Path.Combine(directoryPath, "collections.json");
            File.WriteAllText(collectionsFilePath, JsonConvert.SerializeObject(this.collections.Values.OrderBy(o => o.Id), Formatting.Indented));

            var companiesFilePath = Path.Combine(directoryPath, "companies.json");
            File.WriteAllText(companiesFilePath, JsonConvert.SerializeObject(this.companies.Values.OrderBy(o => o.Id), Formatting.Indented));

            var franchisesFilePath = Path.Combine(directoryPath, "franchises.json");
            File.WriteAllText(franchisesFilePath, JsonConvert.SerializeObject(this.franchises.Values.OrderBy(o => o.Id), Formatting.Indented));

            var gamesFilePath = Path.Combine(directoryPath, "games.json");
            File.WriteAllText(gamesFilePath, JsonConvert.SerializeObject(this.games.Values.OrderBy(o => o.Id), Formatting.Indented));

            var gameModesFilePath = Path.Combine(directoryPath, "gameModes.json");
            File.WriteAllText(gameModesFilePath, JsonConvert.SerializeObject(this.gameModes.Values.OrderBy(o => o.Id), Formatting.Indented));

            var genresFilePath = Path.Combine(directoryPath, "genres.json");
            File.WriteAllText(genresFilePath, JsonConvert.SerializeObject(this.genres.Values.OrderBy(o => o.Id), Formatting.Indented));

            var involvedCompaniesFilePath = Path.Combine(directoryPath, "involvedCompanies.json");
            File.WriteAllText(involvedCompaniesFilePath, JsonConvert.SerializeObject(this.involvedCompanies.Values.OrderBy(o => o.Id), Formatting.Indented));

            var playerPerspectivesFilePath = Path.Combine(directoryPath, "playerPerspectives.json");
            File.WriteAllText(playerPerspectivesFilePath, JsonConvert.SerializeObject(this.playerPerspectives.Values.OrderBy(o => o.Id), Formatting.Indented));

            var themesFilePath = Path.Combine(directoryPath, "themes.json");
            File.WriteAllText(themesFilePath, JsonConvert.SerializeObject(this.themes.Values.OrderBy(o => o.Id), Formatting.Indented));
        }

        public static IgdbCache FromJsonFiles(string directoryPath)
        {
            var cache = new IgdbCache();

            var collectionsFilePath = Path.Combine(directoryPath, "collections.json");
            cache.collections = JsonConvert.DeserializeObject<List<IgdbCollection>>(File.ReadAllText(collectionsFilePath)).ToDictionary(o => o.Id, o => o);

            var companiesFilePath = Path.Combine(directoryPath, "companies.json");
            cache.companies = JsonConvert.DeserializeObject<List<IgdbCompany>>(File.ReadAllText(companiesFilePath)).ToDictionary(o => o.Id, o => o);

            var franchisesFilePath = Path.Combine(directoryPath, "franchises.json");
            cache.franchises = JsonConvert.DeserializeObject<List<IgdbFranchise>>(File.ReadAllText(franchisesFilePath)).ToDictionary(o => o.Id, o => o);

            var gamesFilePath = Path.Combine(directoryPath, "games.json");
            cache.games = JsonConvert.DeserializeObject<List<IgdbGame>>(File.ReadAllText(gamesFilePath)).ToDictionary(o => o.Id, o => o);

            var gameModesFilePath = Path.Combine(directoryPath, "gameModes.json");
            cache.gameModes = JsonConvert.DeserializeObject<List<IgdbGameMode>>(File.ReadAllText(gameModesFilePath)).ToDictionary(o => o.Id, o => o);

            var genresFilePath = Path.Combine(directoryPath, "genres.json");
            cache.genres = JsonConvert.DeserializeObject<List<IgdbGenre>>(File.ReadAllText(genresFilePath)).ToDictionary(o => o.Id, o => o);

            var involvedCompaniesFilePath = Path.Combine(directoryPath, "involvedCompanies.json");
            cache.involvedCompanies = JsonConvert.DeserializeObject<List<IgdbInvolvedCompany>>(File.ReadAllText(involvedCompaniesFilePath)).ToDictionary(o => o.Id, o => o);

            var playerPerspectivesFilePath = Path.Combine(directoryPath, "playerPerspectives.json");
            cache.playerPerspectives = JsonConvert.DeserializeObject<List<IgdbPlayerPerspective>>(File.ReadAllText(playerPerspectivesFilePath)).ToDictionary(o => o.Id, o => o);

            var themesFilePath = Path.Combine(directoryPath, "themes.json");
            cache.themes = JsonConvert.DeserializeObject<List<IgdbTheme>>(File.ReadAllText(themesFilePath)).ToDictionary(o => o.Id, o => o);

            return cache;
        }
    }
}
