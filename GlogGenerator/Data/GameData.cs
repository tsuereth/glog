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

        public string GameType { get; set; } = null;

        public string IgdbUrl { get; set; }

        public HashSet<string> LinkedPostIds { get; set; } = new HashSet<string>();

        public HashSet<string> LinkPostsToOtherGames { get; set; } = new HashSet<string>();

        private IgdbGameReference igdbReference = null;
        private HashSet<string> parentGameReferenceIds { get; set; } = new HashSet<string>();
        private HashSet<string> otherReleaseReferenceIds { get; set; } = new HashSet<string>();
        private HashSet<string> childGameReferenceIds { get; set; } = new HashSet<string>();
        private HashSet<string> tagMetadataReferenceIds { get; set; } = new HashSet<string>();
        private HashSet<string> relatedGameReferenceIds { get; set; } = new HashSet<string>();

        public IgdbGameReference GetIgdbEntityReference()
        {
            return igdbReference;
        }

        public string GetDataId()
        {
            if (this.igdbReference != null && this.igdbReference.HasIgdbEntityData())
            {
                return this.igdbReference.GetIgdbEntityDataId();
            }

            // When a backing data ID isn't available, there's not much choice left but the referenceable key.
            return $"{nameof(GameData)}:key={this.GetReferenceableKey()}";
        }

        public string GetReferenceableKey()
        {
            if (this.igdbReference != null)
            {
                return this.igdbReference.GetReferenceableKey();
            }

            return this.Title;
        }

        public bool MatchesReferenceableKey(string matchKey)
        {
            var thisKey = this.GetReferenceableKey();
            return thisKey.Equals(matchKey, StringComparison.Ordinal);
        }

        public IEnumerable<string> GetIgdbEntityReferenceIds()
        {
            if (this.igdbReference != null && this.igdbReference.HasIgdbEntityData())
            {
                return new List<string>() { this.igdbReference.GetIgdbEntityDataId() };
            }

            return new List<string>();
        }

        public object GetReferenceProperties()
        {
            return this.igdbReference;
        }

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.GetReferenceableKey());
            return $"game/{urlized}/";
        }

        private static void CollectAncestorGameReferenceIdsRecursive(HashSet<string> ancestorGameReferenceIds, ISiteDataIndex siteDataIndex, GameData game)
        {
            foreach (var parentGameReferenceId in game.parentGameReferenceIds)
            {
                if (!ancestorGameReferenceIds.Contains(parentGameReferenceId) && siteDataIndex.TryGetDataByIgdbEntityReferenceId<GameData>(parentGameReferenceId, out var parentGame))
                {
                    ancestorGameReferenceIds.Add(parentGameReferenceId);
                    CollectAncestorGameReferenceIdsRecursive(ancestorGameReferenceIds, siteDataIndex, parentGame);

                    // The parent's other releases are also considered parents.
                    var parentOtherReleaseReferenceIds = new HashSet<string>();
                    CollectAllOtherReleaseReferenceIdsRecursive(parentOtherReleaseReferenceIds, siteDataIndex, parentGame);
                    foreach (var parentOtherReleaseReferenceId in parentOtherReleaseReferenceIds)
                    {
                        if (!ancestorGameReferenceIds.Contains(parentOtherReleaseReferenceId) && siteDataIndex.TryGetDataByIgdbEntityReferenceId<GameData>(parentOtherReleaseReferenceId, out var parentOtherReleaseGame))
                        {
                            ancestorGameReferenceIds.Add(parentOtherReleaseReferenceId);
                            CollectAncestorGameReferenceIdsRecursive(ancestorGameReferenceIds, siteDataIndex, parentOtherReleaseGame);
                        }
                    }
                }
            }

            // Add ancestry for the game's other releases, too.
            var otherReleaseReferenceIds = new HashSet<string>();
            CollectAllOtherReleaseReferenceIdsRecursive(otherReleaseReferenceIds, siteDataIndex, game);
            foreach (var otherReleaseReferenceId in otherReleaseReferenceIds)
            {
                if (siteDataIndex.TryGetDataByIgdbEntityReferenceId<GameData>(otherReleaseReferenceId, out var otherRelease))
                {
                    foreach (var parentGameReferenceId in otherRelease.parentGameReferenceIds)
                    {
                        if (!ancestorGameReferenceIds.Contains(parentGameReferenceId) && siteDataIndex.TryGetDataByIgdbEntityReferenceId<GameData>(parentGameReferenceId, out var parentGame))
                        {
                            ancestorGameReferenceIds.Add(parentGameReferenceId);
                            CollectAncestorGameReferenceIdsRecursive(ancestorGameReferenceIds, siteDataIndex, parentGame);
                        }
                    }
                }
            }
        }

        private static void CollectAllOtherReleaseReferenceIdsRecursive(HashSet<string> allOtherReleaseReferenceIds, ISiteDataIndex siteDataIndex, GameData game)
        {
            foreach (var otherReleaseReferenceId in game.otherReleaseReferenceIds)
            {
                if (!allOtherReleaseReferenceIds.Contains(otherReleaseReferenceId) && siteDataIndex.TryGetDataByIgdbEntityReferenceId<GameData>(otherReleaseReferenceId, out var otherRelease))
                {
                    allOtherReleaseReferenceIds.Add(otherReleaseReferenceId);
                    CollectAllOtherReleaseReferenceIdsRecursive(allOtherReleaseReferenceIds, siteDataIndex, otherRelease);
                }
            }
        }

        public IEnumerable<string> GetParentGameTitles(ISiteDataIndex siteDataIndex)
        {
            var ancestorGameReferenceIds = new HashSet<string>();
            CollectAncestorGameReferenceIdsRecursive(ancestorGameReferenceIds, siteDataIndex, this);

            var ancestorGameTitles = new HashSet<string>();
            foreach (var ancestorGameReferenceId in ancestorGameReferenceIds)
            {
                if (!siteDataIndex.TryGetDataByIgdbEntityReferenceId<GameData>(ancestorGameReferenceId, out var ancestorGame))
                {
                    throw new InvalidDataException($"No Game data found with IGDB entity reference ID {ancestorGameReferenceId}");
                }

                ancestorGameTitles.Add(ancestorGame.Title);
            }

            ancestorGameTitles.Remove(this.Title);

            return ancestorGameTitles;
        }

        public IEnumerable<string> GetOtherReleaseTitles(ISiteDataIndex siteDataIndex)
        {
            var allOtherReleaseReferenceIds = new HashSet<string>();
            CollectAllOtherReleaseReferenceIdsRecursive(allOtherReleaseReferenceIds, siteDataIndex, this);

            var allOtherReleaseTitles = new HashSet<string>();
            foreach (var allOtherReleaseReferenceId in allOtherReleaseReferenceIds)
            {
                if (!siteDataIndex.TryGetDataByIgdbEntityReferenceId<GameData>(allOtherReleaseReferenceId, out var otherRelease))
                {
                    throw new InvalidDataException($"No Game data found with IGDB entity reference ID {allOtherReleaseReferenceId}");
                }

                allOtherReleaseTitles.Add(otherRelease.Title);
            }

            allOtherReleaseTitles.Remove(this.Title);

            return allOtherReleaseTitles;
        }

        public IEnumerable<string> GetChildGameTitles(ISiteDataIndex siteDataIndex)
        {
            var childGameTitles = new HashSet<string>();
            foreach (var childGameReferenceId in this.childGameReferenceIds)
            {
                if (siteDataIndex.TryGetDataByIgdbEntityReferenceId<GameData>(childGameReferenceId, out var childGame))
                {
                    childGameTitles.Add(childGame.Title);
                }
            }

            return childGameTitles;
        }

        public IEnumerable<string> GetTagStrings(ISiteDataIndex siteDataIndex)
        {
            var tagStrings = new HashSet<string>();
            foreach (var tagMetadataReferenceId in this.tagMetadataReferenceIds)
            {
                if (siteDataIndex.TryGetDataByIgdbEntityReferenceId<TagData>(tagMetadataReferenceId, out var tag))
                {
                    tagStrings.Add(tag.Name);
                }
            }

            return tagStrings;
        }

        public IEnumerable<string> GetRelatedGameTitles(ISiteDataIndex siteDataIndex)
        {
            var relatedGameTitles = new HashSet<string>();
            foreach (var relatedGameReferenceId in this.relatedGameReferenceIds)
            {
                if (siteDataIndex.TryGetDataByIgdbEntityReferenceId<GameData>(relatedGameReferenceId, out var relatedGame))
                {
                    relatedGameTitles.Add(relatedGame.Title);
                }
            }

            return relatedGameTitles;
        }

        public static GameData FromIgdbGameReference(IgdbGameReference igdbGameReference)
        {
            var game = new GameData();
            game.igdbReference = igdbGameReference;

            game.Title = game.igdbReference.GetReferenceableKey();

            return game;
        }

        public void PopulateRelatedIgdbData(IIgdbCache igdbCache)
        {
            if (!this.igdbReference.HasIgdbEntityData())
            {
                return;
            }

            var igdbGame = igdbCache.GetGame(this.igdbReference.IgdbEntityId.Value);
            if (igdbGame == null)
            {
                throw new InvalidDataException($"No IGDB Game found with ID {this.igdbReference.IgdbEntityId.Value}");
            }

            this.TryAddTagMetadataReferenceId<IgdbGameType>(igdbCache, igdbGame.GameTypeId);

            if (!string.IsNullOrEmpty(igdbGame.Url))
            {
                this.IgdbUrl = igdbGame.Url;
            }

            foreach (var collectionId in igdbGame.CollectionIds)
            {
                this.TryAddTagMetadataReferenceId<IgdbCollection>(igdbCache, collectionId);
            }

            this.TryAddTagMetadataReferenceId<IgdbFranchise>(igdbCache, igdbGame.MainFranchiseId);

            foreach (var franchiseId in igdbGame.FranchiseIds)
            {
                this.TryAddTagMetadataReferenceId<IgdbFranchise>(igdbCache, franchiseId);
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
                this.TryAddTagMetadataReferenceId<IgdbCompany>(igdbCache, companyId);
            }

            foreach (var genreId in igdbGame.GenreIds)
            {
                this.TryAddTagMetadataReferenceId<IgdbGenre>(igdbCache, genreId);
            }

            foreach (var gameModeId in igdbGame.GameModeIds)
            {
                this.TryAddTagMetadataReferenceId<IgdbGameMode>(igdbCache, gameModeId);
            }

            foreach (var keywordId in igdbGame.KeywordIds)
            {
                this.TryAddTagMetadataReferenceId<IgdbKeyword>(igdbCache, keywordId);
            }

            foreach (var playerPerspectiveId in igdbGame.PlayerPerspectiveIds)
            {
                this.TryAddTagMetadataReferenceId<IgdbPlayerPerspective>(igdbCache, playerPerspectiveId);
            }

            foreach (var themeId in igdbGame.ThemeIds)
            {
                this.TryAddTagMetadataReferenceId<IgdbTheme>(igdbCache, themeId);
            }

            foreach (var parentGameId in igdbCache.GetParentGameIds(igdbGame.Id))
            {
                this.TryAddParentGameReferenceId(igdbCache, parentGameId);
            }

            foreach (var otherReleaseGameId in igdbCache.GetOtherReleaseGameIds(igdbGame.Id))
            {
                this.TryAddOtherReleaseReferenceId(igdbCache, otherReleaseGameId);
            }

            foreach (var childGameId in igdbCache.GetChildGameIds(igdbGame.Id))
            {
                this.TryAddChildGameReferenceId(igdbCache, childGameId);
            }

            foreach (var relatedGameId in igdbCache.GetRelatedGameIds(igdbGame.Id))
            {
                this.TryAddRelatedGameReferenceId(igdbCache, relatedGameId);
            }
        }

        private void TryAddTagMetadataReferenceId<T>(IIgdbCache igdbCache, int metadataId)
            where T : IgdbEntity
        {
            if (metadataId != IgdbEntity.IdNotFound)
            {
                this.tagMetadataReferenceIds.Add(IIgdbEntityReference.FormatIgdbEntityReferenceDataId(typeof(T), metadataId));
            }
        }

        private void TryAddParentGameReferenceId(IIgdbCache igdbCache, int parentGameId)
        {
            var thisGameId = (this.igdbReference != null && this.igdbReference.HasIgdbEntityData()) ? this.igdbReference.IgdbEntityId : IgdbEntity.IdNotFound;
            if (parentGameId != IgdbEntity.IdNotFound && parentGameId != thisGameId)
            {
                this.parentGameReferenceIds.Add(IIgdbEntityReference.FormatIgdbEntityReferenceDataId(typeof(IgdbGame), parentGameId));
            }
        }

        private void TryAddOtherReleaseReferenceId(IIgdbCache igdbCache, int otherReleaseGameId)
        {
            var thisGameId = (this.igdbReference != null && this.igdbReference.HasIgdbEntityData()) ? this.igdbReference.IgdbEntityId : IgdbEntity.IdNotFound;
            if (otherReleaseGameId != IgdbEntity.IdNotFound && otherReleaseGameId != thisGameId)
            {
                this.otherReleaseReferenceIds.Add(IIgdbEntityReference.FormatIgdbEntityReferenceDataId(typeof(IgdbGame), otherReleaseGameId));
            }
        }

        private void TryAddChildGameReferenceId(IIgdbCache igdbCache, int childGameId)
        {
            var thisGameId = (this.igdbReference != null && this.igdbReference.HasIgdbEntityData()) ? this.igdbReference.IgdbEntityId : IgdbEntity.IdNotFound;
            if (childGameId != IgdbEntity.IdNotFound && childGameId != thisGameId)
            {
                this.childGameReferenceIds.Add(IIgdbEntityReference.FormatIgdbEntityReferenceDataId(typeof(IgdbGame), childGameId));
            }
        }

        private void TryAddRelatedGameReferenceId(IIgdbCache igdbCache, int relatedGameId)
        {
            var thisGameId = (this.igdbReference != null && this.igdbReference.HasIgdbEntityData()) ? this.igdbReference.IgdbEntityId : IgdbEntity.IdNotFound;
            if (relatedGameId != IgdbEntity.IdNotFound && relatedGameId != thisGameId)
            {
                this.relatedGameReferenceIds.Add(IIgdbEntityReference.FormatIgdbEntityReferenceDataId(typeof(IgdbGame), relatedGameId));
            }
        }
    }
}
