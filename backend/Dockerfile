# ------------------------------------------------------------------
# Imagen del API para Azure Container Apps (ADR 0007).
# Solo la usa el pipeline de GitHub Actions: para trabajar en local NO se necesita Docker.
# Contexto de build: carpeta backend/  →  docker build -t gestion-notas-api backend
# ------------------------------------------------------------------

# Etapa 1: compilar y publicar con el SDK (imagen grande, se descarta al final).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj: si no cambian, Docker reutiliza la capa del restore (builds más rápidos).
COPY Directory.Build.props ./
COPY src/GestionNotas.Domain/GestionNotas.Domain.csproj src/GestionNotas.Domain/
COPY src/GestionNotas.Application/GestionNotas.Application.csproj src/GestionNotas.Application/
COPY src/GestionNotas.Infrastructure/GestionNotas.Infrastructure.csproj src/GestionNotas.Infrastructure/
COPY src/GestionNotas.Api/GestionNotas.Api.csproj src/GestionNotas.Api/
RUN dotnet restore src/GestionNotas.Api/GestionNotas.Api.csproj

COPY src/ src/
RUN dotnet publish src/GestionNotas.Api/GestionNotas.Api.csproj -c Release -o /app/publish --no-restore

# Etapa 2: solo el runtime de ASP.NET (imagen pequeña, sin SDK ni código fuente).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Las imágenes oficiales de .NET escuchan en el 8080 y traen el usuario sin privilegios "app".
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER app

ENTRYPOINT ["dotnet", "GestionNotas.Api.dll"]
