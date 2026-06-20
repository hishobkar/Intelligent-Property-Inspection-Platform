FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["InspectionService.API/InspectionService.API.csproj", "InspectionService.API/"]
COPY ["InspectionService.Application/InspectionService.Application.csproj", "InspectionService.Application/"]
COPY ["InspectionService.Domain/InspectionService.Domain.csproj", "InspectionService.Domain/"]
COPY ["InspectionService.Infrastructure/InspectionService.Infrastructure.csproj", "InspectionService.Infrastructure/"]
RUN dotnet restore "InspectionService.API/InspectionService.API.csproj"
COPY . .
WORKDIR "/src/InspectionService.API"
RUN dotnet build "InspectionService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InspectionService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InspectionService.API.dll"]
