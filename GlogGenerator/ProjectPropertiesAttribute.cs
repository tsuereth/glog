using System;

namespace GlogGenerator
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ProjectPropertiesAttribute : Attribute
    {
        public string DefaultInputFilesBasePath { get; private set; }

        public ProjectPropertiesAttribute(
            string defaultInputFilesBasePath)
        {
            this.DefaultInputFilesBasePath = defaultInputFilesBasePath;
        }
    }
}
