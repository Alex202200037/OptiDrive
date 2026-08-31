# OptiDrive - Operacao Jira das Sprints 6 e 7

## Estado executado no Jira - Sprint 6

| Campo | Resultado |
| --- | --- |
| Projeto / board | `OP` / `68` |
| Epic | `OP-79` - Sprint 6 - Melhorias de Codigo, UI e Produto |
| Sprint Jira | `269` - S6 - Melhorias de Codigo e UI |
| Estado | Fechada |
| Tarefas | `OP-80` a `OP-84` |
| Estimativa | 28 story points |
| Conclusao | 28 story points / 5 de 5 tarefas |
| Responsavel | Alexandre Miguel |

Relatorios gerados pelo fecho real do sprint:

- Burndown: https://estudantes-team-jwsj3xh7.atlassian.net/jira/software/projects/OP/boards/68/reports/burndown?source=overview
- Velocity: https://estudantes-team-jwsj3xh7.atlassian.net/jira/software/projects/OP/boards/68/reports/velocity
- Backlog: https://estudantes-team-jwsj3xh7.atlassian.net/jira/software/projects/OP/boards/68/backlog

As cinco tarefas foram criadas primeiro no backlog, estimadas, associadas ao Epic, colocadas no sprint e percorreram os estados `To Do`, `In Progress` e `Done` antes do fecho.

## Regra pedida pelo docente

A sprint deve existir no Jira antes do trabalho ser executado. O fluxo correto e:

1. Criar backlog com as tarefas.
2. Criar a sprint no backlog.
3. Arrastar as tarefas para a sprint.
4. Abrir/iniciar a sprint.
5. Mover tarefas entre estados durante o trabalho.
6. Fechar a sprint.
7. Consultar e comentar Burndown Chart e Velocity Chart.

## Sprint 6 - App, codigo e apresentacao

Nome recomendado: `Sprint 6 - Melhorias de Codigo, UI e Produto`.

Duracao configurada: uma semana.

Objetivo da sprint: melhorar a aplicacao para ficar mais apresentavel, robusta e alinhada com DevOps.

Tarefas a colocar na sprint:

| Issue | Estado final esperado | Comentario para fecho |
| --- | --- | --- |
| S6.1 - Corrigir readiness tecnico e endpoint de health | Done | `/health` validado localmente e preparado para Azure |
| S6.2 - Reforcar comportamento do backoffice administrativo | Done | Admin deixa de entrar em fluxos de condutor que nao se aplicam |
| S6.3 - Melhorar apresentacao visual da landing page | Done | Landing page apresenta melhor garagem, planeamento e social |
| S6.4 - Alinhar a app com processo DevOps visivel no backoffice | Done | Backoffice mostra pipeline, testes, stress e roadmap MoSCoW |
| S6.5 - Validar build, testes e pacote de publicacao | Done | Build Release OK, 12 testes OK e imagem Docker validada por healthcheck |

## Estado executado no Jira - Sprint 7

| Campo | Resultado |
| --- | --- |
| Projeto / board | `OP` / `68` |
| Epic | `OP-85` - Sprint 7 - Polimento e Estabilizacao Final |
| Sprint Jira | `270` - S7 - Polimento e Estabilidade |
| Periodo | 29/08/2026 a 31/08/2026 |
| Estado | Fechada em 31/08/2026 |
| Tarefas | `OP-86` a `OP-90` |
| Estimativa | 28 story points |
| Estado inicial | 1 tarefa em progresso e 4 tarefas por iniciar |
| Estado final | 5/5 tarefas concluídas; 28/28 story points |
| Responsavel | Alexandre Miguel |

Objetivo da sprint: polir a experiencia da aplicacao, robustecer garagem e planeamento e validar autenticacao, regressao, Docker e readiness de release.

| Issue | Story points | Estado em 30/08/2026 | Resultado esperado |
| --- | ---: | --- | --- |
| `OP-86` - Auditar responsividade, acessibilidade e consistencia visual | 4 | Done | Interface coerente e utilizavel em desktop e mobile |
| `OP-87` - Polir fluxos da garagem e feedback de operacoes | 5 | Done | Operacoes da garagem claras e sem saidas inesperadas da pagina |
| `OP-89` - Robustecer planeamento, rotas e Smart Save | 6 | Validado em 31/08 | Validação de entradas, mensagens seguras e fallback Leaflet sem dependência obrigatória da API JavaScript Google |
| `OP-88` - Validar autenticacao, perfis e navegacao administrativa | 5 | Validado em 31/08 | Login, MFA, perfil, ações administrativas e navegação por papel validados |
| `OP-90` - Executar regressao, Docker e readiness de release | 8 | Validado em 31/08 | Build/publish Release, 12 testes, Docker build e healthcheck `Healthy` validados |

