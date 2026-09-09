# OptiDrive - Desenho Detalhado Sprint 7

| Campo | Valor |
| --- | --- |
| Documento | Desenho detalhado - Sprint 7 - Polimento e Estabilização |
| Template base | Template - Desenho detalhado |
| Sprint Jira | S7 - Polimento e Estabilidade (`270`) |
| Epic Jira | `OP-85` - Sprint 7 - Polimento e Estabilização Final |
| Período | 29/08/2026 a 31/08/2026 |
| Estado | Encerrada após cumprimento dos critérios de aceitação |
| Módulo | M09 - Polimento e Estabilização |
| Versão | 1.0 |
| Autor | Alexandre Miguel |

## Versões do Trabalho

| Versão | Data | Autor | Descrição |
| --- | --- | --- | --- |
| 1.0 | 31/08/2026 | Alexandre Miguel | Registo final do incremento, desenho, testes e evidências da Sprint 7. |

## Índice

1. SUMÁRIO EXECUTIVO
2. INTRODUÇÃO
3. DESENHO DETALHADO
4. TESTES
5. MANUAL DE UTILIZAÇÃO
6. MANUAL TÉCNICO

## 1. SUMÁRIO EXECUTIVO

A Sprint 7 estabilizou o incremento anterior através da auditoria de responsividade e acessibilidade, polimento da garagem, robustecimento do planeamento e Smart Save, validação da autenticação e preparação da release. O Jira confirma cinco tarefas concluídas, 28 de 28 story points entregues, zero itens incompletos e zero alterações de âmbito.

O resultado foi verificado por build e testes Release, navegação por perfil, inspeção visual em desktop e mobile, fallback do mapa, execução em Docker e healthcheck. Não foram introduzidos novos módulos de negócio: o objetivo foi reduzir risco de regressão e tornar os fluxos existentes mais consistentes e demonstráveis.

## 2. INTRODUÇÃO

Este documento descreve a implementação e validação da Sprint 7. O incremento é transversal e preserva a arquitetura ASP.NET Core MVC / .NET 8, os serviços de domínio, as Razor Views, a persistência EF Core/SQLite e os clientes de integração existentes.

A rastreabilidade parte dos itens `OP-86` a `OP-90`, passa pelos requisitos `RF-M09-01` a `RF-M09-05` e termina nos casos de teste e critérios de aceitação apresentados neste documento.

## 3. DESENHO DETALHADO

### 3.1 Introdução

As alterações da Sprint 7 foram aplicadas nos fluxos já implementados, sem quebrar os contratos entre controllers, serviços, persistência e interface. A estratégia consistiu em rever a experiência visual, reforçar validações, confirmar a separação de perfis e executar uma regressão final antes da release.

### 3.2 Módulo/Sprint

| Módulo | Objetivo técnico | Itens Jira |
| --- | --- | --- |
| M09 - Polimento e Estabilização | Melhorar consistência visual, robustez de garagem/planeamento, autenticação e readiness de release. | `OP-86`, `OP-87`, `OP-88`, `OP-89`, `OP-90` |

#### 3.2.1 Requisitos funcionais implementados

| ID | Módulo | Prioridade MoSCoW | Estado | Requisito implementado | Rastreabilidade Jira |
| --- | --- | --- | --- | --- | --- |
| RF-M09-01 | M09 | Must | Validado | O sistema deverá manter legibilidade, foco e navegação em desktop e mobile. | `OP-86` |
| RF-M09-02 | M09 | Must | Validado | A garagem deverá preservar contexto e apresentar feedback nas operações de veículo e histórico. | `OP-87` |
| RF-M09-03 | M09 | Must | Validado | O planeamento deverá validar origem, destino, autonomia e custos sem exceções não tratadas. | `OP-89` |
| RF-M09-04 | M09 | Must | Validado | A autenticação, MFA e autorização deverão respeitar o perfil do utilizador. | `OP-88` |
| RF-M09-05 | M09 | Must | Validado | A release deverá passar build, testes, Docker e healthcheck. | `OP-90` |

#### 3.2.2 Diagrama de Classes de desenho detalhado do módulo

![Figura 1 - Classes de domínio relevantes para o incremento](assets/diagrams/classes-dominio.png)

*Figura 1 - O incremento mantém as entidades de identidade, veículo, rota e viagem social, reforçando validações e apresentação nos serviços e controllers existentes.*

| Componente | Responsabilidade na Sprint 7 |
| --- | --- |
| `AppStateService` | Preservar regras de conta, garagem, social e persistência. |
| `SmartSaveService` | Manter previsões de consumo, autonomia, reforço e chegada. |
| Controllers MVC | Validar pedidos, autorização e mensagens de retorno. |
| Razor Views / `site.css` | Garantir hierarquia, responsividade, contraste e foco. |
| Docker / `/health` | Confirmar execução reproduzível e estado operacional. |

