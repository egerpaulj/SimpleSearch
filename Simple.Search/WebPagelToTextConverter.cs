
//      Simple search Libraries for .Net C#                                                                                                                                       
//      Copyright (C) 2024  Paul Eger                                                                                                                                                                     

//      This program is free software: you can redistribute it and/or modify                                                                                                                                          
//      it under the terms of the GNU General Public License as published by                                                                                                                                          
//      the Free Software Foundation, either version 3 of the License, or                                                                                                                                             
//      (at your option) any later version.                                                                                                                                                                           

//      This program is distributed in the hope that it will be useful,                                                                                                                                               
//      but WITHOUT ANY WARRANTY; without even the implied warranty of                                                                                                                                                
//      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                                                                                                                                                 
//      GNU General Public License for more details.                                                                                                                                                                  

//      You should have received a copy of the GNU General Public License                                                                                                                                             
//      along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Simple.Search
{
    internal static partial class WebPageToTextConverter
    {
        internal static void Parse(
            string content,
            string xpathToTitle,
            out string title,
            out string text
        )
        {
            /// Html is not well formed XML - hence use the HtmlAgilityPack or similar to parse Html documents.
            var htmlDocument = new HtmlDocument
            {
                OptionOutputAsXml = true,
                OptionFixNestedTags = true,
                OptionReadEncoding = true
            };

            htmlDocument.LoadHtml(content);

            var nodes = htmlDocument.DocumentNode.ChildNodes;

            /// Aggregate all Html elements/nodes and get All text for indexing (i.e. without the HTML Tags)
            text = nodes
                .Where(n => !nodes.Any(n2 => n2.ChildNodes.Contains(n)))
                .Distinct(HtmlNodeComparer.EqualityComparer)
                .Where(node =>
                    node.NodeType == HtmlNodeType.Element || node.NodeType == HtmlNodeType.Text
                )
                .Select(node => node.InnerText)
                .Aggregate(
                    new StringBuilder(),
                    (builder, val) => builder.AppendLine(val),
                    b => b.ToString()
                );

            /// Get the Titles defined by the Xpath
            var titleNodes = htmlDocument.DocumentNode.SelectNodes(xpathToTitle)?.ToList();

            title = titleNodes?.FirstOrDefault()?.InnerText ?? string.Empty;

            /// If the HTML does not have a title -> then generate a title using the content.
            if (string.IsNullOrEmpty(title))
            {
                var generatedTitle = IgnoreChars().Replace(text.Trim(), "");

                var maximumTitleLength = 10;
                if (generatedTitle.Length > maximumTitleLength)
                {
                    title = string.Concat(generatedTitle.AsSpan(0, maximumTitleLength), "...");
                }
            }
        }

        [GeneratedRegex(@"\t|\n|\r")]
        private static partial Regex IgnoreChars();
    }

    public class HtmlNodeComparer : IEqualityComparer<HtmlNode>
    {
        public bool Equals(HtmlNode? x, HtmlNode? y)
        {
            return x?.Name.ToLower() == y?.Name.ToLower() && x?.OuterHtml == y?.OuterHtml;
        }

        public int GetHashCode([DisallowNull] HtmlNode obj)
        {
            return obj.OuterHtml.GetHashCode();
        }

        internal static IEqualityComparer<HtmlNode> EqualityComparer = new HtmlNodeComparer();
    }
}
