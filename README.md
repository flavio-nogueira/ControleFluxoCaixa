
# ControleFluxoCaixa

Sistema para controle de fluxo de caixa com arquitetura moderna, mensageria e observabilidade completa.

---

## ✨ Visão Geral

Este projeto foi desenvolvido para atender ao desafio de Arquiteto de Soluções. Seu objetivo é permitir que um comerciante faça lançamentos financeiros (crédito e débito) e visualize relatórios consolidados de saldo diário. A solução contempla escalabilidade, resiliência, segurança, mensageria assíncrona e observabilidade completa com Prometheus, Grafana e Loki.

Desenvolvido por: **Flávio Nogueira**  
Email: flavio@startupinfosoftware.com.br

---

## 📦 Visão Geral dos Projetos na Solução

| Projeto | Descrição |
|--------|-----------|
| **ControleFluxoCaixa.API** | API principal RESTful com autenticação JWT, comandos CQRS e validações |
| **ControleFluxoCaixa.Gatware.BFF** | API BFF com Ocelot, Swagger manual, autenticação e integração com Prometheus/Grafana |
| **ControleFluxoCaixa.Application** | Camada de Application Services com comandos, queries e validações |
| **ControleFluxoCaixa.CrossCutting** | Logs estruturados com Serilog, configurações e helpers |
| **ControleFluxoCaixa.Domain** | Entidades, enums e contratos da lógica de negócio |
| **ControleFluxoCaixa.Infrastructure** | Integração com banco de dados MySQL |
| **ControleFluxoCaixa.Messaging** | Publicação em fila RabbitMQ |
| **ControleFluxoCaixa.MongoDB** | Leitura e persistência de dados consolidados no MongoDB |
| **ControleFluxoCaixa.WorkerRabbitMq** | Worker Service que consome mensagens do RabbitMQ e grava em MongoDB |
| **ControleFluxoCaixa.Tests.Unit** | Testes unitários com xUnit e mocks |
| **ControleFluxoCaixa.Tests.Integration** | Testes de integração com cenários completos |
| **docker-compose** | Orquestração local com Prometheus, Grafana, MySQL, RabbitMQ e MongoDB |

---

## 🎯 Objetivo da Solução

- Controle financeiro completo de entradas e saídas
- Registro de lançamentos gravados no MySQL
- Publicação automática em RabbitMQ após criação/edição
- Consolidação de saldos diários em MongoDB via Worker
- Visualização de métricas e logs via Grafana + Prometheus + Serilog


## 🧱 Arquitetura Técnica (Diagrama C4)

<img width="1209" height="1102" alt="Diagrama sem nome drawioV2" src="https://github.com/user-attachments/assets/178d9e5e-8829-4eaa-a650-2fad0970e209" />

---

# Diagrama de Fluxo de Processo


<img width="1125" height="962" alt="Diagrama de Fluxo de Processo drawio" src="https://github.com/user-attachments/assets/408c17eb-fdd9-490b-9260-59498d140175" />

---

# Cloud Solution Aquitetura 

---<img width="2011" height="2018" alt="cloud drawio2" src="https://github.com/user-attachments/assets/06e677c7-b2a3-43dc-a96d-d26ac5735cf8" />





## 🚀 Recursos Implementados (Resumo)

✅ Autenticação com JWT (Access + Refresh Token)  
✅ Proteção de rotas com `[Authorize]` + Swagger com botão Authorize  
✅ CQRS com MediatR para leitura e escrita separadas  
✅ Mensageria com RabbitMQ + persistência assíncrona de saldos no Worker  
✅ Observabilidade: Prometheus (métricas), Grafana (dashboards), Serilog (logs estruturados)  
✅ Documentação completa via Swagger (API e BFF com DocumentFilter)

Exemplo de métrica:
```txt
# HELP api_requests_total Contador de requisições
# TYPE api_requests_total counter
api_requests_total{endpoint="/bff/lancamento/create"} 72
```

---

## ✅ Conclusão
Esta solução foi construída com foco em:

- Separação de responsabilidades (API, BFF, Worker)
- Alta observabilidade e rastreabilidade
- Escalabilidade horizontal via Docker/Kubernetes
- Boas práticas de autenticação, logs e métricas
- Padrões modernos como CQRS, DDD e mensageria

---

## 📐 Complementos Arquiteturais

### 📶 Escalabilidade
- Serviços stateless prontos para réplicas
- Banco de dados MySQL configurado com 1 master e 3 slaves
- Pool de conexões ativo na API
- Rate Limiting distribuído com Redis
- Cache com `MemoryCache`
- Arquitetura preparada para uso de Auto Scaling da AWS (ECS/EKS)
- Pronto para Horizontal Pod Autoscaler em K3s

### 🛡️ Resiliência
- Retry com Polly para falhas temporárias
- Worker assíncrono desacoplado por RabbitMQ
- Logs estruturados com rastreamento de exceções
- Failover possível com múltiplas instâncias
- Monitoramento e alertas proativos com Prometheus + Grafana

### 🧱 Padrões Arquiteturais
- Microsserviço orientado a domínio (DDD)
- CQRS + MediatR
- Clean Architecture (camadas Application, Domain, Infra)
- Separação clara de responsabilidades: API, BFF, Worker

### 🔄 Integração
- API ↔ BFF: REST/JSON via HTTP
- API → RabbitMQ: eventos JSON assíncronos
- Worker → MongoDB: persistência direta
- Prometheus coleta métricas via /metrics
- Grafana consulta Prometheus/Loki via datasources

### 📊 Requisitos Não-Funcionais (Detalhados)
- ✅ Disponibilidade: alta via containers escaláveis
- ✅ Desempenho: cache, leitura otimizada, replicação de banco
- ✅ Observabilidade: logs, métricas e alertas configurados
- ✅ Segurança: JWT, RefreshToken, RateLimit, HTTPS (produção)
- ✅ Tolerância a falhas: filas, retries, monitoramento
- ✅ Documentação: Swagger e explicações técnicas no README
- ✅ Auditabilidade: Serilog com traceId e userId nos logs

---

## 📘 Requisitos de Negócio e Arquitetura

- Controle diário de lançamentos financeiros (débito/crédito)
- Relatório de saldo diário consolidado
- Processamento assíncrono desacoplado via RabbitMQ + Worker
- Arquitetura baseada em domínios e responsabilidades bem definidos (DDD)
- Comunicação eficiente entre áreas e camadas
- Segurança com autenticação, autorização e rate limiting
- Escalabilidade com banco replicado e containers
- Observabilidade e rastreabilidade com Prometheus, Grafana, Loki

---

## 🌐 Repositório

https://github.com/flavio-nogueira/ControleFluxoCaixa

---

## 🙌 Autor

Flávio Nogueira  
[Website](https://startupinfosoftware.com.brv)  
[Email](mailto:flavio@startupinfosoftware.com.br)

Licença MIT
