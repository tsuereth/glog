using System;

namespace GlogGenerator
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ProjectPropertiesAttribute : Attribute
    {
        public string DefaultSiteIndexFilesBasePath { get; private set; }

        public string DefaultIgdbCacheFilesBasePath { get; private set; }

        public string DefaultInputFilesBasePath { get; private set; }

        public ProjectPropertiesAttribute(
            string defaultSiteIndexFilesBasePath,
            string defaultIgdbCacheFilesBasePath,
            string defaultInputFilesBasePath)
        {
            this.DefaultSiteIndexFilesBasePath = defaultSiteIndexFilesBasePath;
            this.DefaultIgdbCacheFilesBasePath = defaultIgdbCacheFilesBasePath;
            this.DefaultInputFilesBasePath = defaultInputFilesBasePath;
        }
    }
}
