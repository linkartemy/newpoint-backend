# Use the official ASP.NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5137

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
COPY ["NewPoint.ErrorLocalizationAPI/NewPoint.ErrorLocalizationAPI.csproj", "NewPoint.ErrorLocalizationAPI/"]
COPY ["NewPoint.Common/NewPoint.Common.csproj", "NewPoint.Common/"]

# Restore the dependencies
RUN dotnet restore "NewPoint.ErrorLocalizationAPI/NewPoint.ErrorLocalizationAPI.csproj"

# Copy all project files
COPY . .

# Remove global.json files to avoid conflict
RUN find /src -name 'global.json' -delete

# Build the application
WORKDIR "/src/NewPoint.ErrorLocalizationAPI"
RUN dotnet build "NewPoint.ErrorLocalizationAPI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "NewPoint.ErrorLocalizationAPI.csproj" -c Release -o /app/publish

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy the appsettings.json file
COPY configs/error-localization-service/appsettings.error-localization-service.json /app/appsettings.json
COPY NewPoint.ErrorLocalizationAPI/localizations/*.json /app/localizations/

# Entry point for the application
ENTRYPOINT ["dotnet", "NewPoint.ErrorLocalizationAPI.dll"]
