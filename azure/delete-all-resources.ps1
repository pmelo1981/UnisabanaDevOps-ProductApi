#!/usr/bin/env pwsh

param(
  [string]$ResourceGroup = "productapi-rg"
)

# Delete resource group and all resources
az group delete --name $ResourceGroup --yes --no-wait
