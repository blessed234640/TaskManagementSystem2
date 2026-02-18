## Многостадийный Dockerfile для TaskManagement.Api (.NET 8)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaskManagement.Api/TaskManagement.Api.csproj TaskManagement.Api/
RUN dotnet restore TaskManagement.Api/TaskManagement.Api.csproj

COPY TaskManagement.Api/. TaskManagement.Api/
WORKDIR /src/TaskManagement.Api
RUN dotnet publish TaskManagement.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

# Создаем папку для данных SQLite
RUN mkdir -p /app/data

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]

