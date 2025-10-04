#!/usr/bin/env bash
set -euo pipefail

# Docker
sudo apt-get update
sudo apt-get install -y ca-certificates curl git
# Installe Docker si non présent
if ! command -v docker >/dev/null; then
  curl -fsSL https://get.docker.com | sudo sh
  sudo usermod -aG docker "$USER"
fi

# .NET SDK 8
if ! dotnet --list-sdks | grep -q "8."; then
  sudo apt-get install -y dotnet-sdk-8.0 || true
fi

# kubectl
if ! command -v kubectl >/dev/null; then
  curl -LO "https://storage.googleapis.com/kubernetes-release/release/$(curl -s https://storage.googleapis.com/kubernetes-release/release/stable.txt)/bin/linux/amd64/kubectl"
  chmod +x kubectl && sudo mv kubectl /usr/local/bin/
fi

echo "[OK] Base VM prête. Déconnecte/reconnecte pour groupes docker."
