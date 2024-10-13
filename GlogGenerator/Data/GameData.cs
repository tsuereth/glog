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

        public HashSet<string> Tags { get; set; } = new HashSet<string>();

        public HashSet<string> RelatedGames { get; set; } = new HashSet<string>();

        public HashSet<string> LinkedPostIds { get; set; } = new HashSet<string>();

        public HashSet<string> LinkPostsToOtherGames { get; set; } = new HashSet<string>();

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
            game.referenceableKey = igdbGame.GetReferenceableValue();

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

            foreach (var keywordId in igdbGame.KeywordIds)
            {
                var keyword = igdbCache.GetKeyword(keywordId);
                if (keyword != null && !game.Tags.Contains(keyword.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    game.Tags.Add(keyword.GetReferenceableValue());
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

            if (igdbGame.ParentGameId != IgdbEntity.IdNotFound)
            {
                game.TryAddRelatedGame(igdbCache, igdbGame.ParentGameId);
            }

            if (igdbGame.VersionParentGameId != IgdbEntity.IdNotFound)
            {
                game.TryAddRelatedGame(igdbCache, igdbGame.VersionParentGameId);
            }

            // If this game is a "bundle," then add its bundled games as related.
            if (igdbGame.Category == IgdbGameCategory.bundle)
            {
                var bundledGameIds = igdbCache.GetBundledGameIds(igdbGame.Id);
                foreach (var bundledGameId in bundledGameIds)
                {
                    game.TryAddRelatedGame(igdbCache, bundledGameId);
                }
            }

            foreach (var relatedGameId in igdbGame.BundleGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId);

                // Add other games from the same bundle, too.
                var bundledGameIds = igdbCache.GetBundledGameIds(relatedGameId);
                foreach (var bundledGameId in bundledGameIds)
                {
                    // (But not the current game, as "related" to itself, that'd be silly!)
                    if (bundledGameId == igdbGame.Id)
                    {
                        continue;
                    }

                    game.TryAddRelatedGame(igdbCache, bundledGameId);
                }

                // The bundle should link to posts for this game.
                var bundleGame = igdbCache.GetGame(relatedGameId);
                if (bundleGame != null)
                {
                    game.LinkPostsToOtherGames.Add(bundleGame.GetReferenceableValue());
                }
            }

            foreach (var relatedGameId in igdbGame.DlcGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId);
            }

            foreach (var relatedGameId in igdbGame.ExpandedGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId);
            }

            foreach (var relatedGameId in igdbGame.ExpansionGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId);
            }

            foreach (var relatedGameId in igdbGame.ForkGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId);
            }

            foreach (var relatedGameId in igdbGame.PortGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId);
            }

            foreach (var relatedGameId in igdbGame.RemakeGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId);
            }

            foreach (var relatedGameId in igdbGame.RemasterGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId);
            }

            foreach (var relatedGameId in igdbGame.StandaloneExpansionGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId);
            }

            return game;
        }

        private void TryAddRelatedGame(IIgdbCache igdbCache, int gameId)
        {
            if (gameId != IgdbEntity.IdNotFound)
            {
                var relatedGame = igdbCache.GetGame(gameId);
                if (relatedGame != null && !this.RelatedGames.Contains(relatedGame.GetReferenceableValue(), StringComparer.OrdinalIgnoreCase))
                {
                    this.RelatedGames.Add(relatedGame.GetReferenceableValue());
                }
            }
        }
    }
}
