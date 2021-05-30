This platform is called `goqur`. It is pronounced `go cure`. It is meant to provide a set of API Endpoints to explore the different nouns and concepts in ayahs of the Quran.  

**Please note** because this is a multi-project source, VS code require this `azureFunctions.projectSubpath` setting to properly point to the functions app!!!

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

The main search document is therefore `Ayah`. Please see [auran-ayahs.csv](auran-ayahs.csv) for a sample source file. 

## Design Notes

*Custom handlers are a thing in Functions. The source has an example in Go. But obviously it makes more sense to use C# for this purpose since the client SDKs are much richer.*

- Experiment with custom handlers using Go in Azure Functions to expose  `goqur` REST endpoints. This will be used to serve the actual client requests. Clients can be Postman or any other platform that would want to provide a UI to this API. Perhaps use the auto-generator mentioned in [references](#references) below. This custom handler will use the search service REST APIs directly to render results. 
- Experiment with C# handler as well although I prefer Go for obvious reasons
- Logic app to receive email from known email addresses with the CSV as attachment. The logic app step stores the attachment in storage which then triggers an Azure Function. The Function parses the CSV file and updates the search index using .NET SDK.
- Investigate these existing Quran APIs:
    - https://quran.api-docs.io/v3/verses
    - https://alquran.cloud/api
    - http://docs.globalquran.com/Tutorials

Azure Search supports two modes: [push and pull](https://docs.microsoft.com/en-us/azure/search/search-what-is-data-import). The push model, while flexible, does not allow for external processing (or enrichment). Only pull model indexers supports enrichment. 
- An email with attachment -> Logic App which will store the attachment CSV in BLOB
- An Indexer, runs every hour, pulls the CSV file, parses it and updates the index running any added enrichment skills in the form of an Azure Function called `enricher`. Enricher can use the Ayah's entities to determine whether it needs to auto-translate or not:
    - If the Arabic entities collection is populated but the English one is empty, then auto-translate the Arabic entities to English
    - If the English entities collection is populated and the Arabic one is empty, then auto-translate the English entities to Arabic
    - If both Arabic and English collections are populated, then do not apply any translation
- Clients access the `docs` Azure Function to query/explore the index. 
- Perhaps we can use [Azure search generator](http://azsearchstore.azurewebsites.net/azsearchgenerator/index.html) to produce a static site
- Perhaps use Azure Static Web Apps to deploy the statically generated site. 
Notes:
- When we import data via the portal, it analyzes the storage blob and creates it own index. The index that we create via the `manager` cannot be used.
- Make sure the `Ayah.cs` fileds match the fields created by the inexer
- Add a new `Skill` using the `Skillsets` requires that we did the import via the portal
- There is something wrong with the CSV parsing! It is not understanding the Collection...very unfortunate. I posted an issue in [SFO](https://stackoverflow.com/questions/67688233/how-to-import-from-csv-into-a-collection-of-strings)...no answer. 
- Because this is a multi-project source, `azureFunctions.projectSubpath` setting is needed to point to the functions app!!!
    - Once you add it, go to the Azure icon in VS Code aand you should see a new Local Project created. 
    - To add new functions, click it and click add a new function
- It tutned out that the blob storage trigger is quite easy. Once the blob changes, the function triggers, re-loads the CSV data, parses it and uploads the docs to the search service. 
- The Logic App can be used to read an attachment from Email and stores in BLOB...but there is really no need.
- Since I am uploading the docs programatically, I can easily enrich the data by auto-translating for example
- The CSV file that is saved from Excel does not contain double-quotes on every field. Use the PowerShell script `prepare-ayahs-4-upload.ps1` to force a double quotation before we upload. But also remove the first line manually.
- Use the `--publish-local-settings` switch when you publish to make sure the local settings are added to the function app in Azure
- Remember to disable the function app function to debug locally so it will not listen in the same blob storage
- .NET5 is not supported [https://techcommunity.microsoft.com/t5/apps-on-azure/net-on-azure-functions-roadmap/ba-p/2197916](https://techcommunity.microsoft.com/t5/apps-on-azure/net-on-azure-functions-roadmap/ba-p/2197916) except in isolated loads. [https://stackoverflow.com/questions/67665320/using-vs-code-how-can-i-add-a-durable-function-to-a-function-app-with-target-fr](https://stackoverflow.com/questions/67665320/using-vs-code-how-can-i-add-a-durable-function-to-a-function-app-with-target-fr) and check out the docs on isolated process [https://docs.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#differences-with-net-class-library-functions](https://docs.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#differences-with-net-class-library-functions)
```
A .NET 5 function app runs in an isolated worker process. Instead of building a .NET library loaded by our host, you build a .NET console app that references a worker SDK. This brings immediate benefits: you have full control over the application’s startup and the dependencies it consumes. The new programming model also adds support for custom middleware which has been a frequently requested feature.
While this isolated model for .NET brings the above benefits, it’s worth noting there are some features you may have utilized in previous versions that aren’t yet supported. While the .NET isolated model supports most Azure Functions triggers and bindings, Durable Functions and rich types support are currently unavailable. Take a blob trigger for example, you are limited to passing blob content using data types that are supported in the out-of-process language worker model, which today are string, byte[], and POCO. You can still use Azure SDK types like CloudBlockBlob, but you’ll need to instantiate the SDK in your function process.
```
- This is a sample out-of-process function app that shows how to inject [https://github.com/Azure/azure-functions-dotnet-worker/tree/main/samples/FunctionApp](https://github.com/Azure/azure-functions-dotnet-worker/tree/main/samples/FunctionApp)

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
- `create.yml` to create all `goqur` resources
- `delete.yml` to delete all `goqur` resources

## Nuget

Need several packages. Example:
```
dotnet add package Azure.Search.Documents --version 11.3.0-beta.2
dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage
```

Add the shared lib to the functions app project:
```
cd csharp
dotnet add functionsapp/functionsapp.csproj reference shared/shared.csproj
```

Help Omni sharp by selecting the project

## Manager

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
Func init --help
func init --docker-only --worker-runtime dotnetIsolated
docker image build --tag goqur-functions .
```

Unfortunately the .NET 5.0 environment still requires .NET 3.1 to run. So I had to install .NET 3.1 runtime in the Docker image:
```
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update
RUN apt-get -y install dotnet-sdk-3.1
```

// Pass all the settings as env variables to the local image:
```
docker container run -it -p 8080:80 -e SEARCH_SVC_ENDPOINT=https://your-svs.search.windows.net -e SEARCH_SVC_API_KEY=your-key -e AzureWebJobsAzureWebJobsStorage="DefaultEndpointsProtocol=https;AccountName=your-name;AccountKey=your-key;EndpointSuffix=core.windows.net"  -e AzureWebJobsStorage="DefaultEndpointsProtocol=https;AccountName=your-name;AccountKey=your-key;EndpointSuffix=core.windows.net" goqur-functions
```

Try with Postman:
http://localhost:8080/api/Explorer

// Pass all the settings as env variables to the Docker hub image:
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

