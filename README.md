# Product API - Microservicio REST

## UnisabanaArq1Grupo2 PatronesActividad3
Presentación de la Actividad

Asignatura: Arquitectura de Software

Profesor: Daniel Orlando Saavedra Fonnegra

Integrantes del grupo:
- Pablo Andrés Melo García
- Camilo Andres Padilla Garcia
- Edison Kenneth Campos Avila
- Cristian Alonso Cardona Vega
- Jorge Andres Ayala Valero
- John Harold Diaz Gonzale

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
6. Login a Azure Container Registry
7. docker build -f docker/Dockerfile
8. docker push → ACR (tag: git SHA + latest)
9. sed actualiza values-acr.yaml con nuevo tag
10. git push automatico
    ↓
    ArgoCD detecta (cada 3 min)
    ↓
    helm upgrade en Kubernetes
    ↓
    Rolling update (zero-downtime)
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
Actualiza helm/values-acr.yaml con nueva imagen
    ↓
    ArgoCD detecta el cambio (~3 minutos)
    ↓
    kubectl apply de Helm charts
    ↓
    Deployment actualizado automaticamente en AKS
    ↓
    Disponible en: http://productapi-mpn.centralus.cloudapp.azure.com/api/...

Infraestructura (ArgoCD, Helm, K8s config):  
https://github.com/pmelo1981/UnisabanaArq1Grupo2PatronesActividad3-infrastructure
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
https://github.com/pmelo1981/UnisabanaArq1Grupo2PatronesActividad3-productapi/settings/secrets/actions

| Secret | Ejemplo |
|--------|---------|
| `ACR_USERNAME` | `productapiregistry163505` |
| `ACR_PASSWORD` | `(access key del ACR)` |

---

## Documentacion

- **Infrastructure Repo (GitOps, ArgoCD, K8s)**: https://github.com/pmelo1981/UnisabanaArq1Grupo2PatronesActividad3-infrastructure
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

  
**Ultima actualizacion:** 08/03/2026

