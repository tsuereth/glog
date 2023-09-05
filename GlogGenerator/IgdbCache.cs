using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using GlogGenerator.IgdbApi;

namespace GlogGenerator
{
    public class IgdbCache
    {
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

        // DELETEME: the .sql cache should be transitioned to JSON
        public static IgdbCache FromSqlFile(string filePath)
        {
            var sqlLines = File.ReadAllLines(filePath);

            var cache = new IgdbCache();

            using (var dbConn = new SQLiteConnection("Data Source=:memory:"))
            {
                dbConn.Open();
                foreach (var sqlLine in sqlLines)
                {
                    using (var dbCmd = new SQLiteCommand(sqlLine, dbConn))
                    {
                        dbCmd.ExecuteNonQuery();
                    }
                }

                using (var dbCmd = new SQLiteCommand("SELECT * FROM collection", dbConn))
                using (var dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var collection = new IgdbCollection()
                        {
                            Id = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("id")),
                            Name = (string)dbReader.GetValue(dbReader.GetOrdinal("name")),
                        };

                        cache.collections.Add(collection.Id, collection);
                    }
                }

                using (var dbCmd = new SQLiteCommand("SELECT * FROM company", dbConn))
                using (var dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var company = new IgdbCompany()
                        {
                            Id = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("id")),
                            Name = (string)dbReader.GetValue(dbReader.GetOrdinal("name")),
                        };

                        cache.companies.Add(company.Id, company);
                    }
                }

                using (var dbCmd = new SQLiteCommand("SELECT * FROM franchise", dbConn))
                using (var dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var franchise = new IgdbFranchise()
                        {
                            Id = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("id")),
                            Name = (string)dbReader.GetValue(dbReader.GetOrdinal("name")),
                        };

                        cache.franchises.Add(franchise.Id, franchise);
                    }
                }

                using (var dbCmd = new SQLiteCommand("SELECT * FROM game", dbConn))
                using (var dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var franchisesString = (string)dbReader.GetValue(dbReader.GetOrdinal("franchises"));
                        var gameModesString = (string)dbReader.GetValue(dbReader.GetOrdinal("game_modes"));
                        var genresString = (string)dbReader.GetValue(dbReader.GetOrdinal("genres"));
                        var involvedCompaniesString = (string)dbReader.GetValue(dbReader.GetOrdinal("involved_companies"));
                        var playerPerspectivesString = (string)dbReader.GetValue(dbReader.GetOrdinal("player_perspectives"));
                        var themesString = (string)dbReader.GetValue(dbReader.GetOrdinal("themes"));

                        var game = new IgdbGame()
                        {
                            Id = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("id")),
                            Category = (IgdbGameCategory)(long)dbReader.GetValue(dbReader.GetOrdinal("category")),
                            CollectionId = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("collection")),
                            MainFranchiseId = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("franchise")),
                            OtherFranchiseIds = franchisesString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToList(),
                            GameModeIds = gameModesString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToList(),
                            GenreIds = genresString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToList(),
                            InvolvedCompanyIds = involvedCompaniesString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToList(),
                            Name = (string)dbReader.GetValue(dbReader.GetOrdinal("name")),
                            PlayerPerspectiveIds = playerPerspectivesString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToList(),
                            ThemeIds = themesString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToList(),
                            UpdatedAtSecondsSinceEpoch = (long)dbReader.GetValue(dbReader.GetOrdinal("updated_at")),
                            Url = (string)dbReader.GetValue(dbReader.GetOrdinal("url")),
                        };

                        cache.games.Add(game.Id, game);
                    }
                }

                using (var dbCmd = new SQLiteCommand("SELECT * FROM game_mode", dbConn))
                using (var dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var gameMode = new IgdbGameMode()
                        {
                            Id = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("id")),
                            Name = (string)dbReader.GetValue(dbReader.GetOrdinal("name")),
                        };

                        cache.gameModes.Add(gameMode.Id, gameMode);
                    }
                }

                using (var dbCmd = new SQLiteCommand("SELECT * FROM genre", dbConn))
                using (var dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var genre = new IgdbGenre()
                        {
                            Id = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("id")),
                            Name = (string)dbReader.GetValue(dbReader.GetOrdinal("name")),
                        };

                        cache.genres.Add(genre.Id, genre);
                    }
                }

                using (var dbCmd = new SQLiteCommand("SELECT * FROM involved_company", dbConn))
                using (var dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var involvedCompany = new IgdbInvolvedCompany()
                        {
                            Id = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("id")),
                            CompanyId = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("company")),
                        };

                        cache.involvedCompanies.Add(involvedCompany.Id, involvedCompany);
                    }
                }

                using (var dbCmd = new SQLiteCommand("SELECT * FROM player_perspective", dbConn))
                using (var dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var playerPerspective = new IgdbPlayerPerspective()
                        {
                            Id = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("id")),
                            Name = (string)dbReader.GetValue(dbReader.GetOrdinal("name")),
                        };

                        cache.playerPerspectives.Add(playerPerspective.Id, playerPerspective);
                    }
                }

                using (var dbCmd = new SQLiteCommand("SELECT * FROM theme", dbConn))
                using (var dbReader = dbCmd.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        var theme = new IgdbTheme()
                        {
                            Id = (int)(long)dbReader.GetValue(dbReader.GetOrdinal("id")),
                            Name = (string)dbReader.GetValue(dbReader.GetOrdinal("name")),
                        };

                        cache.themes.Add(theme.Id, theme);
                    }
                }

                dbConn.Close();
            }

            return cache;
        }
    }
}
