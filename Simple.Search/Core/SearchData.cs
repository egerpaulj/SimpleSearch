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
    /// The data which is indexed and returned as a result.
    /// </summary>
    public class SearchData
    {
        /// <summary>
        /// Identity of the data item
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The content to index. This should NOT be stored in the Index and is not returned in the results.
        /// Note: Can consume a lot of memory, if the content is stored in the Index.
        /// </summary>
        public string DataToIndex { get; set; }

        /// <summary>
        /// The target URI - (i.e. if the user clicks on a search-result, it informs App-Rail where to obtain the data)
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// The Text shown in the Search result.
        /// </summary>
        public string ResultToBeShown { get; set; }

        public double Score { get; set; }

        public SearchData()
        {
            DataToIndex = string.Empty;
            Uri = string.Empty;
            ResultToBeShown = string.Empty;
        }
    }
}
