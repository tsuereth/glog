using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.IgdbApi;
using GlogGenerator.TemplateRenderers;

namespace GlogGenerator.Data
{
    public class GameData : IGlogReferenceable
    {
        public string Title { get; set; } = string.Empty;

        public string IgdbUrl { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        public string GetPermalinkRelative()
        {
            var urlized = StringRenderer.Urlize(this.Title);
            return $"game/{urlized}/";
        }

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
                if (collection != null && !game.Tags.Contains(collection.GetReferenceableKey(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(collection.Name);
                }
            }

            foreach (var collectionId in igdbGame.CollectionIds)
            {
                var collection = igdbCache.GetCollection(collectionId);
                if (collection != null && !game.Tags.Contains(collection.GetReferenceableKey(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(collection.GetReferenceableKey());
                }
            }

            if (igdbGame.MainFranchiseId != IgdbFranchise.IdNotFound)
            {
                var franchise = igdbCache.GetFranchise(igdbGame.MainFranchiseId);
                if (franchise != null && !game.Tags.Contains(franchise.GetReferenceableKey(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(franchise.GetReferenceableKey());
                }
            }

            foreach (var franchiseId in igdbGame.FranchiseIds)
            {
                var franchise = igdbCache.GetFranchise(franchiseId);
                if (franchise != null && !game.Tags.Contains(franchise.GetReferenceableKey(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(franchise.GetReferenceableKey());
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
                if (company != null && !game.Tags.Contains(company.GetReferenceableKey(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(company.GetReferenceableKey());
                }
            }

            foreach (var genreId in igdbGame.GenreIds)
            {
                var genre = igdbCache.GetGenre(genreId);
                if (genre != null && !game.Tags.Contains(genre.GetReferenceableKey(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(genre.GetReferenceableKey());
                }
            }

            foreach (var gameModeId in igdbGame.GameModeIds)
            {
                var gameMode = igdbCache.GetGameMode(gameModeId);
                if (gameMode != null && !game.Tags.Contains(gameMode.GetReferenceableKey(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(gameMode.GetReferenceableKey());
                }
            }

            foreach (var playerPerspectiveId in igdbGame.PlayerPerspectiveIds)
            {
                var playerPerspective = igdbCache.GetPlayerPerspective(playerPerspectiveId);
                if (playerPerspective != null && !game.Tags.Contains(playerPerspective.GetReferenceableKey(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(playerPerspective.GetReferenceableKey());
                }
            }

            foreach (var themeId in igdbGame.ThemeIds)
            {
                var theme = igdbCache.GetTheme(themeId);
                if (theme != null && !game.Tags.Contains(theme.GetReferenceableKey(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(theme.GetReferenceableKey());
                }
            }

            return game;
        }
    }
}
