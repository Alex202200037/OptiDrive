# OptiDrive - Desenho Detalhado Sprint 8

| Campo | Valor |
| --- | --- |
| Documento | Desenho detalhado - Sprint 8 - Auditoria Final, Aceitação e Entrega |
| Template base | Template - Desenho detalhado |
| Sprint Jira | S8 - Auditoria Final |
| Epic Jira | `OP-91` - Sprint 8 - Auditoria Final, Aceitação e Entrega |
| Período | 03/09/2026 a 08/09/2026 |
| Estado | Encerrada no Jira em 07/09/2026; revalidação técnica final concluída em 08/09/2026 |
| Módulo | M10 - Qualidade, Aceitação e Fecho |
| Versão | 1.1 |
| Autor | Alexandre Miguel |

## Versões do Trabalho

| Versão | Data | Autor | Descrição |
| --- | --- | --- | --- |
| 0.1 | 02/09/2026 | Alexandre Miguel | Planeamento após reunião com o docente. |
| 0.2 | 04/09/2026 | Alexandre Miguel | Execução da matriz UAT, smoke funcional e validação Release. |
| 0.3 | 07/09/2026 | Alexandre Miguel | Docker, persistência, healthcheck, interface móvel e revisão DevOps. |
| 1.0 | 07/09/2026 | Alexandre Miguel | Encerramento no Jira após conclusão das oito tarefas. |
| 1.1 | 08/09/2026 | Alexandre Miguel | Auditoria final de código, testes, diagramas, documentos e evidências. |

## Índice

1. SUMÁRIO EXECUTIVO
2. INTRODUÇÃO
3. DESENHO DETALHADO
4. TESTES
5. MANUAL DE UTILIZAÇÃO
6. MANUAL TÉCNICO

## 1. SUMÁRIO EXECUTIVO

A Sprint 8 constituiu o último ciclo de qualidade do OptiDrive. O incremento corrigiu e validou `RF-M08-04`, formalizou a matriz de testes de aceitação, reforçou a regressão automática, executou stress com 500 utilizadores e 500 veículos, validou Docker e persistência e reconciliou código, Jira, Confluence, diagramas e documentação.

O Sprint Report confirma 8 de 8 itens e 36 de 36 story points concluídos, sem trabalho incompleto e sem itens concluídos fora da sprint. A auditoria técnica de 08/09/2026 confirmou build Release sem erros ou avisos, 16 de 16 testes aprovados, ausência de vulnerabilidades NuGet conhecidas, contentor saudável e persistência inalterada após reinício.

O Burndown automático preserva eventos corretivos de âmbito: três tarefas foram retiradas inadvertidamente e reinseridas duas vezes. O Jira conserva os 12 eventos, apesar do saldo líquido de 0 story points, e não recalcula retroativamente a linha vermelha. A descontinuidade representa histórico de associação à sprint, não trabalho reaberto nem redução do compromisso final.

## 2. INTRODUÇÃO

O docente solicitou evidência funcional de `RF-M08-04` e uma tabela formal de testes de aceitação. A Sprint 8 respondeu a esses pontos e acrescentou uma auditoria preventiva de requisitos, testes, segurança, desempenho, diagramas, documentos e DevOps.

A implementação mantém a arquitetura ASP.NET Core MVC / .NET 8, Razor Views, serviços de domínio, EF Core/SQLite, Docker e integrações configuráveis. Os itens `OP-92` a `OP-99` asseguram rastreabilidade entre trabalho, requisito, teste e evidência.

## 3. DESENHO DETALHADO

### 3.1 Introdução

O incremento é transversal: não cria um novo domínio de produto, mas garante que os módulos existentes são verificáveis, demonstráveis e coerentes. A alteração funcional principal concentra-se no backoffice e no serviço de evidências; as restantes atividades validam a qualidade do conjunto.

### 3.2 Módulo/Sprint

