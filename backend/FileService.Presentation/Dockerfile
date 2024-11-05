FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8030

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/FileService.Presentation/FileService.Presentation.csproj", "FileService.Presentation/"]
COPY ["backend/SharedKernel/SharedKernel.csproj", "SharedKernel/"]
RUN dotnet restore "FileService.Presentation/FileService.Presentation.csproj"
COPY . .
WORKDIR "/src/FileService.Presentation"
RUN dotnet build "FileService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FileService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.Presentation.dll"]
