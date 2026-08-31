# OptiDrive - Guia rapido para reabrir o Azure

Este guia serve para repor a aplicacao quando o Azure App Service fica parado, apagado, sem deploy recente, sem subscricao ativa ou sem variaveis de ambiente.

## 0. Diagnostico atual

Verificacao efetuada em 29/08/2026:

| Item | Resultado | Acao necessaria |
| --- | --- | --- |
| URL publica anterior | O dominio anterior nao resolveu por DNS | Confirmar se a Web App ainda existe ou criar uma nova |
| Azure CLI local | Instalado e com sessao antiga | Fazer novo `az login` depois de ter uma subscricao ativa |
| Subscricao Azure | A subscricao antiga nao aparece como acessivel | Reativar creditos/subscricao ou selecionar a subscricao correta |
| Docker local | Docker CLI instalado | Abrir Docker Desktop antes de validar `docker compose` |
| Build .NET | Funcional | Manter como validacao antes de deploy |
| Testes xUnit | Funcionais | Executar antes de publicar |

## 1. Verificar estado no Azure Portal

1. Abrir o Azure Portal.
2. Ir a **App Services**.
3. Procurar pela Web App do OptiDrive.
4. Se existir, abrir a Web App e confirmar que o estado esta **Running**.
5. Se estiver parada, clicar em **Start**.
6. Se nao existir, criar nova Web App Linux com runtime **.NET 8**.
7. Abrir o dominio publico da aplicacao.

URL usada na entrega anterior:

```text
https://optidrive-d0dmcjfkhwebgtb3.westeurope-01.azurewebsites.net
```

Se esta URL nao abrir, usar o novo dominio apresentado em **App Service > Overview > Default domain**.

## 2. Se a aplicacao existir mas estiver com erro

1. No App Service, abrir **Log stream**.
2. Confirmar se existe erro de arranque, falta de runtime ou falta de configuracao.
3. Em **Settings > Environment variables**, confirmar as variaveis principais.
4. Fazer **Restart** depois de alterar variaveis.

Variaveis principais, sem colocar segredos no repositorio:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__OptiDrive=Data Source=/home/data/optidrive.db
GoogleMaps__ApiKey=<chave configurada no Azure>
OpenChargeMap__ApiKey=<chave configurada no Azure>
Authentication__Google__ClientId=<client id configurado>
Authentication__Google__ClientSecret=<secret configurado>
Authentication__Microsoft__ClientId=<client id configurado>
Authentication__Microsoft__ClientSecret=<secret configurado>
```

## 3. Redeploy manual com Azure CLI

Pre-requisitos:

```bash
brew install azure-cli
az login
az account list --output table
az account set --subscription "<nome-ou-id-da-subscricao>"
```

Gerar pacote:

```bash
dotnet publish OptiDrive.Web/OptiDrive.Web.csproj -c Release -o artifacts/azure-publish /p:UseAppHost=false
cd artifacts/azure-publish
zip -qr ../OptiDrive.Azure.zip .
cd ../..
```

Enviar para Azure, ajustando o nome da app se for criada outra:

```bash
APP_NAME=<nome-da-web-app> \
AZURE_RESOURCE_GROUP=rg-optidrive-esa \
AZURE_APP_SERVICE_PLAN=asp-optidrive-esa \
./tools/azure_deploy.sh
```

O script carrega automaticamente o ficheiro `.env` local quando existir, sem guardar segredos no Git.

## 4. Deploy por GitHub Actions

Foram adicionados dois workflows:

| Workflow | Ficheiro | Finalidade |
| --- | --- | --- |
| OptiDrive CI | `.github/workflows/ci.yml` | Restore, build, testes, publish e Docker build |
| OptiDrive Azure Deploy | `.github/workflows/azure-deploy.yml` | Deploy para Azure App Service |

Configurar no GitHub:

1. Ir a **Settings > Secrets and variables > Actions**.
2. Criar variavel:

```text
AZURE_WEBAPP_NAME=<nome da Web App Azure>
```

3. Criar secret:

```text
AZURE_WEBAPP_PUBLISH_PROFILE=<conteudo do Publish Profile descarregado do Azure>
```

4. No Azure Portal, em **App Service > Overview**, clicar em **Download publish profile**.
5. Copiar o conteudo do ficheiro para o secret do GitHub.
6. Ir a **Actions > OptiDrive Azure Deploy > Run workflow**.

## 5. Validacao pos-deploy

Validar estes caminhos:

```text
/health
/Admin
/Home/Login
/Garage
/Planning
/Social
```

Contas de apresentacao:

```text
Utilizador: alexandre@optidrive.pt / opti2026
Admin: admin@optidrive.pt / admin123
```

## 6. Como explicar na defesa

O deploy passa a ter uma pipeline verificavel: o codigo e compilado, os testes automaticos sao executados, o pacote e publicado e a versao pode ser promovida para Azure atraves de GitHub Actions. O endpoint `/health` permite confirmar rapidamente se a aplicacao esta operacional depois do deploy.
