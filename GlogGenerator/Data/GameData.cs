using System;
using System.Collections.Generic;
using System.Linq;
using GlogGenerator.IgdbApi;

namespace GlogGenerator.Data
{
    public class GameData : IGlogReferenceable
    {
        public string Title { get; set; } = string.Empty;

        public IgdbGameCategory IgdbCategory { get; set; } = IgdbGameCategory.None;

        public string IgdbUrl { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public List<string> LinkedPostIds { get; set; } = new List<string>();

        private string dataId;
        private string referenceableKey;

        public string GetDataId()
        {
            return this.dataId;
        }

        public string GetReferenceableKey()
        {
            if (!string.IsNullOrEmpty(this.referenceableKey))
            {
                return this.referenceableKey;
            }

            return this.Title;
        }

        public bool MatchesReferenceableKey(string matchKey)
        {
            var thisKey = this.GetReferenceableKey();
            return thisKey.Equals(matchKey, StringComparison.Ordinal);
        }

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.GetReferenceableKey());
            return $"game/{urlized}/";
        }

        public static GameData FromIgdbGame(IIgdbCache igdbCache, IgdbGame igdbGame)
        {
            var game = new GameData();
            game.dataId = igdbGame.GetUniqueIdString();
            game.referenceableKey = igdbGame.GetReferenceableKey();

            game.Title = igdbGame.NameForGlog;

            if (igdbGame.Category != IgdbGameCategory.None)
            {
                game.IgdbCategory = igdbGame.Category;
                game.Tags.Add(igdbGame.Category.Description());
            }

            if (!string.IsNullOrEmpty(igdbGame.Url))
            {
                game.IgdbUrl = igdbGame.Url;
            }

            if (igdbGame.MainCollectionId != IgdbCollection.IdNotFound)
            {
                var collection = igdbCache.GetCollection(igdbGame.MainCollectionId);
                if (collection != null && !game.Tags.Contains(collection.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(collection.Name);
                }
            }

            foreach (var collectionId in igdbGame.CollectionIds)
            {
                var collection = igdbCache.GetCollection(collectionId);
                if (collection != null && !game.Tags.Contains(collection.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(collection.GetReferenceableValue());
                }
            }

            if (igdbGame.MainFranchiseId != IgdbFranchise.IdNotFound)
            {
                var franchise = igdbCache.GetFranchise(igdbGame.MainFranchiseId);
                if (franchise != null && !game.Tags.Contains(franchise.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(franchise.GetReferenceableValue());
                }
            }

            foreach (var franchiseId in igdbGame.FranchiseIds)
            {
                var franchise = igdbCache.GetFranchise(franchiseId);
                if (franchise != null && !game.Tags.Contains(franchise.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(franchise.GetReferenceableValue());
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
                if (company != null && !game.Tags.Contains(company.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(company.GetReferenceableValue());
                }
            }

            foreach (var genreId in igdbGame.GenreIds)
            {
                var genre = igdbCache.GetGenre(genreId);
                if (genre != null && !game.Tags.Contains(genre.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(genre.GetReferenceableValue());
                }
            }

            foreach (var gameModeId in igdbGame.GameModeIds)
            {
                var gameMode = igdbCache.GetGameMode(gameModeId);
                if (gameMode != null && !game.Tags.Contains(gameMode.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(gameMode.GetReferenceableValue());
                }
            }

            foreach (var playerPerspectiveId in igdbGame.PlayerPerspectiveIds)
            {
                var playerPerspective = igdbCache.GetPlayerPerspective(playerPerspectiveId);
                if (playerPerspective != null && !game.Tags.Contains(playerPerspective.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(playerPerspective.GetReferenceableValue());
                }
            }

            foreach (var themeId in igdbGame.ThemeIds)
            {
                var theme = igdbCache.GetTheme(themeId);
                if (theme != null && !game.Tags.Contains(theme.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(theme.GetReferenceableValue());
                }
            }

            return game;
        }
    }
}
