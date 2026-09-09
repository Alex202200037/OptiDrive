# OptiDrive - Documento de Encerramento de Projeto

| Campo | Valor |
| --- | --- |
| Projeto | OptiDrive |
| Documento | Project Closure Report |
| Template base | Documento de Encerramento de Projeto v2.1 |
| Versão | 3.4 |
| Data | 08/09/2026 |
| Autor | Alexandre Miguel |
| Classificação | Académico / Interno |
| Repositório documental | Confluence - Espaço OptiDrive |

## Histórico de Versões e Controlo Documental

| Versão | Data | Autor | Secções Alteradas | Descrição da Alteração | Aprovado Por |
| --- | --- | --- | --- | --- | --- |
| 1.0 | 30/06/2026 | Alexandre Miguel | Todas | Encerramento inicial do projeto académico | Autor/PM |
| 2.0 | 08/07/2026 | Alexandre Miguel | Âmbito, qualidade, riscos, métricas | Consolidação após revisão documental | Autor/PM |
| 3.1 | 13/07/2026 | Alexandre Miguel | Todas | Versão final alinhada com estrutura de Project Closure Report | A validar pelo docente |
| 3.2 | 02/09/2026 | Alexandre Miguel | Qualidade, cronograma e entregáveis | Atualização controlada para as Sprints 6 a 8 e validação final | A validar pelo docente |
| 3.3 | 07/09/2026 | Alexandre Miguel | Qualidade, testes, DevOps e encerramento | Auditoria final antecipada e consolidação das evidências da Sprint 8 | Autor/PM |
| 3.4 | 08/09/2026 | Alexandre Miguel | Encerramento e evidências | Registo do fecho Jira, CI remota e publicação documental final | Autor/PM |

## 1. Sumário Executivo

### 1.1 Declaração de Encerramento

O projeto OptiDrive encontra-se no ciclo final de aceitação como produto académico funcional. A solução foi implementada como aplicação web ASP.NET Core MVC com autenticação, garagem de veículos, planeamento de viagens, Smart Save, postos de combustível, carregadores elétricos, área social, administração, Docker, Azure e documentação em Jira/Confluence.

A auditoria técnica foi revalidada em 08/09/2026, após o encerramento administrativo do Sprint 8 no Jira em 07/09/2026. O Sprint Report confirma `8/8` itens e `36/36` story points concluídos, sem trabalho incompleto ou concluído fora do sprint. O produto dispõe de base técnica suficiente para demonstração local/Docker e evolução futura, mantendo condicionantes externas de OAuth/Azure e riscos residuais identificados para uma eventual passagem a produção comercial.

### 1.2 Ficha de Identidade do Projeto

| Campo | Informação |
| --- | --- |
| Nome do Projeto | OptiDrive |
| Código / ID | OP |
| Unidade Organizacional | Licenciatura em Engenharia Informática - ESA |
| Data de início | 11/05/2026 |
| Data de fecho funcional | 30/06/2026 |
| Data de consolidação documental | 13/07/2026 |
| Gestor de Projeto | Alexandre Miguel |
| Stack principal | ASP.NET Core MVC, .NET 8, EF Core, SQLite, Docker, Azure App Service |
| Ferramentas de gestão | Jira, Confluence, Git/GitHub |

### 1.3 Resumo de Desempenho Global

| Dimensão | Meta | Resultado Real | Desvio | Avaliação |
| --- | --- | --- | --- | --- |
| Âmbito | Entregar módulos essenciais definidos na AER | 7 módulos documentados e funcionalidade principal implementada | Roadmap futuro identificado | Aceite |
| Prazo | Trabalho estruturado em 8 sprints | 8 ciclos concluídos; Sprint 8 encerrado no Jira em 07/09/2026 | Encerramento um dia antes do limite planeado | Aceite |
| Qualidade | Build, testes, revisão funcional e documentação | 16 testes automatizados aprovados, healthcheck, stress 500+500 e matriz UAT com 20 casos | OAuth/Azure condicionados ao ambiente externo | Aceite tecnicamente |
| Documentação | Confluence com templates e artefactos finais | AER, DAN, DD/Atas/Retrospetivas, Encerramento, Testes/Métricas, DevOps/Erros e UAT auditados e publicados | Sem desvio documental em aberto | Concluída |
| Benefícios | Produto demonstrável e coerente com requisitos | Produto funcional em execução local/Docker e base social relevante | Publicação Azure requer credenciais ativas | Aceite tecnicamente |

