using System;

namespace Api.Common
{
    public class StringHelper
    {
        public enum NewLineReplacement
        {
            EnvironmentNewLine,
            Space,
            Semicolon,
            HtmlBreak
        }

        /// <summary>
        /// Cleans a raw string comming from a JSON response. The method removes the leading and trailing quotes,
        /// and replaces cr/lf characters by new line.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FormatJsonString(string text, NewLineReplacement newLineReplacement = NewLineReplacement.EnvironmentNewLine)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.StartsWith("\""))
                text = text.Substring(1);

            if (text.EndsWith("\""))
                text = text.Substring(0, text.Length - 1);

            var rep = default(string);
            if (newLineReplacement == NewLineReplacement.Semicolon)
                rep = "; ";
            else if (newLineReplacement == NewLineReplacement.Space)
                rep = " ";
            else if (newLineReplacement == NewLineReplacement.HtmlBreak)
                rep = "<br/>";
            else
                rep = Environment.NewLine;

            while (text.Contains("\\r\\n"))
                text = text.Replace("\\r\\n", rep);

            return text;
        }
    }
}