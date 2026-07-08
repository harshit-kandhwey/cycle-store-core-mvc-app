#!/usr/bin/env bash
# ============================================================================
# cyclestore-apply - apply /etc/cyclestore/cyclestore.conf to this VM's services.
#
#   sudo cyclestore-apply              regenerate config and restart/reload
#   sudo cyclestore-apply --no-restart regenerate only (no service bounce)
#
# Role is auto-detected, so a single VM running BOTH tiers (Scenario 1) is fully
# supported - both blocks below run:
#   * App tier  (cycleapp.service present)      -> render env file [+ restart app]
#   * Web tier  (nginx + rendered template)     -> render nginx site [+ reload]
# ============================================================================
set -euo pipefail

NO_RESTART=0
[ "${1:-}" = "--no-restart" ] && NO_RESTART=1

CONF=/etc/cyclestore/cyclestore.conf
[ -r "$CONF" ] || { echo "ERROR: missing $CONF" >&2; exit 1; }
# shellcheck disable=SC1090
. "$CONF"

applied=0

# ---- App tier --------------------------------------------------------------
if systemctl cat cycleapp.service >/dev/null 2>&1; then
  install -d -m 755 /etc/cyclestore
  umask 077
  cat > /etc/cyclestore/cyclestore.env <<EOF
ASPNETCORE_URLS=http://${APP_BIND:-0.0.0.0}:${APP_PORT:-5000}
ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}
AllowedHosts=${ALLOWED_HOSTS:-*}
ConnectionStrings__DefaultConnection=Server=${DB_HOST},${DB_PORT:-1433};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};TrustServerCertificate=True;
EOF
  chmod 600 /etc/cyclestore/cyclestore.env
  if [ "$NO_RESTART" = 0 ]; then
    systemctl restart cycleapp
    echo "[app] applied -> DB ${DB_HOST},${DB_PORT:-1433}/${DB_NAME}; AllowedHosts=${ALLOWED_HOSTS:-*}; cycleapp restarted"
  else
    echo "[app] env written (no restart)"
  fi
  applied=1
fi

# ---- Web tier --------------------------------------------------------------
TMPL=/etc/cyclestore/nginx-cycleapp.conf.tmpl
if command -v nginx >/dev/null 2>&1 && [ -r "$TMPL" ]; then
  export APP_HOST APP_PORT WEB_PORT HTTPS_PORT TLS_CERT TLS_KEY
  VARS='${APP_HOST} ${APP_PORT} ${WEB_PORT} ${HTTPS_PORT} ${TLS_CERT} ${TLS_KEY}'
  # Debian/Ubuntu use sites-available/enabled; RHEL/Oracle/Alma use conf.d/*.conf.
  if [ -d /etc/nginx/sites-available ]; then
    dest=/etc/nginx/sites-available/cycleapp
    envsubst "$VARS" < "$TMPL" > "$dest"
    ln -sf "$dest" /etc/nginx/sites-enabled/cycleapp
    rm -f /etc/nginx/sites-enabled/default
  else
    dest=/etc/nginx/conf.d/cycleapp.conf
    envsubst "$VARS" < "$TMPL" > "$dest"
  fi
  nginx -t
  if [ "$NO_RESTART" = 0 ]; then
    systemctl reload-or-restart nginx
    echo "[web] applied -> proxy http://${APP_HOST}:${APP_PORT:-5000}; nginx reloaded ($dest)"
  else
    echo "[web] site rendered (no reload, $dest)"
  fi
  applied=1
fi

if [ "$applied" != 1 ]; then
  echo "No Cycle Store role detected (need cycleapp.service, or nginx + template)." >&2
  exit 1
fi
