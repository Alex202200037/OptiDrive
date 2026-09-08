# OptiDrive - Auditoria Técnica Final

| Campo | Valor |
| --- | --- |
| Sprint | S8 - Auditoria Final |
| Data de execução | 07/09/2026 |
| Responsável | Alexandre Miguel |
| Âmbito | Código, testes, desempenho, segurança, Docker, CI/CD e documentação |
| Resultado | Aprovado para encerramento académico, com condicionantes externas identificadas |

## 1. Quality gates executados

| Verificação | Resultado | Evidência |
| --- | --- | --- |
| Restore .NET | Aprovado | `release-validation.txt` |
| Build Release | Aprovado: 0 erros e 0 avisos | `release-validation.txt` |
| Formatação | Aprovada sem diferenças | `format-verification.txt` e `ci-readiness-final.txt` |
| Suite automatizada | Aprovada: 16/16 | `stress-optimized-validation.txt` |
| Auditoria NuGet | Sem vulnerabilidades conhecidas nas fontes consultadas | `vulnerability-audit.txt` |
| Docker build | Aprovado | `ci-readiness-final.txt` |
| Docker Compose | Contentor `healthy` | `docker-runtime-validation.txt` |
| Healthcheck | HTTP 200 / `Healthy` | `docker-runtime-validation.txt` |
| Persistência | 8 utilizadores, 8 veículos e 2 rotas após reinício | `docker-runtime-validation.txt` e evidência de persistência de 07/09 |
| Artefacto de testes CI | TRX produzido e validado | `ci-readiness-final.txt` |

## 2. Resultado do teste de stress

O cenário cria 500 utilizadores e 500 veículos e constrói 500 dashboards completos. A otimização final eliminou a construção duplicada do diretório social e substituiu pesquisas repetidas por índices locais.

| Métrica | Resultado | Limite | Estado |
| --- | ---: | ---: | --- |
| Criação de 500 utilizadores e 500 veículos | 8,518 s | < 10 s | Aprovado |
| Construção de 500 dashboards | 0,327 s | < 10 s | Aprovado |
| Execução total observada | 8,845 s | < 20 s combinados | Aprovado |

Os valores são próprios do ambiente local de validação e não constituem um SLA de produção.

## 3. Segurança e configuração

- O ficheiro `.env` permanece excluído do controlo de versões.
- Não foram encontrados valores de credenciais nos ficheiros controlados pelo Git.
- O registo de validação do Compose foi sanitizado e contém apenas nomes de configuração com valores `[REDACTED]`.
- A resposta de `/health` não expõe chaves, tokens ou connection strings.
- As credenciais de produção continuam a ser fornecidas por variáveis de ambiente ou App Settings.

## 4. Auditoria documental

- Foram verificados os 25 documentos do índice oficial.
- Não foram encontrados destinos locais inexistentes nas ligações Markdown.
- A matriz UAT, o desenho detalhado, a ata, o relatório Jira e a retrospetiva da Sprint 8 foram reconciliados com a evidência de execução.
- As condicionantes OAuth/Azure permanecem identificadas como dependências externas e não são apresentadas como testes executados.

## 5. Estado Jira observado antes do encerramento

No momento da auditoria, o relatório automático apresenta 7 tarefas concluídas, correspondentes a 33/36 story points, e apenas `OP-99` com 3 pontos por concluir. Não existem itens na tabela de trabalho concluído fora do sprint.

O Burndown contém alterações de âmbito associadas à correção da vinculação de `OP-92`, `OP-93` e `OP-95`. Estes eventos pertencem ao histórico automático do Jira e não alteram o resultado técnico das tarefas. Não serão efetuadas novas movimentações de sprint para evitar distorções adicionais.

## 6. Condicionantes residuais

| Condicionante | Impacto na entrega académica | Tratamento |
| --- | --- | --- |
| OAuth Google/Microsoft depende de callbacks públicos válidos | Não bloqueia login local nem MFA | Configuração por ambiente e UAT condicionado |
| Azure depende de subscrição, App Settings e publish profile | Não bloqueia execução local/Docker | Pipeline preparada e implantação condicionada ao ambiente |
| Histórico do Burndown regista reassociações corretivas | Afeta apenas a representação histórica | Manter relatório automático e explicar com o Sprint Report |

## 7. Decisão de release

Não foram encontrados defeitos bloqueantes no código, build, testes ou Docker. A release ficou tecnicamente preparada para demonstração e para a conclusão de `OP-99`.

## 8. Registo de encerramento

Após a publicação da evidência final, `OP-99` foi transitada para Done e o Sprint 8 foi encerrado antecipadamente em 07/09/2026. O Sprint Report automático confirmou 8/8 itens, 36/36 story points, zero itens incompletos e zero itens concluídos fora do sprint. O histórico automático do Burndown foi preservado sem novos movimentos corretivos.

A execução remota GitHub Actions `34164627808` aprovou build, formatação, auditoria NuGet, 16/16 testes, artefacto TRX, publicação e Docker. A implantação Azure permaneceu condicionada pela ausência de credenciais ativas.
