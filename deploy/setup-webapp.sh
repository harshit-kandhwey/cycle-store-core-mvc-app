#!/usr/bin/env bash
# ============================================================================
# Scenario 1 - Web + App on a SINGLE VM.
# Runs the app tier and the web tier on this host; nginx proxies to the local
# Kestrel process (APP_HOST forced to 127.0.0.1). Idempotent.
#
#   sudo ./deploy/setup-webapp.sh
#
# Point the DB at your DB VM first (edit deploy/config/cyclestore.conf.example,
# or /etc/cyclestore/cyclestore.conf then `sudo cyclestore-apply`).
# ============================================================================
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

sudo bash "$SCRIPT_DIR/setup-app.sh"
sudo bash "$SCRIPT_DIR/setup-web.sh"

# Same-VM: nginx -> local Kestrel.
if grep -q '^APP_HOST=' /etc/cyclestore/cyclestore.conf; then
  sudo sed -i 's/^APP_HOST=.*/APP_HOST=127.0.0.1/' /etc/cyclestore/cyclestore.conf
fi
sudo cyclestore-apply
echo
echo "Scenario 1 ready: browse https://<this-vm>  (Web+App here, DB is DB_HOST)."
