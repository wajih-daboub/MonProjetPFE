# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src/ ./src/
RUN dotnet restore ./src/MonProjetPFE.csproj
RUN dotnet publish ./src/MonProjetPFE.csproj -c Release -o /app

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# dossier data pour SQLite
RUN mkdir -p /app/data
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","MonProjetPFE.dll"]
