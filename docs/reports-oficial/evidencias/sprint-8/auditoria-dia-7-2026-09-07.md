# OptiDrive - Evidência de Auditoria do Dia 7

| Campo | Valor |
| --- | --- |
| Sprint | S8 - Auditoria Final |
| Data | 07/09/2026 |
| Responsável | Alexandre Miguel |
| Objetivo | Validar DevOps, execução reproduzível, segurança, compatibilidade e coerência documental antes do encerramento |

## Resultado executivo

| Area | Resultado | Evidencia |
| --- | --- | --- |
| Build Release | Aprovado, 0 erros e 0 avisos | `build-release-2026-09-07.txt` |
| Testes automatizados | Aprovado, 16/16 | `testes-release-2026-09-07.txt` |
| Docker Compose | Configuração válida e contentor `healthy` | `docker-compose-config-2026-09-07.yml` |
| Healthcheck | HTTP 200, estado `Healthy` | `healthcheck-2026-09-07.json` |
| Persistência | 8 utilizadores, 8 veículos e 2 rotas mantidos após reinício | `docker-persistence-health-2026-09-07.txt` |
| Stress | Aprovado para 500 utilizadores e 500 veículos | Suite Release e teste `StressTests` |
| Dependências | Sem vulnerabilidades NuGet conhecidas nas fontes consultadas | `package-vulnerability-scan-2026-09-07.txt` |
| Interface móvel | Sem overflow em Perfil, Garagem, Planeamento e Social a 390x844 | `mobile-light-pt-2026-09-07.png` e `mobile-dark-en-2026-09-07.png` |
| Idioma e tema | PT/claro e EN/escuro aprovados; traduções residuais corrigidas | Smoke visual de 07/09 |
| Documentação local | Sem placeholders proibidos, ficheiros vazios, imagens ou ligações relativas em falta | Auditoria automatizada de 07/09 |
| Jira Sprint 8 | 6 itens Done, 1 In Progress, 1 To Do; 28/36 SP concluídos; sem alterações de âmbito | Relatórios automáticos do Jira |

## Validações funcionais complementares

- O perfil autenticado abriu corretamente em viewport móvel.
- Garagem, Planeamento e Social abriram sem deslocamento horizontal.
- O healthcheck automático do Docker Compose foi adicionado e validado.
- Os dados persistiram depois do reinício do contentor.
- A homepage deixou de apresentar os blocos identificados em português quando o idioma selecionado é inglês.

## Pendências controladas

| Pendencia | Impacto | Acao necessaria |
| --- | --- | --- |
| Primeira execução remota do GitHub Actions | A pipeline está definida e reproduzida localmente, mas ainda não existe run remoto | Reconciliar `main` local com `origin/main` sem perder os diagramas e efetuar `push` |
| Azure App Service | O domínio anteriormente utilizado deixou de resolver e a CLI não tem sessão ativa | Reativar/criar o recurso e configurar App Settings após autenticação Azure |
| OAuth Google/Microsoft em Azure | Depende do URL público final | Atualizar callbacks depois da reativação do App Service |

## Conclusão

O pacote local encontra-se tecnicamente estável e reproduzível em Docker. O trabalho de 07/09 fica demonstrado por evidências objetivas e não antecipa resultados externos: GitHub Actions, Azure e OAuth permanecem explicitamente condicionados até serem executados no respetivo ambiente.
