FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Server/src/Services/Catalog/Catalog.API/Catalog.API.csproj", "Services/Catalog/Catalog.API/"]
COPY ["Server/src/Services/Catalog/Catalog.Application/Catalog.Application.csproj", "Services/Catalog/Catalog.Application/"]
COPY ["Server/src/Services/Catalog/Catalog.Core/Catalog.Core.csproj", "Services/Catalog/Catalog.Core/"]
COPY ["Server/src/Services/Catalog/Catalog.Persistence/Catalog.Persistence.csproj", "Services/Catalog/Catalog.Persistence/"]
COPY ["Server/src/Shared/Shared.Core/Shared.Core.csproj", "Shared/Shared.Core/"]
COPY ["Server/src/Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "Services/Catalog/Catalog.API/Catalog.API.csproj"
COPY ["Server/src/Services/Catalog", "Services/Catalog"]
COPY ["Server/src/Shared/Shared.Core", "Shared/Shared.Core"]
COPY ["Server/src/Shared/Shared.Messaging", "Shared/Shared.Messaging"]
WORKDIR "/src/Services/Catalog/Catalog.API"
RUN dotnet build "Catalog.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Catalog.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]