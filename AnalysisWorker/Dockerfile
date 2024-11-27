FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AnalysisWorker/AnalysisWorker.csproj", "./"]
RUN dotnet restore
COPY AnalysisWorker/. .
WORKDIR "/src"
RUN dotnet build "AnalysisWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AnalysisWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnalysisWorker.dll"]