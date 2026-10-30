# Imagem da API para o deploy (Render). Contexto de build: a pasta backend/.
#   docker build -t avatar-codex-api backend

# 1. Build: SDK completo, restore só com os .csproj (camada reaproveitada enquanto as dependências não mudam).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/AvatarCodex.Domain/AvatarCodex.Domain.csproj src/AvatarCodex.Domain/
COPY src/AvatarCodex.Application/AvatarCodex.Application.csproj src/AvatarCodex.Application/
COPY src/AvatarCodex.Infrastructure/AvatarCodex.Infrastructure.csproj src/AvatarCodex.Infrastructure/
COPY src/AvatarCodex.Api/AvatarCodex.Api.csproj src/AvatarCodex.Api/
RUN dotnet restore src/AvatarCodex.Api/AvatarCodex.Api.csproj
COPY src/ src/
RUN dotnet publish src/AvatarCodex.Api/AvatarCodex.Api.csproj -c Release -o /app --no-restore

# 2. Execução: só o runtime do ASP.NET, com usuário sem privilégios.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Production
USER $APP_UID
# O Render informa a porta em PORT; fora dele, 8080 (padrão das imagens .NET).
ENTRYPOINT ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet AvatarCodex.Api.dll"]
