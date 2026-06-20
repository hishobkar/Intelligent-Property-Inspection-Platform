FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["DocumentService.API/DocumentService.API.csproj", "DocumentService.API/"]
COPY ["DocumentService.Application/DocumentService.Application.csproj", "DocumentService.Application/"]
COPY ["DocumentService.Domain/DocumentService.Domain.csproj", "DocumentService.Domain/"]
COPY ["DocumentService.Infrastructure/DocumentService.Infrastructure.csproj", "DocumentService.Infrastructure/"]
RUN dotnet restore "DocumentService.API/DocumentService.API.csproj"
COPY . .
WORKDIR "/src/DocumentService.API"
RUN dotnet build "DocumentService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocumentService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocumentService.API.dll"]
