# Controle de Fluxo de Caixa - Solução Completa com BFF, Mensageria, Observabilidade e Escalabilidade

Este repositório contém uma solução moderna para controle de fluxo de caixa, estruturada em microsserviços com suporte a mensageria, autenticação JWT, cache, observabilidade com Prometheus/Grafana, logs com Serilog, e arquitetura escalável.

> Desenvolvido por: **Flavio Nogueira**
> E-mail: [flavio@startupinfosoftware.com.br](mailto:flavio@startupinfosoftware.com.br)
> Website: 

---

## 📦 Visão Geral dos Projetos na Solução

| Projeto                                | Descrição                                                                            |
| -------------------------------------- | ------------------------------------------------------------------------------------ |
| `ControleFluxoCaixa.API`               | API principal RESTful com autenticação JWT, comandos CQRS e validações               |
| `ControleFluxoCaixa.Gatware.BFF`       | API BFF com Ocelot, Swagger manual, autenticação e integração com Prometheus/Grafana |
| `ControleFluxoCaixa.Application`       | Camada de Application Services com comandos, queries e validações                    |
| `ControleFluxoCaixa.CrossCutting`      | Logs estruturados com Serilog, configurações e helpers                               |
| `ControleFluxoCaixa.Domain`            | Entidades, enums e contratos da lógica de negócio                                    |
| `ControleFluxoCaixa.Infrastructure`    | Integração com banco de dados MySQL                                                  |
| `ControleFluxoCaixa.Messaging`         | Publicação em fila RabbitMQ                                                          |
| `ControleFluxoCaixa.MongoDB`           | Leitura e persistência de dados consolidados no MongoDB                              |
| `ControleFluxoCaixa.WorkerRabbitMq`    | Worker Service que consome mensagens do RabbitMQ e grava em MongoDB                  |
| `ControleFluxoCaixa.Tests.Unit`        | Testes unitários com xUnit e mocks                                                   |
| `ControleFluxoCaixa.Tests.Integration` | Testes de integração com cenários completos                                          |
| `docker-compose`                       | Orquestração local com Prometheus, Grafana, MySQL, RabbitMQ e MongoDB                |

---

## 🎯 Objetivo da Solução

* Controle financeiro completo de entradas e saídas
* Registro de lançamentos gravados no **MySQL**
* Publicação automática em **RabbitMQ** após criação/edição
* Consolidação de saldos diários em **MongoDB** via Worker
* Visualização de métricas e logs via **Grafana + Prometheus + Serilog**

---

## 🧱 Arquitetura Técnica

```
[Frontend SPA ou Mobile]
       ↓
[BFF - Gatware com Ocelot]
       ↓
[API REST ControleFluxoCaixa.API]
       ↓                        ↘
[MySQL ← Dapper]         [→ RabbitMQ → Worker → MongoDB (saldos consolidados)]
```

---

## 🚀 Recursos Implementados

### ✅ Autenticação e Autorização

* Login com JWT (Access + Refresh Token)
* Proteção de rotas com `[Authorize]`
* Swagger com botão **Authorize** e segurança Bearer

### ✅ CQRS com MediatR

* Separação clara entre comandos (escrita) e queries (leitura)

### ✅ Mensageria com RabbitMQ

* Eventos publicados após criação ou exclusão de lançamentos
* Worker dedicado para persistência de saldos no MongoDB

### ✅ Observabilidade

* Prometheus coleta métricas HTTP e expõe em `/metrics`
* Grafana exibe dashboards com:

  * Tempo de resposta por rota
  * Quantidade de requisições
  * Erros HTTP
* Serilog grava logs estruturados por controller e serviço

### ✅ Documentação Swagger

* Documentação das rotas da API e BFF
* Endpoints manuais no BFF com `DocumentFilter`
* JWT embutido via botão **Authorize**

---

## 📊 Exemplo de Métricas Prometheus

```txt
# HELP api_requests_total Contador de requisições
# TYPE api_requests_total counter
api_requests_total{endpoint="/bff/lancamento/create"} 72
```

---

## 🐳 Docker Compose (Infraestrutura local)

```yaml
services:
  mysql:
  mongodb:
  rabbitmq:
  prometheus:
  grafana:
  controlefluxocaixa_api:
  controlefluxocaixa_bff:
  controlefluxocaixa_worker:
```

---

## 📁 Estrutura da Solução

```
ControleFluxoCaixa.sln
├── API
│   └── Controllers, Program.cs, appsettings.json
├── Gatware.BFF
│   └── Controllers, Dtos, Filters, Extensions, ocelot.json
├── Application
│   └── Commands, Queries, DTOs
├── Domain
├── Infrastructure (MySQL)
├── Messaging (RabbitMQ Publisher)
├── MongoDB (Consolidação de saldos)
├── WorkerRabbitMq (Consumer + Persistência)
├── Tests (Unit + Integration)
└── docker-compose (Redis, Grafana, Prometheus, etc)
```

---

## ✅ Conclusão

Esta solução foi construída com foco em:

* Separação de responsabilidades (API, BFF, Worker)
* Alta observabilidade e rastreabilidade
* Escalabilidade horizontal via Docker/Kubernetes
* Boas práticas de autenticação, logs e métricas
* Padrões modernos como CQRS, DDD e mensageria

