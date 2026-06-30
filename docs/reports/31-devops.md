# OptiDrive - Documento de DevOps

| Campo | Valor |
| --- | --- |
| Projeto | OptiDrive |
| Stack | ASP.NET Core MVC, .NET 8, SQLite, Docker |
| Data | 30/06/2026 |
| Estado | Concluído |

## 1. Objetivo

Este documento descreve as práticas de DevOps usadas no OptiDrive para garantir execução reproduzível, validação técnica e preparação da entrega.

## 2. Ambientes

| Ambiente | Utilização | Tecnologia |
| --- | --- | --- |
| Local | Desenvolvimento e debug | .NET 8 SDK |
| Docker | Demonstração reproduzível | Dockerfile e docker-compose |
| SQLite LocalDB | Persistência de apresentação | Ficheiro `.db`/volume Docker |

## 3. Pipeline Manual de Validação

| Passo | Comando | Objetivo |
| --- | --- | --- |
| Restaurar dependências | `dotnet restore` | Preparar solução |
| Compilar | `dotnet build` | Validar código |
| Testar | `dotnet test` | Executar testes automatizados |
| Subir Docker | `docker compose up --build` | Validar execução reproduzível |
| Healthcheck | `/health` | Confirmar readiness operacional |

## 4. Configuração

As credenciais e chaves externas são configuradas por variáveis de ambiente e `.env`, evitando hardcoding no código-fonte. O ficheiro `.env.example` documenta as variáveis necessárias.

## 5. Observabilidade

| Elemento | Função |
| --- | --- |
| `/health` | Estado operacional da aplicação |
| Backoffice/Admin | Estado das integrações e dados |
| Logs ASP.NET | Diagnóstico técnico |
| Mensagens UI | Feedback para o utilizador |

## 6. Estratégia de Entrega

A entrega final privilegia Docker e SQLite para reduzir risco durante a apresentação. Esta abordagem permite demonstrar o produto sem depender de infraestrutura cloud externa.
