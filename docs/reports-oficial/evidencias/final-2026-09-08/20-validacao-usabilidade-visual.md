# OptiDrive - Validação de Usabilidade Visual

| Campo | Valor |
| --- | --- |
| Data | 08/09/2026 |
| Ambiente | Docker local, `http://127.0.0.1:5080` |
| Navegador | Browser Chromium integrado |
| Perfis | Visitante, condutor e administrador |
| Viewports | Desktop 1280x720 e mobile 390x844 |
| Modos | Claro e escuro |
| Idiomas | Português e inglês |

## Percursos Validados

| ID | Percurso | Resultado observado | Estado |
| --- | --- | --- | --- |
| UX-FINAL-01 | Landing page como visitante | Proposta de valor, chamadas à ação e cartões visíveis; navegação acessível. | Aprovado |
| UX-FINAL-02 | Alternância PT/EN | Navegação e conteúdo principal mudam de idioma sem perda de contexto. | Aprovado |
| UX-FINAL-03 | Autenticação de condutor | Login conduz ao perfil e apresenta veículo ativo e acessos rápidos. | Aprovado |
| UX-FINAL-04 | Garagem | Frota, estado, alertas e ficha do veículo apresentam hierarquia e ações claras. | Aprovado |
| UX-FINAL-05 | Planeamento e Smart Save | Origem, destino, veículo, autonomia, paragens e custos ficam reunidos no mesmo cockpit. | Aprovado |
| UX-FINAL-06 | Área social | Perfis, contactos, mensagens e viagens colaborativas permanecem separados por secções. | Aprovado |
| UX-FINAL-07 | Modo escuro | Contraste e legibilidade mantidos no planeamento e na área social. | Aprovado |
| UX-FINAL-08 | Autenticação administrativa | Login conduz ao Backoffice; Garagem e Planeamento não aparecem na navegação administrativa. | Aprovado |
| UX-FINAL-09 | Evidências `RF-M08-04` | Backoffice apresenta cartões acionáveis para pipeline, testes, stress, health, Jira e Confluence. | Aprovado |
| UX-FINAL-10 | Responsividade mobile | Landing page mantém leitura linear, botões acessíveis e ausência de corte horizontal. | Aprovado |

## Verificações de Interação e Acessibilidade

- Existe ligação de salto para o conteúdo principal.
- Navegação principal e preferências de interface possuem nomes acessíveis.
- Tema e idioma apresentam estado ativo/pressionado.
- Formulários possuem rótulos associados aos campos.
- Indicadores de nível usam elementos de progresso ou controlos com nome acessível.
- A separação de navegação entre condutor e administrador foi confirmada.
- Não foram executadas ações destrutivas durante o smoke test.

## Evidências

- `11-home-desktop-en.png`
- `12-home-desktop-pt.png`
- `13-login-desktop-pt.png`
- `14-profile-desktop-pt.png`
- `15-garage-desktop-pt.png`
- `16-planning-desktop-light-pt.png`
- `17-planning-desktop-dark-pt.png`
- `18-social-desktop-dark-pt.png`
- `19-backoffice-desktop-dark-pt.png`
- `21-mobile-light-pt.png`
- `22-mobile-dark-en.png`
- `29-backoffice-rf-m08-04.png`

## Conclusão

Os percursos críticos mantêm consistência visual, navegação compreensível, adaptação mobile, tema claro/escuro e separação por perfil. Esta validação constitui um smoke test visual técnico; não substitui a avaliação externa de usabilidade e NPS, que permanece documentada no relatório próprio.