### 1.4 Principais Realizações

| ID | Realização | Evidência |
| --- | --- | --- |
| R-01 | Aplicação ASP.NET Core MVC funcional | Código fonte, execução local/Docker e CI remota aprovada |
| R-02 | Garagem com veículos, nível de combustível/carga e histórico | Módulo Garagem, testes AppStateService |
| R-03 | Planeamento com mapa, custos, autonomia e reforços | Módulo Planeamento, testes SmartSaveService |
| R-04 | Área social com perfis, contactos, mensagens e viagens colaborativas | Módulo Social, dados seed e testes |
| R-05 | Backoffice/admin, healthcheck e estado operacional | Página Admin e endpoint `/health` |
| R-06 | Documentação académica organizada no Confluence | Páginas oficiais finais |

### 1.5 Principais Desvios e Decisões

| Desvio / Decisão | Causa | Impacto | Tratamento |
| --- | --- | --- | --- |
| Projeto executado essencialmente por uma pessoa | Condição real da equipa | Elevada carga de trabalho e risco documental | Priorização MoSCoW, sprints curtas e foco nos fluxos críticos |
| APIs externas dependentes de configuração | Chaves, quotas e callbacks externos | Possível falha em funcionalidades de mapas/OAuth | Variáveis de ambiente, fallback local e documentação técnica |
| Diagramas e relatórios revistos várias vezes | Exigência de templates e normas específicas | Retrabalho documental | Reestruturação no Confluence e melhoria dos artefactos |
| Produto com escopo social elevado | Valorização académica da componente social | Aumento de complexidade | Implementação incremental de perfil, contactos, mensagens e viagens em grupo |

### 1.6 Recomendações Estratégicas

| Recomendação | Prioridade | Justificação |
| --- | --- | --- |
| Migrar SQLite para base de dados gerida | Alta | Necessário para utilização real multiutilizador |
| Manter CI/CD com GitHub Actions e reforcar monitorizacao | Alta | Reduz risco de regressao em producao |
| Validar fontes oficiais de preços de combustível | Alta | Dados reais exigem atualização e fiabilidade |
| Melhorar monitorização com Application Insights | Média | Suporte operacional mais robusto |
| Evoluir o social para notificações em tempo real | Média | Aumenta valor de viagens colaborativas |

## 2. Matriz RACI - Responsabilidades do Projeto

### 2.1 Legenda RACI

| Letra | Significado | Regra |
| --- | --- | --- |
| R | Responsible | Executa o trabalho |
| A | Accountable | Responde pelo resultado final |
| C | Consulted | É consultado antes/durante decisões |
| I | Informed | É informado após decisões ou entregas |

### 2.2 Papéis Considerados

| Abreviatura | Papel / Perfil |
| --- | --- |
| DOC | Docente / stakeholder académico |
| PM | Project Manager / Scrum Master |
| DEV | Desenvolvimento full-stack |
| QA | Qualidade e testes |
| OPS | Operação, deploy e configuração |
| USER | Utilizadores avaliadores / participantes de usabilidade |

### 2.3 Matriz RACI de Encerramento

| Atividade / Entregável | DOC | PM | DEV | QA | OPS | USER |
| --- | --- | --- | --- | --- | --- | --- |
| Validar requisitos e rastreabilidade | C | A/R | R | R | I | I |
| Implementar funcionalidades finais | I | A | R | C | C | I |
| Executar testes e corrigir bugs | I | A | R | R | C | C |
| Publicar documentação no Confluence | I | A/R | C | R | I | I |
| Preparar entrega e defesa | C | A/R | R | R | C | I |
| Encerrar riscos e roadmap | C | A/R | C | R | C | I |

## 3. Identificação e Âmbito do Projeto

### 3.1 Objetivos e Justificação de Negócio

