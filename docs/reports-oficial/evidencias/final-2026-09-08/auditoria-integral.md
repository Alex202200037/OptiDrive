# OptiDrive - Auditoria Integral de Entrega

| Campo | Valor |
| --- | --- |
| Data | 08/09/2026 |
| Âmbito | Código, testes, segurança, Docker, usabilidade, diagramas, Jira e documentação |
| Repositório | OptiDrive |
| Estado técnico | Aprovado para entrega local |
| Estado Confluence | Publicado e auditado |

## 1. Resumo Executivo

A solução foi submetida a uma passagem integral antes da entrega. O código compila sem erros ou avisos, os 16 testes automatizados passam, o cenário de stress com 500 utilizadores e 500 veículos cumpre o limite definido, o Docker mantém estado saudável e os dados persistem após reinício. A interface foi verificada nos percursos críticos, em desktop e mobile, nos modos claro e escuro e em português e inglês.

Foram também revistos 25 documentos oficiais e 13 diagramas únicos, utilizados em 33 posições documentais. Não foram encontrados ficheiros de imagem vazios, ligações locais quebradas, marcas de água, marcadores editoriais proibidos ou relações gráficas sem destino.

## 2. Resultados Técnicos

| Verificação | Resultado | Evidência |
| --- | --- | --- |
| Formatação | Aprovada; nenhuma alteração necessária | `01-format-verification.txt` |
| Build Release | Aprovada; 0 erros e 0 avisos | `02-release-build.txt` |
| Testes automatizados | 16/16 aprovados; 0 falhas; 0 ignorados | `03-automated-tests.txt` e `test-results/optidrive-final-tests.trx` |
| Stress 500 + 500 | 7,998 s para criação; 0,405 s para dashboards; 8,402 s total | `04-stress-500x500.txt` |
| Dependências NuGet | Nenhum pacote vulnerável nas fontes consultadas | `05-nuget-vulnerabilities.txt` |
| Docker Compose | Contentor em estado `healthy` | `06-docker-compose-build-start.txt` e `07-docker-health-smoke.txt` |
| Healthcheck | HTTP 200 e estado `Healthy` | `07-docker-health-smoke.txt` |
| Persistência | 8 utilizadores, 8 veículos e 2 rotas antes e depois do reinício | `08-health-before-restart.json`, `09-health-after-restart.json` e `10-persistence-comparison.txt` |
| Qualidade do código | Sem `TODO`, `FIXME`, `HACK` ou `XXX` no código próprio | `23-source-marker-audit.txt` |
| CI | GitHub Actions `34206950037` aprovada | Registo do workflow |
| Azure | Workflow `34207149170` aprovado; deploy omitido por ausência de credenciais ativas | Registo do workflow |

## 3. Validação de Usabilidade

Foram validados os percursos de visitante, condutor e administrador. A passagem incluiu landing page, idioma, autenticação, perfil, garagem, planeamento/Smart Save, área social, modo escuro, backoffice e evidências de `RF-M08-04`.

| Cobertura | Resultado |
| --- | --- |
| Desktop | Aprovado a 1280 × 720 |
| Mobile | Aprovado a 390 × 844 |
| Idiomas | Português e inglês aprovados |
| Temas | Claro e escuro aprovados |
| Perfis | Visitante, condutor e administrador aprovados |
| Ações destrutivas | Não executadas durante o smoke test |

A avaliação externa de usabilidade e NPS permanece separada desta validação técnica e documentada no relatório próprio.

## 4. Auditoria de Diagramas

| Grupo | Artefactos | Resultado |
| --- | --- | --- |
| Arquitetura UML | Arquitetura geral, componentes, deployment e pacotes | Relações claras e componentes identificados |
| Modelo UML | Classes de domínio e casos de utilização | Atores, casos e dependências legíveis |
| BPMN | Planeamento, garagem, social e DevOps | Início/fim alcançáveis e decisões coerentes |
| Gestão | Gantt, Burndown global documentado e Velocity documentada | Oito sprints representados e proveniência assinalada |

Os diagramas foram inspecionados à resolução original. Não apresentam marcas de água. O Gantt cobre o período até 08/09/2026; a Velocity distingue as estimativas documentadas de S1–S5 dos resultados Jira de S6–S8; o Burndown global é explicitamente identificado como consolidação documental e não substitui os relatórios automáticos do Jira.

## 5. Auditoria Documental