| Módulo | Objetivo técnico | Itens Jira |
| --- | --- | --- |
| M10 - Qualidade, Aceitação e Fecho | Corrigir a evidência DevOps, executar UAT e regressão, validar desempenho/segurança e fechar a entrega. | `OP-92` a `OP-99` |

#### 3.2.1 Requisitos funcionais implementados

| ID | Módulo | Prioridade MoSCoW | Estado | Requisito implementado | Rastreabilidade Jira |
| --- | --- | --- | --- | --- | --- |
| RF-M08-04 | M08 | Must | Corrigido e validado | O backoffice deverá apresentar evidências acionáveis de pipeline, testes, stress, health, Jira e Confluence. | `OP-93` |
| RF-M10-01 | M10 | Must | Implementado | A entrega deverá possuir matriz UAT com cenário, resultado, estado e evidência. | `OP-94` |
| RF-M10-02 | M10 | Must | Implementado | Regressão, integração, stress, compatibilidade e segurança deverão estar testados e documentados. | `OP-95`, `OP-96` |
| RF-M10-03 | M10 | Must | Implementado | Diagramas, gráficos e tabelas deverão estar legíveis, completos e coerentes. | `OP-97` |
| RF-M10-04 | M10 | Must | Implementado | Jira, Confluence, Git e documentos deverão apresentar a mesma versão dos factos finais. | `OP-92`, `OP-98` |
| RF-M10-05 | M10 | Must | Validado com condicionante externa | CI, Docker e healthcheck deverão passar; o deploy Azure depende de credenciais externas. | `OP-98`, `OP-99` |

Backlog comprometido:

| Jira | Trabalho | Story points | Resultado |
| --- | --- | ---: | --- |
| `OP-92` | Auditar requisitos, documentação e rastreabilidade | 3 | Done |
| `OP-93` | Corrigir e validar `RF-M08-04` no backoffice | 5 | Done |
| `OP-94` | Criar e executar a matriz formal de testes de aceitação | 5 | Done |
| `OP-95` | Reforçar testes automatizados, integração e regressão | 5 | Done |
| `OP-96` | Executar stress, compatibilidade, segurança e Docker | 5 | Done |
| `OP-97` | Auditar e corrigir diagramas, gráficos e tabelas | 5 | Done |
| `OP-98` | Rever documentação, DevOps e pacote Confluence | 5 | Done |
| `OP-99` | Auditoria final, demonstração e encerramento | 3 | Done |

#### 3.2.2 Diagrama de Classes de desenho detalhado do módulo

![Figura 1 - Classes de domínio relevantes para a auditoria final](assets/diagrams/classes-dominio.png)

*Figura 1 - As entidades de conta, veículo, rota e colaboração permanecem o núcleo funcional verificado pela matriz UAT.*

| Componente | Alteração/validação na Sprint 8 |
| --- | --- |
| `ProjectEvidenceService` | Produzir métricas e fontes configuráveis sem expor segredos. |
| `AdminViewModel` | Transportar contagens, data de validação e cartões acionáveis. |
| `AdminController` | Incluir evidência de qualidade e entrega no Backoffice. |
| `Views/Admin/Index.cshtml` | Apresentar ligações para pipeline, testes, stress, health, Jira e Confluence. |
| `HealthController` | Confirmar estado, ambiente, contagens e disponibilidade das integrações. |
| `OptiDrive.Web.Tests` | Cobrir autenticação, MFA, garagem, Smart Save, social, administração, health e stress. |

#### 3.2.3 Diagramas de Processos de negócio referentes ao módulo

![Figura 2 - Processo de entrega e operação DevOps](assets/diagrams/bpmn-04-devops-entrega.png)

*Figura 2 - O processo conduz a alteração por build, testes, Docker, healthcheck, evidência e decisão de entrega; uma falha regressa ao passo de correção.*

Fluxo de fecho aplicado:

