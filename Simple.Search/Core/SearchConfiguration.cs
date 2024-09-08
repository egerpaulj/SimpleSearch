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
    /// Search configuration defines the items that should be indexed (i.e. Where to search)
    /// </summary>
    public abstract class SearchConfiguration
    {
        public enum SearchTypeEnum
        {
            /// <summary>
            /// Data is from a website
            /// </summary>
            Web = 1,

            /// <summary>
            /// Data is from a folder
            /// </summary>
            Folder = 2,

            /// <summary>
            /// Data is from Mongodb
            /// </summary>
            Mongodb = 3,
        }

        /// <summary>
        /// The type of data - this helps determine how get the content for indexing.
        /// </summary>
        public SearchTypeEnum SearchType { get; set; }

        /// <summary>
        /// The title will be displayed in the Search Results. If not defined, a title should be generated from the content.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Defines where the data resides. Used by the search result - i.e. when an item is found, the user can navigate to this data source.
        /// </summary>
        public string Uri { get; set; } = string.Empty;
    }
}
