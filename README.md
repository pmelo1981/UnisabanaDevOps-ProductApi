Presentación de la Actividad

Asignatura: Fundamentos de DevOps

Profesora: María Fernanda Ochoa Paipilla

Integrantes del grupo:
- Pablo Andrés Melo García
- Camilo Andres Padilla Garcia
- Jorge Ivan Figueroa
- Jorge Andres Ayala Valero

API REST para gestión de productos con despliegue automatizado en Kubernetes/AKS, Helm, ArgoCD y CI/CD.

**Acceso en vivo:** http://productapi-mpn.centralus.cloudapp.azure.com/api/products
.
---

## Tecnologías

- **.NET 10** - Framework
- **Docker** - Containerización (multietapa)
- **Kubernetes/AKS** - Orquestación
- **Helm 3** - Gestión de configuración
- **NGINX Ingress Controller** - Enrutamiento HTTP(S)
- **ArgoCD** - GitOps automático
- **GitHub Actions** - CI/CD (Build → Test → Docker Push → Auto-deploy)
- **Azure Container Registry** - Registry privado de imágenes
- **SonarQube Community** - Análisis de calidad y coverage
- **Datadog** - Observabilidad cloud y monitoreo AKS
- **Prometheus & Grafana** - Métricas, dashboards y alertas
- **Snyk** - Escaneo de vulnerabilidades y DevSecOps

---

## API REST - 7 Endpoints

```
GET    /api/products              # Obtener todos los productos
GET    /api/products/{id}         # Obtener por ID
GET    /api/products/stats        # Estadísticas (total, promedio, máximo, mínimo)
POST   /api/products              # Crear nuevo producto
PUT    /api/products/{id}         # Actualizar producto
DELETE /api/products/{id}         # Eliminar producto
GET    /api/products/health       # Health check
```

---

## Descripción

Microservicio simple en ASP.NET Core 10 que expone una API REST para gestionar productos.

- 7 endpoints REST (CRUD + stats + health)
- 15 tests unitarios (xUnit)
- Dockerfile multistage (~150MB)
- Helm Charts con values.yaml + values-acr.yaml
- GitHub Actions CI/CD (ACR push automático)
- SonarQube integrado en CI/CD
- Observabilidad centralizada con Datadog
- Métricas y dashboards con Prometheus + Grafana
- Escaneo de seguridad con Snyk
- Despliega automáticamente en AKS vía ArgoCD

---

## Estructura

```
src/
├── ProductAPI/
│   ├── Program.cs                      # Entry point, DI, Swagger
│   ├── Controllers/ProductsController.cs    # 7 endpoints REST
│   ├── Models/Product.cs               # Domain model
│   └── Repositories/ProductRepository.cs    # In-memory storage
└── ProductAPI.Tests/
    └── ProductsControllerTests.cs      # 15 tests (xUnit)

docker/
└── Dockerfile                          # Multistage: sdk → aspnet runtime

helm/
├── Chart.yaml
├── values.yaml                         # Default values
├── values-acr.yaml                     # ACR overrides (image tag versionado)
└── templates/
    ├── deployment.yaml
    ├── service.yaml
    ├── hpa.yaml                        # Horizontal Pod Autoscaler
    └── ingress.yaml                    # NGINX Ingress

.github/workflows/
└── ci-cd.yml                           # Build → Test → Docker Push ACR → Update tag → Push

azure/
├── setup-argocd.ps1                    # Install ArgoCD + apply manifests
├── create-aks-cluster.ps1              # Crear cluster
├── setup-acr-and-deploy.ps1            # Setup ACR
├── verify-deploy.ps1                   # Verificar despliegue
└── delete-all-resources.ps1            # Limpiar

README.md                               # Este archivo
```

---

## Acceso en Vivo

**Base URL (PRODUCCIÓN - FQDN permanente):**
```
http://productapi-mpn.centralus.cloudapp.azure.com
```

### Ejemplos de uso

**Health check:**
```bash
curl http://productapi-mpn.centralus.cloudapp.azure.com/api/products/health
```

