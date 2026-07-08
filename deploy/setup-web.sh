#!/usr/bin/env bash
# ============================================================================
# Web tier setup - nginx reverse proxy (HTTP->HTTPS) in front of the app tier.
# OS-aware (Oracle/RHEL/Alma via dnf+conf.d, Debian/Ubuntu via apt+sites).
# Generates a self-signed TLS cert so it works out of the box. Idempotent.
#
#   sudo ./deploy/setup-web.sh
#
# Set APP_HOST in the config to the App VM IP (Scenario 2) or 127.0.0.1
# (Scenario 1, same VM). Re-run `sudo cyclestore-apply` after edits.
# ============================================================================
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "== 1. nginx + envsubst + openssl =="
if command -v dnf >/dev/null 2>&1; then
  sudo dnf install -y nginx gettext openssl
elif command -v apt-get >/dev/null 2>&1; then
  sudo apt-get update -y && sudo DEBIAN_FRONTEND=noninteractive apt-get install -y nginx gettext-base openssl
else
  echo "Unsupported package manager; install nginx manually." >&2; exit 1
fi

echo "== 2. central config + template + apply tool =="
sudo install -d -m 755 /etc/cyclestore
if [ ! -f /etc/cyclestore/cyclestore.conf ]; then
  sudo cp "$SCRIPT_DIR/config/cyclestore.conf.example" /etc/cyclestore/cyclestore.conf
  sudo chmod 600 /etc/cyclestore/cyclestore.conf
  echo "   -> installed default /etc/cyclestore/cyclestore.conf (SET APP_HOST)"
fi
sudo cp "$SCRIPT_DIR/config/nginx-cycleapp.conf.tmpl" /etc/cyclestore/nginx-cycleapp.conf.tmpl
sudo install -m 755 "$SCRIPT_DIR/bin/cyclestore-apply.sh" /usr/local/bin/cyclestore-apply
sudo ln -sf /usr/local/bin/cyclestore-apply /usr/sbin/cyclestore-apply

echo "== 3. self-signed TLS cert (if missing) =="
sudo install -d -m 700 /etc/cyclestore/tls
if [ ! -f /etc/cyclestore/tls/cycleweb.crt ]; then
  sudo openssl req -x509 -nodes -newkey rsa:2048 -days 825 \
    -keyout /etc/cyclestore/tls/cycleweb.key \
    -out /etc/cyclestore/tls/cycleweb.crt \
    -subj "/CN=cyclestore-web" >/dev/null 2>&1
  echo "   -> generated self-signed cert (replace with a CA cert for production)"
fi

echo "== 4. SELinux: allow nginx to make outbound proxy connections =="
if command -v setsebool >/dev/null 2>&1; then
  sudo setsebool -P httpd_can_network_connect 1 || true
fi

echo "== 5. firewall (80/443) =="
if command -v firewall-cmd >/dev/null 2>&1; then
  sudo firewall-cmd --permanent --add-service=http --add-service=https && sudo firewall-cmd --reload
elif command -v ufw >/dev/null 2>&1; then
  sudo ufw allow 80/tcp && sudo ufw allow 443/tcp || true
fi

echo "== 6. apply + enable =="
sudo systemctl enable nginx
sudo cyclestore-apply

echo "== validate =="
sudo systemctl is-active nginx
curl -sk -o /dev/null -w 'https local http=%{http_code}\n' https://localhost/ || true
echo "Web tier ready at https://<this-vm> (self-signed cert - expect a browser warning)."
