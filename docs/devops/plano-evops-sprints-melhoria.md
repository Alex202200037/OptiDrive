# OptiDrive - Plano EVOPS e Sprints de Melhoria

## Objetivo

Este plano transforma o feedback do docente em trabalho verificavel no Jira, na aplicacao e na documentacao. A regra operacional e simples: primeiro criar backlog, depois abrir a sprint, executar trabalho, mover issues, comentar graficos e fechar a sprint.

## Sprint 6 - Melhorias de Codigo, UI e Produto

Periodo recomendado: 31/08/2026 a 06/09/2026.

Foco: aplicacao a funcionar, mais bonita, mais estavel e alinhada com DevOps.

| ID | Tarefa | Evidencia esperada |
| --- | --- | --- |
| S6.1 | Corrigir readiness tecnico e endpoint `/health` | Endpoint responde e pode ser usado em Azure/App Service |
| S6.2 | Reforcar comportamento do backoffice administrativo | Admin tem comportamento diferente de utilizador normal |
| S6.3 | Melhorar apresentacao visual da landing page | Primeira pagina explica melhor o valor do produto |
| S6.4 | Alinhar a app com processo DevOps no backoffice | Backoffice mostra pipeline, testes, stress e roadmap |
| S6.5 | Validar build, testes e pacote de publicacao | Build/testes passam e pacote Azure fica gerado |

## Sprint 7 - Reforma Documental, Diagramas e Graficos

Periodo recomendado: 07/09/2026 a 13/09/2026.

Foco: relatorios, diagramas, charts e Confluence.

| ID | Tarefa | Evidencia esperada |
| --- | --- | --- |
| S7.1 | Rever diagramas UML e BPMN | Diagramas legiveis, completos e alinhados com slides |
| S7.2 | Reformular AER | Template completo, requisitos, use cases e matrizes corretas |
| S7.3 | Rever desenhos detalhados e testes | Cada sprint com testes e desenho coerente |
| S7.4 | Gerar Burndown e Velocity Chart | Charts do Jira comentados nos relatorios |
| S7.5 | Atualizar retrospectivas, PM, DevOps e encerramento | Documentos finais consistentes no Confluence |

## Fio-condutor para video EVOPS

Tempo maximo: 5 minutos.

1. Requisito: mostrar a necessidade do utilizador, por exemplo planear uma viagem sem ficar sem combustivel/carga.
2. Diagrama: mostrar UML/BPMN correspondente ao fluxo de planeamento e Smart Save.
3. Prototipo: mostrar wireframe/mockup ou ecra inicial planeado.
4. Implementacao: abrir a aplicacao e executar o fluxo real.
5. Testes: mostrar resultado de testes automatizados, healthcheck e stress local.

## Cenas recomendadas para videos separados

| Video | Duracao | Conteudo |
| --- | --- | --- |
| Video 1 | 45-60s | Problema e requisitos: reserva de combustivel, viagem em grupo, custos incertos |
| Video 2 | 60-90s | Diagramas para prototipo: BPMN/use cases/classes e interface prevista |
| Video 3 | 90-120s | App a funcionar: login, garagem, rota, Smart Save, social e admin |
| Video 4 | 30-45s | Testes e DevOps: `/health`, `dotnet test`, pipeline, Azure |

## Como comentar os charts no Jira

Burndown Chart: comentar se o trabalho desceu de forma gradual ou se houve blocos concentrados no fim. Se houver quedas grandes, justificar com execucao individual e consolidacao de tarefas semelhantes.

Velocity Chart: comparar a capacidade entregue por sprint. A Sprint 6 deve evidenciar melhorias tecnicas e de UI; a Sprint 7 deve evidenciar carga documental e graficos. Se a velocity variar, justificar pela natureza diferente das tarefas.

## Escopo MoSCoW adicional

CarPlay e Android Auto devem ficar como Won't Have nesta entrega. Sao integracoes plausiveis para futuro, mas exigem SDKs, certificacao, politicas de plataforma e testes em veiculo, ficando fora do ambito academico atual.
