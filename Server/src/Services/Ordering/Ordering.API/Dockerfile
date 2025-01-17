FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 6080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Server/src/Services/Ordering/Ordering.API/Ordering.API.csproj", "Services/Ordering/Ordering.API/"]
COPY ["Server/src/Services/Ordering/Ordering.Application/Ordering.Application.csproj", "Services/Ordering/Ordering.Application/"]
COPY ["Server/src/Services/Ordering/Ordering.Core/Ordering.Core.csproj", "Services/Ordering/Ordering.Core/"]
COPY ["Server/src/Services/Ordering/Ordering.Persistence/Ordering.Persistence.csproj", "Services/Ordering/Ordering.Persistence/"]
COPY ["Server/src/Shared/Shared.Core/Shared.Core.csproj", "Shared/Shared.Core/"]
COPY ["Server/src/Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "Services/Ordering/Ordering.API/Ordering.API.csproj"
COPY ["Server/src/Services/Ordering", "Services/Ordering"]
COPY ["Server/src/Shared/Shared.Core", "Shared/Shared.Core"]
COPY ["Server/src/Shared/Shared.Messaging", "Shared/Shared.Messaging"]
WORKDIR "/src/Services/Ordering/Ordering.API"
RUN dotnet build "Ordering.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Ordering.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ordering.API.dll"]