#!/usr/bin/env pwsh
<#
.SYNOPSIS
Runs tests/stress-test.js as a k6 Job inside AKS.

.DESCRIPTION
Refreshes the ConfigMap from the local stress-test.js, replaces the k6-stress-test
Job, then streams the runner pod's logs until completion. Targets the in-cluster
service productapi-productapi.productapi.svc.cluster.local by default.

.PARAMETER Namespace
Kubernetes namespace where the Job is created. Defaults to "productapi".

.PARAMETER TargetUrl
Overrides the TARGET_URL env passed to k6. Defaults to the in-cluster service.

.EXAMPLE
./tests/run-k6-aks.ps1

.EXAMPLE
./tests/run-k6-aks.ps1 -TargetUrl http://productapi-mpn.centralus.cloudapp.azure.com
#>

param(
  [string]$Namespace = "productapi",
  [string]$TargetUrl = "http://productapi-productapi.productapi.svc.cluster.local"
)

$ErrorActionPreference = "Stop"

$scriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptFile  = Join-Path $scriptDir "stress-test.js"
$jobManifest = Join-Path $scriptDir "k6-job.yaml"
$jobName     = "k6-stress-test"
$cmName      = "k6-stress-test-script"

if (-not (Test-Path $scriptFile))  { throw "Missing $scriptFile" }
if (-not (Test-Path $jobManifest)) { throw "Missing $jobManifest" }

Write-Host "Ensuring namespace '$Namespace' exists..."
kubectl get ns $Namespace *> $null
if ($LASTEXITCODE -ne 0) { kubectl create namespace $Namespace | Out-Null }

Write-Host "Refreshing ConfigMap '$cmName' from $scriptFile..."
kubectl -n $Namespace delete configmap $cmName --ignore-not-found | Out-Null
kubectl -n $Namespace create configmap $cmName --from-file=stress-test.js=$scriptFile | Out-Null

Write-Host "Deleting prior Job '$jobName' (if any)..."
kubectl -n $Namespace delete job $jobName --ignore-not-found --wait=true | Out-Null

Write-Host "Applying Job (TARGET_URL=$TargetUrl)..."
# Patch TARGET_URL in-flight without editing the manifest on disk.
$patchedYaml = (Get-Content -Raw $jobManifest) -replace `
  'value: http://productapi-productapi.productapi.svc.cluster.local', `
  "value: $TargetUrl"
$patchedYaml | kubectl -n $Namespace apply -f -

Write-Host "Waiting for runner pod to be scheduled..."
$podName = $null
for ($i = 0; $i -lt 60; $i++) {
  $podName = kubectl -n $Namespace get pods -l job-name=$jobName -o jsonpath='{.items[0].metadata.name}' 2>$null
  if ($podName) { break }
  Start-Sleep -Seconds 1
}
if (-not $podName) { throw "Runner pod did not appear within 60s." }
Write-Host "Pod: $podName"

Write-Host "Waiting for pod to leave Pending..."
kubectl -n $Namespace wait --for=condition=PodReadyToStartContainers pod/$podName --timeout=120s 2>$null | Out-Null

Write-Host "----- k6 logs -----"
kubectl -n $Namespace logs -f job/$jobName

Write-Host "----- Job status -----"
kubectl -n $Namespace get job $jobName
