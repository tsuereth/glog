using System.ComponentModel;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#game-enums
#pragma warning disable CA1707 // Identifiers should not contain underscores
    public enum IgdbGameCategory
    {
        None = -1,

        [Description("Main game")]
        main_game = 0,

        [Description("DLC / Addon")]
        dlc_addon = 1,

        [Description("Expansion")]
        expansion = 2,

        [Description("Bundle")]
        bundle = 3,

        [Description("Standalone expansion")]
        standalone_expansion = 4,

        [Description("Mod")]
        mod = 5,

        [Description("Episode")]
        episode = 6,

        [Description("Season")]
        season = 7,

        [Description("Remake")]
        remake = 8,

        [Description("Remaster")]
        remaster = 9,

        [Description("Expanded Game")]
        expanded_game = 10,

        [Description("Port")]
        port = 11,

        [Description("Fork")]
        fork = 12,

        [Description("Pack")]
        pack = 13,

        [Description("Update")]
        update = 14,
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
