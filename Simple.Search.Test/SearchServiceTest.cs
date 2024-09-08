
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Simple.Search;
using Simple.Search.Core;

namespace Simple.Search.Tests;

[TestClass]
public class SearchServiceTest
{
    [TestMethod]
    public async Task BasicGrammarTests()
    {
        // similarity matches
        await RunTest("emergencies");
        await RunTest("emergency");
        await RunTest("emergent");
        await RunTest("emerge");

        // present/past/continuous
        await RunTest("frequently"); // text exists
        await RunTest("frequent"); // text missing - match anyway

        await RunTest("including"); // text exists
        await RunTest("included"); // text missing - match anyway
        await RunTest("includes"); // text missing - match anyway
        await RunTest("include"); // text missing - match anyway

        // Case sensitive
        await RunTest("INCLUDING"); // text exists lower case
        await RunTest("INCLUDED"); // text missing - match anyway
        await RunTest("INCLUDES"); // text missing - match anyway
        await RunTest("INCLUDE"); // text missing - match anyway
    }

    [TestMethod]
    public async Task Index_WhenIndexConfiguredHtml_WhenHtmlHasParagraphs_ThenFound()
    {
        await RunTest("procedures emergency");
    }

    [TestMethod]
    public async Task Index_WhenIndexConfiguredHtml_WhenHtmlHasParagraphs_WhenExistsSingular_WhenSearchTermIsPlural_ThenFound()
    {
        await RunTest("emergencies");
    }

    [TestMethod]
    public async Task Index_WhenIndexConfiguredHtml_WhenHtmlHasParagraphs_WhenExistsPlural_WhenSearchTermIsSingular_ThenFound()
    {
        await RunTest("tube");
    }

    [TestMethod]
    public async Task Index_WhenIndexConfiguredHtml_WhenHtmlHasParagraphs_WhenExistsLowerCase_WhenSearchTermIsUpperCase_ThenFound()
    {
        await RunTest("PRACTICE");
    }

    [TestMethod]
    public async Task Index_WhenIndexConfiguredHtml_WhenHtmlHasLists_WhenTextSearch_ThenFound()
    {
        await RunTest("frequently");
    }

    [TestMethod]
    public async Task Index_WhenIndexConfiguredHtml_WhenHtmlHasFormatting_WhenTextSearch_ThenFound()
    {
        await RunTest("liaison");
    }

    [TestMethod]
    public async Task Index_WhenIndexConfiguredHtml_WhenIndexTitleMissing_WhenHtmlHasFormatting_WhenTextSearch_ThenFound_ThenTitleGenerated()
    {
        await RunTest("liaison");
    }

    [TestMethod]
    public async Task Index_WhenIndexConfiguredHtml_WhenHtmlIsEmpty_ThenNotFound()
    {
        // ARRANGE
        var searchFactoryMock = SetupSearchConfigFactory("missing");

        var testee = new SearchService(
            Mock.Of<ILogger<SearchService>>(),
            searchFactoryMock.Object,
            new SearchDataFactory(Mock.Of<ILogger<SearchDataFactory>>())
        );

        // ACT
        testee.CreateIndex();
        string searchTerm = "PRACTICE";

        var result = await testee.Search(searchTerm);

        // ASSERT
        Assert.IsNotNull(result);
        Assert.IsTrue(result.NotFound);
    }

    [TestMethod]
    public async Task Index_WhenIndexConfiguredHtml_WhenHtmlHasParagraphs_WhenSearchTermIsMissingInHtml_ThenNotFound()
    {
        // ARRANGE
        var searchFactoryMock = SetupSearchConfigFactory("missing");

        var testee = new SearchService(
            Mock.Of<ILogger<SearchService>>(),
            searchFactoryMock.Object,
            new SearchDataFactory(Mock.Of<ILogger<SearchDataFactory>>())
        );

        // ACT
        testee.CreateIndex();

        string searchTerm = "nowayMyGoodSir";

        var result = await testee.Search(searchTerm);

        // ASSERT
        Assert.IsNotNull(result);
        Assert.IsTrue(result.NotFound);
        Assert.AreEqual($"Did not find any matches for: {searchTerm}", result.DataResults);
    }

