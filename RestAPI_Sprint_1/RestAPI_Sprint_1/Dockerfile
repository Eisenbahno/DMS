FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080 

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RestAPI_Sprint_1/RestAPI_Sprint_1.csproj", "RestAPI_Sprint_1/"]
RUN dotnet restore "RestAPI_Sprint_1/RestAPI_Sprint_1.csproj"
COPY . .
WORKDIR "/src/RestAPI_Sprint_1"
RUN dotnet build "RestAPI_Sprint_1.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RestAPI_Sprint_1.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestAPI_Sprint_1.dll"]
