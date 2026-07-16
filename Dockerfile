# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["StockApi/MyWebService.csproj", "StockApi/"]
RUN dotnet restore "StockApi/MyWebService.csproj"

# Copy everything and publish
COPY . .
WORKDIR /src/StockApi
RUN dotnet publish "MyWebService.csproj" -c Release -o /app --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app .
ENTRYPOINT ["dotnet", "MyWebService.dll"]