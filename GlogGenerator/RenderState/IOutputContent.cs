using System.Net;

namespace GlogGenerator.RenderState
{
    public interface IOutputContent
    {
        public string GetContentType();

        public bool GetContentTypeIsText();

        public byte[] GetBytes(SiteState site);

        public string GetText(SiteState site);

        public void WriteFile(SiteState site, string filePath);

        public void WriteHttpListenerResponse(SiteState site, ref HttpListenerResponse response);
    }
}