| Verificação | Resultado |
| --- | --- |
| Documentos oficiais locais | 25 |
| Ligações locais verificadas | 33 |
| Ligações inexistentes | 0 |
| Colocações de diagramas | 33 |
| Diagramas únicos | 13 |
| Marcadores editoriais proibidos | 0 |

Os Desenhos Detalhados 6, 7 e 8 possuem a estrutura completa de testes do template, incluindo especificação do caso, procedimento e resultado. O Desenho Detalhado 8 foi atualizado para o estado final, com revalidação técnica de 08/09/2026 e explicação do Burndown automático.

## 6. Estado do Sprint 8 no Jira

O Sprint Report confirma 8 de 8 itens e 36 de 36 story points concluídos, sem trabalho incompleto e sem itens concluídos fora da sprint. A auditoria atual foi estritamente de leitura e não alterou os Sprints 1–5 nem o histórico do Sprint 8.

As provas de leitura estão guardadas em `27-jira-sprint8-burndown.png` e `28-jira-sprint8-report.txt`. A prova visual atualizada do requisito administrativo encontra-se em `29-backoffice-rf-m08-04.png`.

Durante uma correção anterior da associação ao Sprint 8, `OP-92`, `OP-93` e `OP-95` foram retiradas inadvertidamente e reinseridas duas vezes. O Jira conserva seis remoções e seis adições, num saldo líquido de 0 story points, e não recalcula retroativamente a linha vermelha. Assim, a descontinuidade visual representa o histórico de âmbito preservado pela ferramenta, e não trabalho reaberto. O resultado final deve ser lido em conjunto com o Sprint Report: 8/8 itens, 36/36 story points, 0 incompletos e 0 concluídos fora da sprint.

Nota operacional: a página de Burndown pode conservar a última sprint selecionada na sessão. Para consultar a prova correta, deve confirmar-se `S8 - Auditoria Final` no seletor Sprint.

## 7. Sincronização e Auditoria Final no Confluence

As atualizações foram publicadas e seguidas por uma auditoria de leitura ao corpo final de 36 páginas. Não foram identificadas páginas vazias, marcadores de diagramas por substituir, métricas finais desatualizadas ou afirmações incorretas de disponibilidade pública no Azure.

| Verificação | Resultado |
| --- | --- |
| Páginas finais auditadas | 36 |
| Páginas vazias | 0 |
| Páginas por sincronizar | 0 |
| Documentos adicionados à estrutura final | Desenho Detalhado Sprint 7, Atas 6 e 7 e Sprint Retrospective 6 |
| Documentos de Sprint | Desenhos Detalhados 1–8 e Atas 1–8 acessíveis a partir da página principal |
| Relatórios Jira | Sprints 1–8 acessíveis a partir da página principal |
| Retrospectives | Sprints 6–8 acessíveis a partir da página principal |
| Conteúdo final atualizado | AER, DAN, Testes e Métricas, DevOps, Encerramento, UAT, DD6–DD8 e Relatório Jira S8 |
| Gráficos nas páginas críticas | 15 colocações verificadas em 7 páginas; todas após a respetiva legenda e fora de tabelas |
| Dashboard | Estado Azure corrigido para condicionado; sem indicação falsa de serviço online |
| Página principal | Índice, ligações, estado técnico, Sprint 8 e limitações finais atualizados |

A auditoria incidiu no conteúdo publicado e nos anexos que constituem a documentação oficial. O histórico de comentários do Confluence foi preservado como registo de colaboração e não foi usado como fonte do estado final.

## 8. Limitações e Riscos Residuais

- O serviço Azure não está publicamente disponível sem credenciais válidas; o funcionamento foi provado localmente e em Docker.
- Os logins OAuth externos dependem de credenciais e configuração de produção; a autenticação local e MFA foram testadas.
- A validação visual foi um smoke test técnico; não substitui uma campanha externa adicional de usabilidade.
- Os Sprints 1–5 não foram reconstruídos retroativamente no Jira e permanecem intocados, preservando a integridade histórica solicitada.
- O Burndown automático do Sprint 8 preserva eventos de âmbito e não deve ser manipulado para imitar artificialmente a guideline.

## 9. Conclusão

O OptiDrive encontra-se tecnicamente validado e documentalmente preparado para entrega, com provas reproduzíveis de build, testes, stress, segurança, Docker, persistência e usabilidade visual. A documentação final está publicada e auditada no Confluence, e o histórico do Jira foi preservado. As limitações residuais restringem-se a integrações externas dependentes de credenciais, sem comprometer a execução local ou em Docker.
