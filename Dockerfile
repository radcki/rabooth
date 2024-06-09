#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["raBooth.Web.Host/raBooth.Web.Host.csproj", "raBooth.Web.Host/"]
RUN dotnet restore "./raBooth.Web.Host/raBooth.Web.Host.csproj"

COPY ./raBooth.Web.Host/. ./raBooth.Web.Host/
COPY ./raBooth.Web.Core/. ./raBooth.Web.Core/
WORKDIR "/src/raBooth.Web.Host"
RUN dotnet build "./raBooth.Web.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./raBooth.Web.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "raBudget.Api.dll"]
