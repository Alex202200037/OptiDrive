# OptiDrive - Tabela de Testes de Aceitação

| Campo | Valor |
| --- | --- |
| Documento | Matriz de Testes de Aceitação (UAT) |
| Projeto | OptiDrive |
| Versão | 1.2 |
| Data | 07/09/2026 |
| Responsável | Alexandre Miguel |

## Critério de classificação

- **Aprovado:** resultado observado e evidência disponível.
- **Reprovado:** não cumpre o critério e origina correção/regressão.
- **Em validação:** parte do critério foi aprovada, mantendo-se uma verificação complementar pendente.
- **A executar:** planeado para Sprint 8, sem resultado antecipado.
- **Condicionado:** depende de credenciais ou serviço externo.

## Matriz de aceitação

| ID | Requisito/Área | Cenário | Resultado esperado | Estado 07/09 | Evidência |
| --- | --- | --- | --- | --- | --- |
| UAT-001 | RF-M01-01 Login | Autenticar conta local válida | Sessão e dashboard corretos | Aprovado | `Register_StoresHashedPasswordAndValidatesLogin` |
| UAT-002 | RF-M01-05 MFA | Introduzir TOTP válido | Segundo fator aceite; inválido bloqueado | Aprovado | `Authenticator_CanBeEnabledAndVerifiedWithTotp` |
| UAT-003 | OAuth Google | Concluir consentimento | Retorno sem `redirect_uri_mismatch` | Condicionado | Ambiente Azure Sprint 8 |
| UAT-004 | OAuth Microsoft | Concluir consentimento | Retorno com sessão | Condicionado | Ambiente Azure Sprint 8 |
| UAT-005 | RF-M02-01 Garagem | Criar/editar veículo | Associação e persistência | Aprovado | `AddVehicle_ForNewUser_CreatesDefaultVehicleAndHistoryEntry` |
| UAT-006 | RF-M02-03 Abastecimento | Registar abastecimento | Nível, custo e histórico atualizados | Aprovado | `RefuelVehicle_UpdatesLevelCostAndGarageHistory` |
| UAT-007 | RF-M03-01 Rota | Introduzir origem/destino | Distância, duração e custos | Aprovado | Smoke funcional `Setúbal, Portugal` → `Évora, Portugal`, 90 km/h, em 04/09 |
| UAT-008 | RF-M05 Smart Save | Rota com autonomia limitada | Reforço e reserva de chegada | Aprovado | `SmartSaveServiceTests` |
| UAT-009 | Confiabilidade mapa | Indisponibilizar Google browser | Fallback utilizável | Aprovado | Rota calculada com indicador `Mapa: Fallback Leaflet`, sem erro, em 04/09 |
| UAT-010 | RF-M06 Mensagens | Enviar mensagem direta | Conversa registada | Aprovado | `SendDirectMessage_AddsConversationToSocialDashboard` |
| UAT-011 | RF-M06 Viagem social | Criar, aderir e iniciar | Participantes e estado persistem | Aprovado | `CollaborativeTrip_CanBeCreatedJoinedAndStarted` |
| UAT-012 | RF-M07 Autorização | Tentar ação admin com dois papéis | Só admin altera segurança | Aprovado | `AdminActions_ManageUserSecurityWithoutAllowingRegularUsers` |
| UAT-013 | RF-M08-04 Evidências | Abrir seis cartões do backoffice | Fontes corretas e sem segredos | Aprovado | `ProjectEvidenceServiceTests` + smoke visual local de 04/09 |
| UAT-014 | RF-M08-01 Health | Consultar `/health` | 200, Healthy e sem segredos | Aprovado | `HealthControllerTests` + `healthcheck-2026-09-04.txt` |
| UAT-015 | UI | Alternar tema/idioma em desktop/mobile | Sem perda de navegação | Aprovado | Smoke mobile 390×844 em PT/claro e EN/escuro; tradução residual da homepage corrigida em 07/09 |
| UAT-016 | Persistência | Reiniciar aplicação/contentor | Dados mantidos | Aprovado | `docker-persistence-health-2026-09-07.txt`: 8 utilizadores, 8 veículos e 2 rotas antes/depois do reinício |
| UAT-017 | CI | Executar workflow | Build, testes, publish e Docker | A executar | Workflows validados localmente; execução remota pendente de reconciliação e push da branch |
| UAT-018 | Docker | Build e arranque Compose | Contentor e health operacionais | Aprovado | Compose reconstruído; contentor `healthy`, `/health` HTTP 200 e healthcheck automático ativo em 07/09 |
| UAT-019 | Stress | 500 users + 500 veículos | Operações < 10 s, sem exceções | Aprovado | 8,518 s para criação e 0,327 s para 500 dashboards após otimização em 07/09 |
| UAT-020 | Documentação | Cruzar requisitos/issues/testes/páginas | Sem vazios ou contradições | Aprovado | Auditoria final: 25 documentos verificados e 0 destinos Markdown locais inexistentes |

