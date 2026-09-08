# OptiDrive - Sprint Retrospective 8

| Campo | Valor |
| --- | --- |
| Sprint | Sprint 8 - Auditoria Final, Aceitação e Entrega |
| Período | 02/09/2026 a 08/09/2026 |
| Data | 07/09/2026 (execução antecipada) |
| Estado | Concluída e encerrada antecipadamente em 07/09/2026 |

## O que correu bem

- `RF-M08-04` foi transformado numa área funcional e acionável, com fontes de pipeline, testes, stress, health, Jira e Confluence.
- A matriz UAT passou a concentrar 20 cenários com requisito, resultado, estado e evidência.
- O build Release terminou com zero erros e zero avisos e os 16 testes automatizados foram aprovados.
- O cenário de stress 500+500 ficou abaixo dos limites: 8,518 s para criação e 0,327 s para 500 dashboards.
- Docker Compose, persistência e `/health` foram validados em ambiente reproduzível.
- A pipeline passou a verificar formatação, vulnerabilidades, testes TRX, publicação e imagem Docker.
- A auditoria documental verificou 25 documentos oficiais e não encontrou ligações Markdown locais inexistentes.

## Dificuldades

- A execução individual concentrou desenvolvimento, QA, DevOps, Jira e documentação na mesma pessoa.
- OAuth e Azure dependem de credenciais, callbacks e subscrição externos ao código.
- A correção da associação de três tarefas produziu eventos permanentes de alteração de âmbito no Burndown automático, apesar de o Sprint Report reconhecer as tarefas concluídas.
- O teste de stress revelou trabalho duplicado na construção do dashboard social, corrigido durante a auditoria final.

## Aprendizagens

- O backlog deve estar estimado e associado antes da ativação do sprint.
- Uma tarefa concluída não deve ser retirada e recolocada num sprint ativo, porque o Jira preserva cada evento no relatório.
- Os testes de carga devem apresentar tempos medidos, limites e contexto do ambiente.
- A documentação só deve declarar um resultado depois de existir evidência reproduzível.
- A preservação dos templates deve ser verificada em paralelo com a qualidade do conteúdo.

## Ações

| Ação | Resultado / destino |
| --- | --- |
| Otimizar construção do dashboard social | Concluído em 07/09/2026 |
| Repetir build, formatação, vulnerabilidades, testes e Docker | Concluído com evidência final |
| Atualizar UAT, Desenho Detalhado, DevOps, Encerramento e relatório Jira | Concluído e publicado |
| Não voltar a movimentar tarefas concluídas entre sprints | Regra de gestão adotada |
| Manter OAuth/Azure como condicionantes externas explícitas | Roadmap operacional |
| Encerrar administrativamente o Sprint 8 após conclusão integral | Concluído em 07/09/2026 |
