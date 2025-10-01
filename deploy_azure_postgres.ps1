<#
deploy_azure_postgres.ps1 (corrigido)
- Usa subscription correta
- Registra Microsoft.Web
- Cria PostgreSQL Flexible Server com --tier Burstable + SKU válido
- Corrige captura de IP
- Garante execução na pasta do .csproj
- Publica self-contained e faz deploy no App Service
#>

# ======= CONFIGURE AQUI =======
$SUBSCRIPTION = "693a4857-146a-4cc1-8bc6-eceaf74cef2b"   # sua subscription correta
$RG   = "rg-sprint3-dotnet"
$LOC  = "brazilsouth"
$PLAN = "asp-sprint3-dotnet"
$WEB  = "app-sprint3-dotnet"

# Postgres (conforme você informou)
$PG_DB    = "mottu_frota"
$PG_ADMIN = "postgres"
$PG_PASS  = "postgres"   # apenas teste; não commitar isso em produção

# criar Postgres no Azure? (true recomendado p/ App Service)
$CREATE_AZURE_PG = $true

# ======= INTERNOS (não precisa mexer) =======
$PG_SERVER  = "pg-sprint3-$(Get-Random)"
$PG_VERSION = "16"
$SKU_NAME   = "Standard_B1ms"   # Burstable
# IP público para firewall
try {
  $MY_IP = (Invoke-RestMethod -Uri "https://ifconfig.me/ip" -ErrorAction Stop).Trim()
} catch { $MY_IP = "0.0.0.0" }

Write-Host "==> Iniciando deploy (Azure Postgres Flexible Server + App Service)..." -ForegroundColor Cyan

# Precisa estar na pasta do projeto (.csproj)
$csproj = Get-ChildItem -Filter *.csproj -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $csproj) {
  Write-Error "Não encontrei um .csproj na pasta atual. Abra o PowerShell NA PASTA do projeto (.csproj) e rode de novo."
  exit 1
}

# Login + Subscription
az login
az account set -s $SUBSCRIPTION

# Registrar provider Microsoft.Web
Write-Host "==> Registrando Microsoft.Web (se necessário)..." -ForegroundColor Green
az provider register --namespace Microsoft.Web | Out-Null
az provider register --namespace Microsoft.Web --wait

# RG
Write-Host "==> Criando Resource Group (se não existir): $RG ..." -ForegroundColor Green
az group create --name $RG --location $LOC | Out-Null

# App Service Plan + WebApp
Write-Host "==> Criando App Service Plan e WebApp..." -ForegroundColor Green
az appservice plan create --resource-group $RG --name $PLAN --sku B1 --is-linux | Out-Null
az webapp create --resource-group $RG --plan $PLAN --name $WEB --runtime "DOTNETCORE:8.0" | Out-Null

# PostgreSQL Flexible
if ($CREATE_AZURE_PG) {
  Write-Host "==> Criando PostgreSQL Flexible Server: $PG_SERVER (DB: $PG_DB) ..." -ForegroundColor Green
  az postgres flexible-server create `
    --resource-group $RG `
    --name $PG_SERVER `
    --location $LOC `
    --admin-user $PG_ADMIN `
    --admin-password $PG_PASS `
    --tier Burstable `
    --sku-name $SKU_NAME `
    --version $PG_VERSION `
    --storage-size 32 | Out-Null

  Write-Host "==> Regra de firewall para seu IP: $MY_IP" -ForegroundColor Green
  az postgres flexible-server firewall-rule create `
    --resource-group $RG `
    --server-name $PG_SERVER `
    --name allow_local_ip `
    --start-ip-address $MY_IP `
    --end-ip-address $MY_IP | Out-Null

  $PG_FQDN = "$PG_SERVER.postgres.database.azure.com"
  $CONN = "Host=$PG_FQDN;Port=5432;Database=$PG_DB;Username=$PG_ADMIN@$PG_SERVER;Password=$PG_PASS;Ssl Mode=Require;"

  Write-Host "==> App Settings: ConnectionStrings__DefaultConnection (Azure Postgres)..." -ForegroundColor Green
  az webapp config appsettings set --resource-group $RG --name $WEB --settings `
    "ConnectionStrings__DefaultConnection=$CONN" `
    "ASPNETCORE_ENVIRONMENT=Production" | Out-Null

  Write-Host "==> Azure Postgres criado. Host: $PG_FQDN" -ForegroundColor Cyan
} else {
  Write-Host "==> MODO LOCAL: pulando criação do Azure Postgres (App Service NÃO acessa localhost)." -ForegroundColor Yellow
  $LOCAL_CONN = "Host=localhost;Port=5432;Database=mottu_frota;Username=postgres;Password=postgres"
  az webapp config appsettings set --resource-group $RG --name $WEB --settings `
    "ConnectionStrings__DefaultConnection=$LOCAL_CONN" `
    "ASPNETCORE_ENVIRONMENT=Production" | Out-Null
}

# Build/Publish self-contained
Write-Host "==> dotnet restore/build/publish..." -ForegroundColor Green
dotnet restore
dotnet build -c Release
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish

# Zip & Deploy
if (-not (Test-Path .\publish)) {
  Write-Error "Pasta 'publish' não encontrada. O build/publish falhou? Verifique mensagens acima."
  exit 1
}

Write-Host "==> Zipando e fazendo deploy no App Service..." -ForegroundColor Green
if (Test-Path .\app.zip) { Remove-Item .\app.zip -Force }
Compress-Archive -Path ".\publish\*" -DestinationPath ".\app.zip" -Force
az webapp deploy --resource-group $RG --name $WEB --type zip --src-path ".\app.zip" | Out-Null

$APP_HOST = az webapp show --resource-group $RG --name $WEB --query defaultHostName -o tsv
Write-Host "==> Deploy concluído. URL pública: https://$APP_HOST" -ForegroundColor Cyan

if ($CREATE_AZURE_PG) {
  Write-Host "==> PostgreSQL Azure:" -ForegroundColor Cyan
  Write-Host "   Host: $PG_FQDN"
  Write-Host "   DB:   $PG_DB"
  Write-Host "   User: $PG_ADMIN@$PG_SERVER"
}

Write-Host "==> FIM. (Dica: rode 'dotnet ef database update' apontando para a Azure para criar as tabelas)" -ForegroundColor Magenta
