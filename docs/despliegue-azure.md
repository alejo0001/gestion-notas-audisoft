# Despliegue en Azure (una sola vez)

Guía para crear la infraestructura de la demo pública. Después de esto, **cada `git push` a `main` despliega solo** mediante GitHub Actions (`.github/workflows/ci-cd.yml`). Decisión y costos en el [ADR 0007](decisions/0007-despliegue-azure-github-actions.md).

> Para trabajar en local o evaluar la prueba **no se necesita nada de esto** (ni Docker, ni Azure): ver el README.

## Arquitectura

```
Navegador ──► Azure Static Web Apps (Angular, plan Free)
                 │  HTTPS
                 ▼
          Azure Container Apps (API .NET 10, escala a 0)
                 │
                 ▼
          Azure SQL Database (oferta gratuita, serverless)

GitHub Actions: pruebas → imagen en ghcr.io → migraciones → Container App → Static Web App
```

## 0. Requisitos

- Suscripción de Azure (Pay-As-You-Go sirve).
- Usar **Azure Cloud Shell** en modo **Bash** (icono `>_` arriba en portal.azure.com): ya trae la CLI de Azure y la sesión iniciada.

## 1. Variables

Copie y pegue, cambiando **solo** las tres primeras líneas. El nombre del servidor SQL debe ser único en todo Azure (use sus iniciales o un número). La contraseña: mínimo 12 caracteres con mayúsculas, minúsculas y números; **sin símbolos** para evitar problemas con Bash.

```bash
SQL_SERVER=sql-gestion-notas-XXXX          # único global, solo minúsculas, números y guiones
SQL_ADMIN=adminnotas
SQL_PASS='CambieEstaClave2026'

RG=rg-gestion-notas
LOC=eastus2
SWA=swa-gestion-notas
CAE=cae-gestion-notas
API=ca-gestion-notas-api
GH_REPO=alejo0001/gestion-notas-audisoft
```

## 2. Proveedores y grupo de recursos

```bash
for p in Microsoft.Sql Microsoft.Web Microsoft.App Microsoft.OperationalInsights; do
  az provider register --namespace $p --wait
done
az group create --name $RG --location $LOC
```

## 3. Base de datos (oferta gratuita)

```bash
az sql server create -g $RG -n $SQL_SERVER -l $LOC \
  --admin-user $SQL_ADMIN --admin-password "$SQL_PASS"

# Permite conexiones desde servicios de Azure (el Container App).
az sql server firewall-rule create -g $RG -s $SQL_SERVER -n AllowAzureServices \
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# Serverless con límite gratuito: si se agota el cupo mensual, se PAUSA (no cobra).
az sql db create -g $RG -s $SQL_SERVER -n GestionNotas \
  -e GeneralPurpose -f Gen5 -c 2 --compute-model Serverless \
  --use-free-limit --free-limit-exhaustion-behavior AutoPause

SQL_CONN="Server=tcp:$SQL_SERVER.database.windows.net,1433;Initial Catalog=GestionNotas;User ID=$SQL_ADMIN;Password=$SQL_PASS;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;"
```

`Connection Timeout=60`: la base serverless se "duerme" sin uso y tarda unos segundos en despertar.

## 4. Frontend: Static Web App (plan Free)

```bash
az staticwebapp create -n $SWA -g $RG -l $LOC --sku Free
SWA_HOST=$(az staticwebapp show -n $SWA -g $RG --query defaultHostname -o tsv)
SWA_TOKEN=$(az staticwebapp secrets list -n $SWA -g $RG --query properties.apiKey -o tsv)
```

## 5. API: Container Apps (plan Consumption)

```bash
# Entorno sin Log Analytics (--logs-destination none) para no generar costos de logs.
az containerapp env create -n $CAE -g $RG -l $LOC --logs-destination none

# Se crea con una imagen temporal; el pipeline la reemplaza por la del API en el primer despliegue.
az containerapp create -n $API -g $RG --environment $CAE \
  --image mcr.microsoft.com/k8se/quickstart:latest \
  --target-port 8080 --ingress external \
  --min-replicas 0 --max-replicas 1 --cpu 0.25 --memory 0.5Gi \
  --secrets sql-conn="$SQL_CONN" \
  --env-vars ConnectionStrings__GestionNotas=secretref:sql-conn \
             Cors__AllowedOrigins__0=https://$SWA_HOST \
             Swagger__Enabled=true

API_HOST=$(az containerapp show -n $API -g $RG --query properties.configuration.ingress.fqdn -o tsv)
```

- `min-replicas 0`: sin tráfico no hay réplicas ni cobro (la primera petición tarda unos segundos).
- La cadena de conexión va como **secret** del Container App, no en el código.

## 6. Identidad para GitHub Actions (OIDC, sin contraseñas)

GitHub se autentica en Azure con un token temporal que Azure valida por confianza federada; no se guarda ninguna clave de Azure en GitHub.

```bash
APP_ID=$(az ad app create --display-name gh-gestion-notas --query appId -o tsv)
az ad sp create --id $APP_ID
az role assignment create --assignee $APP_ID --role Contributor \
  --scope $(az group show -n $RG --query id -o tsv)

az ad app federated-credential create --id $APP_ID --parameters "{
  \"name\": \"github-produccion\",
  \"issuer\": \"https://token.actions.githubusercontent.com\",
  \"subject\": \"repo:$GH_REPO:environment:produccion\",
  \"audiences\": [\"api://AzureADTokenExchange\"]
}"
```

El rol **Contributor** queda limitado al grupo de recursos `rg-gestion-notas`, no a toda la suscripción.

## 7. Valores para GitHub

```bash
echo "===== SECRETS ====="
echo "AZURE_CLIENT_ID=$APP_ID"
echo "AZURE_TENANT_ID=$(az account show --query tenantId -o tsv)"
echo "AZURE_SUBSCRIPTION_ID=$(az account show --query id -o tsv)"
echo "SQL_CONNECTION_STRING=$SQL_CONN"
echo "SWA_DEPLOYMENT_TOKEN=$SWA_TOKEN"
echo "===== VARIABLES ====="
echo "AZURE_RESOURCE_GROUP=$RG"
echo "SQL_SERVER_NAME=$SQL_SERVER"
echo "CONTAINER_APP_NAME=$API"
echo "API_URL=https://$API_HOST/api"
echo "===== URLS ====="
echo "Frontend: https://$SWA_HOST"
echo "Swagger:  https://$API_HOST/swagger"
```

En GitHub: **Settings → Secrets and variables → Actions**
- Pestaña **Secrets** → *New repository secret* para cada valor de la sección SECRETS.
- Pestaña **Variables** → *New repository variable* para cada valor de la sección VARIABLES.

## 8. Primer despliegue

1. GitHub → **Actions** → *CI/CD* → **Run workflow** (o haga un push a `main`).
2. La primera vez, el job *Desplegar API* falla en el último paso: la imagen en `ghcr.io` se crea **privada** y Azure no puede descargarla. Hágala pública una sola vez: GitHub → su perfil → **Packages** → `gestion-notas-api` → *Package settings* → *Change visibility* → **Public**.
3. Vuelva a Actions y pulse **Re-run failed jobs**.
4. Abra la URL del frontend. La primera carga puede tardar hasta un minuto (API y base de datos despiertan).

## Costos y control

- Los tres servicios usan planes/cupos gratuitos (ver ADR 0007). Recomendado: **Cost Management → Budgets** con alerta de US$5.
- Para borrar todo: `az group delete -n rg-gestion-notas --yes` (y la app registration `gh-gestion-notas` en Microsoft Entra ID).
