FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

ENV DOTNET_ENVIRONMENT=Production

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

ENV ASPNETCORE_URLS=http://+:5191
EXPOSE 5191

ENTRYPOINT ["dotnet", "RockServers.dll"]
