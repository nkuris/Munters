# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Add Node.js into the build image because the solution contains a client project
# that invokes npm/vite during the build. Install Node.js before building.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS with-node
RUN apt-get update \
	&& apt-get install -y curl ca-certificates gnupg \
	&& curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
	&& apt-get install -y nodejs \
	&& apt-get clean && rm -rf /var/lib/apt/lists/*

FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Munters.Server/Munters.Server.csproj", "Munters.Server/"]
COPY ["munters.client/munters.client.esproj", "munters.client/"]
RUN dotnet restore "./Munters.Server/Munters.Server.csproj"
COPY . .
WORKDIR "/src/Munters.Server"
RUN dotnet build "./Munters.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Munters.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Munters.Server.dll"]