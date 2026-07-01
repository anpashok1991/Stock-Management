FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/StockManagement.Domain/StockManagement.Domain.csproj", "StockManagement.Domain/"]
COPY ["src/StockManagement.Application/StockManagement.Application.csproj", "StockManagement.Application/"]
COPY ["src/StockManagement.Infrastructure/StockManagement.Infrastructure.csproj", "StockManagement.Infrastructure/"]
COPY ["src/StockManagement.Web/StockManagement.Web.csproj", "StockManagement.Web/"]
RUN dotnet restore "StockManagement.Web/StockManagement.Web.csproj"
COPY src/ .
WORKDIR "/src/StockManagement.Web"
RUN dotnet build "StockManagement.Web.csproj" -c Release -o /app/build
RUN dotnet publish "StockManagement.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StockManagement.Web.dll"]
