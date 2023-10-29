using System;
using Antlr4.StringTemplate.Misc;

namespace GlogGenerator.TemplateRenderers
{
    public class ExceptionThrowingErrorListener : Antlr4.StringTemplate.ITemplateErrorListener
    {
        public void CompiletimeError(TemplateMessage msg)
        {
            throw new StringTemplateErrorException(msg.ToString(), msg.Cause);
        }

        public void InternalError(TemplateMessage msg)
        {
            throw new StringTemplateErrorException(msg.ToString(), msg.Cause);
        }

        public void IOError(TemplateMessage msg)
        {
            throw new StringTemplateErrorException(msg.ToString(), msg.Cause);
        }

        public void RuntimeError(TemplateMessage msg)
        {
            throw new StringTemplateErrorException(msg.ToString(), msg.Cause);
        }
    }

    public class StringTemplateErrorException : Exception
    {
        public StringTemplateErrorException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