O Sprint 7 foi criado e iniciado antes da execucao das tarefas. Em 30/08 foram concluídos e documentados no Jira os itens `OP-86` e `OP-87`, reduzindo o trabalho remanescente de 28 para 19 story points. A validação incluiu viewport móvel de 390 px, desktop de 1280 px, ausência de overflow horizontal e erros de consola, build sem avisos/erros e 12/12 testes automáticos aprovados.

Em 31/08 foram concluídas as validações técnicas dos itens `OP-89`, `OP-88` e `OP-90`. O planeamento passou a rejeitar origem/destino inválidos e iguais, a não expor detalhes de exceções e a usar Leaflet quando a API JavaScript Google não está explicitamente ativada. A navegação administrativa foi alinhada por papel. A readiness final obteve build e publish Release sem erros, 12/12 testes aprovados, `docker compose config` válido, imagem Docker construída e `/health` com estado `Healthy` em Development e Docker. A sprint foi encerrada no Jira em 31/08/2026 com 5/5 itens e 28/28 story points concluídos; o Burndown confirmou zero trabalho incompleto e o Velocity registou compromisso 28 e conclusão 28.

## Sprint 8 planeada - Documentacao, diagramas e graficos

Periodo previsto: semana seguinte ao fecho do Sprint 7.

Objetivo: reformular relatorios, rever diagramas/figuras, comentar os charts do Jira e preparar o Confluence final.

Backlog previsto:

| Tarefa | Resultado esperado |
| --- | --- |
| Rever todos os diagramas UML e BPMN | Diagramas alinhados com Visual Paradigm/BPMN e slides |
| Reformular AER com indice, tabelas e rastreabilidade | AER validada contra template original |
| Rever desenhos detalhados e testes por sprint | Cada sprint inclui desenho e testes adequados |
| Gerar e comentar Burndown e Velocity Chart | Charts do Jira exportados e comentados |
| Atualizar retrospectivas, PM, DevOps e encerramento | Relatorios finais consistentes no Confluence |

## Comentario sugerido para Burndown da Sprint 6

O burndown evidencia uma sprint curta de melhoria tecnica e visual. A descida do trabalho concentra-se nos pontos de maior impacto: healthcheck, backoffice, landing page, pipeline e validacao. Como o projeto foi executado essencialmente por um elemento, algumas tarefas foram fechadas em blocos apos validacao conjunta de build e testes.

## Comentario sugerido para Velocity da Sprint 6

A velocity da Sprint 6 representa trabalho de hardening e apresentacao, com menor volume funcional novo mas alto valor de qualidade. A entrega inclui melhorias visiveis na interface, robustez operacional e evidencias tecnicas para DevOps.

## Retrospectiva da Sprint 6

### Correu bem

- O objetivo tecnico e visual foi cumprido: 5 de 5 tarefas e 28 de 28 story points.
- O endpoint `/health`, o backoffice, a landing page, a pipeline e o pacote Azure ficaram validados.
- A rastreabilidade entre Epic, tarefas, estados e relatorios Jira ficou operacional.

### Dificuldades

- A formalizacao do trabalho no Jira ocorreu concentrada numa sprint de hardening.
- O trabalho foi executado por um unico elemento, acumulando desenvolvimento, testes, documentacao e gestao.
- A indisponibilidade do recurso Azure anterior obrigou a preparar novamente o processo de publicacao.

### Acoes de melhoria

- Criar e estimar todas as tarefas antes do inicio da Sprint 7.
- Atualizar o Jira durante a execucao, evitando transicoes concentradas no fecho.
- Rever os diagramas e relatorios por lotes pequenos, validando cada template antes da publicacao.
- Registar evidencias de pipeline, testes e deploy logo apos cada validacao.

## Comentario sugerido para Burndown da Sprint 7

O burndown inicia com 28 story points distribuídos por cinco tarefas de polimento e estabilizacao. A primeira tarefa entrou em progresso no dia de abertura, mantendo o trabalho remanescente enquanto decorre a validacao. A descida deve ocorrer por incrementos concluídos e verificados, evitando encerrar tarefas apenas para alterar o grafico.

## Comentario sugerido para Velocity da Sprint 7

A velocity final so sera consolidada quando a sprint for fechada. A comparacao entre os 28 story points comprometidos e os efetivamente concluídos mede a capacidade real de executar hardening visual, funcional e operacional num ciclo curto.
