# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/ESGMonitoring.API/ESGMonitoring.API.csproj src/ESGMonitoring.API/
COPY src/ESGMonitoring.Tests/ESGMonitoring.Tests.csproj src/ESGMonitoring.Tests/

RUN dotnet restore src/ESGMonitoring.API/ESGMonitoring.API.csproj

COPY src/ESGMonitoring.API/ src/ESGMonitoring.API/
COPY src/ESGMonitoring.Tests/ src/ESGMonitoring.Tests/

# Build and test
RUN dotnet build src/ESGMonitoring.API/ESGMonitoring.API.csproj -c Release -o /app/build
RUN dotnet test src/ESGMonitoring.Tests/ESGMonitoring.Tests.csproj -c Release --no-build --logger "trx;LogFileName=test_results.trx"

# Publish stage
FROM build AS publish
RUN dotnet publish src/ESGMonitoring.API/ESGMonitoring.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ESGMonitoring.API.dll"]