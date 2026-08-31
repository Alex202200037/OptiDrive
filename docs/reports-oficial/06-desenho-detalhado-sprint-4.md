# OptiDrive - Desenho Detalhado Sprint 4

| Campo | Valor |
| --- | --- |
| Documento | Desenho detalhado - Sprint 4 - Social, OAuth e Viagens Colaborativas |
| Template base | Template - Desenho detalhado |
| Período | 09/06/2026 a 16/06/2026 |
| Módulos | M01, M06 |
| Versão | 3.0 |

## 1. Sumário Executivo

A Sprint 4 teve como objetivo: Perfis sociais, contactos, mensagens, viagens em grupo e autenticação externa. O desenho detalhado apresenta requisitos implementados, componentes técnicos, processo BPMN, interface, testes, manual de utilização e manual técnico.

## 2. Introdução

Este documento detalha apenas o incremento da Sprint 4, seguindo a separação entre análise e desenho indicada nos slides: a análise define o que o sistema deverá fazer; o desenho detalhado descreve como a solução foi organizada para implementar esses requisitos.

## 3. Desenho Detalhado

### 3.1 Introdução

O incremento foi implementado no projeto `OptiDrive.Web`, usando controllers MVC, serviços de domínio, views Razor e persistência EF Core/SQLite.

### 3.2 Módulos da Sprint

| Módulo(s) | Objetivo técnico |
| --- | --- |
| M01, M06 | Perfis sociais, contactos, mensagens, viagens em grupo e autenticação externa |

#### 3.2.1 Requisitos funcionais implementados

| ID | Módulo | Prioridade | Estado | Requisito |
| --- | --- | --- | --- | --- |
| RF-M01-04 | M01 | Should | Implementado | O sistema deverá permitir autenticação externa por Google e Microsoft quando configurada. |
| RF-M06-01 | M06 | Must | Implementado | O sistema deverá permitir manter perfil social com nome, fotografia, cidade e preferências. |
| RF-M06-02 | M06 | Must | Implementado | O sistema deverá permitir pesquisar utilizadores e solicitar ligação social. |
| RF-M06-03 | M06 | Must | Implementado | O sistema deverá permitir trocar mensagens entre utilizadores ligados. |
| RF-M06-04 | M06 | Should | Implementado | O sistema deverá permitir criar viagens colaborativas com participantes. |
| RF-M06-05 | M06 | Could | Implementado | O sistema deverá permitir dividir custos estimados entre participantes. |


#### 3.2.2 Diagrama de Classes de desenho detalhado

![Figura 1 - Classes de domínio relevantes](assets/diagrams/classes-dominio.png)

*Figura 1 - Classes de domínio relevantes.*

#### 3.2.3 Diagramas de Processos de negócio referentes ao módulo

![Figura 2 - Processo BPMN da Sprint 4](assets/diagrams/bpmn-03-social-viagem.png)

*Figura 2 - Processo BPMN da Sprint 4.*

#### 3.2.4 Diagramas de Estados referentes ao módulo

| Entidade | Estados considerados |
| --- | --- |
| Veículo | Criado, atualizado, abastecido/carregado, usado em viagem |
| Rota | Planeada, guardada, aplicada ao veículo |
| Viagem colaborativa | Planned, Scheduled, Active, Completed, Cancelled |
| Pedido social | Pending, Accepted, Declined |

#### 3.2.5 Interface com o utilizador referente ao módulo

| Página | Função |
| --- | --- |
| Perfil | Sessão, MFA, informação do utilizador |
| Garagem | Ficha do veículo, nível, histórico e abastecimento |
| Planeamento | Mapa, rota, postos, Smart Save e aplicação da rota |
| Social | Perfis, contactos, mensagens e viagens colaborativas |
| Admin | Backoffice, sincronizações e estado operacional |

## 3.3 Testes

### 3.3.1 Testes Unitários

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Validar regras principais da Sprint 4 |
| Código | UT-S4-001 |
| Finalidade | Garantir comportamento esperado dos serviços e modelos do incremento |
| Entradas | Dados válidos e inválidos de acordo com o módulo |
| Resultados esperados | Cálculo, validação ou atualização de estado correta |
| Dependências | xUnit, serviços de domínio e dados controlados |

### 3.3.2 Testes de Automação

Os testes automatizados existentes podem ser executados por `dotnet test`, com foco em SmartSaveService e AppStateService.

### 3.3.3 Testes de Integração

| Código | Componentes | Resultado esperado |
| --- | --- | --- |
| IT-S4-001 | Controller + Service + EF Core | Pedido válido altera o estado persistido sem erro |

### 3.3.4 Testes de Sistema

| Código | Cenário E2E | Resultado esperado |
| --- | --- | --- |
| ST-S4-001 | Utilizador percorre o fluxo principal da Sprint 4 | Fluxo conclui sem erro bloqueante |

### 3.3.5 Testes de aceitação

| Código | Given | When | Then |
| --- | --- | --- | --- |
| UAT-S4-001 | O utilizador está autenticado | Executa o fluxo principal da sprint | O sistema apresenta resultado claro e persistente |

## 3.4 Manual de utilização

1. Iniciar sessão com uma conta válida.
2. Aceder à área correspondente ao módulo da sprint.
3. Preencher dados obrigatórios.
4. Confirmar a ação e validar feedback visual.
5. Consultar histórico/resultado quando aplicável.

## 3.5 Manual técnico

1. Abrir a solução `OptiDrive.sln`.
2. Configurar variáveis no `.env` quando existirem APIs externas.
3. Executar `dotnet build` e `dotnet test`.
4. Executar `dotnet run --project OptiDrive.Web` ou `docker compose up --build`.
5. Validar `/health` e os fluxos críticos da sprint.