**Listar todos los productos:**
```bash
curl http://productapi-mpn.centralus.cloudapp.azure.com/api/products
```

**Obtener estadísticas:**
```bash
curl http://productapi-mpn.centralus.cloudapp.azure.com/api/products/stats
```

**Crear producto:**
```bash
curl -X POST http://productapi-mpn.centralus.cloudapp.azure.com/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop","description":"Gaming laptop","price":1299.99}'
```

**Swagger UI (documentación interactiva):**
```
http://productapi-mpn.centralus.cloudapp.azure.com/swagger
```

---

## Quick Start (Local)

### Tests

```bash
dotnet test
# Output: 15 passed OK
```

### Ejecucion Local

```bash
dotnet run --project src/ProductAPI/ProductAPI.csproj
# Swagger: http://localhost:5034/swagger
```

### Docker Local

```bash
docker build -f docker/Dockerfile -t productapi:local .
docker run -p 8080:8080 productapi:local
# Probar: curl http://localhost:8080/api/products/health
```

---

## API Endpoints Detallados

| Metodo | Endpoint | Body | Descripcion |
|--------|----------|------|------------|
| GET | `/api/products` | - | Lista todos los productos |
| GET | `/api/products/{id}` | - | Obtiene producto por ID |
| GET | `/api/products/stats` | - | Estadisticas: total, promedio, maximo, minimo |
| POST | `/api/products` | `{name, description, price}` | Crear nuevo |
| PUT | `/api/products/{id}` | `{name, description, price}` | Actualizar |
| DELETE | `/api/products/{id}` | - | Eliminar |
| GET | `/api/products/health` | - | Status de salud |

**Ejemplo de respuesta `/stats`:**
```json
{
  "total": 5,
  "promedio": 299.99,
  "maximo": 999.99,
  "minimo": 9.99
}
```
---

## CI/CD Pipeline (GitHub Actions)

El pipeline se dispara automáticamente al hacer `git push` en `main`:

```
1. Checkout codigo
2. Setup .NET 10
3. dotnet restore (NuGet)
4. dotnet build -c Release
5. dotnet test (15 tests)
6. SonarQube analysis + Quality Scan
7. Snyk Open Source Scan
8. Snyk Container Scan
9. Login a Azure Container Registry
10. docker build -f docker/Dockerfile
11. docker push → ACR (tag: git SHA + latest)
12. sed actualiza values-acr.yaml con nuevo tag
13. git push automatico
    ↓
    ArgoCD detecta (cada 3 min)
    ↓
    helm upgrade en Kubernetes
    ↓
    Rolling update (zero-downtime)
    ↓
    Datadog monitorea logs, métricas y traces
    ↓
    Prometheus recolecta métricas
    ↓
    Grafana visualiza dashboards y alertas
```

**No se necesita Docker Desktop.** Todo se construye en runners de GitHub en la nube.

---

## GitOps Workflow

```
ProductAPI Repo (main branch)
    ↓
git push
    ↓
GitHub Actions: Build → Test → Docker Push a ACR
    ↓
SonarQube analiza calidad y coverage
    ↓
Snyk analiza vulnerabilidades y contenedores
    ↓
Actualiza helm/values-acr.yaml con nueva imagen
    ↓
    ArgoCD detecta el cambio (~3 minutos)
    ↓
    kubectl apply de Helm charts
    ↓
    Deployment actualizado automaticamente en AKS
    ↓
    Datadog recolecta métricas, logs y APM
    ↓
    Prometheus recolecta métricas (Prometheus-net)
    ↓
    Grafana muestra dashboards operacionales
    ↓
    Disponible en: http://productapi-mpn.centralus.cloudapp.azure.com/api/...

Infraestructura (ArgoCD, Helm, K8s config):  
https://github.com/pmelo1981/UnisabanaDevOps-Infrastructure
```

---

## Despliegue Manual en Kubernetes