## Resultado atual

| Indicador | Valor |
| --- | ---: |
| Casos definidos | 20 |
| Aprovados com evidência | 17 |
| Em validação | 0 |
| Condicionados | 2 |
| A executar | 1 |
| Reprovados | 0 |

A matriz será atualizada durante a Sprint 8. Só muda para **Aprovado** depois da execução e identificação da evidência.

## Execução de aceitação de 07/09/2026

- Build Release concluído com zero erros e zero avisos; suite Release com `16/16` testes aprovados.
- Docker Compose reconstruído e iniciado com o novo healthcheck automático; contentor confirmado como `healthy`.
- `/health` devolveu HTTP 200 antes e depois do reinício do contentor.
- Persistência confirmada: mantiveram-se 8 utilizadores, 8 veículos e 2 rotas após o reinício.
- Homepage validada em viewport móvel `390×844`, nos modos PT/claro e EN/escuro, sem perda de navegação.
- Foram corrigidas traduções residuais da homepage e normalizada a tradução de blocos com quebras de linha.
- Workflows CI e Azure Deploy revistos e reforçados com gates de formatação, dependências, testes TRX, publicação e Docker; a execução local equivalente foi aprovada.
- Teste 500+500 otimizado e repetido: criação em 8,518 s, 500 dashboards em 0,327 s e total em 8,845 s.
- Auditoria documental concluída: 25 documentos oficiais inspecionados e zero destinos Markdown locais inexistentes.

## Execução de aceitação de 04/09/2026

- Autenticação administrativa e acesso ao Backoffice aprovados.
- Os seis cartões de evidência de `RF-M08-04` apresentaram estado, detalhe e ligação acionável.
- Login do condutor `alexandre@optidrive.pt` aprovado; perfil apresentou dois veículos e seis rotas antes do ensaio.
- A rota `Setúbal, Portugal` → `Évora, Portugal`, a 90 km/h, foi calculada e guardada sem erro.
- O mesmo fluxo funcionou com o mapa em `Fallback Leaflet`, validando a degradação controlada sem Google Maps no browser.
- Alternância entre modo claro/escuro e PT/EN aprovada em desktop sem perda de navegação.
- `/health` devolveu HTTP 200 e estado `Healthy`, sem credenciais ou segredos na resposta.
- Build Release concluído com zero erros e zero avisos; suite Release com `16/16` testes aprovados.

## Evidências técnicas

- Build Release: `evidencias/sprint-8/build-release-2026-09-02.txt`.
- Suite xUnit Release: `evidencias/sprint-8/testes-release-2026-09-02.txt`.
- Validação da configuração Docker Compose: `evidencias/sprint-8/docker-compose-config-2026-09-02.txt`.
- Smoke visual administrativo: seis fontes acionáveis, `16/16` testes, stress `500 + 500` e `/health` apresentados sem exposição de segredos.
- Build Release de 04/09: `evidencias/sprint-8/build-release-2026-09-04.txt`.
- Suite xUnit Release de 04/09: `evidencias/sprint-8/testes-release-2026-09-04.txt`.
- Healthcheck de 04/09: `evidencias/sprint-8/healthcheck-2026-09-04.txt`.
- Registo do smoke UAT de 04/09: `evidencias/sprint-8/smoke-uat-2026-09-04.md`.
- Build Release de 07/09: `evidencias/sprint-8/build-release-2026-09-07.txt`.
- Suite xUnit Release de 07/09: `evidencias/sprint-8/testes-release-2026-09-07.txt`.
- Configuração Compose validada em 07/09: `evidencias/sprint-8/docker-compose-config-2026-09-07.yml`.
- Healthcheck de 07/09: `evidencias/sprint-8/healthcheck-2026-09-07.json`.
- Persistência e reinício Docker: `evidencias/sprint-8/docker-persistence-health-2026-09-07.txt`.
- Readiness CI/CD: `evidencias/sprint-8/ci-readiness-2026-09-07.txt`.
- Verificação de vulnerabilidades NuGet: `evidencias/sprint-8/package-vulnerability-scan-2026-09-07.txt`.
- Smoke mobile PT/EN: `evidencias/sprint-8/mobile-light-pt-2026-09-07.png` e `evidencias/sprint-8/mobile-dark-en-2026-09-07.png`.
- Auditoria final e checklist de release: `evidencias/sprint-8/final-2026-09-07/auditoria-final.md`.
- Teste detalhado após otimização: `evidencias/sprint-8/final-2026-09-07/stress-optimized-validation.txt`.
- Readiness final da pipeline: `evidencias/sprint-8/final-2026-09-07/ci-readiness-final.txt`.
