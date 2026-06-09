#!/usr/bin/env pwsh

param(
  [string]$Namespace = "productapi",
  [string]$HealthUrl = "http://localhost:8080/api/products/health"
)

# Check rollout and health endpoint
kubectl rollout status deployment/productapi-productapi -n $Namespace --timeout=2m

try {
  curl -sSf $HealthUrl > $null
  Write-Output "Health OK"
  exit 0
} catch {
  Write-Error "Health check failed"
  exit 1
}