1. Rever requisito, critério e rastreabilidade Jira.
2. Compilar e executar regressão automática em Release.
3. Construir o contentor e validar `/health` e persistência.
4. Executar smoke visual de visitante, condutor e administrador.
5. Confirmar diagramas, tabelas, documentos e fontes de evidência.
6. Atualizar a matriz UAT e encerrar apenas os itens demonstrados.

#### 3.2.4 Diagramas de Estados referentes ao módulo

| Entidade/Processo | Estado inicial | Transições válidas | Estado terminal |
| --- | --- | --- | --- |
| Caso UAT | A executar | Em validação → Aprovado/Reprovado/Condicionado | Estado com evidência |
| Pipeline CI | Pendente | Restore → Build → Testes → Publish → Docker | Aprovada ou Falhada |
| Deploy Azure | Pendente | Publicação → Verificar credenciais → Deploy | Publicado ou Ignorado por configuração |
| Release | Em auditoria | Código → Testes → Operação → Documentação | Validada |
| Tarefa Jira | To Do | In Progress → Validação → Done | Done |
| Sprint Jira | Ativa | Execução → Relatório final → Encerrar | Encerrada |

#### 3.2.5 Interface com o utilizador referente ao módulo

| Página | Verificação da Sprint 8 | Resultado observado |
| --- | --- | --- |
| Landing page | Proposta, navegação, tema, idioma e mobile. | Aprovado em desktop e 390 px. |
| Perfil | Dados da conta, veículo ativo, MFA e acessos rápidos. | Aprovado como condutor. |
| Garagem | Frota, ficha, nível, alertas e histórico. | Aprovado sem perda de contexto. |
| Planeamento | Formulário, mapa, rota, Smart Save e paragens. | Aprovado com fallback Leaflet. |
| Social | Perfis, mensagens, grupos e privacidade. | Aprovado em modo escuro. |
| Backoffice | Separação de perfil e cartões `RF-M08-04`. | Aprovado com seis fontes acionáveis. |

![Figura 3 - Componentes técnicos da solução](assets/diagrams/componentes.png)

*Figura 3 - Controllers e Razor Views consomem serviços especializados, persistência e clientes externos com responsabilidades separadas.*

## 4. TESTES

Esta secção segue a estrutura do template e regista caso, procedimento e resultado. As provas finais encontram-se em `evidencias/final-2026-09-08/`.

### 4.1 Testes Unitários

#### 4.1.1 Especificação dos Casos de testes: regressão automática

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Suite final de regras de domínio e segurança |
| Código | UT-S8-001 |
| Finalidade | Validar Smart Save, conta, MFA, autorização, garagem, social, health e evidências. |
| Entradas | Veículos ICE/EV, rotas, contas, TOTP, ações administrativas e dados temporários. |
| Resultados esperados | 16 testes aprovados e zero falhas/ignorados. |
| Dependências | xUnit, .NET 8 e projeto `OptiDrive.Web.Tests`. |

#### 4.1.2 Especificação dos Procedimentos: regressão automática

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Suite final de regras de domínio e segurança |
| Código | UT-S8-001 |
| Preparação | Restaurar dependências e compilar em Release. |
| Inicialização | Executar `dotnet test` com produção de resultado TRX. |
| Recursos específicos | .NET 8 SDK, xUnit e runner VSTest. |

#### 4.1.3 Resultados: regressão automática

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Suite final de regras de domínio e segurança |
| Código | UT-S8-001 |
| Responsável | Alexandre Miguel |
| Período de teste | 08/09/2026 |
| Resultados obtidos | 16 aprovados, 0 falhados e 0 ignorados. |
| Observações | Resultado detalhado guardado em `03-automated-tests.txt` e `test-results/optidrive-final-tests.trx`. |

### 4.2 Testes de Automação

#### 4.2.1 Especificação dos Casos de testes: smoke visual

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Percursos críticos por perfil, tema e idioma |
| Código | AUTO-S8-001 |
| Finalidade | Verificar apresentação e navegação de visitante, condutor e administrador. |
| Entradas | Landing, Login, Perfil, Garagem, Planeamento, Social e Backoffice. |
| Resultados esperados | Sem bloqueio, corte horizontal ou perda de contexto. |
| Dependências | Contentor local, browser Chromium e dados seed. |

