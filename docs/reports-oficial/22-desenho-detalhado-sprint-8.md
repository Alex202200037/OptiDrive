# OptiDrive - Desenho Detalhado Sprint 8

| Campo | Valor |
| --- | --- |
| Documento | Desenho detalhado - Sprint 8 - Auditoria Final, Aceitação e Entrega |
| Template base | Template - Desenho detalhado |
| Sprint Jira | Sprint `304` - S8 - Auditoria Final |
| Epic Jira | `OP-91` - Sprint 8 - Auditoria Final, Aceitação e Entrega |
| Período | 02/09/2026 a 08/09/2026 |
| Estado | Validação técnica concluída; encerramento Jira em 08/09/2026 |
| Módulo | M10 - Qualidade, Aceitação e Fecho |
| Versão | 1.0 |
| Autor | Alexandre Miguel |

## Versões do Trabalho

| Versão | Data | Autor | Descrição |
| --- | --- | --- | --- |
| 0.1 | 02/09/2026 | Alexandre Miguel | Planeamento após reunião com o docente. |
| 0.2 | 04/09/2026 | Alexandre Miguel | Execução da matriz UAT, smoke funcional e repetição da validação Release. |
| 0.3 | 07/09/2026 | Alexandre Miguel | Validação Docker, persistência, healthcheck, interface móvel e revisão DevOps. |
| 1.0 | 07/09/2026 | Alexandre Miguel | Auditoria final antecipada, otimização do stress 500+500 e reconciliação documental. |

## Índice

1. SUMÁRIO EXECUTIVO
2. INTRODUÇÃO
3. DESENHO DETALHADO
4. TESTES
5. MANUAL DE UTILIZAÇÃO
6. MANUAL TÉCNICO

## 1. SUMÁRIO EXECUTIVO

A Sprint 8 é o último ciclo de validação. Corrige `RF-M08-04`, executa a matriz UAT, reforça a cobertura automatizada, revê diagramas/tabelas/documentos e valida DevOps, Docker, pipeline, Jira, Confluence e entrega. A auditoria técnica final foi antecipada para 07/09/2026; o encerramento administrativo mantém a data planeada de 08/09/2026.

## 2. INTRODUÇÃO

O docente pediu evidência funcional de `RF-M08-04` e uma tabela formal de testes de aceitação. A sprint inclui auditoria preventiva completa.

## 3. DESENHO DETALHADO

### 3.1 Requisitos

| ID | Prioridade | Estado inicial | Objetivo |
| --- | --- | --- | --- |
| RF-M08-04 | Must | Reaberto | Evidências acionáveis no backoffice |
| RF-M10-01 | Must | Concluído | Matriz UAT executada e rastreada |
| RF-M10-02 | Must | Concluído | Regressão, integração, stress, compatibilidade e segurança documentados |
| RF-M10-03 | Must | Concluído | Diagramas e tabelas legíveis e coerentes |
| RF-M10-04 | Must | Concluído tecnicamente | Jira, Confluence, Git e documentos reconciliados |
| RF-M10-05 | Must | Concluído localmente | Pipeline, Docker, health e release validados; deploy externo depende do ambiente |

### 3.2 Alteração RF-M08-04

| Componente | Alteração |
| --- | --- |
| `ProjectEvidenceService` | Métricas e fontes configuráveis |
| `AdminViewModel` | Contagens, data e cartões acionáveis |
| `AdminController` | Evidência incluída no backoffice |
| `Views/Admin/Index.cshtml` | Links para pipeline, testes, stress, health, Jira e Confluence |
| Testes | Fontes, métricas, fallback e ausência de segredos |

### 3.3 Plano diário

| Data | Trabalho | Saída |
| --- | --- | --- |
| 02/09 | Auditoria, ata, backlog e RF-M08-04 | Planeamento criado e trabalhos iniciais concluídos |
| 03/09 | Ativação do Sprint no Jira, UAT e smoke do backoffice | Sprint `304` ativo e UAT-013 atualizado |
| 04/09 | Concluir UAT, automação, integração e regressão | Matriz UAT atualizada e evidência Release |
| 05/09 | Stress, compatibilidade, segurança e Docker | Resultados documentados |
| 06/09 | Diagramas, gráficos, tabelas e templates | Artefactos revistos |
| 07/09 | DevOps, pipeline, Azure/readiness | Release validada |
| 08/09 | Auditoria cruzada e encerramento | Jira/Confluence/pacote final |

### 3.4 Backlog comprometido no Jira

| Jira | Trabalho | Story points | Data-limite |
| --- | --- | ---: | --- |
| `OP-92` | Auditar requisitos, documentação e rastreabilidade | 3 | 02/09 |
| `OP-93` | Corrigir e validar `RF-M08-04` no backoffice | 5 | 03/09 |
| `OP-94` | Criar e executar a matriz formal de testes de aceitação | 5 | 04/09 |
| `OP-95` | Reforçar testes automatizados, integração e regressão | 5 | 05/09 |
| `OP-96` | Executar stress, compatibilidade, segurança e Docker | 5 | 05/09 |
| `OP-97` | Auditar e corrigir diagramas, gráficos e tabelas | 5 | 06/09 |
| `OP-98` | Rever documentação, DevOps e pacote Confluence | 5 | 07/09 |
| `OP-99` | Auditoria final, demonstração e encerramento | 3 | 08/09 |

Total comprometido: **8 tarefas / 36 story points**.

## 4. TESTES

| Campo | Resultado revalidado 07/09 |
| --- | --- |
| Comando | `dotnet test OptiDrive.sln --configuration Release` |
| Total | 16 |
| Aprovados / Falhados / Ignorados | 16 / 0 / 0 |
| Duração | 9,361 s na execução detalhada após otimização |

### 4.1 Aceitação RF-M08-04

| Campo | Valor |
| --- | --- |
| Código | UAT-013 |
| Passos | Abrir Backoffice e acionar Pipeline, Testes, Stress, `/health`, Jira e Confluence |
| Esperado | Fontes corretas, números coerentes e nenhum segredo exposto |
| Estado | Aprovado: automação e smoke visual local repetidos em 04/09/2026 e revalidados pela suite Release de 07/09/2026 |

Em 04/09/2026 foram repetidos o smoke visual de `RF-M08-04`, o healthcheck e a suite Release. A matriz registou 13 casos aprovados, 1 em validação, 2 condicionados por serviços externos e 4 planeados para os dias seguintes.

Em 07/09/2026, foram aprovados o build Release, `16/16` testes, o arranque e healthcheck automático do Docker Compose, a persistência após reinício e o smoke móvel PT/EN. O teste 500+500 registou 8,518 s na criação dos dados e 0,327 s na construção de 500 dashboards. A pipeline foi reforçada com gates de formatação, vulnerabilidades, testes TRX, publicação e Docker; a implantação Azure continua condicionada ao ambiente externo.

A matriz global encontra-se em `21-tabela-testes-aceitacao.md`.

## 5. MANUAL DE UTILIZAÇÃO

O administrador consulta **Qualidade, entrega e rastreabilidade** no Backoffice e abre a fonte de cada evidência.

## 6. MANUAL TÉCNICO

Executar restore, build e testes Release; validar formatação, dependências, Docker e `/health`; confirmar a matriz UAT; reconciliar Jira, Confluence, Git e evidências; e só então encerrar. A checklist foi executada em 07/09/2026 e encontra-se registada em `evidencias/sprint-8/final-2026-09-07/auditoria-final.md`.
