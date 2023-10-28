using System.Collections.Generic;
using System.IO;
using GlogGenerator.Data;

namespace GlogGenerator
{
    public class SiteBuilder
    {
        private const string VariableNameSiteBaseURL = "SiteBaseURL";

        private readonly ConfigData configData;

        private VariableSubstitution variableSubstitution;

        public SiteBuilder() : this(new ConfigData()) { }

        public SiteBuilder(ConfigData configData)
        {
            this.configData = configData;

            this.variableSubstitution = new VariableSubstitution();
            this.variableSubstitution.SetSubstitution(VariableNameSiteBaseURL, configData.BaseURL);
        }

        public string GetBaseURL()
        {
            if (!this.variableSubstitution.TryGetSubstitution(VariableNameSiteBaseURL, out var baseURL))
            {
                // Since this variable is set in the constructor, it should never, ever be missing.
                throw new InvalidDataException();
            }

            return baseURL;
        }

        public void SetBaseURL(string baseURL)
        {
            this.variableSubstitution.SetSubstitution(VariableNameSiteBaseURL, baseURL);
        }

        public List<string> GetNowPlaying()
        {
            return this.configData.NowPlaying;
        }

        public VariableSubstitution GetVariableSubstitution()
        {
            return this.variableSubstitution;
        }
    }
}
