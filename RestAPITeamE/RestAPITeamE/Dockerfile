# Base image using .NET 8.0 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8081 
EXPOSE 8080

# Build image using .NET 8.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RestAPITeamE/RestAPITeamE.csproj", "RestAPITeamE/"]
RUN dotnet restore "RestAPITeamE/RestAPITeamE.csproj"

# Copy the rest of the source code and build the project
COPY . .
WORKDIR "/src/RestAPITeamE"
RUN dotnet build "RestAPITeamE.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the build artifacts
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RestAPITeamE.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image, running the published app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestAPITeamE.dll"]
