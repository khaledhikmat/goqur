This platform is called `goqur`. It is pronounced `go cure`. It is meant to provide a set of API Endpoints to explore the different nouns and concepts in ayahs of the Quran.  

**Please note** that, because this is a multi-project source, VS code requires this `azureFunctions.projectSubpath` setting point to the functions app!!!

## Data source

The `goqur` data source is a CSV file with one row per Ayaha and a header. Each row has the following columns:
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

The main search document is therefore `Ayah`. Please see [auran-ayahs.csv](auran-ayahs.csv) for a sample source file. 

## Design Notes

*Custom handlers are a thing in Functions. This source code has an [example](golang/server.go) in Go. But obviously it makes more sense to use C# for this purpose since the client SDKs are much richer.*

- Experiment with custom handlers using Go in Azure Functions to expose  `goqur` REST endpoints.
- Investigate these existing Quran APIs [https://quran.api-docs.io/v3/verses](https://quran.api-docs.io/v3/verses), [https://alquran.cloud/api](https://alquran.cloud/api) and [http://docs.globalquran.com/Tutorials](http://docs.globalquran.com/Tutorials).
- Azure Search supports two modes [push and pull](https://docs.microsoft.com/en-us/azure/search/search-what-is-data-import):
    - The push model, while flexible, does not allow for external processing (or enrichment).
    - Only pull model indexers supports enrichment. 
- A parser triggers on the blob storage file to parse the CSV and update the index.
- Enrich manually using 3rd party translation API
- When we import data via the portal, it analyzes the storage blob and creates it own index. The index created by the `manager` cannot be used.
- Adding a new `Skill` using the `Skillsets` requires that we did the import via the portal
- There is something wrong with the CSV parsing! It is not understanding the Collection...very unfortunate. I posted an issue in [SFO](https://stackoverflow.com/questions/67688233/how-to-import-from-csv-into-a-collection-of-strings)...no answer.
- The CSV file that is saved from Excel does not contain double-quotes on every field.
- Remember to disable the function app function to debug locally so it will not listen in the same blob storage
- .NET5 is not supported in all bindings and definitely it does not work with duarable functions. This is a sample out-of-process function app that shows how to inject [https://github.com/Azure/azure-functions-dotnet-worker/tree/main/samples/FunctionApp](https://github.com/Azure/azure-functions-dotnet-worker/tree/main/samples/FunctionApp). Here is what the documentation says anout .NET 5 support:

```
A .NET 5 function app runs in an isolated worker process. Instead of building a .NET library loaded by our host, you build a .NET console app that references a worker SDK.

This brings immediate benefits: you have full control over the application’s startup and the dependencies it consumes. The new programming model also adds support for custom middleware which has been a frequently requested feature.

While this isolated model for .NET brings the above benefits, it's worth worth noting there are some features you may have utilized in previous versions that aren't yet supported.

While the .NET isolated model supports most Azure Functions triggers and bindings, Durable Functions and rich types support are currently unavailable. Take a blob trigger for example, you are limited to passing blob content using data types that are supported in the out-of-process language worker model, which today are string, byte[] and POCO. You can still use Azure SDK types like CloudBlockBlob, you you'll need to instantiate the SDK in your function process.  
```

## Microservices

Initially, the following will be implemented:
- Several Azure Functions: 
    - `Parser`: triggers on storage blob change, read the CSV file, parses it and qneueues its content in chunks to the `Indexer` 
    - `Indexer`: triggers on storage queue to process ayah documents and indexes them in an Azure search service. Document enrichment is limited to nouns and concepts translation for now:
        - If the Arabic nouns (or concepts) collection is populated but the English one is empty, then auto-translate the Arabic entities to English
        - If the English nouns (or concepts) collection is populated and the Arabic one is empty, then auto-translate the English entities to Arabic
        - If both Arabic and English collections are populated, then do not apply any translation
    - `Explorer`: provides REST Endpoints to clients to interact with the clients
- Exploration client:
    - [Azure search generator](http://azsearchstore.azurewebsites.net/azsearchgenerator/index.html) to produce a static site to expriment with content exploration. This can be customized a little as described [here](https://jj09.net/cognitive-search-azure-search-with-ai/) 
    - wix.com
    - Azure Static Web Apps to deploy the statically generated site
    - Flutter, MAUI or Blazor
    - Etc 

## Azure Resources

There are two workflows:
- `create-resources.yml` to create all `goqur` resources
- `delete-resources.yml` to delete all `goqur` resources

**WARNING:** I could not auto-create the blob storage `files` container. Please see comments in `create.yml`. 

## Nuget

- Needed several packages. Example:

```
dotnet add package Azure.Search.Documents --version 11.3.0-beta.2
dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage
```

- Added the shared lib to the functions app project:

```
cd csharp
dotnet add functionsapp/functionsapp.csproj reference shared/shared.csproj
```

- Omni sharp needed help by selecting the project


## Manager

This is a Console app created to experiment with `delete`, `create` and `upload` documents programmatically. This app is not only used for experimentation purpose and has no bearing on the entire solution:

```
cd csharp/manager
dotnet run delete
dotnet run create
```

## Dockerization

[Microsoft Azure Functions Images](https://github.com/Azure/azure-functions-docker)

To initialize a Docker file, use this command in the functions app project:
```
func init --help
func init --docker-only --worker-runtime dotnetIsolated
docker image build --tag goqur-functions .
```

Unfortunately the .NET 5.0 functions app environment still requires .NET 3.1 to run. So I had to install .NET 3.1 runtime in the Docker image:
```
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update
RUN apt-get -y install dotnet-sdk-3.1
```

Pass all the settings as env variables to the local image:
```
docker container run -it -p 8080:80 -e SEARCH_SVC_ENDPOINT=https://your-svs.search.windows.net -e SEARCH_SVC_API_KEY=your-key -e AzureWebJobsAzureWebJobsStorage="DefaultEndpointsProtocol=https;AccountName=your-name;AccountKey=your-key;EndpointSuffix=core.windows.net"  -e AzureWebJobsStorage="DefaultEndpointsProtocol=https;AccountName=your-name;AccountKey=your-key;EndpointSuffix=core.windows.net" goqur-functions
```

Pass all the settings as env variables to the Docker hub image:
```
docker container run -it -p 8080:80 -e SEARCH_SVC_ENDPOINT=https://your-svs.search.windows.net -e SEARCH_SVC_API_KEY=your-key -e AzureWebJobsAzureWebJobsStorage="DefaultEndpointsProtocol=https;AccountName=your-name;AccountKey=your-key;EndpointSuffix=core.windows.net"  -e AzureWebJobsStorage="DefaultEndpointsProtocol=https;AccountName=your-name;AccountKey=your-key;EndpointSuffix=core.windows.net" khaledhikmat/goqur-functions:latest
```

Deploy to the default namespace. Make sure to update the env variables in `deployment.yml` for your environment:
```
kubectl apply -f k8s/functions/deployment.yml
kubectl apply -f k8s/functions/service.yml
kubectl get all
```

Validate:
```
kubectl logs replicaset.apps/goqur-functions-deployment-replace-with-your-own --tail 100
```

Undeploy:
```
kubectl delete -f k8s/functions/service.yml
kubectl delete -f k8s/functions/deployment.yml
kubectl get all
```

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
- [Github actions worklfow commands](https://docs.github.com/en/actions/reference/workflow-commands-for-github-actions)
- [Github actions for Azure CLI](https://github.com/marketplace/actions/azure-cli-action)