O OptiDrive responde à necessidade de planear viagens de forma mais inteligente, associando cada rota ao veículo real do utilizador, ao respetivo consumo/autonomia e à disponibilidade de postos de combustível ou carregamento. O sistema também acrescenta uma dimensão social para permitir viagens em grupo, partilha de informação e coordenação entre condutores.

### 3.2 Estrutura de Decomposição do Trabalho

| Work Package | Descrição | Estado |
| --- | --- | --- |
| WP01 | Identidade, login, MFA e OAuth | Concluído |
| WP02 | Garagem, veículos, níveis e histórico | Concluído |
| WP03 | Planeamento de rotas e integração Google Maps | Concluído com dependência externa |
| WP04 | Postos de combustível e carregadores elétricos | Concluído com dados externos/fallback |
| WP05 | Smart Save, autonomia e consumo por velocidade | Concluído |
| WP06 | Área social e viagens colaborativas | Concluído |
| WP07 | Administração, DevOps, Docker e Azure | Concluído; publicação Azure condicionada a credenciais externas |
| WP08 | Documentação, Jira, Confluence e apresentação | Concluído |

### 3.3 Entregáveis Finais - Registo de Aceitação

| Entregável | Critério de Aceitação | Data Prevista | Data Real | Aceite Por | Estado |
| --- | --- | --- | --- | --- | --- |
| AER | Requisitos por módulo, atores, use cases e rastreabilidade | 11/05/2026 | 13/07/2026 | Docente | A validar |
| DAN | Arquitetura, processos, persistência e deployment | 30/05/2026 | 13/07/2026 | Docente | A validar |
| Desenhos detalhados | 8 sprints documentadas com testes e interfaces | 08/09/2026 | 07/09/2026 | Docente | Concluído; a validar |
| Atas | 8 atas correspondentes às sprints | 08/09/2026 | 07/09/2026 | Docente | Concluído; a validar |
| Aplicação web | Fluxos principais demonstráveis | 30/06/2026 | 30/06/2026 | Autor/Docente | Preparado |
| Plano de Testes e Métricas | Test cases, métricas e gráficos | 13/07/2026 | 13/07/2026 | Docente | A validar |
| DevOps e Gestão de Erros | Deploy, configuração, healthcheck e processo de incidentes | 13/07/2026 | 13/07/2026 | Docente | A validar |
| Documento de Encerramento | Fecho formal, riscos, lições e transição | 13/07/2026 | 13/07/2026 | Docente | A validar |

## 4. Desempenho de Âmbito

### 4.1 Análise de Variância de Âmbito

O âmbito entregue corresponde aos módulos definidos na AER: identidade, garagem, planeamento, postos/energia, Smart Save, social e administração. Algumas funcionalidades foram implementadas com limitações próprias de um contexto académico: dados externos com dependência de APIs, OAuth dependente de callbacks e monitorização ainda simples.

### 4.2 KPIs de Âmbito

| KPI | Fórmula / Definição | Meta | Resultado | Status |
| --- | --- | --- | --- | --- |
| Taxa de módulos documentados | Módulos documentados / módulos previstos | 100% | 7/7 | OK |
| Taxa de sprints documentadas | Sprints com relatório+ata / sprints planeadas | 100% | 8/8 documentadas | OK |
| Requisitos funcionais rastreados | Requisitos com use case associado | 100% | 35/35 | OK |
| Use cases documentados | Use cases identificados | >= 12 | 15 | OK |
| Roadmap residual explícito | Itens futuros identificados | Sim | Sim | OK |

### 4.3 Registo de Change Requests

| ID | Pedido / Alteração | Origem | Decisão | Impacto |
| --- | --- | --- | --- | --- |
| CR-01 | Melhorar garagem com níveis de combustível/carga | Feedback funcional | Aprovada | Reforço do módulo M02 |
| CR-02 | Adicionar social mais completo | Valorização da cadeira | Aprovada | Criação/expansão M06 |
| CR-03 | Integrar OAuth Google/Microsoft | Requisito de realismo | Aprovada | Dependência de configuração externa |
| CR-04 | Adicionar tema claro/escuro e PT/EN | Feedback UX | Aprovada | Melhoria transversal |
| CR-05 | Refazer documentação por templates | Feedback docente | Aprovada | Retrabalho documental relevante |

