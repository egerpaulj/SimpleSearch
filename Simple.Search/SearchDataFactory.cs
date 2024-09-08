
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

using Microsoft.Extensions.Logging;
using Simple.Search.Core;
namespace Simple.Search
{
    public interface ISearchDataFactory
    {
        IEnumerable<SearchData> CreateSearchData(SearchConfiguration searchConfig);
    }

    public class SearchDataFactory(ILogger<SearchDataFactory> logger) : ISearchDataFactory
    {
        public IEnumerable<SearchData> CreateSearchData(SearchConfiguration searchConfig)
        {
            if (searchConfig is SearchConfigFolder searchConfigFolder)
                return CreateSearchData(searchConfigFolder);

            throw new Exception("Unrecognised Search Configuration");
        }

        private IEnumerable<SearchData> CreateSearchData(SearchConfigFolder configuration)
        {
            if (!Directory.Exists(configuration.FolderPath))
            {
                logger.LogCritical(
                    "Configured data folder missing: {Path}",
                    configuration.FolderPath
                );
                return [];
            }

            return Directory
                .GetFiles(
                    configuration.FolderPath,
                    string.IsNullOrEmpty(configuration.SearchPattern)
                        ? "*"
                        : configuration.SearchPattern,
                    SearchOption.AllDirectories
                )
                .Select(file => Map(file, configuration.XpathToTitle));
        }

        private static SearchData Map(string filePath, string xpathToTitle)
        {
            var content = File.ReadAllText(filePath);

            WebPageToTextConverter.Parse(content, xpathToTitle, out var title, out var text);
            return new SearchData
            {
                DataToIndex = $"{text}",
                ResultToBeShown = string.IsNullOrEmpty(title) ? filePath : title,
                Uri = filePath,
                Id = Guid.NewGuid(),
            };
        }
    }
}
