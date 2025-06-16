FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ControleFluxoCaixa.BFF/ControleFluxoCaixa.BFF.csproj", "ControleFluxoCaixa.BFF/"]
RUN dotnet restore "ControleFluxoCaixa.BFF/ControleFluxoCaixa.BFF.csproj"
COPY . .
WORKDIR "/src/ControleFluxoCaixa.BFF"
RUN dotnet build "ControleFluxoCaixa.BFF.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ControleFluxoCaixa.BFF.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ControleFluxoCaixa.BFF.dll"]
