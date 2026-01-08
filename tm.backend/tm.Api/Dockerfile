FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY tm.backend/tm.Api/tm.Api.csproj tm.backend/tm.Api/
COPY tm.backend/tm.Application/tm.Application.csproj tm.backend/tm.Application/
COPY tm.backend/tm.Infrastructure/tm.Infrastructure.csproj tm.backend/tm.Infrastructure/
COPY tm.backend/tm.Domain/tm.Domain.csproj tm.backend/tm.Domain/

RUN dotnet restore tm.backend/tm.Api/tm.Api.csproj

COPY . .
WORKDIR /src/tm.backend/tm.Api
RUN dotnet build tm.Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish tm.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "tm.Api.dll"]


