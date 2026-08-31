#!/usr/bin/env bash
set -euo pipefail

# OptiDrive Azure App Service deployment helper.
# Usage:
#   az login
#   APP_NAME=optidrive-202200037 ./tools/azure_deploy.sh

load_dotenv() {
  local env_file=".env"
  if [ ! -f "$env_file" ]; then
    return
  fi

  while IFS='=' read -r key value; do
    key="$(echo "${key:-}" | xargs)"
    if [ -z "$key" ] || [[ "$key" == \#* ]]; then
      continue
    fi

    value="$(echo "${value:-}" | sed -e 's/^"//' -e 's/"$//' -e \"s/^'//\" -e \"s/'$//\")"
    if [ -z "${!key:-}" ]; then
      export "$key=$value"
    fi
  done < "$env_file"
}

load_dotenv

RG="${AZURE_RESOURCE_GROUP:-rg-optidrive-esa}"
LOCATION="${AZURE_LOCATION:-westeurope}"
PLAN="${AZURE_APP_SERVICE_PLAN:-asp-optidrive-esa}"
APP_NAME="${APP_NAME:-optidrive-202200037}"
SKU="${AZURE_SKU:-B1}"
ZIP_PATH="${ZIP_PATH:-artifacts/OptiDrive.Azure.zip}"

if ! command -v az >/dev/null 2>&1; then
  echo "Azure CLI not found. Install it first: brew install azure-cli" >&2
  exit 1
fi

if ! az account show >/dev/null 2>&1; then
  echo "Azure account not logged in. Run: az login" >&2
  exit 1
fi

if [ ! -f "$ZIP_PATH" ]; then
  echo "Publish ZIP not found at $ZIP_PATH. Building it now..."
  rm -rf artifacts/azure-publish "$ZIP_PATH"
  dotnet publish OptiDrive.Web/OptiDrive.Web.csproj -c Release -o artifacts/azure-publish /p:UseAppHost=false
  (cd artifacts/azure-publish && zip -qr ../OptiDrive.Azure.zip .)
fi

echo "Creating/updating resource group: $RG ($LOCATION)"
az group create --name "$RG" --location "$LOCATION" >/dev/null

echo "Creating/updating App Service plan: $PLAN ($SKU Linux)"
az appservice plan create \
  --name "$PLAN" \
  --resource-group "$RG" \
  --location "$LOCATION" \
  --sku "$SKU" \
  --is-linux >/dev/null

echo "Creating/updating Web App: $APP_NAME"
if ! az webapp show --resource-group "$RG" --name "$APP_NAME" >/dev/null 2>&1; then
  az webapp create \
    --resource-group "$RG" \
    --plan "$PLAN" \
    --name "$APP_NAME" \
    --runtime "DOTNETCORE:8.0" >/dev/null
fi

echo "Configuring application settings"
az webapp config appsettings set \
  --resource-group "$RG" \
  --name "$APP_NAME" \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__OptiDrive='Data Source=/home/data/optidrive.db' \
    GoogleMaps__ApiKey="${GOOGLE_MAPS_API_KEY:-}" \
    OpenChargeMap__ApiKey="${OPEN_CHARGE_MAP_API_KEY:-}" \
    Authentication__Google__ClientId="${GOOGLE_OAUTH_CLIENT_ID:-}" \
    Authentication__Google__ClientSecret="${GOOGLE_OAUTH_CLIENT_SECRET:-}" \
    Authentication__Microsoft__ClientId="${MICROSOFT_OAUTH_CLIENT_ID:-}" \
    Authentication__Microsoft__ClientSecret="${MICROSOFT_OAUTH_CLIENT_SECRET:-}" >/dev/null

echo "Deploying ZIP: $ZIP_PATH"
az webapp deploy \
  --resource-group "$RG" \
  --name "$APP_NAME" \
  --src-path "$ZIP_PATH" \
  --type zip \
  --restart true >/dev/null

URL="https://${APP_NAME}.azurewebsites.net"
echo "Deployment complete: $URL"
echo "Healthcheck: $URL/health"
