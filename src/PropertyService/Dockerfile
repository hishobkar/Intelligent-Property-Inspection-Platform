FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PropertyService.API/PropertyService.API.csproj", "PropertyService.API/"]
COPY ["PropertyService.Application/PropertyService.Application.csproj", "PropertyService.Application/"]
COPY ["PropertyService.Domain/PropertyService.Domain.csproj", "PropertyService.Domain/"]
COPY ["PropertyService.Infrastructure/PropertyService.Infrastructure.csproj", "PropertyService.Infrastructure/"]
RUN dotnet restore "PropertyService.API/PropertyService.API.csproj"
COPY . .
WORKDIR "/src/PropertyService.API"
RUN dotnet build "PropertyService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PropertyService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PropertyService.API.dll"]
