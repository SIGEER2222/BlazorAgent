# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution content
COPY . .

# Restore and publish Inventory.Web
RUN dotnet restore src/Inventory.Web/Inventory.Web.csproj \
 && dotnet publish src/Inventory.Web/Inventory.Web.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# App_Data volume for SQLite
VOLUME ["/app/App_Data"]

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Inventory.Web.dll"]