## 5. Desempenho de Cronograma

### 5.1 Cronograma Final

| Sprint | Período | Objetivo | Estado |
| --- | --- | --- | --- |
| Sprint 1 | 11/05/2026 - 20/05/2026 | Identidade, segurança e garagem | Fechada |
| Sprint 2 | 21/05/2026 - 30/05/2026 | Mapas, postos e energia | Fechada |
| Sprint 3 | 01/06/2026 - 08/06/2026 | Smart Save e histórico operacional | Fechada |
| Sprint 4 | 09/06/2026 - 16/06/2026 | Social, OAuth e viagens colaborativas | Fechada |
| Sprint 5 | 17/06/2026 - 30/06/2026 | DevOps, administração e entrega final | Fechada |
| Sprint 6 | 29/08/2026 - 31/08/2026 | Melhorias de código, UI e readiness | Fechada |
| Sprint 7 | 29/08/2026 - 31/08/2026 | Polimento e estabilidade | Fechada |
| Sprint 8 | 03/09/2026 - 08/09/2026 | Auditoria final, aceitação e entrega | Encerrada antecipadamente no Jira em 07/09/2026 |

![Figura 1 - Burndown geral](assets/diagrams/burndown-geral.png)

*Figura 1 - Burndown global documentado do âmbito consolidado. Os Burndowns automáticos de cada sprint mantêm-se no Jira como fonte operacional.*

![Figura 2 - Velocity geral](assets/diagrams/velocity-geral.png)

*Figura 2 - Velocity documentada: S1-S5 correspondem às estimativas históricas do projeto e S6-S8 aos resultados automáticos do Jira.*

### 5.2 KPIs de Cronograma

| KPI | Alvo | Resultado |
| --- | --- | --- |
| Sprints planeadas | 8 | 8 |
| Sprints encerradas | 8 | 8 ciclos concluídos; Sprint 8 fechado no Jira em 07/09/2026 |
| Desenhos detalhados | 8 | 8 concluídos |
| Ata por sprint | 8 | 8 concluídas |
| Revisão final documental | Sim | Revalidada em 08/09/2026 |

## 6. Desempenho de Orçamento

O projeto não teve orçamento comercial formal. Foram usados recursos gratuitos, académicos ou de baixo custo. Para efeitos académicos, o custo principal foi esforço humano.

| Categoria | Orçamento Aprovado | Real Gasto | Observação |
| --- | --- | --- | --- |
| Recursos humanos | N/A | Esforço individual elevado | Trabalho acumulado por um elemento |
| Licenças software | 0 EUR | 0 EUR | Uso de ferramentas gratuitas/académicas |
| Infraestrutura local | 0 EUR | 0 EUR | MacBook + Docker local |
| Azure/App Service | Crédito/conta académica | Sem custo confirmado | Configuração preparada; ambiente público não ativo no fecho |
| APIs externas | Chaves próprias/quotas | Controlado | Google Maps/OpenChargeMap |

## 7. Desempenho de Qualidade

### 7.1 Evidências de Teste e Validação

| Evidência | Resultado |
| --- | --- |
| `dotnet build` | Validável na solução `OptiDrive.sln` |
| `dotnet test` | 16 testes automatizados aprovados em Release; stress 500+500 com criação em 7,998 s e 500 dashboards em 0,405 s |
| Testes SmartSaveService | Autonomia, reforços e consumo por velocidade |
| Testes AppStateService | Garagem, rotas aplicadas, social, MFA e admin |
| Healthcheck | Endpoint `/health` disponível |
| Usabilidade | Questionário com NPS e avaliação qualitativa |

### 7.2 KPIs de Qualidade

| KPI | Meta | Resultado | Status |
| --- | --- | --- | --- |
| Testes automatizados executáveis | Sim | Sim, 16 testes aprovados | OK |
| Falhas críticas conhecidas no fecho | 0 | 0 documentadas | OK |
| Fluxos críticos demonstráveis | >= 5 | Login, garagem, planeamento, social, admin | OK |
| Avaliação média de usabilidade | >= 4/5 | 4.44/5 | OK |
| NPS | >= +30 | +71 | OK |

