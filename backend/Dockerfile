# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for caching restore layer
COPY ["AgenticBoard.sln", "./"]
COPY ["AgenticBoard.Domain/AgenticBoard.Domain.csproj", "AgenticBoard.Domain/"]
COPY ["AgenticBoard.Application/AgenticBoard.Application.csproj", "AgenticBoard.Application/"]
COPY ["AgenticBoard.Infrastructure/AgenticBoard.Infrastructure.csproj", "AgenticBoard.Infrastructure/"]
COPY ["AgenticBoard.Api/AgenticBoard.Api.csproj", "AgenticBoard.Api/"]
COPY ["AgenticBoard.Tests/AgenticBoard.Tests.csproj", "AgenticBoard.Tests/"]

RUN dotnet restore "AgenticBoard.sln"

# Copy source code and build
COPY . .
WORKDIR /src/AgenticBoard.Api
RUN dotnet publish "AgenticBoard.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "AgenticBoard.Api.dll"]
