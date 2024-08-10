# Use the official ASP.NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5139

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies
COPY ["NewPoint.PostAPI/NewPoint.PostAPI.csproj", "NewPoint.PostAPI/"]
COPY ["NewPoint.Common/NewPoint.Common.csproj", "NewPoint.Common/"]

# Restore the dependencies for both projects
RUN dotnet restore "NewPoint.PostAPI/NewPoint.PostAPI.csproj"

# Copy all project files
COPY . .

# Remove global.json files to avoid conflict
RUN find /src -name 'global.json' -delete

# Build the application
WORKDIR "/src/NewPoint.PostAPI"
RUN dotnet build "NewPoint.PostAPI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "NewPoint.PostAPI.csproj" -c Release -o /app/publish

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Entry point for the application
ENTRYPOINT ["dotnet", "NewPoint.PostAPI.dll"]
