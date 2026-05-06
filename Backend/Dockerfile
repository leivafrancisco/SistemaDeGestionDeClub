FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["API/SistemaDeGestionDeClub.API.csproj", "API/"]
COPY ["Application/SistemaDeGestionDeClub.Application.csproj", "Application/"]
COPY ["Infrastructure/SistemaDeGestionDeClub.Infrastructure.csproj", "Infrastructure/"]
COPY ["Domain/SistemaDeGestionDeClub.Domain.csproj", "Domain/"]
RUN dotnet restore "API/SistemaDeGestionDeClub.API.csproj"
COPY . .
RUN dotnet publish "API/SistemaDeGestionDeClub.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SistemaDeGestionDeClub.API.dll"]
