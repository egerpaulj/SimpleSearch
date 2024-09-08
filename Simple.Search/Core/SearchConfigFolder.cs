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

namespace Simple.Search.Core
{
    /// <summary>
    /// Html file-based search configuration.
    /// Instead of Crawling via HTTP, a file-based configuration allows indexing all HTML files.
    /// </summary>
    public class SearchConfigFolder : SearchConfiguration
    {
        /// <summary>
        /// The path (relative/absolute) where the files reside.
        /// </summary>
        public string FolderPath => Uri;

        /// <summary>
        /// File to include. E.g. *.html
        /// </summary>
        public string SearchPattern { get; set; } = string.Empty;

        public string XpathToTitle { get; set; } = string.Empty;
    }
}
