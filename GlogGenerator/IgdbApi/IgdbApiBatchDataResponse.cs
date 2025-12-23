using System.Collections.Generic;

namespace GlogGenerator.IgdbApi
{
    public struct IgdbApiBatchDataResponse
    {
        public List<IgdbCollection> Collections { get; set; }
        public List<IgdbCompany> Companies { get; set; }
        public List<IgdbFranchise> Franchises { get; set; }
        public List<IgdbGame> Games { get; set; }
        public List<IgdbGameMode> GameModes { get; set; }
        public List<IgdbGameStatus> GameStatuses { get; set; }
        public List<IgdbGameType> GameTypes { get; set; }
        public List<IgdbGenre> Genres { get; set; }
        public List<IgdbInvolvedCompany> InvolvedCompanies { get; set; }
        public List<IgdbKeyword> Keywords { get; set; }
        public List<IgdbPlatform> Platforms { get; set; }
        public List<IgdbPlayerPerspective> PlayerPerspectives { get; set; }
        public List<IgdbTheme> Themes { get; set; }
    }
}
