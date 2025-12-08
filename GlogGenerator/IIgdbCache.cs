using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlogGenerator.IgdbApi;

namespace GlogGenerator
{
    public interface IIgdbCache
    {
        public T GetEntity<T>(int id)
            where T : IgdbEntity;

        public IgdbCollection GetCollection(int id);

        public IgdbCompany GetCompany(int id);

        public IgdbFranchise GetFranchise(int id);

        public IgdbGame GetGame(int id);

        public List<IgdbGame> GetAllGames();

        public IgdbGameMode GetGameMode(int id);

        public IgdbGameType GetGameType(int id);

        public IgdbGenre GetGenre(int id);

        public IgdbInvolvedCompany GetInvolvedCompany(int id);

        public IgdbKeyword GetKeyword(int id);

        public IgdbPlatform GetPlatform(int id);

        public List<IgdbPlatform> GetAllPlatforms();

        public IgdbPlayerPerspective GetPlayerPerspective(int id);

        public IgdbReleaseDate GetReleaseDate(int id);

        public IgdbTheme GetTheme(int id);

        public List<IgdbEntity> GetAllGameMetadata();

        public IEnumerable<int> GetParentGameIds(int gameId);

        public IEnumerable<int> GetOtherReleaseGameIds(int gameId);

        public IEnumerable<int> GetChildGameIds(int gameId);

        public IEnumerable<int> GetRelatedGameIds(int gameId);

        public void SetAdditionalGames(List<IgdbGame> additionalGames);

        public Task UpdateFromApiClient(IgdbApiClient client);

        public void RemoveEntityById(Type entityType, int id);

        public void RemoveEntityByReferenceString(Type entityType, string referenceString);

        public void WriteToJsonFiles(string directoryPath);
    }
}
