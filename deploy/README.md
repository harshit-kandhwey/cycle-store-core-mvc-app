# Cycle Store - deployment (QUICKSTART)

Deploy the whole store from **one repo clone**. Two topologies are supported;
pick one. In both, the database is a separate SQL Server VM.

```
Scenario 1 - two VMs                  Scenario 2 - three VMs (classic 3-tier)
  VM1  Web + App  (nginx + .NET)        VM1  Web  (nginx)
  VM2  Database   (SQL Server)          VM2  App  (.NET / Kestrel)
                                        VM3  Database (SQL Server)
```

Web VMs: Oracle Linux / RHEL / Alma / Rocky / Ubuntu / Debian.
DB VM: SQL Server 2019+ (Windows or Linux).

Everything is driven by one file per VM - `/etc/cyclestore/cyclestore.conf` -
applied with `sudo cyclestore-apply`. Change hosts later? Edit that file and
re-run `sudo cyclestore-apply`; nothing else to touch.

---

## 0. Database (both scenarios)
On the DB VM, follow **[deploy/db/README.md](db/README.md)**: install SQL Server,
load the base schema, create the `cycleapp` login, then apply `catalog_pivot.sql`.
Note the DB VM's IP and the `cycleapp` password.

---

## Scenario 1 - Web + App on one VM

**VM1 (Web + App):**
```bash
git clone https://github.com/harshit-kandhwey/cycle-store-core-mvc-app.git
cd cycle-store-core-mvc-app
# point at your DB before first apply:
sed -i 's/^DB_HOST=.*/DB_HOST=<VM2-DB-IP>/' deploy/config/cyclestore.conf.example
sudo ./deploy/setup-webapp.sh
```
Browse **https://VM1** (self-signed cert warning is expected). `APP_HOST` is set
to `127.0.0.1` automatically so nginx proxies to the local app.

---

## Scenario 2 - Web / App / DB on three VMs

**VM2 (App):**
```bash
git clone https://github.com/harshit-kandhwey/cycle-store-core-mvc-app.git
cd cycle-store-core-mvc-app
sed -i 's/^DB_HOST=.*/DB_HOST=<VM3-DB-IP>/' deploy/config/cyclestore.conf.example
sudo ./deploy/setup-app.sh
```

**VM1 (Web):**
```bash
git clone https://github.com/harshit-kandhwey/cycle-store-core-mvc-app.git
cd cycle-store-core-mvc-app
sed -i 's/^APP_HOST=.*/APP_HOST=<VM2-APP-IP>/' deploy/config/cyclestore.conf.example
sudo ./deploy/setup-web.sh
```
Browse **https://VM1**.

---

## What each piece does
| File | Role |
|------|------|
| `deploy/setup-app.sh`    | installs .NET 8, publishes this checkout to `/opt/cycleapp/publish`, runs it as `cycleapp.service` on :5000 |
| `deploy/setup-web.sh`    | installs nginx, self-signed TLS, renders the reverse-proxy vhost |
| `deploy/setup-webapp.sh` | Scenario 1: both of the above on one VM |
| `deploy/bin/cyclestore-apply.sh` | renders runtime config from `cyclestore.conf`; auto-detects app/web/both |
| `deploy/config/cyclestore.conf.example` | the single config: DB, app listener, AllowedHosts, nginx proxy target, TLS |
| `deploy/config/cycleapp.service` / `nginx-cycleapp.conf.tmpl` | systemd unit / nginx template |
| `deploy/db/` | SQL Server setup guide + `catalog_pivot.sql` seed |

## Change something later
Edit `/etc/cyclestore/cyclestore.conf` on the VM, then:
```bash
sudo cyclestore-apply
```
- New DB host/password -> update `DB_*` (app tier).
- Move the app to another VM -> update `APP_HOST` (web tier).
- Lock down host header -> set `ALLOWED_HOSTS="your.domain"` (app tier).

## Notes
- **TLS** is self-signed by default (browser warning). Drop a CA cert at the
  `TLS_CERT`/`TLS_KEY` paths and `sudo cyclestore-apply` to switch.
- **HSTS** is intentionally not sent while using self-signed certs.
- **DB writes**: `cycleapp` has SELECT/INSERT/UPDATE/DELETE on the `Production`
  schema (needed by the catalog seed). Tighten to SELECT after seeding if desired.
