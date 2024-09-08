# Simple Search

A simple search service to index data and retrieve using text analysers.

The library circumvents resources necessary to run elastic search and instead provides limited data analysers for search.

The library uses Lucene for .NET:
https://lucenenet.apache.org/

## Key features:
- Webpages hosted locally are indexed
- Snowball analyser used to search pages and return results
https://snowballstem.org/algorithms/english/stemmer.html
- API: search data
- API: Index data 

**Analysers**: analysers are not configurable. For custom analysers, update the search service:

**SearchService.cs**
```
private SnowballAnalyzer? _analyzer = new(LuceneVersion.LUCENE_48, "English");
```

## Start the service
Navigate to the Simple.Search.Rest.Api, and start the Rest API

```
dotnet run
```

## Configuration
The documents located in **wwwroot** are configured for indexing.

The folder to index can be configured in appsettings:

```
"SearchOptions": {
    "FolderBasedSearchConfigurations": [
      {
        "SearchType": 2,
        "Uri": "wwwroot",
        "Title": "Local pages",
        "XpathToTitle": "//h2[contains(@class, 'content-title')]",
        "SearchPattern": "*.html"
      }
    ]
  }
```

## Search API

See API documentation:
https://localhost:7031/swagger/


To search, call the api with a search term.

E.g.

```
curl --insecure  https://localhost:7031/search?q=some
```

Alternatively navigate to **https://localhost:7031/** and start searching

## Index other content

Call the API with data to index.

E.g.

**dataToIndex**: text here will be used by the analyser when searching

**resultToBeShown**: text returned when an item matches the search term

```
curl -X 'PUT' \
  'https://localhost:7031/index' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dataToIndex": "mellow marsh",
  "uri": "custom yall",
  "resultToBeShown": "custom jazz",
  "score": 0
}'
```

## Data sources

It is also possible to add other data sources to index on startup.

E.g.
- Google Firestore tables
- MongoDb
- External web pages
- etc.

To create a custom data ingestion on startup:

- adapt the **SearchDataFactory** 
- create a new **SearchConfiguration** to load the data.



Alternatively feed data using the API to index.

**Note:** the data is stored in memory, monitor the amount of data indexed



# SonarQube analysis

- Start Sonar Qube Server

```
docker run --rm  -p 9000:9000 -p 9092:9092  sonarqube:community
```

- Manual analysis: Create a project called "simplesearch" and generate a token

```
http://localhost:9090
```

- Copy the generated token and add it to the settings file

```
sonarqube.settings
```

- Disable "Force Authentication"

```
Administration -> Configuration -> Security -> Force User Authentication
```

- Run the scan

```
dotnet sonarscanner begin -key:"simplesearch"
dotnet build
dotnet sonarscanner end
```

- Review detailed report
```
http://localhost:9000/dashboard?id=simplesearch
```

## License

Copyright (C) 2024  Paul Eger

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
