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

        public HashSet<string> LinkedPostIds { get; set; } = new HashSet<string>();

        public HashSet<string> LinkPostsToOtherGames { get; set; } = new HashSet<string>();

        private string dataId;
        private string referenceableKey;
        private HashSet<string> parentGames { get; set; } = new HashSet<string>();
        private HashSet<string> otherReleases { get; set; } = new HashSet<string>();
        private HashSet<string> childGames { get; set; } = new HashSet<string>();
        private HashSet<string> relatedGames { get; set; } = new HashSet<string>();

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

        public IEnumerable<string> GetParentGames(ISiteDataIndex siteDataIndex)
        {
            return this.parentGames.Where(s => siteDataIndex.HasGame(s));
        }

        public IEnumerable<string> GetOtherReleases(ISiteDataIndex siteDataIndex)
        {
            return this.otherReleases.Where(s => siteDataIndex.HasGame(s));
        }

        public IEnumerable<string> GetChildGames(ISiteDataIndex siteDataIndex)
        {
            return this.childGames.Where(s => siteDataIndex.HasGame(s));
        }

        public IEnumerable<string> GetRelatedGames(ISiteDataIndex siteDataIndex)
        {
            return this.relatedGames.Where(s => siteDataIndex.HasGame(s));
        }

        public static GameData FromIgdbGame(IIgdbCache igdbCache, IgdbGame igdbGame)
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
            }

            if (!string.IsNullOrEmpty(igdbGame.Url))
            {
                game.IgdbUrl = igdbGame.Url;
            }

            game.TryAddTag<IgdbCollection>(igdbCache, igdbGame.MainCollectionId);

            foreach (var collectionId in igdbGame.CollectionIds)
            {
                game.TryAddTag<IgdbCollection>(igdbCache, collectionId);
            }

            game.TryAddTag<IgdbFranchise>(igdbCache, igdbGame.MainFranchiseId);

            foreach (var franchiseId in igdbGame.FranchiseIds)
            {
                game.TryAddTag<IgdbFranchise>(igdbCache, franchiseId);
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
                game.TryAddTag<IgdbCompany>(igdbCache, companyId);
            }

            foreach (var genreId in igdbGame.GenreIds)
            {
                game.TryAddTag<IgdbGenre>(igdbCache, genreId);
            }

            foreach (var gameModeId in igdbGame.GameModeIds)
            {
                game.TryAddTag<IgdbGameMode>(igdbCache, gameModeId);
            }

            foreach (var keywordId in igdbGame.KeywordIds)
            {
                game.TryAddTag<IgdbKeyword>(igdbCache, keywordId);
            }

            foreach (var playerPerspectiveId in igdbGame.PlayerPerspectiveIds)
            {
                game.TryAddTag<IgdbPlayerPerspective>(igdbCache, playerPerspectiveId);
            }

            foreach (var themeId in igdbGame.ThemeIds)
            {
                game.TryAddTag<IgdbTheme>(igdbCache, themeId);
            }

            foreach (var parentGameId in igdbCache.GetParentGameIds(igdbGame.Id))
            {
                game.TryAddParentGame(igdbCache, parentGameId);
            }

            foreach (var otherReleaseGameId in igdbCache.GetOtherReleaseGameIds(igdbGame.Id))
            {
                game.TryAddOtherRelease(igdbCache, otherReleaseGameId);
            }

            foreach (var childGameId in igdbCache.GetChildGameIds(igdbGame.Id))
            {
                game.TryAddChildGame(igdbCache, childGameId);
            }

            foreach (var relatedGameId in igdbCache.GetRelatedGameIds(igdbGame.Id))
            {
                game.TryAddRelatedGame(igdbCache, relatedGameId);
            }

            return game;
        }

        private void TryAddTag<T>(IIgdbCache igdbCache, int tagId)
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
                }
            }
        }

        private void TryAddParentGame(IIgdbCache igdbCache, int parentGameId)
        {
            if (parentGameId != IgdbEntity.IdNotFound)
            {
                var parentGame = igdbCache.GetGame(parentGameId);
                if (parentGame != null &&
                    !parentGame.GetUniqueIdString(igdbCache).Equals(this.dataId, StringComparison.Ordinal) &&
                    !this.parentGames.Contains(parentGame.GetReferenceString(igdbCache), StringComparer.Ordinal))
                {
                    var parentGameReferenceString = parentGame.GetReferenceString(igdbCache);
                    this.parentGames.Add(parentGameReferenceString);

                    // Include this game's posts in the parent's list of posts.
                    this.LinkPostsToOtherGames.Add(parentGameReferenceString);

                    // Add the parent's other releases, too.
                    foreach (var parentOtherReleaseGameId in igdbCache.GetOtherReleaseGameIds(parentGameId))
                    {
                        this.TryAddParentGame(igdbCache, parentOtherReleaseGameId);
                    }

                    // AND, recursively add the parent's parents.
                    foreach (var grandparentGameId in igdbCache.GetParentGameIds(parentGameId))
                    {
                        this.TryAddParentGame(igdbCache, grandparentGameId);
                    }
                }
            }
        }

        private void TryAddOtherRelease(IIgdbCache igdbCache, int otherReleaseGameId)
        {
            if (otherReleaseGameId != IgdbEntity.IdNotFound)
            {
                var otherReleaseGame = igdbCache.GetGame(otherReleaseGameId);
                if (otherReleaseGame != null &&
                    !otherReleaseGame.GetUniqueIdString(igdbCache).Equals(this.dataId, StringComparison.Ordinal) &&
                    !this.otherReleases.Contains(otherReleaseGame.GetReferenceString(igdbCache), StringComparer.Ordinal))
                {
                    var otherReleaseReferenceString = otherReleaseGame.GetReferenceString(igdbCache);
                    this.otherReleases.Add(otherReleaseReferenceString);

                    // Include this game's posts in the other release's list of posts.
                    this.LinkPostsToOtherGames.Add(otherReleaseReferenceString);

                    // Add the other release's parents, too.
                    foreach (var parentGameId in igdbCache.GetParentGameIds(otherReleaseGameId))
                    {
                        this.TryAddParentGame(igdbCache, parentGameId);
                    }
                }
            }
        }

        private void TryAddChildGame(IIgdbCache igdbCache, int childGameId)
        {
            if (childGameId != IgdbEntity.IdNotFound)
            {
                var childGame = igdbCache.GetGame(childGameId);
                if (childGame != null &&
                    !childGame.GetUniqueIdString(igdbCache).Equals(this.dataId, StringComparison.Ordinal) &&
                    !this.childGames.Contains(childGame.GetReferenceString(igdbCache), StringComparer.Ordinal))
                {
                    var childGameReferenceString = childGame.GetReferenceString(igdbCache);
                    this.childGames.Add(childGameReferenceString);
                }
            }
        }

        private void TryAddRelatedGame(IIgdbCache igdbCache, int relatedGameId)
        {
            if (relatedGameId != IgdbEntity.IdNotFound)
            {
                var relatedGame = igdbCache.GetGame(relatedGameId);
                if (relatedGame != null &&
                    !relatedGame.GetUniqueIdString(igdbCache).Equals(this.dataId, StringComparison.Ordinal) &&
                    !this.relatedGames.Contains(relatedGame.GetReferenceString(igdbCache), StringComparer.Ordinal))
                {
                    var relatedGameReferenceString = relatedGame.GetReferenceString(igdbCache);
                    this.relatedGames.Add(relatedGameReferenceString);
                }
            }
        }
    }
}
