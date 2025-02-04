# Use the official .NET 9.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Use the official .NET 9.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["DatabaseMigrations/DatabaseMigrations.csproj", "DatabaseMigrations/"]
COPY ["NewPoint.Common/NewPoint.Common.csproj", "NewPoint.Common/"]

# Restore dependencies
RUN dotnet restore "DatabaseMigrations/DatabaseMigrations.csproj"

# Copy the rest of the project files
COPY . .

# Remove all global.json files to avoid version conflicts (optional)
RUN find /src -name 'global.json' -delete

# Build the application
WORKDIR "/src/DatabaseMigrations"
RUN dotnet build "DatabaseMigrations.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "DatabaseMigrations.csproj" -c Release -o /app/publish

# Use the base image for the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Define the entry point for the container
ENTRYPOINT ["dotnet", "DatabaseMigrations.dll"]
