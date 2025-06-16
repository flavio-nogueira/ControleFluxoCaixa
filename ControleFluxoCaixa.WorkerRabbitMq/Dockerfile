# Etapa base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ControleFluxoCaixa.WorkerRabbitMq/ControleFluxoCaixa.WorkerRabbitMq.csproj", "ControleFluxoCaixa.WorkerRabbitMq/"]
COPY ["ControleFluxoCaixa.MongoDB/ControleFluxoCaixa.MongoDB.csproj", "ControleFluxoCaixa.MongoDB/"]
COPY ["ControleFluxoCaixa.Messaging/ControleFluxoCaixa.Messaging.csproj", "ControleFluxoCaixa.Messaging/"]
RUN dotnet restore "ControleFluxoCaixa.WorkerRabbitMq/ControleFluxoCaixa.WorkerRabbitMq.csproj"

COPY . .
WORKDIR /src/ControleFluxoCaixa.WorkerRabbitMq
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ControleFluxoCaixa.WorkerRabbitMq.dll"]
