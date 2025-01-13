FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Server/src/Services/Users/Users.API/Users.API.csproj", "Services/Users/Users.API/"]
COPY ["Server/src/Services/Users/Users.Application/Users.Application.csproj", "Services/Users/Users.Application/"]
COPY ["Server/src/Services/Users/Users.Core/Users.Core.csproj", "Users/Users.Core/"]
COPY ["Server/src/Services/Users/Users.Infrastructure/Users.Infrastructure.csproj", "Services/Users/Users.Infrastructure/"]
COPY ["Server/src/Services/Users/Users.Persistence/Users.Persistence.csproj", "Services/Users/Users.Persistence/"]
COPY ["Server/src/Shared/Shared.Core/Shared.Core.csproj", "Shared/Shared.Core"]
RUN dotnet restore "Users/Users.API/Users.API.csproj"
COPY . .
WORKDIR "/src/Services/Users/Users.API"
RUN dotnet build "Users.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Users.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Users.API.dll"]