## 8. Gestão de Riscos e Problemas

### 8.1 Resumo do Registo de Riscos

| Categoria | Identificados | Materializados | Mitigados | Residuais | Transferidos para Operação |
| --- | --- | --- | --- | --- | --- |
| Técnico | 6 | 3 | 5 | 1 | Sim |
| Organizacional | 3 | 2 | 2 | 1 | Sim |
| Externo/API | 5 | 3 | 4 | 1 | Sim |
| Documental | 4 | 3 | 4 | 0 | Não |
| Segurança | 3 | 1 | 2 | 1 | Sim |

### 8.2 Riscos Residuais

| Risco Residual | Probabilidade | Impacto | Owner pós-projeto | Plano de Resposta |
| --- | --- | --- | --- | --- |
| Quotas/callbacks de OAuth em produção | Média | Alto | Operação | Registar domínio final e validar callbacks |
| Dados externos incompletos | Alta | Médio | Produto | Integrar fonte oficial ou acordo de dados |
| Escalabilidade SQLite | Média | Alto | DevOps | Migrar para PostgreSQL/Azure SQL |
| Monitorização insuficiente | Média | Médio | DevOps | Ativar Application Insights e alertas |

### 8.3 Estado Final de Issues

| Tipo | Estado |
| --- | --- |
| Bugs críticos | Encerrados ou sem ocorrência conhecida no fecho |
| Bugs médios/baixos | Documentados para roadmap quando aplicável |
| Melhorias UX | Parcialmente tratadas, restantes em backlog futuro |
| Dependências externas | Documentadas e configuráveis por variáveis de ambiente |

## 9. Gestão de Stakeholders e Comunicação

| Stakeholder | Papel | Engagement Previsto | Engagement Real | Observações |
| --- | --- | --- | --- | --- |
| Docente | Avaliador e orientador | Leading | Leading | Feedback usado para reformular documentação |
| Utilizadores avaliadores | Validação de usabilidade | Supportive | Supportive | Questionário e feedback qualitativo |
| Condutores | Utilizadores finais | Supportive | Simulado/avaliado | Perfis e dados seed para demonstração |
| Administrador | Operação/backoffice | Supportive | Implementado | Página Admin e permissões |

## 10. Desempenho da Equipa

A execução foi concentrada em Alexandre Miguel, acumulando funções de Project Manager, Scrum Master, desenvolvimento, QA, DevOps e documentação. Este fator aumentou o risco de sobrecarga e de desalinhamento documental, mas também permitiu consistência técnica e decisão rápida.

| Função | Responsável | Contributo |
| --- | --- | --- |
| Project Manager | Alexandre Miguel | Planeamento, sprints, Jira, Confluence |
| Scrum Master | Alexandre Miguel | Organização das sprints e impedimentos |
| Developer | Alexandre Miguel | Backend, frontend, integração e dados |
| QA | Alexandre Miguel | Testes automatizados, manuais e usabilidade |
| DevOps | Alexandre Miguel | Docker, Azure, configuração e healthcheck |

## 11. Registo de Lições Aprendidas

### 11.1 Sucessos a Replicar

| ID | Área | Descrição do Sucesso | Impacto | Como Replicar |
| --- | --- | --- | --- | --- |
| S-001 | Produto | Fio condutor claro entre veículo, rota, autonomia e custos | Alto | Começar por fluxos de valor antes de extras |
| S-002 | Social | Integração social aumentou o valor demonstrável | Alto | Tratar social como módulo próprio desde o início |
| S-003 | Técnica | Separação MVC/serviços facilitou testes | Médio | Manter regras de domínio fora das views |
| S-004 | DevOps | Docker tornou execução reproduzível | Médio | Manter `.env.example` e compose atualizados |

### 11.2 Falhas e Problemas a Evitar

