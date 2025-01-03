FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyOwnDb.Server/MyOwnDb.Server.csproj", "MyOwnDb.Server/"]
RUN dotnet restore "MyOwnDb.Server/MyOwnDb.Server.csproj"
COPY . .
WORKDIR "/src/MyOwnDb.Server"
RUN dotnet build "MyOwnDb.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MyOwnDb.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyOwnDb.Server.dll"]
