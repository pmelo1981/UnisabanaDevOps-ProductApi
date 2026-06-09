#!/usr/bin/env pwsh

param(
  [string]$AcrName,
  [string]$ImageTag = "latest"
)

# Build and push image to ACR, then deploy with Helm
if (-not $AcrName) {
  Write-Error "AcrName is required"
  exit 1
}

az acr build --registry $AcrName --image productapi:$ImageTag .

helm upgrade --install productapi ./helm -f helm/values-acr.yaml --namespace productapi --create-namespace --wait --timeout 10m