#### 4.2.2 Especificação dos Procedimentos: smoke visual

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Percursos críticos por perfil, tema e idioma |
| Código | AUTO-S8-001 |
| Preparação | Iniciar Docker e confirmar `/health`. |
| Inicialização | Percorrer os fluxos em PT/EN, claro/escuro, desktop e mobile; autenticar os dois perfis. |
| Recursos específicos | Browser 1280x720 e 390x844, aplicação em `127.0.0.1:5080`. |

#### 4.2.3 Resultados: smoke visual

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Percursos críticos por perfil, tema e idioma |
| Código | AUTO-S8-001 |
| Responsável | Alexandre Miguel |
| Período de teste | 07/09/2026 e 08/09/2026 |
| Resultados obtidos | Dez verificações de usabilidade visual aprovadas. |
| Observações | Evidência textual e capturas em `20-validacao-usabilidade-visual.md` e imagens `11` a `22`. |

### 4.3 Testes de Integração

Estratégia adotada: integração incremental da camada de domínio para persistência, controllers, UI e serviços externos. O ensaio final reuniu aplicação, base SQLite, Docker e healthcheck.

#### 4.3.1 Especificação dos Casos de testes: Docker e persistência

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Arranque, healthcheck e recuperação de dados |
| Código | IT-S8-001 |
| Finalidade | Validar o artefacto em Linux/Docker e a persistência após reinício. |
| Entradas | `docker compose up`, `GET /health`, reinício e nova consulta. |
| Resultados esperados | Contentor saudável; contagens iguais antes e depois. |
| Dependências | Docker Compose, Kestrel, EF Core e volume SQLite. |

#### 4.3.2 Especificação dos Procedimentos: Docker e persistência

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Arranque, healthcheck e recuperação de dados |
| Código | IT-S8-001 |
| Preparação | Construir a imagem e iniciar o serviço. |
| Inicialização | Consultar `/health`, reiniciar o contentor, aguardar estado saudável e repetir a consulta. |
| Recursos específicos | Docker Desktop, Compose e `curl`. |

#### 4.3.3 Resultados: Docker e persistência

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Arranque, healthcheck e recuperação de dados |
| Código | IT-S8-001 |
| Responsável | Alexandre Miguel |
| Período de teste | 08/09/2026 |
| Resultados obtidos | `Healthy` antes/depois; 8 utilizadores, 8 veículos e 2 rotas preservados. |
| Observações | Evidência em `06-docker-compose-build-start.txt` a `10-persistence-comparison.txt`. |

### 4.4 Testes de Regressão

| Campo | Valor |
| --- | --- |
| Código | REG-S8-001 |
| Âmbito | Login, MFA, administração, garagem, rotas, Smart Save, social, health, stress e evidências. |
| Procedimento | Executar build e os 16 testes após as correções documentais e técnicas. |
| Resultado | 16/16 aprovados; 0 falhas e 0 ignorados. |

### 4.5 Testes de Integração com 3rdparty

| Integração | Validação | Resultado/Tratamento |
| --- | --- | --- |
| Google Maps | Configuração e fallback de mapa. | Configurado localmente; fallback Leaflet mantém a aplicação utilizável. |
| OpenChargeMap | Configuração e consulta de carregadores. | Configurado localmente; falha externa é tratada. |
| Google OAuth | Configuração do fornecedor. | Configurado localmente; aceitação ponta a ponta condicionada ao ambiente publicado. |
| Microsoft OAuth | Configuração do fornecedor. | Configurado localmente; aceitação ponta a ponta condicionada ao ambiente publicado. |
| Azure App Service | Workflow de publicação. | Restore/publicação aprovados; deploy omitido por ausência de credenciais no repositório. |

### 4.6 Testes de Sistema - ISO/IEC 25010

