#!/usr/bin/env bash
# ============================================================================
# App tier setup - builds and runs the Cycle Store .NET app (Kestrel :5000)
# straight from THIS repo checkout. Idempotent.
#
#   sudo ./deploy/setup-app.sh
#
# Edit deploy/config/cyclestore.conf.example first, or edit
# /etc/cyclestore/cyclestore.conf afterwards and re-run `sudo cyclestore-apply`.
# ============================================================================
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO="$(cd "$SCRIPT_DIR/.." && pwd)"
WEBPROJ="$REPO/AdventureWorksMVCCore.Web/AdventureWorksMVCCore.Web.csproj"

echo "== 1. .NET 8 SDK =="
if ! command -v dotnet >/dev/null 2>&1; then
  if command -v dnf >/dev/null 2>&1; then
    sudo dnf install -y dotnet-sdk-8.0 || { sudo dnf install -y oracle-epel-release-el8 || true; sudo dnf install -y dotnet-sdk-8.0; }
  elif command -v apt-get >/dev/null 2>&1; then
    sudo apt-get update -y && sudo DEBIAN_FRONTEND=noninteractive apt-get install -y dotnet-sdk-8.0
  else
    echo "Unsupported package manager; install .NET 8 SDK manually." >&2; exit 1
  fi
fi

echo "== 2. publish from local checkout -> /opt/cycleapp/publish =="
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
sudo rm -rf /opt/cycleapp/publish
sudo mkdir -p /opt/cycleapp
sudo dotnet publish "$WEBPROJ" -c Release -o /opt/cycleapp/publish

echo "== 3. central config + apply tool + service =="
sudo install -d -m 755 /etc/cyclestore
if [ ! -f /etc/cyclestore/cyclestore.conf ]; then
  sudo cp "$SCRIPT_DIR/config/cyclestore.conf.example" /etc/cyclestore/cyclestore.conf
  sudo chmod 600 /etc/cyclestore/cyclestore.conf
  echo "   -> installed default /etc/cyclestore/cyclestore.conf (EDIT DB_HOST etc.)"
fi
sudo install -m 755 "$SCRIPT_DIR/tools/cyclestore-apply.sh" /usr/local/bin/cyclestore-apply
sudo ln -sf /usr/local/bin/cyclestore-apply /usr/sbin/cyclestore-apply
sudo cp "$SCRIPT_DIR/config/cycleapp.service" /etc/systemd/system/cycleapp.service

echo "== 4. firewall (5000) =="
if command -v firewall-cmd >/dev/null 2>&1; then
  sudo firewall-cmd --permanent --add-port=5000/tcp && sudo firewall-cmd --reload
elif command -v ufw >/dev/null 2>&1; then
  sudo ufw allow 5000/tcp || true
fi

echo "== 5. enable + apply =="
sudo systemctl daemon-reload
sudo systemctl enable cycleapp
sudo cyclestore-apply
sleep 5

echo "== validate =="
sudo systemctl is-active cycleapp
curl -s -o /dev/null -w 'local app http=%{http_code}\n' http://localhost:5000 || true
echo "App tier ready. Edit /etc/cyclestore/cyclestore.conf + 'sudo cyclestore-apply' to change the DB."
