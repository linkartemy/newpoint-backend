# Use the official ASP.NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5136

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the csproj and restore dependencies
COPY ["NewPoint/NewPoint.csproj", "NewPoint/"]
RUN dotnet restore "NewPoint/NewPoint.csproj"

# Copy the rest of the application
COPY . .

# Build the application
WORKDIR "/src/NewPoint"
RUN dotnet build "NewPoint.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NewPoint.csproj" -c Release -o /app/publish

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NewPoint.dll"]
