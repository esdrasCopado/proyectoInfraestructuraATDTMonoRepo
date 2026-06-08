# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SolicitudServidores.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish SolicitudServidores.csproj -c Release -o /app/publish

# ── Stage 2: Test ─────────────────────────────────────────────────────────────
# Parte del stage build (tiene el SDK y el código fuente ya copiado).
# Las pruebas usan SQLite en memoria; no necesitan la BD de Postgres.
FROM build AS test
WORKDIR /src
RUN dotnet restore SolicitudServidores.Tests/SolicitudServidores.Tests.csproj
ENTRYPOINT ["dotnet", "test", "SolicitudServidores.Tests/SolicitudServidores.Tests.csproj", \
            "--no-restore", "-v", "normal", "--logger", "console;verbosity=normal"]

# ── Stage 3: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SolicitudServidores.dll"]