    [TestMethod]
    public async Task Index_WhenHasHtmlData_EmptySearchTerm_ThenNotFound_ThenMessageToUser()
    {
        // ARRANGE
        var searchFactoryMock = SetupSearchConfigFactory();

        var testee = new SearchService(
            Mock.Of<ILogger<SearchService>>(),
            searchFactoryMock.Object,
            new SearchDataFactory(Mock.Of<ILogger<SearchDataFactory>>())
        );

        // ACT
        testee.CreateIndex();

        string searchTerm = "";

        var result = await testee.Search(searchTerm);

        // ASSERT
        Assert.IsNotNull(result);
        Assert.IsTrue(result.NotFound);
        Assert.AreEqual($"Please enter text to search", result.DataResults);
    }

    [TestMethod]
    public async Task Index_WhenIndexConfiguredHtml_WhenHtmlHasParagraphs_WhenTitleSearch_ThenFound()
    {
        // ARRANGE
        var searchFactoryMock = SetupSearchConfigFactory();

        var testee = new SearchService(
            Mock.Of<ILogger<SearchService>>(),
            searchFactoryMock.Object,
            new SearchDataFactory(Mock.Of<ILogger<SearchDataFactory>>())
        );

        // ACT
        testee.CreateIndex();

        string searchTerm = "AverySpecificTitleText";

        var result = await testee.Search(searchTerm);

        // ASSERT
        AssertResultExists(result);
        var firstResult = AssertResultHasData(result);

        Assert.IsTrue(firstResult.ResultToBeShown.Contains(searchTerm));
    }

    private static async Task<SearchData> RunTest(string searchTerm)
    {
        // ARRANGE
        var searchFactoryMock = SetupSearchConfigFactory();

        var testee = new SearchService(
            Mock.Of<ILogger<SearchService>>(),
            searchFactoryMock.Object,
            new SearchDataFactory(Mock.Of<ILogger<SearchDataFactory>>())
        );

        // ACT
        testee.CreateIndex();

        var result = await testee.Search(searchTerm);

        // ASSERT
        AssertResultExists(result);
        return AssertResultHasData(result);
    }

    private static SearchData AssertResultHasData(SearchResult? result)
    {
        var firstResult = result?.Results?.First();
        Assert.IsNotNull(firstResult);

        Assert.IsFalse(string.IsNullOrEmpty(firstResult.ResultToBeShown));
        Assert.IsFalse(string.IsNullOrEmpty(firstResult.Uri));

        return firstResult;
    }

    private static void AssertResultExists(SearchResult result)
    {
        Assert.IsFalse(result.NotFound);
        Assert.IsNotNull(result.Results);
        Assert.IsTrue(result.Results.Count > 0);
    }

    private static Mock<IOptionsMonitor<SearchConfigurationOptions>> SetupSearchConfigFactory(
        string folderPath = "/home/user/src/SimpleSearch/Simple.Search.Test/TestPages"
    )
    {
        var searchOptionMock = new Mock<IOptionsMonitor<SearchConfigurationOptions>>();

        searchOptionMock
            .Setup(mock => mock.CurrentValue)
            .Returns(
                new SearchConfigurationOptions
                {
                    FolderBasedSearchConfigurations = new List<SearchConfigFolder>
                    {
                        new() {
                            Title = "Test index",
                            SearchType = SearchConfiguration.SearchTypeEnum.Folder,
                            Uri = folderPath,
                            XpathToTitle = "//h2[contains(@class, 'content-title')]",
                        },
                    },
                }
            );
        return searchOptionMock;
    }
}
