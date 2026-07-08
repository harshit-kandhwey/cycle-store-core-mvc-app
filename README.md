# Unicorn Bike Rentals — Cycle Store

A modern ASP.NET Core (.NET 8) storefront for a bicycle shop: a curated catalog
of ~114 real cycling products with a full browsing and shopping experience,
served live from SQL Server across a classic web / app / database tier split.

> **Fork notice.** This project began as a fork of
> [`aws-samples/cycle-store-core-mvc-app`](https://github.com/aws-samples/cycle-store-core-mvc-app).
> It has since **diverged substantially** — new UI, a rebuilt catalog, e-commerce
> features, a security pass, and a self-contained deployment kit. The upstream
> repository is now **archived by its owner**, so these changes are **not**
> submitted back as a pull request; this repository is maintained independently.
> Original code remains under the upstream licence (see [`LICENSE`](LICENSE)).

## Highlights
- **Curated catalog** — 114 real cycling products across 36 subcategories
  (bikes, components, clothing, accessories), each with a hand-matched photo.
- **Rich product browsing** — category & subcategory pages, breadcrumb, quick-view
  modal, image gallery, infinite scroll, loading skeletons, empty/error states.
- **Filtering & sorting** — multi-colour, price range, in-stock, and **brand** facets
  with removable active-filter chips.
- **Merchandising** — star **ratings & reviews**, **sale pricing** (struck-through
  “was” price + % off), generated **product descriptions**.
- **Cart & checkout** — session-based guest cart, add-to-cart, checkout form, and
  an order confirmation (CSRF-protected).
- **Dark / light theme** — no-flash, OS-aware, persisted.
- **Security pass** — nonce-based CSP + security headers, host-header allow-list,
  search input bounds, no secrets in source.

## Repository layout
```
AdventureWorksMVCCore.Web/   the ASP.NET Core MVC app (controllers, views, models, wwwroot)
AdventureWorksMVCCore.sln    solution
CYCLE_STORE_Schema_data.sql  base database schema + seed data
deploy/                      self-contained deployment kit (scripts, config, DB seed)
docs/                        architecture, deployment, configuration, catalog & feature docs
legacy/                      obsolete upstream AWS artifacts (Proton / ECS / RDS), kept for reference
```

## Quick start
The database is a separate SQL Server VM in both supported topologies:

- **Scenario 1** — Web + App on one VM, Database on another.
- **Scenario 2** — Web, App, and Database on three VMs (classic 3-tier).

Clone the repo on each VM and run the matching script. Full copy-paste steps:
**[`deploy/README.md`](deploy/README.md)**. In short:

```bash
git clone https://github.com/harshit-kandhwey/cycle-store-core-mvc-app.git
cd cycle-store-core-mvc-app
# DB VM: see deploy/db/README.md
# Scenario 1 (Web+App VM):   sudo ./deploy/setup-webapp.sh
# Scenario 2 (App VM):       sudo ./deploy/setup-app.sh
# Scenario 2 (Web VM):       sudo ./deploy/setup-web.sh
```

Everything on a VM is driven by one file — `/etc/cyclestore/cyclestore.conf` —
applied with `sudo cyclestore-apply`. Change a host or password later? Edit that
file and re-run; nothing else to touch.

## Documentation
| Doc | What |
|-----|------|
| [docs/01-architecture.md](docs/01-architecture.md) | tiers, request flow, tech stack |
| [docs/02-deployment.md](docs/02-deployment.md) | the two topologies, step by step |
| [docs/03-configuration.md](docs/03-configuration.md) | `cyclestore.conf` keys + `cyclestore-apply` |
| [docs/04-catalog-data.md](docs/04-catalog-data.md) | catalog curation, images, adding products |
| [docs/05-features.md](docs/05-features.md) | the storefront & e-commerce features |

## Tech stack
ASP.NET Core MVC (.NET 8) · Entity Framework Core 8 · SQL Server 2019 ·
Razor views · vanilla CSS/JS (no build step) · nginx (reverse proxy + TLS) ·
systemd (Kestrel service).

## Local development
Set the connection string (`ConnectionStrings__DefaultConnection`) via environment
or `appsettings.Development.json`, then:
```bash
dotnet run --project AdventureWorksMVCCore.Web
```
