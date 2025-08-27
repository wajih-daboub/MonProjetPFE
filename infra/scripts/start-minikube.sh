#!/usr/bin/env bash
set -euo pipefail

minikube status >/dev/null 2>&1 || minikube start --cpus=4 --memory=8192
minikube addons enable ingress || true

IP=$(minikube ip)
if ! grep -q "portal.local" /etc/hosts; then
  echo "$IP portal.local jenkins.local" | sudo tee -a /etc/hosts
fi

echo "[OK] Minikube prêt sur $IP"
