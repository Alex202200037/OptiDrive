# OptiDrive - Readiness de Produto e Configuração

| Campo | Valor |
| --- | --- |
| Documento | Checklist operacional e configuração |
| Versão | 2.0 |
| Data | 30/06/2026 |
| Estado | Pronto para publicação |

## 1. Objetivo

Este documento consolida a configuração necessária para apresentar o OptiDrive como produto funcional, incluindo Docker, LocalDB SQLite, APIs externas, OAuth, Authenticator, healthcheck e backoffice de readiness.

## 2. Checklist Operacional

| Área | Estado esperado | Evidência |
| --- | --- | --- |
| Build .NET | OK | `dotnet build OptiDrive.sln` |
| Testes automatizados | OK | `dotnet test OptiDrive.sln` |
| Docker | OK | `docker compose up -d --build` |
| SQLite persistente | OK | Volume `./data:/app/data` |
| Healthcheck | OK | `http://127.0.0.1:5080/health` |
| Google Maps | Configurável | `GOOGLE_MAPS_API_KEY` |
| OpenChargeMap | Configurável | `OPEN_CHARGE_MAP_API_KEY` |
| Google OAuth | Configurável | `GOOGLE_OAUTH_CLIENT_ID`, `GOOGLE_OAUTH_CLIENT_SECRET` |
| Microsoft OAuth | Configurável | `MICROSOFT_OAUTH_CLIENT_ID`, `MICROSOFT_OAUTH_CLIENT_SECRET` |
| Apple OAuth | Indisponível neste ambiente | Página explicativa e integração técnica preparada |
| Authenticator MFA | Operacional | QR code + TOTP + recovery codes |
| Social | Operacional | Perfis, mensagens e viagens colaborativas |

## 3. Execução em Docker

```bash
cp .env.example .env
docker compose up -d --build
```

Abrir:

```text
http://127.0.0.1:5080
```

## 4. Execução Local sem Docker

```bash
dotnet run --project OptiDrive.Web/OptiDrive.Web.csproj --urls http://127.0.0.1:5088
```

Abrir:

```text
http://127.0.0.1:5088
```

## 5. Variáveis de Ambiente

```text
GOOGLE_MAPS_API_KEY=
OPEN_CHARGE_MAP_API_KEY=
GOOGLE_OAUTH_CLIENT_ID=
GOOGLE_OAUTH_CLIENT_SECRET=
MICROSOFT_OAUTH_CLIENT_ID=
MICROSOFT_OAUTH_CLIENT_SECRET=
APPLE_OAUTH_CLIENT_ID=
APPLE_OAUTH_CLIENT_SECRET=
APPLE_OAUTH_TEAM_ID=
APPLE_OAUTH_KEY_ID=
APPLE_OAUTH_PRIVATE_KEY_PATH=
```

## 6. Redirect URLs OAuth

| Provider | Docker | Local |
| --- | --- | --- |
| Google | `http://127.0.0.1:5080/signin-google` | `http://127.0.0.1:5088/signin-google` |
| Microsoft | `http://127.0.0.1:5080/signin-microsoft` | `http://127.0.0.1:5088/signin-microsoft` |
| Apple | `http://127.0.0.1:5080/signin-apple` | `http://127.0.0.1:5088/signin-apple` |

## 7. Healthcheck

O endpoint `/health` devolve um JSON com:

| Campo | Significado |
| --- | --- |
| `status` | Estado global da aplicação |
| `environment` | Ambiente ASP.NET Core |
| `counts.users` | Número de utilizadores carregados |
| `counts.vehicles` | Número de veículos |
| `counts.routes` | Número de rotas |
| `integrations.googleMaps` | API key de mapas configurada |
| `integrations.openChargeMap` | OpenChargeMap configurada |
| `integrations.googleOAuth` | Google OAuth configurado |
| `integrations.microsoftOAuth` | Microsoft OAuth configurado |
| `integrations.appleOAuth` | Apple OAuth configurado |
| `integrations.authenticatorMfa` | MFA operacional |
| `integrations.socialPulse` | Social operacional |

## 8. Estado dos Providers de Login

| Provider | Estado atual | Observação |
| --- | --- | --- |
| Google | Configurado por `.env` | Requer redirect URI configurado no Google Cloud Console |
| Microsoft | Configurado por `.env` | App Registration criada no Azure/Entra com redirect URIs locais |
| Apple | Indisponível neste ambiente | A UI apresenta página explicativa e mantém integração técnica preparada |

## 9. Backoffice

O backoffice apresenta cartões de readiness para runtime, base de dados, mapas, OpenChargeMap, OAuth, MFA e social online. Esta área permite apresentar maturidade operacional e confirmar visualmente quais integrações estão ativas.

## 10. Riscos e Mitigações

| Risco | Mitigação |
| --- | --- |
| OAuth sem credenciais reais | Botões escondidos e readiness pendente |
| API externa indisponível | Fallback local/cache e mensagens controladas |
| Base vazia na apresentação | Dataset realista carregado no bootstrap |
| Ambiente diferente do MacBook | Docker Compose reproduz dependências |
| Falha de Authenticator | Recovery codes e reset de MFA |

## 11. Conclusão

A configuração final permite apresentar o OptiDrive como produto funcional, com execução reproduzível, persistência, autenticação forte, integrações reais e observabilidade suficiente para defesa académica.
