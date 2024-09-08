
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
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Simple.Search.Core;


namespace Simple.Search.Rest.Api
{
    /// API to Search Data
    /// CTOR
    [ApiController]
    public class SearchController(ISearchService searchService) : Controller
    {
        private readonly ISearchService _searchService = searchService;

        /// <summary>
        /// Search All Content
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /search?q=searchText
        ///
        /// </remarks>
        /// <response code="200">Returns the list of Search Results.</response>
        /// <response code="404">Return a 404 if results are empty.</response>
        [Route("/search")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SearchResult>>> Search(string? q)
        {
            var result = await _searchService.Search(q ?? string.Empty);
            return result.NotFound
            ? StatusCode(404)
            : Ok(result?.Results!);
        }

        /// <summary>
        /// Search All Content
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /search?q=searchText
        ///
        /// </remarks>
        /// <response code="200">Returns success.</response>
        [Route("/index")]
        [HttpPut]
        public void Index([FromBody] SearchData searchData)
        {
            _searchService.Index(searchData);
            
        }
    }
}