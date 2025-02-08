# Use the official ASP.NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5141

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
COPY ["NewPoint.ObjectAPI/NewPoint.ObjectAPI.csproj", "NewPoint.ObjectAPI/"]
COPY ["NewPoint.Common/NewPoint.Common.csproj", "NewPoint.Common/"]

# Restore the dependencies
RUN dotnet restore "NewPoint.ObjectAPI/NewPoint.ObjectAPI.csproj"

# Copy all project files
COPY . .

# Remove global.json files to avoid conflict
RUN find /src -name 'global.json' -delete

# Build the application
WORKDIR "/src/NewPoint.ObjectAPI"
RUN dotnet build "NewPoint.ObjectAPI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "NewPoint.ObjectAPI.csproj" -c Release -o /app/publish

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy the appsettings.json file
COPY configs/object-service/appsettings.object-service.json /app/appsettings.json

# Entry point for the application
ENTRYPOINT ["dotnet", "NewPoint.ObjectAPI.dll"]
