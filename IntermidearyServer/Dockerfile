# Base stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
USER app
WORKDIR /app

# Expose ports for the web server
EXPOSE 6000
EXPOSE 8000

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IntermidearyServer.csproj", "./"]
RUN dotnet restore "IntermidearyServer.csproj"
COPY . . 
WORKDIR "/src/"
RUN dotnet build "IntermidearyServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IntermidearyServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IntermidearyServer.dll"]
