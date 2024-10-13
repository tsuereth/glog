using System.Collections.Generic;
using System.Threading.Tasks;
using GlogGenerator.IgdbApi;

namespace GlogGenerator
{
    public interface IIgdbCache
    {
        public IgdbCollection GetCollection(int id);

        public IgdbCompany GetCompany(int id);

        public IgdbFranchise GetFranchise(int id);

        public IgdbGame GetGame(int id);

        public List<IgdbGame> GetAllGames();

        public IgdbGameMode GetGameMode(int id);

        public IgdbGenre GetGenre(int id);

        public IgdbInvolvedCompany GetInvolvedCompany(int id);

        public IgdbKeyword GetKeyword(int id);

        public IgdbPlatform GetPlatform(int id);

        public List<IgdbPlatform> GetAllPlatforms();

        public IgdbPlayerPerspective GetPlayerPerspective(int id);

        public IgdbReleaseDate GetReleaseDate(int id);

        public IgdbTheme GetTheme(int id);

        public List<IgdbEntity> GetAllGameMetadata();

        public List<int> GetBundledGameIds(int bundleGameId);

        public Task UpdateFromApiClient(IgdbApiClient client);

        public void WriteToJsonFile(string directoryPath);
    }
}
