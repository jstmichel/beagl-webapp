# Dockerfile for Beagl.WebApp
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /src
COPY . .
RUN dotnet restore src/Beagl.WebApp/Beagl.WebApp.csproj
RUN dotnet publish src/Beagl.WebApp/Beagl.WebApp.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Beagl.WebApp.dll"]
