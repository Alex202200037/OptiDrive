# OptiDrive - Estado Atual da Implementacao

Estado do documento: versao final de apoio a apresentacao.

Data de revisao: 2026-06-24

## 1. Resumo executivo

O OptiDrive encontra-se implementado como aplicacao web em ASP.NET Core MVC / .NET 8, com interface Razor, persistencia SQLite via Entity Framework Core, Docker, integracoes externas e modulo social. A aplicacao esta pronta para apresentacao academica completa, com fluxos principais funcionais e documentação publicada no Confluence.

O produto cobre:

- autenticacao local segura;
- login federado configurável para Google, Microsoft e Apple;
- MFA TOTP compatível com Microsoft Authenticator, Google Authenticator e apps equivalentes;
- perfil de utilizador;
- garagem por veiculo;
- historico de abastecimentos, cargas e viagens;
- planeamento de rotas;
- mapas, postos de combustivel e carregadores eletricos;
- Smart Save para custos/autonomia/paragens;
- modulo social com contactos, mensagens e viagens colaborativas;
- backoffice com readiness operacional;
- Docker e healthcheck.

## 2. Confirmacao tecnologica

O projeto principal e ASP.NET Core:

- projeto: `OptiDrive.Web/OptiDrive.Web.csproj`;
- SDK: `Microsoft.NET.Sdk.Web`;
- framework: `net8.0`;
- padrao: MVC com Controllers e Razor Views;
- persistencia: EF Core SQLite;
- deploy local: Docker Compose.

Apesar de existir uma estrutura Next.js antiga no repositorio, a entrega funcional atual da cadeira esta concentrada em `OptiDrive.Web`.

## 3. Estado por modulo

| Modulo | Estado | Evidencia principal |
| --- | --- | --- |
| M01 - Gestao de Utilizadores e Seguranca | Implementado | login local, hashing PBKDF2, lockout, OAuth Google/Microsoft/Apple configurável, MFA Authenticator |
| M02 - Gestao de Garagem e Veiculos | Implementado | criacao/edicao/remocao, ficha detalhada, nivel de combustivel/carga, abastecimento/carga, historico |
| M03 - Gestao de Postos e Carregadores | Implementado | DGEG, OpenChargeMap, filtros por combustivel/marca e mapa |
| M04 - Planeamento de Rotas e Portagens | Implementado | origem/destino livres, Google Directions, evitar portagens, custo estimado, aplicar rota ao veiculo |
| M05 - Otimizacao Smart Save | Implementado | consumo, autonomia, sugestao de posto, paragens intermédias, suporte EV |
| M06 - Colaboracao e Partilha Social | Implementado | perfil social, contactos, partilhas, grupos, mensagens, viagens colaborativas e live pulse |
| M07 - Persistencia e Deploy Local | Implementado | EF Core SQLite, Dockerfile, docker-compose, healthcheck |
| M08 - Backoffice e Observabilidade | Implementado | readiness operacional, refresh de postos, reports e estado das integracoes |

## 4. Pontos fortes para defesa

- O sistema tem stack coerente e defensavel para a cadeira: ASP.NET Core MVC, EF Core, SQLite, Docker e testes xUnit.
- A garagem e o planeamento estao ligados: uma rota guardada pode ser aplicada ao veiculo e reflete-se no nivel/autonomia/historico.
- O Smart Save considera combustao e eletrico, com paragens intermedias quando a autonomia nao chega.
- A parte social tem presenca funcional e narrativa forte: mensagens, contactos, grupos, viagens partilhadas e live pulse.
- A seguranca foi reforcada com hashing de passwords, MFA, cookies HttpOnly e anti-CSRF.
- O backoffice apresenta readiness de produto, ajudando a apresentar maturidade operacional.
- A documentacao foi organizada por relatorios, sprints, Jira e Confluence.

## 5. Validacao atual

Comandos executados na revisao final:

```bash
dotnet build OptiDrive.Web/OptiDrive.Web.csproj
dotnet test OptiDrive.sln
```

Resultado:

- build concluido com sucesso;
- `8` testes executados;
- `8` testes com sucesso;
- `0` falhas.

## 6. Limitacoes assumidas

Estas limitacoes nao impedem a apresentacao, mas devem ser explicadas com honestidade tecnica:

- Google, Microsoft e Apple OAuth precisam de credenciais reais para ficarem ativos nos respetivos portais;
- nao foi identificada API oficial portuguesa simples e livre para custo exato de portagens, pelo que o sistema usa estimativa/Google Directions para evitar portagens;
- nao ha pipeline CI/CD cloud, embora exista Docker local;
- o social usa live pulse por HTTP e nao WebSocket dedicado.

## 7. Recomendacao para apresentacao

Apresentar o OptiDrive como produto academico funcional e apresentavel, com maturidade acima de produto inicial simples. A linguagem recomendada e:

> O OptiDrive e uma aplicacao ASP.NET Core MVC funcional, com persistencia local, Docker, integracoes externas, seguranca reforcada e modulo social colaborativo, pronta para apresentacao final da cadeira.

## 8. Conclusao

O estado atual e adequado para apresentacao final. Existem pontos evolutivos naturais para produto comercial, mas a entrega academica esta coerente, testada, documentada e rastreada em Jira/Confluence.
