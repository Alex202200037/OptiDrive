# OptiDrive - Checklist de Fecho para Entrega Final

Estado do documento: checklist final para apresentacao.

Data de revisao: 2026-06-24

## 1. Objetivo

Este documento consolida o estado final do OptiDrive antes da apresentacao, substituindo o plano de fecho anterior por uma checklist de entrega.

## 2. Estado do produto

| Area | Estado | Observacao |
| --- | --- | --- |
| Stack principal ASP.NET Core MVC | Fechado | Projeto `OptiDrive.Web` em `.NET 8` |
| UI Razor responsiva | Fechado | Perfil, Garagem, Planeamento, Social, Backoffice |
| LocalDB SQLite | Fechado | EF Core com bootstrap de schema |
| Docker MacBook | Fechado | `docker compose build` e `docker compose up -d` |
| Autenticacao local | Fechado | hashing PBKDF2 e lockout |
| MFA Authenticator | Fechado | QR code TOTP e codigos de recuperacao |
| Google/Microsoft/Apple OAuth | Configurável | requer credenciais reais para ativação externa |
| APIs combustivel/carregadores | Fechado | DGEG e OpenChargeMap |
| Mapas/rotas | Fechado | Google Maps/Directions com fallback |
| Smart Save | Fechado | custos, autonomia, paragens e EV |
| Social | Fechado | perfis, mensagens, grupos, viagens e live pulse |
| Backoffice | Fechado | readiness, sync status e reports |
| Testes | Fechado | 8 testes automatizados |
| Confluence | Fechado | relatorios publicados no espaco `OptiDrive` |
| Jira | Fechado | backlog principal e issues finais em `Done` |

## 3. Comandos de validacao

```bash
dotnet build OptiDrive.Web/OptiDrive.Web.csproj
dotnet test OptiDrive.sln
docker compose build
docker compose up -d
```

URLs de verificacao:

```text
http://127.0.0.1:5080
http://127.0.0.1:5080/health
```

## 4. Contas de apresentacao

| Perfil | Email | Password | Objetivo |
| --- | --- | --- | --- |
| Utilizador apresentacao | `alexandre@optidrive.pt` | `opti2026` | fluxo normal da app |
| Administrador | `admin@optidrive.pt` | `admin123` | backoffice/readiness |
| Contacto social | `marta.costa@optidrive.pt` | `marta123` | social e mensagens |

## 5. Sequencia recomendada de apresentacao

1. Abrir `http://127.0.0.1:5080`.
2. Entrar com `alexandre@optidrive.pt`.
3. Mostrar Perfil e seguranca da conta.
4. Abrir Garagem e mostrar veiculos, nivel, abastecimento/carga e historico.
5. Abrir Planeamento e apresentar rota, mapa, filtros e Smart Save.
6. Aplicar uma rota ao veiculo e mostrar impacto no historico.
7. Abrir Social e mostrar contactos, mensagens e viagens colaborativas.
8. Entrar como admin e mostrar Backoffice/readiness.
9. Abrir `/health` como evidencia operacional.
10. Mostrar Confluence e Jira.

## 6. O que dizer sobre limitacoes

As limitacoes devem ser apresentadas como evolucao natural, nao como falha da entrega:

- OAuth externo depende de credenciais reais configuradas nos portais Google, Microsoft e Apple;
- portagens exatas dependem de uma fonte oficial/licenciada, sendo usado atualmente Google Directions para evitar portagens e uma estimativa de custo;
- WebSocket em tempo real pode substituir o live pulse numa versao comercial;
- CI/CD cloud pode ser adicionado numa fase DevOps.

## 7. Conclusao

O projeto esta fechado para apresentacao academica. A entrega tem produto funcional, relatorios, Jira, Confluence, testes e Docker. A mensagem principal deve ser que o OptiDrive e uma solucao funcional com maturidade de produto apresentavel.
