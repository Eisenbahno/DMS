# Dockerfile for RestServer
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./RestServerSprint_2.csproj", "RestServerSprint_2/"]
RUN dotnet restore "RestServerSprint_2/RestServerSprint_2.csproj"
COPY . .
WORKDIR "/src/RestServerSprint_2"
#RUN dotnet build "RestServerSprint_2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RestServerSprint_2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestServerSprint_2.dll"]