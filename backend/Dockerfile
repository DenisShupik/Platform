# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файлы проекта и восстанавливаем зависимости
COPY Production.slnx ./
COPY SharedKernel/SharedKernel.csproj SharedKernel/
COPY SharedKernel.Domain/SharedKernel.Domain.csproj SharedKernel.Domain/
COPY ApiGateway/ApiGateway.csproj ApiGateway/
COPY CoreService/CoreService.csproj CoreService/
COPY CoreService.Domain/CoreService.Domain.csproj CoreService.Domain/
COPY FileService/FileService.csproj FileService/
COPY UserService/UserService.csproj UserService/
RUN dotnet restore

# Копируем остальные файлы и публикуем проекты
COPY . ./
RUN dotnet publish ApiGateway/ApiGateway.csproj -c Release -o /app/ApiGateway
RUN dotnet publish CoreService/CoreService.csproj -c Release -o /app/CoreService
RUN dotnet publish FileService/FileService.csproj -c Release -o /app/FileService
RUN dotnet publish UserService/UserService.csproj -c Release -o /app/UserService

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Копируем опубликованные файлы
COPY --from=build /app/ApiGateway ./ApiGateway
COPY --from=build /app/CoreService ./CoreService
COPY --from=build /app/FileService ./FileService
COPY --from=build /app/UserService ./UserService

# Устанавливаем рабочую директорию по умолчанию
WORKDIR /app

# Точка входа, позволяющая запускать нужный сервис через аргумент
ENTRYPOINT ["dotnet"]