#### 3.2.3 Diagramas de Processos de negócio referentes ao módulo

![Figura 2 - Processo de planeamento de rota e Smart Save](assets/diagrams/bpmn-01-planeamento-rota.png)

*Figura 2 - O fluxo valida veículo e dados de viagem, consulta direções e postos, calcula autonomia e introduz reforço quando a margem não é suficiente.*

O processo estabilizado segue estes marcos:

1. O condutor seleciona o veículo e introduz origem e destino.
2. A aplicação valida os dados e o nível atual.
3. Os serviços de rota e energia são consultados com fallback controlado.
4. O Smart Save calcula custo, consumo, paragens e margem de chegada.
5. O itinerário é apresentado e pode ser associado ao veículo.

#### 3.2.4 Diagramas de Estados referentes ao módulo

| Entidade/Processo | Estado inicial | Transições válidas | Estado terminal |
| --- | --- | --- | --- |
| Rota | Em edição | Validar → Calcular → Guardar → Aplicar | Guardada ou Aplicada |
| Veículo | Registado | Editar → Abastecer/Carregar → Atualizar histórico | Atualizado |
| Sessão | Não autenticada | Login → Credenciais → MFA quando ativo → Resolver papel | Condutor ou Administrador autenticado |
| Release | Em validação | Build → Testes → Docker → Healthcheck | Pronta ou Bloqueada |
| Tarefa Jira | To Do | In Progress → Validada → Done | Done |

#### 3.2.5 Interface com o utilizador referente ao módulo

| Página | Verificação da Sprint 7 | Resultado esperado |
| --- | --- | --- |
| Landing page | Hierarquia, proposta de valor, tema e idioma. | Conteúdo compreensível e responsivo antes do login. |
| Perfil | Conta, veículo ativo, MFA e acessos rápidos. | Informação prioritária visível sem navegação excessiva. |
| Garagem | Lista, ficha, estado, alertas e histórico. | Operações mantêm o utilizador no contexto do veículo. |
| Planeamento | Formulário, mapa, rota, Smart Save e feedback. | Erros controlados e itinerário legível. |
| Social | Perfis, contactos, mensagens e viagens colaborativas. | Informação privada separada da informação partilhável. |
| Backoffice | Utilizadores, readiness e evidências. | Acesso exclusivo ao administrador. |

Navegação por papel:

| Papel | Entrada | Áreas apresentadas | Proteção |
| --- | --- | --- | --- |
| Condutor | Perfil | Perfil, Garagem, Planeamento e Social | Backoffice recusado. |
| Administrador | Backoffice | Perfil, Social e Backoffice | Fluxos de garagem/planeamento não são apresentados na navegação administrativa. |

## 4. TESTES

Esta secção segue a estrutura do template: caso, procedimento e resultado são registados para cada teste principal. As evidências correspondem ao incremento verificado durante a Sprint 7.

### 4.1 Testes Unitários

#### 4.1.1 Especificação dos Casos de testes: regras de domínio

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Regressão dos serviços e regras principais |
| Código | UT-S7-001 |
| Finalidade | Validar Smart Save, garagem, autenticação, MFA, administração, social e persistência. |
| Entradas | Contas, códigos TOTP, veículos ICE/EV, rotas, ações administrativas e viagens sociais. |
| Resultados esperados | Cálculos, autorização e transições de estado corretos, sem exceções. |
| Dependências | xUnit, serviços de domínio e dados controlados. |

#### 4.1.2 Especificação dos Procedimentos: regras de domínio

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Regressão dos serviços e regras principais |
| Código | UT-S7-001 |
| Preparação | Restaurar dependências e compilar a solução. |
| Inicialização | Executar a suite `OptiDrive.Web.Tests` em Release. |
| Recursos específicos | .NET 8 SDK, xUnit e dados temporários de teste. |

#### 4.1.3 Resultados: regras de domínio

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Regressão dos serviços e regras principais |
| Código | UT-S7-001 |
| Responsável | Alexandre Miguel |
| Período de teste | 29/08/2026 a 31/08/2026 |
| Resultados obtidos | Suite automatizada aprovada sem falhas bloqueantes. |
| Observações | A suite foi novamente executada e aprovada na auditoria final da Sprint 8. |

### 4.2 Testes de Automação

#### 4.2.1 Especificação dos Casos de testes: fluxo principal

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Smoke de navegação, perfis e responsividade |
| Código | AUTO-S7-001 |
| Finalidade | Confirmar os percursos de condutor e administrador e a adaptação visual. |
| Entradas | Login de teste, Perfil, Garagem, Planeamento, Social e Backoffice. |
| Resultados esperados | Navegação coerente, ações legíveis e ausência de erros não tratados. |
| Dependências | Aplicação local, navegador moderno e dados seed. |

