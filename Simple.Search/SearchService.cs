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
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Simple.Search;
using Simple.Search.Core;

namespace Simple.Search
{
    public interface ISearchService
    {
        void CreateIndex();
        Task<SearchResult> Search(string searchText);
        void Index(SearchData searchData);
    }

    /// <summary>
    /// Indexes data and searches indexed data.
    /// </summary>
    public sealed class SearchService(
        ILogger<SearchService> logger,
        IOptionsMonitor<SearchConfigurationOptions> optionsMonitor,
        ISearchDataFactory searchDataFactory
    ) : IDisposable, ISearchService
    {
        private readonly ILogger<SearchService> _logger = logger;

        /// <summary>
        /// Store index in memory.
        /// </summary>
        private RAMDirectory? _ramDirectory = new();

        private IndexSearcher? _indexSearcher;

#pragma warning disable 0618 // Alternate analyser not released yet
        /// See https://snowballstem.org/algorithms/english/stemmer.html
        private SnowballAnalyzer? _analyzer = new(LuceneVersion.LUCENE_48, "English"); // Snowball algorithm with an english Stemmer covers all our use-cases. It is not strict and will match more. e.g. Consorted -> Consort

        private bool disposedValue;
        private DirectoryReader? _directoryReader;
        private IndexWriter _writer;
        private readonly SearchConfigurationOptions _searchConfiguration =
            optionsMonitor.CurrentValue;
#pragma warning disable 0618 // Alternate analyser not released yet

        public void CreateIndex()
        {
            _logger.LogInformation("Starting index creation");
            var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, _analyzer)
            {
                OpenMode = OpenMode.CREATE,
            };

            // Write documents to index in memory - using the _ramDirectory
            _writer = new(_ramDirectory, indexConfig);
            var indexId = 1;

            // Iterate all configurations
            // The config does not have all the data - it does not make sense to load all the data into memory
            foreach (var searchConfig in _searchConfiguration.SearchConfigurations)
            {
                try
                {
                    _logger.LogInformation("Building index for: {Name}", searchConfig.Title);
                    // Read the data from the configuration
                    foreach (var searchData in searchDataFactory.CreateSearchData(searchConfig))
                    {
                        if (!string.IsNullOrEmpty(searchData.DataToIndex))
                        {
                            _logger.LogInformation("Indexing: {Uri}", searchData.Uri);
                            IndexHelper.IndexDocs(_writer, searchData);
                        }
                        else
                            _logger.LogWarning(
                                "Search data not found for configuration: {Uri}, {Type}",
                                searchData.Uri,
                                searchConfig.SearchType
                            );

                        indexId++;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to index: {Uri}", searchConfig.Uri);
                    throw;
                }

                // NOTE: if you want to maximize search performance,
                // you can optionally call forceMerge here.  This can be
                // a terribly costly operation, so generally it's only
                // worth it when your index is relatively static (ie
                // you're done adding documents to it):
                //
                _writer.ForceMerge(1);
                _writer.Commit();
                _logger.LogInformation("Index created successfully");
            }

            // Keep the index in memory
            _directoryReader = DirectoryReader.Open(_ramDirectory);
            _indexSearcher = new IndexSearcher(_directoryReader);
        }

        public async Task<SearchResult> Search(string searchText)
        {
            if (_writer == null)
            {
                _logger.LogWarning(
                    "Create an index before searching - Returning empty search results"
                );
                return await CreateEmptyResult();
            }

            _indexSearcher = new IndexSearcher(_writer.GetReader(false));

            if (string.IsNullOrEmpty(searchText))
                return await Task.FromResult(
                    new SearchResult
                    {
                        NotFound = true,
                        DataResults = "Please enter text to search",
                    }
                );

            try
            {
                var results = SearchIndex(searchText);

                var numberOfResults = results.Count;

                return await Task.FromResult(
                    new SearchResult
                    {
                        Results = results,
                        NotFound = numberOfResults == 0,
                        DataResults =
                            numberOfResults != 0
                                ? $"Found {numberOfResults} matches for: {searchText}"
                                : $"Did not find any matches for: {searchText}",
                    }
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during search");
                return await CreateEmptyResult();
            }
        }

        public void Index(SearchData searchData)
        {
            _logger.LogInformation("Indexing: {Uri}", searchData.Uri);
            IndexHelper.IndexDocs(_writer, searchData);
            _writer.Commit();
            _writer.Flush(triggerMerge: false, applyAllDeletes: false);
        }

        private List<SearchData> SearchIndex(string searchText)
        {
            var parser = new QueryParser(LuceneVersion.LUCENE_48, IndexHelper.IndexData, _analyzer);
            var query = parser.Parse(searchText);

            if (_indexSearcher == null)
                throw new NotSupportedException("Index before searching");

            // Order the search result with the Highest score first
            var hits = _indexSearcher
                .Search(query, null, 25)
                .ScoreDocs.OrderByDescending(scoreDoc => scoreDoc.Score)
                .ToList();

            var results = hits.Select(hit =>
                // Convert the Lucene document back to SearchData (note that the data is not stored in the Lucene document)
                new SearchData
                {
                    Id = Guid.Parse(_indexSearcher.Doc(hit.Doc).Get(IndexHelper.IndexId)),
                    Uri = _indexSearcher.Doc(hit.Doc).Get(IndexHelper.IndexUri),
                    ResultToBeShown = _indexSearcher.Doc(hit.Doc).Get(IndexHelper.IndexResults),
                    Score = hit.Score,
                })
                .ToList();
            return results;
        }

        private static async Task<SearchResult> CreateEmptyResult()
        {
            return await Task.FromResult(
                new SearchResult { NotFound = true, DataResults = "Did not find any matches" }
            );
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _ramDirectory?.Dispose();
                    _analyzer?.Dispose();
                }

                _indexSearcher = null;
                _ramDirectory = null;
                _analyzer = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
