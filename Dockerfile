FROM mcr.microsoft.com/dotnet/sdk:5.0 AS installer-env

# In their infinite wisdom, it seems that both dotnet 3.1 and 5.0 are required to dockerize .NET 5 Azure functions
# Please refer to a similar issue I was having: https://www.gitmemory.com/issue/Azure/azure-functions-dotnet-worker/297/803397702
# and https://github.com/Azure/azure-functions-host/issues/6674
# The post describes adding the dotnet 3.1 to devops pipeline but I wanted build the image locally and test it
# so I installed dotent-3.1 SDK using these commands:  
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update
RUN apt-get -y install dotnet-sdk-3.1

COPY ./csharp/functionsapp /src/dotnet-function-app
COPY ./csharp/shared /src/shared
RUN cd /src/dotnet-function-app && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet-isolated:3.0-dotnet-isolated5.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:3.0-dotnet-isolated5.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]