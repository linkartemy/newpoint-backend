FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DatabaseMigrations/DatabaseMigrations.csproj", "DatabaseMigrations/"]
RUN dotnet restore "DatabaseMigrations/DatabaseMigrations.csproj"
COPY . .
WORKDIR "/src/DatabaseMigrations"
RUN dotnet build "DatabaseMigrations.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DatabaseMigrations.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatabaseMigrations.dll"]
