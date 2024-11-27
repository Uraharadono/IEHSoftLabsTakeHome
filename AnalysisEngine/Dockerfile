FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AnalysisEngine/AnalysisEngine.csproj", "AnalysisEngine/"]
COPY ["FoodTester.Infrastructure/FoodTester.Infrastructure.csproj", "FoodTester.Infrastructure/"]
RUN dotnet restore "AnalysisEngine/AnalysisEngine.csproj"
COPY . .
WORKDIR "/src/AnalysisEngine"
RUN dotnet build "AnalysisEngine.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AnalysisEngine.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnalysisEngine.dll"]
