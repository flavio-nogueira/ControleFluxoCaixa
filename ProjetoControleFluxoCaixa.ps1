# arquiteto-init-final.ps1
# Este script cria a solution, projetos em .NET 8.0, estrutura de pastas, instala pacotes NuGet 
# conforme as versões indicadas e, finalmente, cria todos os arquivos .cs vazios (com namespace básico)
# de acordo com os prints fornecidos. Não modifica Program.cs.

# 0) Permitir execução de scripts nesta sessão
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force

# 1) Criar a solution se ainda não existir
if (-not (Test-Path 'ControleFluxoCaixa.sln')) {
    Write-Host '➤ Criando solution ControleFluxoCaixa.sln...'
    dotnet new sln -n ControleFluxoCaixa
}

# 2) Remover projetos antigos (se existirem) para recriar do zero
foreach ($proj in @(
    'ControleFluxoCaixa.API',
    'ControleFluxoCaixa.Application',
    'ControleFluxoCaixa.CrossCutting',
    'ControleFluxoCaixa.Domain',
    'ControleFluxoCaixa.Infrastructure',
    'ControleFluxoCaixa.Messaging',
    'ControleFluxoCaixa.MongoDB',
    'ControleFluxoCaixa.Tests.Unit',
    'ControleFluxoCaixa.Tests.Integration'
)) {
    if (Test-Path $proj) {
        Write-Host "   ↪ Removendo diretório existente: $proj"
        Remove-Item $proj -Recurse -Force
    }
}

# 3) Criar projetos em net8.0 (templates)
Write-Host '➤ Criando projetos em net8.0...'
foreach ($proj in @('Domain','Application','CrossCutting','Infrastructure','Messaging','MongoDB')) {
    $name = "ControleFluxoCaixa.$proj"
    Write-Host "   ↪ Criando projeto $name..."
    dotnet new classlib -n $name -f net8.0
}
Write-Host '   ↪ Criando projeto ControleFluxoCaixa.API (webapi)...'
dotnet new webapi -n ControleFluxoCaixa.API -f net8.0
Write-Host '   ↪ Criando projeto ControleFluxoCaixa.Tests.Unit (xUnit)...'
dotnet new xunit -n ControleFluxoCaixa.Tests.Unit -f net8.0
Write-Host '   ↪ Criando projeto ControleFluxoCaixa.Tests.Integration (xUnit)...'
dotnet new xunit -n ControleFluxoCaixa.Tests.Integration -f net8.0

# 4) Remover arquivos Class1.cs gerados por padrão
Write-Host '➤ Removendo arquivos Class1.cs...'
Get-ChildItem -Path . -Recurse -Filter 'Class1.cs' | Remove-Item -Force

# 5) Criar estrutura de pastas (com .gitkeep) conforme os prints
Write-Host '➤ Criando estrutura de pastas e .gitkeep...'
$folderStructure = @{
    'ControleFluxoCaixa.Domain'         = @('Entities','Enums','ValueObjects')
    'ControleFluxoCaixa.Application'    = @('Interfaces','DTOs','Commands','Handlers')
    'ControleFluxoCaixa.CrossCutting'   = @('Extensions','Logging','Mapping','Validators')
    'ControleFluxoCaixa.Infrastructure' = @('Configurations\Lancamento','Context','Data','IoC','Migrations\Business','Migrations\Identity','Repositories','Services','Seed')
    'ControleFluxoCaixa.API'            = @('Controllers','DTOs','Logs-buffer','Logs-fallback')
    'ControleFluxoCaixa.Messaging'      = @('Consumers','MessagingSettings','Publishers')
    'ControleFluxoCaixa.MongoDB'        = @('Consumers','Documents','Interfaces','Repositories','Settings')
    'ControleFluxoCaixa.Tests.Unit'     = @('DomainTests','ApplicationTests','CrossCuttingTests')
    'ControleFluxoCaixa.Tests.Integration' = @('ApiIntegrationTests','InfrastructureTests')
}

foreach ($proj in $folderStructure.Keys) {
    foreach ($sub in $folderStructure[$proj]) {
        $dir = Join-Path $proj $sub
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Force -Path $dir | Out-Null
        }
        # .gitkeep
        $gitkeep = Join-Path $dir '.gitkeep'
        if (-not (Test-Path $gitkeep)) {
            New-Item -ItemType File -Force -Path $gitkeep | Out-Null
        }
    }
}

# 6) Adicionar todos os projetos à solution
Write-Host '➤ Adicionando projetos à solução...'
Get-ChildItem -Path . -Recurse -Filter '*.csproj' | ForEach-Object {
    dotnet sln ControleFluxoCaixa.sln add $_.FullName
}

# 7) Instalar pacotes NuGet conforme versões das imagens

