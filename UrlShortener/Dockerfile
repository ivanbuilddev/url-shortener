FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5112
ENV ASPNETCORE_URLS=http://+:5112

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["UrlShortener/UrlShortener.csproj", "UrlShortener/"]
COPY ["UrlShortener.Shared/UrlShortener.Shared.csproj", "UrlShortener.Shared/"]
RUN dotnet restore "UrlShortener/UrlShortener.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/UrlShortener"
RUN dotnet build "UrlShortener.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UrlShortener.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UrlShortener.dll"]
