# escape=`

# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ---------- Base runtime image ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Install curl if need automatic healthcheck via docker-compose.yml
# RUN apt-get update \
#     && apt-get install -y --no-install-recommends curl \
#     && rm -rf /var/lib/apt/lists/*

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TAG
WORKDIR /src

COPY ./.editorconfig ./
COPY ./Directory.Build.props ./
COPY ["src/Template.Library/Template.Library.csproj", "Template.Library/"]
RUN dotnet restore "./Template.Library/Template.Library.csproj"  --runtime linux-x64

COPY ./src .
RUN rm **/appsettings.Development.json
WORKDIR "/src/Template.Library"

SHELL ["pwsh", "-Command"]
RUN dotnet publish "./Template.Library.csproj" `
  -c "$BUILD_CONFIGURATION" `
  -o /app/publish `
  -r linux-x64 `
  --no-restore `
  # TAG is expected to have format: "1.2.0-16.cf82de85"
  $(if ($env:TAG -match '(?<ver>(\d+\.?){3})(-.*)?') { '-p:Version=' + $matches['ver'] }) `
  -p:InformationalVersion=$env:TAG `
  -p:UseAppHost=false

# ---------- Final image ----------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Template.Library.dll"]
