using System.Net;

namespace GlogGenerator.RenderState
{
    public interface IOutputContent
    {
        public void WriteFile(SiteState site, string filePath);

        public void WriteHttpListenerResponse(SiteState site, ref HttpListenerResponse response);
    }
}
