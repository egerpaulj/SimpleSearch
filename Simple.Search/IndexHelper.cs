
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
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Simple.Search.Core;

namespace Simple.Search
{
    internal static class IndexHelper
    {
        internal const string IndexResults = "results";
        internal const string IndexData = "data";
        internal const string IndexUiType = "uitype";
        internal const string IndexId = "indexid";
        internal const string IndexUri = "uri";

        /// <summary>
        /// Adds a document to the index. SearchData is converted to a Lucene Document.
        /// </summary>
        internal static void IndexDocs(IndexWriter writer, SearchData searchData)
        {
            var doc = new Document
            {
                /// Don't store data in the index
                new TextField(IndexData, searchData.DataToIndex, Field.Store.NO),
                new StringField(IndexResults, searchData.ResultToBeShown, Field.Store.YES),
                new TextField(IndexId, searchData.Id.ToString(), Field.Store.YES),
                new StringField(IndexUri, searchData.Uri, Field.Store.YES),
            };

            writer.AddDocument(doc);
        }
    }
}