#### 4.6.1 Testes de Funcionalidade

| Código | Critério | Resultado |
| --- | --- | --- |
| SYS-FUN-S8-001 | `RF-M08-04` apresenta seis fontes acionáveis. | Aprovado |
| SYS-FUN-S8-002 | Matriz UAT cobre os fluxos críticos. | Aprovado: 18 aprovados e 2 condicionados |
| SYS-FUN-S8-003 | Sprint Report confirma 36/36 story points. | Aprovado |

#### 4.6.2 Testes de Eficiência (Carga)

| Campo | Resultado |
| --- | --- |
| Cenário | Criação de 500 utilizadores e 500 veículos; construção de 500 dashboards. |
| Critério | Conclusão em menos de 10 segundos, sem exceções. |
| Resultado | Criação: 7,998 s; dashboards: 0,405 s; total: 8,402 s. |
| Estado | Aprovado |

#### 4.6.3 Compatibilidade

| Plataforma | Resultado |
| --- | --- |
| macOS / .NET 8 ARM64 | Build e testes aprovados. |
| Linux em Docker | Imagem, arranque e healthcheck aprovados. |
| Desktop 1280x720 | Percursos críticos legíveis. |
| Mobile 390x844 | Landing PT/claro e EN/escuro sem corte horizontal. |

#### 4.6.4 Capacidade de interação

Foram verificados rótulos, ligação de salto, nomes acessíveis, estado dos controlos de tema/idioma, formulários e navegação por papel. O relatório de usabilidade e NPS conserva a avaliação externa; o teste desta sprint é uma validação técnica complementar.

#### 4.6.5 Testes de Confiabilidade

| Critério | Mecanismo | Resultado |
| --- | --- | --- |
| Recuperabilidade | Volume SQLite e reinício do contentor. | Contagens preservadas. |
| Disponibilidade | Healthcheck automático. | Estado `healthy`. |
| Falha de mapa | Fallback Leaflet. | Planeamento mantém-se utilizável. |
| Erros de aplicação | Mensagens controladas e logging. | Sem detalhe interno exposto ao utilizador. |

#### 4.6.6 Segurança

| Controlo | Evidência |
| --- | --- |
| Hashing de password | Teste de registo/login PBKDF2. |
| MFA TOTP | Teste de ativação e verificação. |
| Lockout e administração | Teste de ações de segurança por papel. |
| Segredos | Variáveis de ambiente; `/health` expõe apenas flags booleanas. |
| Dependências | Auditoria NuGet sem pacotes vulneráveis conhecidos. |

#### 4.6.7 Testes de Manutenibilidade

`dotnet format --verify-no-changes` foi aprovado e não foram encontrados marcadores `TODO`, `FIXME`, `HACK` ou `XXX` no código próprio. A solução compilou com zero erros e zero avisos. A rastreabilidade está distribuída por oito itens Jira e 20 casos UAT.

#### 4.6.8 Testes de Flexibilidade

O produto foi verificado em desktop/mobile, PT/EN e claro/escuro. Integrações e URLs de evidência são configuráveis por ambiente. A aplicação pode correr diretamente em .NET 8 ou em Docker.

#### 4.6.9 Testes de Segurança Física (Safety)

O OptiDrive apoia planeamento e não controla o veículo. Autonomia, margem e paragens são apresentadas antes da deslocação. A utilização durante a condução não é recomendada; CarPlay e Android Auto permanecem `Won't Have` neste âmbito.

### 4.7 Testes de aceitação

| Indicador | Resultado final |
| --- | ---: |
| Casos definidos | 20 |
| Aprovados com evidência | 18 |
| Condicionados por serviço/credencial externa | 2 |
| Em validação | 0 |
| Reprovados | 0 |

