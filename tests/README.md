# Pruebas de Estrés con k6 en AKS

Esta carpeta contiene la prueba de estrés (`stress-test.js`) y los manifiestos necesarios para ejecutarla **dentro del cluster AKS** como un Kubernetes Job.

## Contenido

| Archivo | Descripción |
|---------|-------------|
| `stress-test.js` | Script k6: rampa 50 → 100 VUs durante 5 minutos. Hace `GET /api/products`, `GET /api/products/stats`, `POST /api/products`, `GET /api/products/{id}` y `DELETE /api/products/{id}`. |
| `k6-job.yaml` | Manifiesto del Job que ejecuta `grafana/k6:0.54.0` y monta el script desde un ConfigMap. |
| `run-k6-aks.ps1` | Script de PowerShell que regenera el ConfigMap desde `stress-test.js`, reemplaza el Job y muestra los logs en vivo. |

## Requisitos previos

- `kubectl` instalado y configurado.
- `az` CLI autenticado contra la suscripción correcta.
- Credenciales del cluster AKS:
  ```powershell
  az aks get-credentials --resource-group productapi-rg --name productapi-aks
  ```
- El servicio `productapi-productapi` debe estar desplegado en el namespace `productapi` (despliegue normal vía Helm/ArgoCD).

## Ejecución rápida (recomendada)

```powershell
./tests/run-k6-aks.ps1
```

El script hace lo siguiente:

1. Crea el namespace `productapi` si no existe.
2. Elimina el ConfigMap anterior y lo recrea desde `stress-test.js` (fuente única de verdad).
3. Borra el Job anterior si existe (los Jobs son inmutables).
4. Aplica el Job con `TARGET_URL` apuntando al servicio interno del cluster.
5. Espera a que el pod arranque y hace `kubectl logs -f` hasta que termine.

### Cambiar el objetivo de la prueba

Por defecto la prueba ataca el servicio interno (sin pasar por NGINX Ingress ni LoadBalancer):

```
http://productapi-productapi.productapi.svc.cluster.local
```

Para estresar también el ingreso público (NGINX + Azure LoadBalancer), pasa `-TargetUrl`:

```powershell
./tests/run-k6-aks.ps1 -TargetUrl http://productapi-mpn.centralus.cloudapp.azure.com
```

### Cambiar de namespace

```powershell
./tests/run-k6-aks.ps1 -Namespace k6
```

## Ejecución manual (sin el script de PowerShell)

```powershell
# 1. Crear el namespace si no existe
kubectl create namespace productapi --dry-run=client -o yaml | kubectl apply -f -

# 2. (Re)crear el ConfigMap con el script
kubectl -n productapi delete configmap k6-stress-test-script --ignore-not-found
kubectl -n productapi create configmap k6-stress-test-script `
  --from-file=stress-test.js=tests/stress-test.js

# 3. (Re)crear el Job
kubectl -n productapi delete job k6-stress-test --ignore-not-found
kubectl apply -f tests/k6-job.yaml

# 4. Ver los resultados en vivo
kubectl -n productapi logs -f job/k6-stress-test
```

## Monitoreo durante la prueba

En otra terminal, observa el escalado automático del HPA (configurado en 2–5 réplicas a 80 % CPU):

```powershell
kubectl -n productapi get hpa -w
kubectl -n productapi get pods -w
```

Las métricas se ven también en Grafana (dashboard de ProductAPI) y en Datadog/Prometheus, gracias a la observabilidad ya integrada en el cluster.

## Umbrales (thresholds) configurados en `stress-test.js`

- `http_req_failed: rate<0.01` — tasa de errores menor al 1 %.
- `http_req_duration: p(95)<1000` — el 95 % de las requests por debajo de 1 segundo.

Si la prueba falla los umbrales, k6 termina con código distinto a 0 y el Job aparece como **Failed**. Revisa la salida con:

```powershell
kubectl -n productapi describe job k6-stress-test
kubectl -n productapi logs job/k6-stress-test
```

## Limpieza

```powershell
kubectl -n productapi delete job k6-stress-test --ignore-not-found
kubectl -n productapi delete configmap k6-stress-test-script --ignore-not-found
```

El Job se autodestruye también de forma automática a la hora de terminar (`ttlSecondsAfterFinished: 3600`).

## Notas

- El Job corre en **un solo pod** k6. Es suficiente para los ~100 VUs configurados.
- Para cargas superiores a ~1 000 VUs, conviene migrar al [k6-operator](https://github.com/grafana/k6-operator) para ejecutar runners distribuidos.
- El objetivo interno (Service DNS) prueba solo los pods de la aplicación; el objetivo FQDN público prueba además el camino completo (NGINX Ingress → LoadBalancer → pods).
