# OptiDrive - Plano de Testes e Validacao

Estado do documento: versao final de validacao.

Data de revisao: 2026-06-30

## 1. Objetivo

Registar a estrategia de validacao final do OptiDrive, incluindo testes automatizados, build, Docker, smoke tests HTTP e checklist manual de apresentacao.

## 2. Testes automatizados

Foi criado o projeto de testes:

- `OptiDrive.Web.Tests/OptiDrive.Web.Tests.csproj`;
- `OptiDrive.Web.Tests/AppStateServiceTests.cs`;
- `OptiDrive.Web.Tests/SmartSaveServiceTests.cs`.

### 2.1 Casos cobertos

| ID | Componente | Caso de teste | Resultado esperado |
| --- | --- | --- | --- |
| TST-01 | `AppStateService` | adicionar veiculo para novo utilizador | veiculo criado, predefinido e com historico |
| TST-02 | `AppStateService` | aplicar rota a veiculo | rota aplicada, odometro incrementado e nivel atualizado |
| TST-03 | `SmartSaveService` | rota com autonomia suficiente | sem paragens obrigatorias, custo e recomendacao coerentes |
| TST-04 | `SmartSaveService` | EV com bateria baixa | paragens intermédias eletricas planeadas |
| TST-05 | `AppStateService` | enviar mensagem direta | conversa aparece no dashboard social |
| TST-06 | `AppStateService` | criar, aderir e iniciar viagem colaborativa | viagem passa para `Active` e gera mensagem |
| TST-07 | `AppStateService` | registo com password segura | password guardada com hash PBKDF2 e login valido |
| TST-08 | `AuthenticatorService`/`AppStateService` | ativar e validar MFA TOTP | codigo TOTP aceite e 2FA ativado |

## 3. Evidencia automatizada

Comando executado:

```bash
dotnet test OptiDrive.sln
```

Resultado final observado:

- `8` testes executados;
- `8` testes com sucesso;
- `0` falhas;
- `0` testes ignorados.

## 4. Build principal

Comando executado:

```bash
dotnet build OptiDrive.Web/OptiDrive.Web.csproj
```

Resultado:

- build concluido com sucesso;
- `0` erros;
- `0` warnings reportados.

## 5. Docker e LocalDB

Comandos executados:

```bash
docker compose build
docker compose up -d
```

Resultado:

- imagem Docker criada com sucesso;
- container `optidrive-web` ativo;
- porta exposta `5080 -> 8080`;
- SQLite persistido em volume local;
- endpoint `/health` operacional.

## 6. Smoke tests HTTP

Validacoes executadas por HTTP:

| Endpoint/Fluxo | Resultado |
| --- | --- |
| `GET /Home/Login` | pagina de login carrega |
| `POST /Home/Login` com anti-CSRF | sessao criada e redireciona para Perfil |
| `GET /Profile` | mostra seguranca, MFA e QR code |
| `GET /Admin` com admin | mostra readiness operacional |
| `GET /Social/Pulse` | devolve mensagens e estado social |
| `GET /health` | devolve `Healthy` |

## 7. Checklist manual de apresentacao

| Area | Validacao manual | Estado |
| --- | --- | --- |
| Login/Registo | entrar com conta principal e criar nova conta | validado |
| Perfil | visualizar seguranca, OAuth preparado e MFA | validado |
| Garagem | consultar fichas, editar estado e registar abastecimento/carga | validado |
| Planeamento | calcular rota, ver mapa, filtros e custo | validado |
| Smart Save | verificar paragens quando autonomia e baixa | validado |
| Aplicar rota | diminuir nivel do veiculo e criar historico | validado |
| Social | mensagens, contactos, grupos e viagens colaborativas | validado |
| Backoffice | readiness, sync status e reports | validado |
| Docker | correr app em `127.0.0.1:5080` | validado |

## 8. Riscos residuais

- Testes UI automatizados ainda nao foram integrados numa pipeline Playwright/Selenium.
- APIs externas podem variar em disponibilidade.
- OAuth depende de credenciais reais configuradas.

## 9. Conclusao

A validacao final sustenta uma apresentacao academica segura: o sistema compila, os testes passam, Docker funciona, os fluxos principais foram verificados e a documentacao esta alinhada com o estado real do produto.
