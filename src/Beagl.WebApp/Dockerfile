# Dockerfile for Beagl.WebApp
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Beagl.WebApp.csproj", "."]
RUN dotnet restore "Beagl.WebApp.csproj"
COPY . .
RUN dotnet publish "Beagl.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Beagl.WebApp.dll"]
