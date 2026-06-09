#!/usr/bin/env pwsh

# Install ArgoCD and apply application manifest if present
kubectl create namespace argocd --dry-run=client -o yaml | kubectl apply -f -
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
kubectl rollout status deployment/argocd-server -n argocd --timeout=5m

if (Test-Path "argocd/applications/productapi.yaml") {
  kubectl apply -f argocd/applications/productapi.yaml
}
