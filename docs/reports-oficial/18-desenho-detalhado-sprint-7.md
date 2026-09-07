# OptiDrive - Desenho Detalhado Sprint 7

| Campo | Valor |
| --- | --- |
| Documento | Desenho detalhado - Sprint 7 - Polimento e Estabilização |
| Template base | Template - Desenho detalhado |
| Sprint Jira | S7 - Polimento e Estabilidade (`270`) |
| Epic Jira | `OP-85` - Sprint 7 - Polimento e Estabilização Final |
| Período | 29/08/2026 a 31/08/2026 |
| Estado | Encerrada |
| Módulo | M09 - Polimento e Estabilização |
| Versão | 1.0 |
| Autor | Alexandre Miguel |

## Versões do Trabalho

| Versão | Data | Autor | Descrição |
| --- | --- | --- | --- |
| 1.0 | 31/08/2026 | Alexandre Miguel | Registo do incremento, testes e evidências da Sprint 7. |

## Índice

1. SUMÁRIO EXECUTIVO
2. INTRODUÇÃO
3. DESENHO DETALHADO
4. TESTES
5. MANUAL DE UTILIZAÇÃO
6. MANUAL TÉCNICO

## 1. SUMÁRIO EXECUTIVO

A Sprint 7 estabilizou o incremento anterior através da auditoria de responsividade e acessibilidade, polimento da garagem, robustecimento do planeamento e Smart Save, validação da autenticação e preparação da release. O Jira confirma cinco tarefas e 28 de 28 story points concluídos.

## 2. INTRODUÇÃO

O incremento é transversal e preserva a arquitetura ASP.NET Core MVC, serviços de domínio, Razor, SQLite e integrações externas.

## 3. DESENHO DETALHADO

### 3.1 Módulo/Sprint

| ID | Prioridade | Estado | Requisito | Jira |
| --- | --- | --- | --- | --- |
| RF-M09-01 | Must | Validado | O sistema deverá manter legibilidade e navegação em desktop e mobile. | `OP-86` |
| RF-M09-02 | Must | Validado | O sistema deverá manter contexto e feedback nas operações da garagem. | `OP-87` |
| RF-M09-03 | Must | Validado | O sistema deverá validar origem, destino, autonomia e custos sem exceções não tratadas. | `OP-89` |
| RF-M09-04 | Must | Validado | O sistema deverá respeitar autenticação, MFA e autorização por perfil. | `OP-88` |
| RF-M09-05 | Must | Validado | A release deverá passar build, testes, Docker e healthcheck. | `OP-90` |

### 3.2 Diagramas e interface

![Figura 1 - Classes de domínio](assets/diagrams/classes-dominio.png)

![Figura 2 - Processo de planeamento](assets/diagrams/bpmn-01-planeamento-rota.png)

| Área | Verificação | Resultado |
| --- | --- | --- |
| Interface global | Desktop, mobile, claro/escuro, foco e mensagens | Validado |
| Garagem | CRUD, abastecimento/carga, histórico | Validado |
| Planeamento | Origem/destino, rota, Smart Save e fallback | Validado |
| Autenticação/Admin | Login, MFA e papel | Validado |

## 4. TESTES

| Código | Cenário | Critério | Resultado |
| --- | --- | --- | --- |
| REG-S7-001 | Regressão automatizada | Suite xUnit sem falhas | Aprovado |
| SYS-S7-001 | Interface responsiva | Sem overflow e com ações legíveis | Aprovado |
| SYS-S7-002 | Garagem | Operações persistem e apresentam feedback | Aprovado |
| SYS-S7-003 | Planeamento | Erros controlados e Smart Save coerente | Aprovado |
| SYS-S7-004 | Release | Build, testes, Docker e healthcheck | Aprovado |

### 4.1 Testes de aceitação

| Código | Given | When | Then | Estado |
| --- | --- | --- | --- | --- |
| UAT-S7-001 | Condutor autenticado | Gere veículo e planeia rota | Estado persiste e a rota apresenta custos/autonomia | Aprovado |
| UAT-S7-002 | Administrador autenticado | Acede ao backoffice | Não recebe fluxos exclusivos de condutor | Aprovado |
| UAT-S7-003 | Solução disponível | É validada a release | Build, testes, Docker e healthcheck concluem | Aprovado |

## 5. MANUAL DE UTILIZAÇÃO

O condutor usa Perfil, Garagem, Planeamento e Social. O administrador usa Backoffice e ações de segurança.

## 6. MANUAL TÉCNICO

Executar `dotnet build`, `dotnet test`, `docker compose build`, iniciar o contentor, consultar `/health` e registar evidências no Jira/Confluence.
