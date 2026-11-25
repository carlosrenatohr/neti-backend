# syntax=docker/dockerfile:1

# -- Build stage: restore and publish with the SDK image --
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# -- Copy project files first so restore layers are cached independently of source changes --
COPY MarcasAutos.sln ./
COPY src/MarcasAutos.Api/MarcasAutos.Api.csproj src/MarcasAutos.Api/
COPY src/MarcasAutos.Application/MarcasAutos.Application.csproj src/MarcasAutos.Application/
COPY src/MarcasAutos.Infrastructure/MarcasAutos.Infrastructure.csproj src/MarcasAutos.Infrastructure/
COPY tests/MarcasAutos.UnitTests/MarcasAutos.UnitTests.csproj tests/MarcasAutos.UnitTests/
COPY tests/MarcasAutos.IntegrationTests/MarcasAutos.IntegrationTests.csproj tests/MarcasAutos.IntegrationTests/
RUN dotnet restore MarcasAutos.sln

COPY . .
RUN dotnet publish src/MarcasAutos.Api/MarcasAutos.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# -- Migration stage: applies EF Core migrations once, before the API starts --
FROM build AS migrator
RUN dotnet tool install --global dotnet-ef --version 10.0.15
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "src/MarcasAutos.Infrastructure", "--startup-project", "src/MarcasAutos.Api"]

# -- Runtime stage: minimal ASP.NET image running as the built-in non-root user --
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# -- Npgsql probes GSSAPI on first connection; the trimmed image omits it --
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "MarcasAutos.Api.dll"]
