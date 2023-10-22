using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.IgdbApi;
using GlogGenerator.TemplateRenderers;

namespace GlogGenerator.Data
{
    public class GameData
    {
        public string PermalinkRelative
        {
            get
            {
                var urlized = StringRenderer.Urlize(this.Title);
                return $"game/{urlized}/";
            }
        }

        public string Title { get; set; } = string.Empty;

        public string IgdbUrl { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        public static GameData FromIgdbGame(IgdbCache igdbCache, IgdbGame igdbGame)
        {
            var game = new GameData();

            game.Title = igdbGame.NameForGlog;

            if (!string.IsNullOrEmpty(igdbGame.Url))
            {
                game.IgdbUrl = igdbGame.Url;
            }

            if (igdbGame.Category != IgdbGameCategory.None)
            {
                game.Tags.Add(igdbGame.Category.Description());
            }

            if (igdbGame.MainCollectionId != IgdbCollection.IdNotFound)
            {
                var collection = igdbCache.GetCollection(igdbGame.MainCollectionId);
                if (collection != null && !game.Tags.Contains(collection.Name, StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(collection.Name);
                }
            }

            foreach (var collectionId in igdbGame.CollectionIds)
            {
                var collection = igdbCache.GetCollection(collectionId);
                if (collection != null && !game.Tags.Contains(collection.Name, StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(collection.Name);
                }
            }

            if (igdbGame.MainFranchiseId != IgdbFranchise.IdNotFound)
            {
                var franchise = igdbCache.GetFranchise(igdbGame.MainFranchiseId);
                if (franchise != null && !game.Tags.Contains(franchise.Name, StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(franchise.Name);
                }
            }

            foreach (var franchiseId in igdbGame.FranchiseIds)
            {
                var franchise = igdbCache.GetFranchise(franchiseId);
                if (franchise != null && !game.Tags.Contains(franchise.Name, StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(franchise.Name);
                }
            }

            var companyIds = new List<int>(igdbGame.InvolvedCompanyIds.Count);
            foreach (var involvedCompanyId in igdbGame.InvolvedCompanyIds)
            {
                var involvedCompany = igdbCache.GetInvolvedCompany(involvedCompanyId);
                if (involvedCompany != null)
                {
                    companyIds.Add(involvedCompany.CompanyId);
                }
            }

            // Quirk note: company tags appear sorted by their ID numbers.
            companyIds.Sort();

            foreach (var companyId in companyIds)
            {
                var company = igdbCache.GetCompany(companyId);
                if (company != null && !game.Tags.Contains(company.Name, StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(company.Name);
                }
            }

            foreach (var genreId in igdbGame.GenreIds)
            {
                var genre = igdbCache.GetGenre(genreId);
                if (genre != null && !game.Tags.Contains(genre.Name, StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(genre.Name);
                }
            }

            foreach (var gameModeId in igdbGame.GameModeIds)
            {
                var gameMode = igdbCache.GetGameMode(gameModeId);
                if (gameMode != null && !game.Tags.Contains(gameMode.Name, StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(gameMode.Name);
                }
            }

            foreach (var playerPerspectiveId in igdbGame.PlayerPerspectiveIds)
            {
                var playerPerspective = igdbCache.GetPlayerPerspective(playerPerspectiveId);
                if (playerPerspective != null && !game.Tags.Contains(playerPerspective.Name, StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(playerPerspective.Name);
                }
            }

            foreach (var themeId in igdbGame.ThemeIds)
            {
                var theme = igdbCache.GetTheme(themeId);
                if (theme != null && !game.Tags.Contains(theme.Name, StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(theme.Name);
                }
            }

            return game;
        }
    }
}
