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
    /// Search result provides an abstraction to handle successful and unsuccessful searches
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// NotFound marker - Will be set to true if the results are empty.
        /// </summary>
        public bool NotFound { get; set; }

        public string DataResults { get; set; }

        /// <summary>
        /// The results sorted with the highest score first.
        /// </summary>
        public List<SearchData>? Results { get; set; }

        public SearchResult()
        {
            DataResults = string.Empty;
        }
    }
}
