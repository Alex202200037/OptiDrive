# OptiDrive - Entrega Final e Guia de Apresentação

| Campo | Valor |
| --- | --- |
| Documento | Guia executivo de apresentação |
| Versão | 2.0 |
| Data | 30/06/2026 |
| Estado | Pronto para apresentação |

## 1. Resumo

O OptiDrive é uma aplicação web em ASP.NET Core MVC / .NET 8 para ajudar condutores a gerir veículos, planear viagens, comparar postos de combustível/carregamento, otimizar custos e colaborar socialmente em deslocações.

A entrega final inclui:

- Produto funcional em ASP.NET Core MVC.
- SQLite LocalDB persistente.
- Docker Compose para MacBook.
- Login local, Google, Microsoft e Apple OAuth configurável.
- Authenticator MFA com QR code e recovery codes.
- Garagem com veículos reais, níveis e histórico.
- Planeamento com mapas, postos, carregadores, portagens e Smart Save.
- Social com perfis, contactos, mensagens e viagens colaborativas.
- Tema claro/escuro e português/inglês.
- Relatórios organizados em 5 sprints, atas, Jira e Confluence.

## 1.1 Organização Final por Sprints

| Sprint | Tema | Evidência documental |
| --- | --- | --- |
| Sprint 1 | Identidade, segurança e garagem | Desenho Detalhado Sprint 1 + Ata Sprint 1 |
| Sprint 2 | Mapas, postos, carregadores e rotas | Desenho Detalhado Sprint 2 + Ata Sprint 2 |
| Sprint 3 | Smart Save, autonomia e histórico operacional | Desenho Detalhado Sprint 3 + Ata Sprint 3 |
| Sprint 4 | Social, OAuth e colaboração | Desenho Detalhado Sprint 4 + Ata Sprint 4 |
| Sprint 5 | Readiness, i18n, Docker e entrega final | Desenho Detalhado Sprint 5 + Ata Sprint 5 |

## 2. Guião Recomendado

| Ordem | Apresentação | Mensagem principal |
| --- | --- | --- |
| 1 | Home | O produto resolve custo, autonomia e colaboração numa só plataforma |
| 2 | Login/Registo | Contas reais com login local e providers externos |
| 3 | Perfil | Segurança com Authenticator e estado OAuth |
| 4 | Garagem | Cada carro tem ficha, níveis, consumo, manutenção e histórico |
| 5 | Planeamento | Rota real com mapa, postos, custo e instruções |
| 6 | Smart Save | Sistema decide reforços e evita chegada a 0% |
| 7 | Aplicar viagem | O combustível/carga desce e o histórico é atualizado |
| 8 | Social | Perfis, mensagens e viagens em grupo valorizam a componente social |
| 9 | Backoffice | Readiness mostra maturidade operacional |
| 10 | Confluence/Jira | Evidência académica organizada e rastreável |

## 3. Funcionalidades-Chave por Área

| Área | Funcionalidades |
| --- | --- |
| Identidade | Local login, Google OAuth, Microsoft OAuth, Apple OAuth, MFA TOTP |
| Garagem | Veículos, marcas/modelos, níveis, consumos, abastecimentos, cargas e histórico |
| Planeamento | Google Maps, geocoding, rota visual, instruções, evitar portagens e custo total |
| Energia | Postos de combustível, carregadores EV, filtros por combustível e marca |
| Smart Save | Consumo por velocidade, autonomia, reforços e reserva de chegada |
| Social | Perfil, contactos, mensagens, viagens colaborativas e split |
| Operação | Docker, SQLite, `/health`, backoffice e readiness |
| UX | Design responsivo, tema claro/escuro e PT/EN |

## 4. Antes da Apresentação

```bash
docker compose up -d --build
curl http://127.0.0.1:5080/health
```

Confirmar:

| Verificação | Resultado esperado |
| --- | --- |
| Home abre | Página inicial carrega |
| Login funciona | Entrar com conta local |
| Perfil abre | Mostra MFA e providers |
| Garagem tem carros | Lista e ficha aparecem |
| Planeamento calcula rota | Mapa e resumo aparecem |
| Social tem utilizadores | Mensagens e viagens aparecem |
| Backoffice abre com admin | Readiness visível |

## 5. Conta Recomendada para Apresentação

| Campo | Valor |
| --- | --- |
| Email | `alexandre@optidrive.pt` |
| Palavra-passe | `opti2026` |
| Perfil | Condutor principal com garagem e histórico |

## 6. Pontos Fortes para Defender

| Ponto | Argumento |
| --- | --- |
| Stack correta | ASP.NET Core MVC, EF Core, SQLite e Docker |
| Produto completo | Não é apenas CRUD: inclui mapas, energia, Smart Save e social |
| Segurança | Password hashing, MFA, recovery codes e OAuth configurável |
| Social valorizado | Mensagens, contactos e viagens colaborativas são parte central |
| Dados realistas | A apresentação mostra uso realista por múltiplos utilizadores |
| Documentação | Relatórios seguem templates do professor e feedback aplicado |
| Operação | Healthcheck e backoffice mostram maturidade de produto |

## 7. Respostas Preparadas

| Pergunta provável | Resposta recomendada |
| --- | --- |
| Porque ASP.NET Core MVC? | Porque é uma stack robusta, alinhada com a cadeira e adequada para produto web com Razor, controllers, services e EF Core |
| Porque SQLite? | Porque permite LocalDB simples, persistente e portátil para MacBook/Docker, podendo evoluir para SQL Server/PostgreSQL |
| OAuth funciona mesmo? | A integração está implementada e ativada por credenciais reais em `.env`; sem credenciais, o provider fica escondido e marcado como pendente |
| Apple é obrigatório? | Está preparado, mas depende de Apple Developer para credenciais reais; Google/Microsoft são mais simples de ativar |
| Como o Smart Save decide? | Usa veículo, consumo, velocidade, autonomia, postos e margem mínima de chegada |
| Porque a parte social é relevante? | Permite transformar planeamento individual em deslocação colaborativa com mensagens, contactos e viagens conjuntas |

## 8. Conclusão

O OptiDrive está pronto para ser apresentado como produto académico completo: funcional, coerente, documentado, com UX cuidada, autenticação forte, social valorizado, Docker, base persistente e rastreabilidade em Jira/Confluence.
