# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/StoreOps.Api/StoreOps.Api.csproj src/StoreOps.Api/
RUN dotnet restore src/StoreOps.Api/StoreOps.Api.csproj

COPY src/StoreOps.Api/ src/StoreOps.Api/
RUN dotnet publish src/StoreOps.Api/StoreOps.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StoreOps.Api.dll"]
