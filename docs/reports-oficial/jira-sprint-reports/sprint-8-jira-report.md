# OptiDrive - Relatório Jira Sprint 8

| Campo | Valor |
| --- | --- |
| Documento | Sprint Report |
| Sprint | S8 - Auditoria Final |
| Período planeado | 02/09/2026 a 08/09/2026 |
| Ativação no Jira | 03/09/2026 |
| Estado | Encerrado antecipadamente em 07/09/2026 |
| Sprint Jira | `304` |
| Epic Jira | `OP-91` |
| Fonte | Board, Burndown, Sprint Report e Velocity automáticos do Jira |
| Responsável | Alexandre Miguel |

## 1. Objetivo da Sprint

Corrigir e validar `RF-M08-04`, executar a matriz formal de testes de aceitação, reforçar regressão, integração, stress, compatibilidade e segurança, e auditar diagramas, tabelas, documentação, DevOps, Jira, Confluence e pacote final.

## 2. Compromisso inicial

| Indicador | Valor na ativação em 03/09/2026 |
| --- | ---: |
| Itens comprometidos | 8 |
| Story points comprometidos | 36 |
| Itens concluídos | 3 |
| Itens em curso | 1 |
| Itens por iniciar | 4 |

## 3. Backlog da Sprint

| Chave | Resumo | Story points | Estado inicial registado |
| --- | --- | ---: | --- |
| `OP-92` | Auditar requisitos, documentação e rastreabilidade | 3 | Done |
| `OP-93` | Corrigir e validar `RF-M08-04` no backoffice | 5 | Done |
| `OP-94` | Criar e executar a matriz formal de testes de aceitação | 5 | In Progress |
| `OP-95` | Reforçar testes automatizados, integração e regressão | 5 | Done |
| `OP-96` | Executar stress, compatibilidade, segurança e Docker | 5 | To Do |
| `OP-97` | Auditar e corrigir diagramas, gráficos e tabelas | 5 | To Do |
| `OP-98` | Rever documentação, DevOps e pacote Confluence | 5 | To Do |
| `OP-99` | Auditoria final, demonstração e encerramento | 3 | To Do |

## 4. Relatórios automáticos

### Estado em 04/09/2026

| Indicador | Valor |
| --- | ---: |
| Itens concluídos | 4 |
| Itens em curso | 0 |
| Itens por iniciar | 4 |
| Story points concluídos | 18 / 36 |

O item `OP-94` foi concluído após a atualização da matriz UAT, execução do smoke funcional e publicação das evidências de 04/09/2026.

### Estado antes do encerramento em 07/09/2026

| Indicador | Valor |
| --- | ---: |
| Itens concluídos | 7 |
| Itens em curso | 0 |
| Itens por iniciar | 1 |
| Story points concluídos | 33 / 36 |
| Story points restantes no Sprint Report | 3 / 36 |
| Alterações de âmbito registadas | 12 eventos corretivos, saldo líquido 0 SP |
| Itens concluídos fora da sprint | 0 |

Os itens `OP-97` e `OP-98` foram concluídos após a revisão dos diagramas, documentação, DevOps, Docker, interface móvel e pacote Confluence. Este instantâneo foi recolhido antes da transição final de `OP-99` e é mantido apenas como registo histórico da execução.

O log de âmbito contém duas sequências de remoção e reposição de `OP-92`, `OP-93` e `OP-95`, totalizando seis eventos negativos e seis positivos. O saldo do âmbito é zero, mas o Burndown preserva os movimentos históricos e apresenta uma descontinuidade que não corresponde a trabalho reaberto. Não foram realizados novos movimentos para alterar retroativamente o gráfico.

### Estado final de encerramento em 07/09/2026

| Indicador | Valor final automático |
| --- | ---: |
| Itens concluídos | 8 / 8 |
| Story points concluídos | 36 / 36 |
| Itens incompletos | 0 |
| Itens concluídos fora do sprint | 0 |
| Alterações de âmbito | 12 eventos corretivos, saldo líquido 0 SP |
| Estado do sprint | Encerrado |

`OP-99` foi transitada para Done após a publicação das evidências finais. O Sprint Report automático passou a confirmar todos os oito itens como concluídos dentro do Sprint 8, sem trabalho incompleto e sem itens concluídos fora do sprint.

### Ligações do Jira

- [Burndown automático da Sprint 8](https://estudantes-team-jwsj3xh7.atlassian.net/jira/software/projects/OP/boards/68/reports/burndown?source=overview&sprint=304)
- [Sprint Report automático da Sprint 8](https://estudantes-team-jwsj3xh7.atlassian.net/jira/software/projects/OP/boards/68/reports/sprint-retrospective?source=overview&sprint=304)
- [Velocity automática](https://estudantes-team-jwsj3xh7.atlassian.net/jira/software/projects/OP/boards/68/reports/velocity?source=overview)

Os valores finais foram confirmados no Sprint Report após o encerramento antecipado em 07/09/2026. O relatório distingue os dados automáticos do Jira da interpretação da anomalia histórica do Burndown.

## 5. Critérios de encerramento

- `RF-M08-04` validado por automação e smoke visual.
- Matriz UAT atualizada com resultado e evidência por caso.
- Build e 16 testes automatizados Release sem falhas.
- Stress, Docker, `/health`, compatibilidade e segurança documentados.
- Diagramas, gráficos e tabelas legíveis e coerentes.
- Jira, Confluence, Git, DevOps e documentos finais reconciliados.
- Ata, Desenho Detalhado, Sprint Report e Retrospective atualizados e publicados.
- Execução GitHub Actions CI `34164627808` aprovada.