| Código | Given | When | Then | Estado |
| --- | --- | --- | --- | --- |
| UAT-S8-001 | Conta local válida | O utilizador inicia sessão | Perfil e navegação corretos | Aprovado |
| UAT-S8-002 | Veículo com autonomia limitada | É calculada uma rota longa | Smart Save propõe reforço e margem | Aprovado |
| UAT-S8-003 | Administrador autenticado | Abre o Backoffice | Seis fontes `RF-M08-04` estão acionáveis | Aprovado |
| UAT-S8-004 | Contentor operacional | É reiniciado | Dados e healthcheck recuperam | Aprovado |
| UAT-S8-005 | Dataset 500+500 | É executado o stress test | Conclui abaixo de 10 s | Aprovado |
| UAT-S8-006 | Credenciais OAuth/Azure incompletas | É pedido o fluxo externo | Estado fica condicionado, sem falsa aprovação | Condicionado |

A matriz completa encontra-se em `21-tabela-testes-aceitacao.md`.

## 5. MANUAL DE UTILIZAÇÃO

### 5.1 Condutor

1. Iniciar sessão e confirmar o Perfil.
2. Gerir veículos, nível e histórico na Garagem.
3. Planear a rota com veículo, origem, destino e pontos intermédios.
4. Confirmar consumo, custos, portagens, paragens e margem de chegada.
5. Usar Social para contactos, mensagens e viagens colaborativas.
6. Alternar tema e idioma nas preferências da interface.

### 5.2 Administrador

1. Iniciar sessão com conta administrativa.
2. Consultar utilizadores, segurança, rotas, social e dados externos.
3. Abrir os cartões de Pipeline, Testes, Stress, Health, Jira e Confluence.
4. Confirmar que cada estado apresenta fonte e data de verificação.
5. Executar ações administrativas apenas após a confirmação mostrada pela interface.

## 6. MANUAL TÉCNICO

### 6.1 Preparação local

1. Instalar .NET 8 SDK e Docker Desktop.
2. Criar `.env` a partir de `.env.example`, sem versionar credenciais.
3. Executar restore, formatação, build e testes em Release.
4. Construir e iniciar a aplicação com Docker Compose.
5. Consultar `/health` e confirmar o estado `Healthy`.
6. Executar o smoke visual com os perfis de teste.

### 6.2 Pipeline e release

1. `ci.yml` executa restore, build, formatação, auditoria NuGet, testes TRX, publish e Docker.
2. `azure-deploy.yml` prepara o artefacto e só executa deploy quando nome e publish profile estão configurados.
3. A execução CI `34206950037` foi aprovada.
4. A execução Azure `34207149170` terminou com sucesso e omitiu corretamente o deploy sem credenciais.
5. A aplicação permanece demonstrável localmente e em Docker; publicação Azure é uma condicionante externa.

### 6.3 Evidências e rastreabilidade

| Artefacto | Localização |
| --- | --- |
| Epic Sprint 8 | `OP-91` |
| Tarefas | `OP-92` a `OP-99` |
| Sprint Report | `jira-sprint-reports/sprint-8-jira-report.md` |
| Matriz UAT | `21-tabela-testes-aceitacao.md` |
| Provas finais | `evidencias/final-2026-09-08/` |
| Testes | `OptiDrive.Web.Tests` |
| Diagramas | `assets/diagrams/` e `assets/mermaid-source/` |
| Pipeline | `.github/workflows/ci.yml` e `.github/workflows/azure-deploy.yml` |

### 6.4 Nota sobre o Burndown automático

Durante a correção da associação ao Sprint 8, `OP-92`, `OP-93` e `OP-95` foram retiradas inadvertidamente e depois reinseridas. O Jira preserva seis remoções e seis adições, num saldo líquido de 0 story points, e não reconstrói retroativamente a linha de trabalho remanescente. Por esse motivo, a linha vermelha apresenta descontinuidades que não significam trabalho reaberto.

O indicador final com valor probatório é o Sprint Report: 8/8 itens concluídos, 36/36 story points entregues, 0 itens incompletos e 0 itens concluídos fora da sprint. Não foram efetuados novos movimentos para alterar artificialmente o histórico automático.
