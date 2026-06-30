# OptiDrive - Estrutura da Apresentação Final

Estado do documento: roteiro base para a apresentação final do projeto e apoio à defesa académica.

## 1. Objetivo da Apresentação

Apresentar o problema, a solução proposta, a arquitetura, o produto funcional e o valor académico/técnico do OptiDrive, deixando claro que a entrega está organizada em 5 sprints, com implementação, relatórios, Jira e Confluence alinhados.

## 2. Estrutura Sugerida do Deck

### Slide 1 - Capa

- Título: `OptiDrive`.
- Subtítulo: `Planeamento inteligente de rotas, postos, autonomia e viagens sociais`.
- Contexto: Engenharia de Software Aplicada.
- Autor e data.

### Slide 2 - Problema

- Custos de combustível e carregamento variam por localização, marca e tipo de energia.
- Planeamento manual de viagens é pouco previsível.
- Veículos elétricos trazem risco adicional de autonomia.
- Ferramentas generalistas não integram garagem, consumo, histórico, postos e componente social no mesmo sistema.

### Slide 3 - Solução Proposta

- Garagem digital por utilizador.
- Mapa com postos e carregadores.
- Planeamento de rotas com filtros, portagens e itinerário.
- Motor Smart Save para otimizar custo e autonomia.
- Camada social com perfis, mensagens, contactos e viagens colaborativas.

### Slide 4 - Objetivos do Sistema

- Reduzir custo operacional da viagem.
- Ajudar o utilizador a planear paragens certas.
- Centralizar dados do veículo.
- Melhorar decisão de abastecimento/carregamento.
- Permitir colaboração entre pessoas que viajam em conjunto.

### Slide 5 - Organização em 5 Sprints

| Sprint | Tema | Resultado |
| --- | --- | --- |
| Sprint 1 | Identidade, segurança e garagem | Base ASP.NET Core MVC, login local, MFA e garagem |
| Sprint 2 | Mapas, postos e rotas | Google Maps, postos, carregadores, filtros e itinerário |
| Sprint 3 | Smart Save e histórico | Consumo por velocidade, autonomia, reforços e aplicação da rota ao veículo |
| Sprint 4 | Social e OAuth | Perfis, contactos, mensagens, viagens colaborativas, Google/Microsoft e fallback Apple |
| Sprint 5 | Readiness e entrega | Docker, SQLite, healthcheck, PT/EN, tema, dados realistas, Jira e Confluence |

### Slide 6 - Arquitetura

- `.NET 8 MVC`.
- Razor Views, controllers e services.
- `AppStateService` para orquestração de estado e persistência.
- `SmartSaveService` para consumo, autonomia e reforços.
- Integrações: `Google Maps`, `OpenChargeMap`, OAuth Google/Microsoft/Apple configurável.
- SQLite LocalDB com Docker Compose.

### Slide 7 - Módulos

- M01 - Identidade e Segurança.
- M02 - Garagem e Veículos.
- M03 - Planeamento Inteligente.
- M04 - Postos e Energia.
- M05 - Otimização Smart Save.
- M06 - Social e Viagens Colaborativas.
- M07 - Administração e Observabilidade.

### Slide 8 - Demonstração: Login e Perfil

- Login local.
- Google/Microsoft OAuth configurável.
- Apple com fallback profissional.
- Authenticator MFA com QR code e recovery codes.

### Slide 9 - Demonstração: Garagem

- Lista de viaturas.
- Ficha individual.
- Nível de combustível/carga.
- Abastecimentos/cargas.
- Manutenção e histórico.

### Slide 10 - Demonstração: Planeamento

- Origem/destino livres.
- Mapa com rota e postos.
- Filtros por combustível, energia e marca.
- Evitar portagens.
- Guardar e aplicar rotas.

### Slide 11 - Demonstração: Smart Save e EV

- Cálculo de consumo.
- Consumo ajustado pela velocidade média.
- Recomendação de posto/carregador.
- Paragens quando autonomia não chega.
- Reserva mínima de chegada.

### Slide 12 - Demonstração: Social

- Perfil social.
- Contactos de confiança.
- Mensagens diretas.
- Viagens colaborativas.
- Split de custos.

### Slide 13 - Qualidade e Operação

- `dotnet test OptiDrive.sln`.
- `docker compose up -d --build`.
- `/health`.
- Backoffice de readiness.
- Tema claro/escuro.
- Português/inglês.

### Slide 14 - Jira e Confluence

- Backlog estruturado por 5 sprints.
- Relatórios publicados no Confluence.
- Atas por sprint.
- Matriz de rastreabilidade RF x UC.
- Evidência de testes e entrega.

### Slide 15 - Limites e Evolução Futura

- Chat em tempo real com SignalR.
- Deploy cloud com domínio HTTPS.
- API oficial de portagens se disponível.
- Base de dados gerida em cloud.
- Apple Sign In real com Apple Developer Program.

### Slide 16 - Conclusão

- Produto funcional e apresentável.
- Stack alinhada com a cadeira.
- Diferenciação por Smart Save e social.
- Documentação completa e rastreável.
- Preparado para evolução real.

## 3. Guião Curto de Defesa

1. Problema: custos, autonomia e planeamento fragmentado.
2. Produto: mostrar login/perfil -> garagem -> planeamento -> Smart Save -> social.
3. Arquitetura: explicar MVC, services, SQLite, Docker e integrações.
4. Gestão: mostrar 5 sprints, Jira e Confluence.
5. Qualidade: mostrar testes, healthcheck e readiness.
6. Futuro: apresentar evolução sem desvalorizar o que já está feito.

## 4. Materiais a Incluir no Deck Final

- Screenshot da home/login.
- Screenshot do perfil com Authenticator.
- Screenshot da garagem.
- Screenshot do mapa/rota.
- Screenshot de uma recomendação Smart Save.
- Screenshot da área social.
- Diagrama simples da arquitetura.
- Resumo das issues/sprints do Jira.
- Link para Confluence.

## 5. Nota Final

Este documento serve de base ao deck final. A narrativa recomendada é apresentar o OptiDrive como um produto académico completo, com ambição realista de produto comercial: funcional hoje, tecnicamente coerente e preparado para evolução.