```bash
# Usar valores desde ProductAPI repo
helm upgrade --install productapi ./helm \
  --namespace productapi \
  --create-namespace \
  -f helm/values-acr.yaml
```

## Imagen desplegada (ACR)

La imagen usada por defecto en `helm/values-acr.yaml` se publica en Azure Container Registry. Valores actuales:

- ACR repository: `productapiacrmpn.azurecr.io/productapi`
- Último tag desplegado: `aad0d3511a6ffce2477049786ab47e4e6f563476`

Para desplegar una versión concreta edita `helm/values-acr.yaml` (campo `image.tag`) y aplica un `helm upgrade` o deja que el pipeline lo actualice automáticamente al hacer push.

Ejemplo de `helm/values-acr.yaml`:

```yaml
image:
  repository: productapiacrmpn.azurecr.io/productapi
  tag: aad0d3511a6ffce2477049786ab47e4e6f563476
  pullPolicy: Always
```

---

## Secretos Necesarios (GitHub)

Para que el CI/CD funcione, agrega estos **Repository Secrets** en:  
https://github.com/pmelo1981/UnisabanaDevOps-ProductApi/settings/secrets/actions

| Secret | Ejemplo |
|--------|---------|
| `ACR_USERNAME` | `productapiregistry163505` |
| `ACR_PASSWORD` | `(access key del ACR)` |
| `SONAR_TOKEN` | `(token SonarQube)` |
| `SONAR_HOST_URL` | `http://57.162.160.232:9000` |
| `SNYK_TOKEN` | `(token API Snyk)` |

---

## Observabilidad y Monitoreo

El cluster AKS está integrado con:

- Datadog
- Prometheus
- Grafana

para observabilidad, dashboards, alertas, métricas y monitoreo centralizado.

Servicios monitoreados:

- Kubernetes cluster (AKS) a través de kube-state-metrics
- Nodes (Node Exporter) y pods
- Métricas HTTP y personalizadas de la aplicación (prometheus-net)
- Dashboard centralizado en Grafana
- Logs centralizados con Datadog
- Métricas CPU/Memoria/Network
- Application Performance Monitoring (APM)
- Kubernetes events
- Container monitoring

Despliegue y Configuración:

- Stack de monitoreo desplegado vía Helm Chart (`kube-prometheus-stack`)
- ServiceMonitor configurado para descubrir los endpoints `/metrics` de la aplicación.
- Grafana provisionado con dashboards automáticos de `.NET` y `ProductAPI`.
- Reglas de alertas configuradas en PrometheusRule (ej. Error Rate > 5%, Memoria Alta, etc.).

---

## Calidad de Código (SonarQube)

El pipeline CI/CD ejecuta análisis automático de SonarQube Community:

- Bugs
- Vulnerabilities
- Code Smells
- Coverage
- Quality Gates

Servidor SonarQube:

```plaintext
http://57.162.160.232:9000
```

---

## DevSecOps (Snyk)

El pipeline CI/CD integra Snyk para análisis de seguridad:

- Vulnerabilidades NuGet
- Escaneo de imágenes Docker
- Detección de librerías vulnerables
- Seguridad en contenedores

Escaneos ejecutados:

- Snyk Open Source
- Snyk Container

---

## Documentacion

- **Infrastructure Repo (GitOps, ArgoCD, K8s)**: https://github.com/pmelo1981/UnisabanaDevOps-Infrastructure)
- **ArgoCD UI**: https://172.169.162.125 (usuario: admin, ver Infrastructure README)
- [ArgoCD Docs](https://argo-cd.readthedocs.io/)
- [Helm Docs](https://helm.sh/docs/)
- [AKS Best Practices](https://learn.microsoft.com/en-us/azure/aks/)
- [Kubernetes Docs](https://kubernetes.io/docs/)

---

## Limpieza

**Elimina TODOS los recursos**:

```bash
az group delete --name productapi-rg --yes
```

Esto borra: AKS, ACR, Load Balancer, Storage, todo.

---

  
**Ultima actualizacion:** 08/Junio/2026