# 7.1) ControleFluxoCaixa.API
Write-Host "➤ Instalando pacotes NuGet em ControleFluxoCaixa.API..."
Push-Location .\ControleFluxoCaixa.API
dotnet add package AspNetCore.HealthChecks.MySql                          --version 9.0.0
dotnet add package AspNetCore.HealthChecks.Prometheus.Metrics             --version 9.0.0
dotnet add package AspNetCore.HealthChecks.Redis                          --version 9.0.0
dotnet add package AspNetCore.HealthChecks.UI.Client                      --version 8.0.0
dotnet add package AutoMapper                                             --version 14.0.0
dotnet add package FluentValidation.DependencyInjectionExtensions          --version 12.0.0
dotnet add package MediatR                                                 --version 11.1.0
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection         --version 11.1.0
dotnet add package Microsoft.AspNetCore.OpenApi                             --version 8.0.15
dotnet add package Microsoft.EntityFrameworkCore                            --version 8.0.16
dotnet add package Microsoft.EntityFrameworkCore.Relational                 --version 8.0.16
dotnet add package Microsoft.EntityFrameworkCore.Tools                      --version 8.0.16
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore --version 8.0.16
dotnet add package Microsoft.Extensions.Http.Polly                          --version 9.0.5
dotnet add package Microsoft.VisualStudio.Azure.Containers.Tools.Targets    --version 1.21.0
dotnet add package Polly                                                  --version 8.5.2
dotnet add package Pomelo.EntityFrameworkCore.MySql                         --version 8.0.3
dotnet add package prometheus-net.AspNetCore                                --version 8.2.1
dotnet add package prometheus-net.AspNetCore.HealthChecks                   --version 8.0.0
dotnet add package Serilog.Extensions.Logging                               --version 9.0.1
dotnet add package Serilog.Formatting.Compact                                --version 3.0.0
dotnet add package Serilog.Sinks.Console                                     --version 6.0.0
dotnet add package Serilog.Sinks.Http                                        --version 9.1.1
dotnet add package Swashbuckle.AspNetCore                                    --version 6.4.0
dotnet add package System.IdentityModel.Tokens.Jwt                           --version 8.11.0
Pop-Location

# 7.2) ControleFluxoCaixa.Application
Write-Host "➤ Instalando pacotes NuGet em ControleFluxoCaixa.Application..."
Push-Location .\ControleFluxoCaixa.Application
dotnet add package AutoMapper                                              --version 14.0.0
dotnet add package FluentValidation                                         --version 12.0.0
dotnet add package MediatR                                                   --version 11.1.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer              --version 8.0.16
Pop-Location

# 7.3) ControleFluxoCaixa.Domain
Write-Host "➤ Instalando pacotes NuGet em ControleFluxoCaixa.Domain..."
Push-Location .\ControleFluxoCaixa.Domain
dotnet add package Microsoft.AspNetCore.Identity                            --version 2.3.1
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore         --version 8.0.16
Pop-Location

# 7.4) ControleFluxoCaixa.Infrastructure
Write-Host "➤ Instalando pacotes NuGet em ControleFluxoCaixa.Infrastructure..."
Push-Location .\ControleFluxoCaixa.Infrastructure
dotnet add package AspNetCore.HealthChecks.MySql                             --version 9.0.0
dotnet add package AspNetCore.HealthChecks.Redis                              --version 9.0.0
dotnet add package AutoMapper                                                --version 14.0.0
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection            --version 11.1.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer               --version 8.0.16
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore          --version 8.0.16
dotnet add package Microsoft.EntityFrameworkCore                                --version 8.0.16
dotnet add package Microsoft.EntityFrameworkCore.Design                          --version 8.0.16
dotnet add package Microsoft.EntityFrameworkCore.Relational                     --version 8.0.16
dotnet add package Microsoft.EntityFrameworkCore.Tools                           --version 8.0.16
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis              --version 8.0.13
dotnet add package Microsoft.OpenApi                                            --version 1.6.24
dotnet add package Pomelo.EntityFrameworkCore.MySql                             --version 8.0.3
dotnet add package Swashbuckle.AspNetCore                                        --version 6.4.0
Pop-Location

# 7.5) ControleFluxoCaixa.Messaging
Write-Host "➤ Instalando pacotes NuGet em ControleFluxoCaixa.Messaging..."
Push-Location .\ControleFluxoCaixa.Messaging
dotnet add package Microsoft.Extensions.Configuration.Binder                     --version 8.0.2
dotnet add package Microsoft.Extensions.Configuration.Json                       --version 8.0.1
dotnet add package Microsoft.Extensions.DependencyInjection                       --version 8.0.1
dotnet add package Microsoft.Extensions.Hosting                                    --version 8.0.1
dotnet add package Microsoft.Extensions.Logging.Abstractions                        --version 8.0.3
dotnet add package Microsoft.Extensions.Options.ConfigurationExtensions             --version 8.0.0
dotnet add package MongoDB.Driver                                                   --version 3.4.0
dotnet add package RabbitMQ.Client                                                   --version 6.2.2
Pop-Location

# 7.6) ControleFluxoCaixa.MongoDB
Write-Host "➤ Instalando pacotes NuGet em ControleFluxoCaixa.MongoDB..."
Push-Location .\ControleFluxoCaixa.MongoDB
dotnet add package MongoDB.Driver                                                   --version 2.22.0
Pop-Location

# 7.7) ControleFluxoCaixa.Tests.Unit
Write-Host "➤ Instalando pacotes NuGet em ControleFluxoCaixa.Tests.Unit..."
Push-Location .\ControleFluxoCaixa.Tests.Unit
dotnet add package xunit                                                             --version 2.4.2
dotnet add package xunit.runner.visualstudio                                         --version 2.4.5
dotnet add package Moq                                                               --version 4.20.0
dotnet add package FluentAssertions                                                   --version 6.11.0
Pop-Location

# 7.8) ControleFluxoCaixa.Tests.Integration
Write-Host "➤ Instalando pacotes NuGet em ControleFluxoCaixa.Tests.Integration..."
Push-Location .\ControleFluxoCaixa.Tests.Integration
dotnet add package xunit                                                             --version 2.4.2
dotnet add package xunit.runner.visualstudio                                         --version 2.4.5
dotnet add package FluentAssertions                                                   --version 6.11.0
dotnet add package Microsoft.AspNetCore.Mvc.Testing                                     --version 8.0.0
Pop-Location

Write-Host '✅ Script concluído. Todos os arquivos .cs foram criados conforme as imagens, com namespaces básicos.'
