using System;
using System.Collections.Generic;
using System.IO;
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

        public static GameData FromIgdbGame(IIgdbCache igdbCache, IgdbGame igdbGame, ISiteDataIndex siteDataIndex)
        {
            var game = new GameData();
            game.dataId = igdbGame.GetUniqueIdString(igdbCache);
            game.referenceableKey = igdbGame.GetReferenceString(igdbCache);

            game.Title = igdbGame.GetReferenceString(igdbCache);

            if (igdbGame.Category != IgdbGameCategory.None)
            {
                game.IgdbCategory = igdbGame.Category;

                var categoryString = igdbGame.Category.Description();
                game.Tags.Add(categoryString);

                // Register a data reference to the category, so the data index knows it is in-use (and won't delete it).
                siteDataIndex.CreateReference<TagData>(categoryString);
            }

            if (!string.IsNullOrEmpty(igdbGame.Url))
            {
                game.IgdbUrl = igdbGame.Url;
            }

            game.TryAddTag<IgdbCollection>(igdbCache, igdbGame.MainCollectionId, siteDataIndex);

            foreach (var collectionId in igdbGame.CollectionIds)
            {
                game.TryAddTag<IgdbCollection>(igdbCache, collectionId, siteDataIndex);
            }

            game.TryAddTag<IgdbFranchise>(igdbCache, igdbGame.MainFranchiseId, siteDataIndex);

            foreach (var franchiseId in igdbGame.FranchiseIds)
            {
                game.TryAddTag<IgdbFranchise>(igdbCache, franchiseId, siteDataIndex);
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
                game.TryAddTag<IgdbCompany>(igdbCache, companyId, siteDataIndex);
            }

            foreach (var genreId in igdbGame.GenreIds)
            {
                game.TryAddTag<IgdbGenre>(igdbCache, genreId, siteDataIndex);
            }

            foreach (var gameModeId in igdbGame.GameModeIds)
            {
                game.TryAddTag<IgdbGameMode>(igdbCache, gameModeId, siteDataIndex);
            }

            foreach (var keywordId in igdbGame.KeywordIds)
            {
                game.TryAddTag<IgdbKeyword>(igdbCache, keywordId, siteDataIndex);
            }

            foreach (var playerPerspectiveId in igdbGame.PlayerPerspectiveIds)
            {
                game.TryAddTag<IgdbPlayerPerspective>(igdbCache, playerPerspectiveId, siteDataIndex);
            }

            foreach (var themeId in igdbGame.ThemeIds)
            {
                game.TryAddTag<IgdbTheme>(igdbCache, themeId, siteDataIndex);
            }

            game.TryAddRelatedGame(igdbCache, igdbGame.ParentGameId, siteDataIndex);
            game.TryAddRelatedGame(igdbCache, igdbGame.VersionParentGameId, siteDataIndex);

            // If this game is a "bundle," then add its bundled games as related.
            if (igdbGame.Category == IgdbGameCategory.bundle)
            {
                var bundledGameIds = igdbCache.GetBundledGameIds(igdbGame.Id);
                foreach (var bundledGameId in bundledGameIds)
                {
                    game.TryAddRelatedGame(igdbCache, bundledGameId, siteDataIndex);
                }
            }

            foreach (var relatedGameId in igdbGame.BundleGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId, siteDataIndex);

                // Add other games from the same bundle, too.
                var bundledGameIds = igdbCache.GetBundledGameIds(relatedGameId);
                foreach (var bundledGameId in bundledGameIds)
                {
                    // (But not the current game, as "related" to itself, that'd be silly!)
                    if (bundledGameId == igdbGame.Id)
                    {
                        continue;
                    }

                    game.TryAddRelatedGame(igdbCache, bundledGameId, siteDataIndex);
                }

                // The bundle should link to posts for this game.
                var bundleGame = igdbCache.GetGame(relatedGameId);
                if (bundleGame != null)
                {
                    game.LinkPostsToOtherGames.Add(bundleGame.GetReferenceString(igdbCache));
                }
            }

            foreach (var relatedGameId in igdbGame.DlcGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId, siteDataIndex);
            }

            foreach (var relatedGameId in igdbGame.ExpandedGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId, siteDataIndex);
            }

            foreach (var relatedGameId in igdbGame.ExpansionGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId, siteDataIndex);
            }

            foreach (var relatedGameId in igdbGame.ForkGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId, siteDataIndex);
            }

            foreach (var relatedGameId in igdbGame.PortGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId, siteDataIndex);
            }

            foreach (var relatedGameId in igdbGame.RemakeGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId, siteDataIndex);
            }

            foreach (var relatedGameId in igdbGame.RemasterGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId, siteDataIndex);
            }

            foreach (var relatedGameId in igdbGame.StandaloneExpansionGameIds)
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId, siteDataIndex);
            }

            return game;
        }

        private void TryAddTag<T>(IIgdbCache igdbCache, int tagId, ISiteDataIndex siteDataIndex)
            where T : IgdbEntity
        {
            if (tagId != IgdbEntity.IdNotFound)
            {
                var tagEntity = igdbCache.GetEntity<T>(tagId);
                if (tagEntity != null)
                {
                    var tagReferenceString = tagEntity.GetReferenceString(igdbCache);
                    var addTagString = false;

                    var tagUrlized = UrlizedString.Urlize(tagReferenceString);
                    var matchingTag = this.Tags.Where(s => UrlizedString.Urlize(s) == tagUrlized);
                    if (!matchingTag.Any())
                    {
                        addTagString = true;
                    }
                    else
                    {
                        if (matchingTag.Count() > 1)
                        {
                            throw new InvalidDataException($"More than one game tag matches urlized string {tagUrlized}");
                        }

                        // Ensure the listed tag value is the "canonical" value.
                        // (When a tag has multiple variant strings, use the alpha-sorted-higher string.)
                        var existingTagReferenceString = matchingTag.First();
                        if (string.CompareOrdinal(tagReferenceString, existingTagReferenceString) < 0)
                        {
                            this.Tags.Remove(existingTagReferenceString);
                            addTagString = true;
                        }
                    }

                    if (addTagString)
                    {
                        this.Tags.Add(tagReferenceString);
                    }

                    // Register a data reference to the tag, so the data index knows it is in-use (and won't delete it).
                    siteDataIndex.CreateReference<TagData>(tagReferenceString);
                }
            }
        }

        private void TryAddRelatedGame(IIgdbCache igdbCache, int gameId, ISiteDataIndex siteDataIndex)
        {
            if (gameId != IgdbEntity.IdNotFound)
            {
                var relatedGame = igdbCache.GetGame(gameId);
                if (relatedGame != null && !this.RelatedGames.Contains(relatedGame.GetReferenceString(igdbCache), StringComparer.OrdinalIgnoreCase))
                {
                    this.RelatedGames.Add(relatedGame.GetReferenceString(igdbCache));

                    // Register a data reference to the related game, so the data index knows it is in-use (and won't delete it).
                    siteDataIndex.CreateReference<GameData>(relatedGame.GetReferenceString(igdbCache));
                }
            }
        }
    }
}
