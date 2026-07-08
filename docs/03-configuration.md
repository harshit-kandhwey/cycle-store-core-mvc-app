# Configuration

Every VM has one source of truth: **`/etc/cyclestore/cyclestore.conf`**
(installed from [`deploy/config/cyclestore.conf.example`](../deploy/config/cyclestore.conf.example)).
Apply changes with:

```bash
sudo cyclestore-apply          # regenerate config + restart/reload affected services
sudo cyclestore-apply --no-restart
```

`cyclestore-apply` **auto-detects the tier(s)** on the host, so a combined
Web+App VM (Scenario 1) has both applied from the same file.

## Keys

| Key                      | Tier | Meaning                                                                                 |
| ------------------------ | ---- | --------------------------------------------------------------------------------------- |
| `DB_HOST`, `DB_PORT`     | app  | SQL Server address (default port 1433)                                                  |
| `DB_NAME`                | app  | database name (`CYCLE_STORE`)                                                           |
| `DB_USER`, `DB_PASSWORD` | app  | SQL login (`cycleapp`) — must match the DB                                              |
| `APP_BIND`, `APP_PORT`   | app  | Kestrel listener (default `0.0.0.0:5000`)                                               |
| `ASPNETCORE_ENVIRONMENT` | app  | usually `Production`                                                                    |
| `ALLOWED_HOSTS`          | app  | Host-header allow-list (`*`, or `"host1;host2"`). Quoted — the file is sourced by bash. |
| `APP_HOST`               | web  | where nginx proxies: `127.0.0.1` (Scenario 1) or the App VM IP (Scenario 2)             |
| `WEB_PORT`, `HTTPS_PORT` | web  | nginx listeners (80 → 443)                                                              |
| `TLS_CERT`, `TLS_KEY`    | web  | certificate paths                                                                       |

## What `cyclestore-apply` generates

- **App tier** → `/etc/cyclestore/cyclestore.env` (mode 600), the `EnvironmentFile`
  for `cycleapp.service`: `ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`,
  `AllowedHosts`, and `ConnectionStrings__DefaultConnection`. Then restarts `cycleapp`.
- **Web tier** → the nginx site rendered from `nginx-cycleapp.conf.tmpl`
  (`/etc/nginx/conf.d/cycleapp.conf` on RHEL, `sites-available/cycleapp` on Debian),
  runs `nginx -t`, then reloads.

## Common changes

```bash
# Move the DB (e.g. after a migration)
sudo sed -i 's/^DB_HOST=.*/DB_HOST=10.0.0.99/' /etc/cyclestore/cyclestore.conf
sudo cyclestore-apply

# Point the web tier at a new app VM
sudo sed -i 's/^APP_HOST=.*/APP_HOST=10.0.0.20/' /etc/cyclestore/cyclestore.conf
sudo cyclestore-apply

# Lock the host header down for production
sudo sed -i 's/^ALLOWED_HOSTS=.*/ALLOWED_HOSTS="shop.example.com"/' /etc/cyclestore/cyclestore.conf
sudo cyclestore-apply
```

## Ship new code (app tier)

```bash
cd cycle-store-core-mvc-app && git pull
sudo ./deploy/setup-app.sh     # re-publishes and restarts cycleapp
```

## Security notes

- Secrets live only in `cyclestore.conf` / `cyclestore.env` (mode 600) — never in source.
- The app sends a nonce-based CSP and security headers.
- HSTS is intentionally omitted while TLS is self-signed; add it once a CA cert is in place.
