FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy projects and restore
COPY ["UrlShortener.Frontend/UrlShortener.Frontend.csproj", "UrlShortener.Frontend/"]
COPY ["UrlShortener.Shared/UrlShortener.Shared.csproj", "UrlShortener.Shared/"]
RUN dotnet restore "UrlShortener.Frontend/UrlShortener.Frontend.csproj"

# Copy all source files and publish
COPY . .
WORKDIR "/src/UrlShortener.Frontend"
RUN dotnet publish "UrlShortener.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY UrlShortener.Frontend/nginx.conf /etc/nginx/nginx.conf

# Expose Nginx port
EXPOSE 5157

# Generate appsettings.json dynamically at startup and start Nginx
ENTRYPOINT ["/bin/sh", "-c", "echo \"{\\\"BackendUrl\\\":\\\"${BACKEND_URL:-http://localhost:5112}\\\"}\" > /usr/share/nginx/html/appsettings.json && nginx -g 'daemon off;'"]