#### 4.2.2 Especificação dos Procedimentos: fluxo principal

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Smoke de navegação, perfis e responsividade |
| Código | AUTO-S7-001 |
| Preparação | Iniciar a aplicação e confirmar `/health`. |
| Inicialização | Autenticar os dois papéis, percorrer as páginas críticas e alternar tema/idioma. |
| Recursos específicos | Browser desktop/mobile, dados de demonstração e Docker local. |

#### 4.2.3 Resultados: fluxo principal

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Smoke de navegação, perfis e responsividade |
| Código | AUTO-S7-001 |
| Responsável | Alexandre Miguel |
| Período de teste | 29/08/2026 a 31/08/2026 |
| Resultados obtidos | Percursos críticos, tema, idioma e separação de perfis aprovados. |
| Observações | O núcleo repetível está automatizado em xUnit/CI; a inspeção visual foi repetida na Sprint 8. |

### 4.3 Testes de Integração

Estratégia adotada: integração incremental entre domínio, persistência, controllers, interface e serviços externos, evitando uma integração `big bang` no fecho.

#### 4.3.1 Especificação dos Casos de testes: aplicação e persistência

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Integração de estado, persistência e planeamento |
| Código | IT-S7-001 |
| Finalidade | Validar a comunicação entre controllers, serviços, SQLite e healthcheck. |
| Entradas | Login, veículo, rota e pedido `GET /health`. |
| Resultados esperados | Estado coerente, dados persistidos e HTTP 200. |
| Dependências | ASP.NET Core, EF Core, SQLite e Docker. |

#### 4.3.2 Especificação dos Procedimentos: aplicação e persistência

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Integração de estado, persistência e planeamento |
| Código | IT-S7-001 |
| Preparação | Compilar, iniciar a aplicação e carregar dados seed. |
| Inicialização | Autenticar, consultar garagem/planeamento, guardar uma rota controlada e consultar `/health`. |
| Recursos específicos | Kestrel, SQLite, Docker Compose e browser. |

#### 4.3.3 Resultados: aplicação e persistência

| Campo | Valor |
| --- | --- |
| Nome Caso de teste | Integração de estado, persistência e planeamento |
| Código | IT-S7-001 |
| Responsável | Alexandre Miguel |
| Período de teste | 31/08/2026 |
| Resultados obtidos | Fluxos concluídos e estado operacional confirmado. |
| Observações | A recuperabilidade foi revalidada por reinício do contentor na Sprint 8. |

### 4.4 Testes de Regressão

| Campo | Valor |
| --- | --- |
| Código | REG-S7-001 |
| Âmbito | Login, MFA, administração, garagem, rotas, Smart Save, social e persistência. |
| Procedimento | Executar a suite automática depois do polimento de UI e das validações de fluxo. |
| Resultado | Aprovado; não foi identificada regressão bloqueante. |

### 4.5 Testes de Integração com 3rdparty

| Integração | Validação | Resultado/Tratamento |
| --- | --- | --- |
| Google Maps | Geocoding/directions configuráveis e mapa de browser. | Fallback Leaflet/OpenStreetMap mantém o fluxo utilizável. |
| OpenChargeMap | Consulta de carregadores e estado de configuração. | Indisponibilidade externa não bloqueia o planeamento. |
| Google/Microsoft OAuth | Configuração e resolução de fornecedor. | Fluxos condicionados às credenciais do ambiente. |
| Apple OAuth | Ponto de extensão previsto. | Não ativo sem credenciais; não é apresentado como concluído. |

### 4.6 Testes de Sistema - ISO/IEC 25010

#### 4.6.1 Testes de Funcionalidade

| Código | Critério | Resultado |
| --- | --- | --- |
| SYS-FUN-S7-001 | Cinco itens Jira satisfazem os critérios de aceitação. | Aprovado |
| SYS-FUN-S7-002 | Garagem e planeamento mantêm estado e feedback. | Aprovado |
| SYS-FUN-S7-003 | Perfis recebem navegação e autorização adequadas. | Aprovado |

#### 4.6.2 Testes de Eficiência (Carga)

| Campo | Resultado |
| --- | --- |
| Cenário | Dataset de stress com 500 utilizadores e 500 veículos. |
| Critério | Conclusão sem exceções e dentro do limite automático definido. |
| Resultado | Aprovado na suite; repetido com medição detalhada na Sprint 8. |

#### 4.6.3 Compatibilidade

