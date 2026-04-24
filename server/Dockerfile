# syntax=docker/dockerfile:1

# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all project/solution files first for a cache-friendly restore.
# Copying the full server tree so that project references and a .sln file
# (if present) are all available before restore runs.
COPY server/ server/
RUN dotnet restore server/src/Todoify.Api/Todoify.Api.csproj

# Publish (restore already done above; omit --no-restore so NuGet can recover
# from any edge-case cache miss rather than hard-failing)
RUN dotnet publish server/src/Todoify.Api/Todoify.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8081

ENTRYPOINT ["dotnet", "Todoify.Api.dll"]
