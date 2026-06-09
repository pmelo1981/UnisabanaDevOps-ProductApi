#!/usr/bin/env pwsh

param(
  [string]$ResourceGroup = "productapi-rg",
  [string]$ClusterName = "productapi-aks",
  [string]$Location = "eastus",
  [int]$NodeCount = 1,
  [string]$VmSize = "Standard_B2s"
)

# Create resource group and AKS cluster, get credentials, install nginx ingress
az group create --name $ResourceGroup --location $Location

az aks create \
  --resource-group $ResourceGroup \
  --name $ClusterName \
  --node-count $NodeCount \
  --vm-set-type VirtualMachineScaleSets \
  --load-balancer-sku standard \
  --network-plugin kubenet \
  --vm-size $VmSize --yes

az aks get-credentials --resource-group $ResourceGroup --name $ClusterName --overwrite-existing

helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update
helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx --namespace ingress-nginx --create-namespace --set controller.service.type=LoadBalancer --wait --timeout 5m
