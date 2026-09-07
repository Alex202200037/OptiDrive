# Auditoria visual dos gráficos e diagramas

## Âmbito

A documentação final reutiliza 13 diagramas técnicos únicos em 29 posições dos relatórios. Estes 13 elementos foram reconstruídos em Mermaid, mantendo o conteúdo e melhorando a leitura.

Foram também revistos seis gráficos de usabilidade/NPS. Estes já se encontram limpos, sem marcas, com dados visíveis e uma composição consistente, pelo que devem ser preservados sem conversão. Os gráficos Burndown e Velocity automáticos do Jira devem igualmente permanecer no formato gerado pelo Jira, por constituírem evidência nativa da execução dos sprints.

## Resultado dos 13 diagramas técnicos

| Diagrama | Elementos preservados | Melhoria principal |
|---|---|---|
| Arquitetura geral | cliente, MVC, domínio, persistência, Azure e serviços externos | camadas e protocolos explícitos |
| Pacotes lógicos | M01 a M07 e dependências | separação entre núcleo e capacidades transversais |
| Classes de domínio | oito classes, atributos, relações e cardinalidades | leitura vertical sem cruzamentos críticos |
| Componentes | aplicação Web, cinco serviços, dados e clientes externos | agrupamento por responsabilidade |
| Deployment | cliente, GitHub, Azure, aplicação, health, persistência e APIs | fronteira de implantação e protocolos visíveis |
| Casos de utilização | quatro atores, UC01 a UC12 e relações `include` | divisão por módulo e eliminação do emaranhado de linhas |
| BPMN de rotas | validação, APIs, Smart Save, autonomia e reforço | fases e resultados das decisões identificados |
| BPMN de garagem | seleção, validação, erro, persistência e histórico | circuito de correção completo |
| BPMN social | pedido, aceitação, colaboração, mensagens e alternativa individual | ramos `Sim` e `Não` completos |
| BPMN DevOps | commit, build, testes, decisão, correção, publicação e evidência | ciclo de falha e novo commit explícito |
| Gantt | Sprints 1 a 5, revisão e marco de entrega | cronologia e duração mais legíveis |
| Burndown geral | valores 52, 39, 26, 13 e 0 | linha de esforço com contraste adequado |
| Velocity geral | valores 18, 22, 24, 21 e 20 | barras com valores visíveis |

## Gráficos preservados

- Perfil dos 31 respondentes.
- Categorias do NPS e valor final 71.
- Distribuição das pontuações de recomendação.
- Médias das dimensões de usabilidade.
- Funcionalidades mais valorizadas.
- Temas dos comentários válidos.
- Burndown e Velocity automáticos do Jira.

Nenhuma versão destinada aos relatórios contém marca de avaliação.

## Publicação validada no Confluence

Validação efetuada em 07/09/2026 nas páginas finais do espaço OptiDrive:

- AER: Gantt global e diagrama geral de casos de uso.
- Desenho de Alto Nível: arquitetura, pacotes, classes, três processos BPMN, componentes e deployment.
- Desenhos Detalhados dos Sprints 1 a 5: classes e processo BPMN correspondente ao incremento.
- Desenho Detalhado do Sprint 6: classes e processo de validação DevOps.
- DevOps e Gestão de Erros: diagrama de instalação/deployment.
- Plano de Testes e Métricas: Burndown geral e Velocity geral.
- Avaliação de Usabilidade e NPS: seis gráficos preservados sem alteração.

Todos os anexos técnicos publicados foram verificados com estado completo no Confluence. As referências locais inválidas do Sprint 6 foram substituídas por anexos reais e os gráficos automáticos do Jira foram mantidos intactos.