| ID | Área | Problema | Causa Raiz | Recomendação |
| --- | --- | --- | --- | --- |
| F-001 | Documentação | Reformulação tardia dos templates | Subestimação da exigência formal | Preencher templates desde o primeiro sprint |
| F-002 | Diagramas | Necessidade de normas UML/BPMN formais | Uso inicial demasiado ilustrativo | Produzir diagramas em ferramenta adequada desde cedo |
| F-003 | Gestão | Escopo demasiado ambicioso para um elemento | Várias funcionalidades com peso semelhante | Fechar Must antes de Should/Could |
| F-004 | Integrações | OAuth e mapas dependentes de plataformas externas | Configuração fora do código | Criar checklist de callbacks/keys por ambiente |

## 12. Realização de Benefícios

| ID | Benefício Esperado | KPI | Valor Esperado | Valor Realizado | % Realizado | Owner Pós-Projeto |
| --- | --- | --- | --- | --- | --- | --- |
| B-001 | Planear viagens com custo/autonomia | Fluxo planeamento | Funcional | Funcional | 100% | Produto |
| B-002 | Associar rota ao veículo real | Histórico e nível | Funcional | Funcional | 100% | Produto |
| B-003 | Apoiar viagens colaborativas | Social/viagens | Funcionalidade implementada | Funcional | 90% | Produto |
| B-004 | Ter base vendável/demonstrável | Demo + documentação | Apresentável | Apresentável | 85% | Produto/DevOps |
| B-005 | Reduzir incerteza em combustível/carga | Smart Save | Funcional | Funcional com dados externos | 85% | Produto |

## 13. Inventário de Ativos e Propriedade Intelectual

| Ativo | Localização | Observação |
| --- | --- | --- |
| Código fonte | Git/GitHub | Projeto ASP.NET Core MVC |
| Base de dados | SQLite/App_Data ou volume Docker | Dados locais e seed |
| Documentação oficial | Confluence OptiDrive | Relatórios e atas |
| Diagramas | `docs/reports-oficial/assets/diagrams` | UML/BPMN/imagens |
| Configuração | `.env.example` / Azure App Settings | Chaves não devem estar em código |
| Testes | `OptiDrive.Web.Tests` | xUnit |

## 14. Conformidade Legal, Regulatória e Contratual

Não existem contratos comerciais ativos. Para passagem a produção seriam necessários termos de utilização, política de privacidade, consentimento para dados pessoais, gestão de cookies, revisão das licenças das APIs e política de retenção de dados.

## 15. Dependências Externas - Encerramento Formal

| Dependência | Uso | Estado no Encerramento | Plano Futuro |
| --- | --- | --- | --- |
| Google Maps APIs | Mapas, geocoding e direções | Configurável | Rever quotas e callbacks |
| OpenChargeMap | Carregadores EV | Configurável | Validar cobertura por rota |
| Fontes de combustível | Postos/preços | Parcial/fallback | Contratualizar ou validar fonte oficial |
| Google OAuth | Login externo | Configurável | Validar domínio final |
| Microsoft OAuth | Login externo | Configurável | Validar app registration final |
| Azure App Service | Hosting | Configuração e workflow preparados; ambiente público não ativo | Adicionar credenciais, publicar e ativar monitorização |

## 16. Plano de Transição e Suporte

| Área | Ação de Transição | Responsável |
| --- | --- | --- |
| Execução local | Usar README, `.env.example`, `dotnet run` ou Docker | DevOps |
| Deploy | Configurar variáveis no Azure App Service | DevOps |
| Dados | Manter seed de demonstração e backups SQLite | Produto |
| Suporte | Consultar healthcheck, Admin e logs | Operação |
| Documentação | Usar Confluence como fonte oficial | PM |

## 17. Administração e Arquivo

A documentação final fica arquivada no espaço Confluence OptiDrive. O código fonte, os testes e os artefactos locais ficam no repositório Git. As versões anteriores devem ser mantidas apenas como histórico, evitando confusão com a versão oficial final.

## 18. Aprovações Formais

| Papel | Nome | Estado | Data |
| --- | --- | --- | --- |
| Autor / Project Manager | Alexandre Miguel | Preparado para entrega | 13/07/2026 |
| Docente | A validar | Pendente de avaliação | A definir |
| Operação futura | A definir | Transferência documentada | A definir |
