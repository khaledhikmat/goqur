This platform is called `goqur`. It is pronounced `go cure`. It is meant to provide a set of API Endpoints to explore the different nouns and concepts in ayahs of the Quran.  

##Please note## because this is a multi-project source, `azureFunctions.projectSubpath` setting is needed to point to the functions app!!!

## Data source

A CSV file that has one row per Ayah. Each has the following columns:
- Surah number
- Surah Arabic name
- Surah English name
- Surah Arabic type i.e. madani or makki
- Surah English type i.e. madani or makki
- Surah #ayahs
- Ayah number
- Number of Ayah words
- All tags: Arabic and English attributes to characterize the Ayah. 

The richer the attributes/tags are, the richer the results would be. This can be contributed to by bunch of people. Perhaps we would able to provide diferent search indices for different content.  

The main search document is therefore `Ayah`

## Microservices

Initially, the following will be implemented:
- Several Azure Functions: 
    - `Parser`: triggers on storage blob change, read the CSV file, parses it and qneueues its content in chunks to the `Indexer` 
    - `Indexer`: triggers on storage queue to process ayah documents and indexes them in an Azure search service. Document enrichment is limited to nouns and concepts translation for now:
        - If the Arabic nouns (or concepts) collection is populated but the English one is empty, then auto-translate the Arabic entities to English
        - If the English nouns (or concepts) collection is populated and the Arabic one is empty, then auto-translate the English entities to Arabic
        - If both Arabic and English collections are populated, then do not apply any translation
    - `Explorer`; provides REST Endpoints to clients to interact with the 
- Logic App to update the index. This will be deferred as Logic Apps preview in Azure is going through a lot of changes 
- Exploration client:
    - [Azure search generator](http://azsearchstore.azurewebsites.net/azsearchgenerator/index.html) to produce a static site to expriment with content exploration. This can be customozed a little as described [here](https://jj09.net/cognitive-search-azure-search-with-ai/) 
    - wix.com
    - Azure Static Web Apps to deploy the statically generated site
    - Flutter, MAUI or Blazor
    - Etc 

## Things to do

- Beautify index file for the auto-genetrated search site as in Jacob's blog
- Logic app to receive an email with attachment from a specific email source and update the blob storage. This is less important as I can easily upload from the Azure portal
- Translation API for the Enricher service
- Explorer function APIs
- Front-end:
    - wix.com
    - Flutter, MAUI or Blazor
    - Azure Static Web Sites
- CSV import via an indexer...still not sure why I am not able to do it...[posted to SFO](https://stackoverflow.com/questions/67688233/how-to-import-from-csv-into-a-collection-of-strings)
- Durable 3.1 version for the `WaitAll` pattern
- ~~Auto-create index instead of having to reply on manager~~
- There are some hard-coded stuff such as:
    - Queue name
    - Blob name

## References

- Golang custom handler - [https://www.youtube.com/watch?v=RPCEH247twU](https://www.youtube.com/watch?v=RPCEH247twU)
- Azure Search with Jacob - [https://www.youtube.com/watch?v=6kw8SHwxp9c](https://www.youtube.com/watch?v=6kw8SHwxp9c)
- A static generator to generate search site for your index - [AzSearch.js in Github to auto-generate UI](https://aka.ms/azfr/572/03)
- Jacob's blog post about the generation process - [https://jj09.net/cognitive-search-azure-search-with-ai/](https://jj09.net/cognitive-search-azure-search-with-ai/)
- JFK Files after they have been de-classified - [https://jfkfiles2.azurewebsites.net/](https://jfkfiles2.azurewebsites.net/)
- Azure Functions custom handler documentation - [https://docs.microsoft.com/en-us/azure/azure-functions/functions-custom-handlers](https://docs.microsoft.com/en-us/azure/azure-functions/functions-custom-handlers)
- Azure Static Web Apps - [https://www.youtube.com/watch?v=cgAL6z_FcLY](https://www.youtube.com/watch?v=cgAL6z_FcLY) 
- Power Skills - [https://github.com/Azure-Samples/azure-search-power-skills](https://github.com/Azure-Samples/azure-search-power-skills)
- Custom Skills - [https://docs.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-web-api](https://docs.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-web-api)
- Bing Custom Skill - [https://docs.microsoft.com/en-us/azure/search/cognitive-search-create-custom-skill-example](https://docs.microsoft.com/en-us/azure/search/cognitive-search-create-custom-skill-example)
- Data Imports - [https://docs.microsoft.com/en-us/azure/search/search-what-is-data-import](https://docs.microsoft.com/en-us/azure/search/search-what-is-data-import)
- [Azure Static Web Apps](aka.ms/StaticWebApps) 
- [How to index CSV blobs](https://docs.microsoft.com/en-us/azure/search/search-howto-index-csv-blobs)

