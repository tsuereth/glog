using System.Collections.Generic;

namespace GlogGenerator
{
    public class VariableSubstitution
    {
        private Dictionary<string, string> substitutions = new Dictionary<string, string>();

        public void SetSubstitution(string variableName, string variableValue)
        {
            this.substitutions[variableName] = variableValue;
        }

        public bool TryGetSubstitution(string variableName, out string variableValue)
        {
            if (this.substitutions.TryGetValue(variableName, out var substitionValue))
            {
                variableValue = substitionValue;
                return true;
            }

            variableValue = null;
            return false;
        }

        public string TryMakeSubstitutions(string str)
        {
            var variableRefStart = str.IndexOf('$');
            while (variableRefStart != -1)
            {
                var variableRefEnd = str.IndexOf('$', variableRefStart + 1);
                if (variableRefEnd == -1)
                {
                    break;
                }

                var variableNameStart = variableRefStart + 1;
                var variableNameLen = variableRefEnd - variableNameStart;
                ++variableRefEnd;
                var variableRefLen = variableRefEnd - variableRefStart;
                var variableName = str.Substring(variableNameStart, variableNameLen);
                if (this.substitutions.TryGetValue(variableName, out var variableValue))
                {
                    str = str.Remove(variableRefStart, variableRefLen).Insert(variableRefStart, variableValue);

                    variableRefEnd = variableRefStart + variableValue.Length;
                }

                variableRefStart = str.IndexOf('$', variableRefEnd);
            }

            return str;
        }
    }
}
