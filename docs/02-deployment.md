# Deployment

Deploy the whole store from one repo clone. The database is always its own
SQL Server VM. The web/app tiers run on Linux (Oracle/RHEL/Alma/Rocky/Ubuntu/Debian).

The authoritative, copy-paste quick start is **[`deploy/README.md`](../deploy/README.md)**.
This page explains the two topologies and how the pieces fit.

## Scenario 1 — Web + App on one VM
```
VM1  Web + App   (nginx + .NET, nginx -> 127.0.0.1:5000)
VM2  Database    (SQL Server)
```
On VM1:
```bash
git clone https://github.com/harshit-kandhwey/cycle-store-core-mvc-app.git
cd cycle-store-core-mvc-app
sed -i 's/^DB_HOST=.*/DB_HOST=<VM2-DB-IP>/' deploy/config/cyclestore.conf.example
sudo ./deploy/setup-webapp.sh
```
`setup-webapp.sh` runs the app tier and the web tier, then forces `APP_HOST=127.0.0.1`.

## Scenario 2 — three VMs (classic 3-tier)
```
VM1  Web   (nginx)
VM2  App   (.NET / Kestrel :5000)
VM3  Database (SQL Server)
```
On **VM2 (App)**:
```bash
git clone ... && cd cycle-store-core-mvc-app
sed -i 's/^DB_HOST=.*/DB_HOST=<VM3-DB-IP>/' deploy/config/cyclestore.conf.example
sudo ./deploy/setup-app.sh
```
On **VM1 (Web)**:
```bash
git clone ... && cd cycle-store-core-mvc-app
sed -i 's/^APP_HOST=.*/APP_HOST=<VM2-APP-IP>/' deploy/config/cyclestore.conf.example
sudo ./deploy/setup-web.sh
```

## Database (both scenarios)
Follow **[`deploy/db/README.md`](../deploy/db/README.md)**: install SQL Server,
load `CYCLE_STORE_Schema_data.sql`, create the `cycleapp` login, then apply
`deploy/db/catalog_pivot.sql` (the 114-product catalog). The `DB_USER`/`DB_PASSWORD`
must match `cyclestore.conf`.

## What the scripts install
- `setup-app.sh` — .NET 8 SDK, publishes this checkout to `/opt/cycleapp/publish`,
  installs `cycleapp.service` (Kestrel :5000), central config, `cyclestore-apply`.
- `setup-web.sh` — nginx, self-signed TLS cert, the reverse-proxy vhost
  (Debian `sites-available` **or** RHEL `conf.d`, auto-detected).
- `setup-webapp.sh` — both, on one VM.

## Reconfigure later
Edit `/etc/cyclestore/cyclestore.conf` on the VM and run `sudo cyclestore-apply`.
No redeploy needed for host/password/AllowedHosts changes. To ship new **code**,
`git pull` and re-run `setup-app.sh` (app tier). See [03-configuration](03-configuration.md).

## Notes
- TLS is self-signed by default (browser warning). Replace the cert at the
  `TLS_CERT`/`TLS_KEY` paths and re-apply.
- On RHEL/Oracle, `setup-web.sh` sets the SELinux boolean
  `httpd_can_network_connect` so nginx may proxy to the app tier.