| Plataforma | Resultado |
| --- | --- |
| macOS / .NET 8 ARM64 | Build e testes aprovados. |
| Linux em Docker | Imagem e healthcheck aprovados. |
| Desktop | Páginas críticas sem overflow horizontal. |
| Mobile | Navegação, cartões e formulários mantiveram leitura. |

#### 4.6.4 Capacidade de interação

Foram revistos rótulos, foco, mensagens, estados de botões e preferências de tema/idioma. O questionário de usabilidade e NPS permanece a evidência de utilizadores externos; este smoke visual técnico não substitui essa amostra.

#### 4.6.5 Testes de Confiabilidade

| Critério | Mecanismo | Resultado |
| --- | --- | --- |
| Tolerância a falha de mapas | Fallback Leaflet/OpenStreetMap. | Fluxo continua operacional. |
| Persistência | SQLite e volume Docker. | Dados recuperáveis após reinício. |
| Erros | Mensagem controlada e detalhe em logging. | Sem exposição de exceções na interface. |

#### 4.6.6 Segurança

| Controlo | Evidência |
| --- | --- |
| Password hashing | Teste de registo/login com password cifrada. |
| MFA TOTP | Teste de ativação e verificação de código. |
| Autorização por papel | Condutor e administrador com áreas distintas. |
| Segredos | Variáveis de ambiente e flags booleanas no healthcheck. |

#### 4.6.7 Testes de Manutenibilidade

As alterações permaneceram separadas entre controllers, serviços, views, estilos e testes. Os itens `OP-86` a `OP-90` conservam a rastreabilidade. Não foi calculado MTTR estatístico por o projeto ter sido executado por uma pessoa; as transições do Jira permanecem como evidência temporal.

#### 4.6.8 Testes de Flexibilidade

A interface foi verificada em desktop/mobile, claro/escuro e PT/EN. As integrações externas são configuráveis e o mapa possui fallback independente do Google Maps no browser.

#### 4.6.9 Testes de Segurança Física (Safety)

O OptiDrive não controla o veículo. A aplicação apresenta autonomia, margem e paragens para suportar planeamento antes da condução. CarPlay e Android Auto permanecem `Won't Have` no âmbito académico atual.

### 4.7 Testes de aceitação

| Código | Given | When | Then | Estado |
| --- | --- | --- | --- | --- |
| UAT-S7-001 | Condutor autenticado | Gere veículo e planeia rota | Estado persiste e custos/autonomia são apresentados | Aprovado |
| UAT-S7-002 | Administrador autenticado | Acede à aplicação | É encaminhado para o Backoffice | Aprovado |
| UAT-S7-003 | API Google indisponível no browser | O condutor calcula uma rota | O fallback mantém o planeamento utilizável | Aprovado |
| UAT-S7-004 | Solução disponível | São executados build, testes, Docker e healthcheck | Todos os passos concluem sem erro bloqueante | Aprovado |
| UAT-S7-005 | Sprint estimada e concluída | É consultado o Sprint Report | 5/5 itens e 28/28 story points aparecem concluídos | Aprovado |

## 5. MANUAL DE UTILIZAÇÃO

### 5.1 Condutor

1. Iniciar sessão e confirmar o veículo ativo no Perfil.
2. Abrir Garagem para editar o veículo ou registar combustível/carga.
3. Abrir Planeamento, selecionar veículo e introduzir origem/destino.
4. Confirmar custos, autonomia, paragens e margem de chegada.
5. Usar Social para contactos, mensagens e viagens colaborativas.

### 5.2 Administrador

1. Iniciar sessão com conta administrativa.
2. Consultar o Backoffice para utilizadores, integrações e readiness.
3. Confirmar estado operacional, testes e ligações de evidência.
4. Executar ações de segurança apenas após a confirmação da interface.

## 6. MANUAL TÉCNICO

### 6.1 Preparação e validação

1. Instalar .NET 8 SDK e Docker Desktop.
2. Configurar `.env` a partir de `.env.example`, sem versionar segredos.
3. Executar build e testes em Release.
4. Construir e iniciar a aplicação com Docker Compose.
5. Consultar `/health` e confirmar o estado `Healthy`.
6. Percorrer os fluxos de condutor e administrador.

### 6.2 Evidências e rastreabilidade

| Artefacto | Localização |
| --- | --- |
| Epic Sprint 7 | `OP-85` |
| Tarefas | `OP-86` a `OP-90` |
| Burndown | Jira Board 68 → Reports → Sprint Burndown → S7 |
| Velocity | Jira Board 68 → Reports → Velocity |
| Testes | `OptiDrive.Web.Tests` |
| Pipeline | `.github/workflows/ci.yml` e `.github/workflows/azure-deploy.yml` |
| Diagramas | `docs/reports-oficial/assets/diagrams` |
