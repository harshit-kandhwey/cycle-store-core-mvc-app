# Database tier setup (SQL Server)

The app needs a SQL Server 2019+ instance holding the `CYCLE_STORE` database
(schema `Production`) and a `cycleapp` SQL login. Works on Windows SQL Server or
SQL Server on Linux. All commands use `sqlcmd`.

## 1. Install SQL Server 2019 (mixed-mode / SQL auth enabled), open TCP 1433

- Windows: SQL Server 2019 Developer, enable **TCP 1433** + firewall.
- Linux: `mssql-server` package, `mssql-conf setup`, open 1433.

## 2. Load the base schema + data

The base catalog schema/data ships in this repo (`CYCLE_STORE_Schema_data.sql`
at the repo root):

```bash
sqlcmd -S localhost -U sa -P '<sa-password>' -b -i CYCLE_STORE_Schema_data.sql
```

## 3. Create the application login (used by the app's connection string)

```bash
sqlcmd -S localhost -U sa -P '<sa-password>' -b -Q "IF SUSER_ID('cycleapp') IS NULL CREATE LOGIN cycleapp WITH PASSWORD='<cycleapp-password>', DEFAULT_DATABASE=CYCLE_STORE, CHECK_POLICY=ON;"
sqlcmd -S localhost -U sa -P '<sa-password>' -b -d CYCLE_STORE -Q "IF USER_ID('cycleapp') IS NULL CREATE USER cycleapp FOR LOGIN cycleapp; GRANT SELECT,INSERT,UPDATE,DELETE ON SCHEMA::Production TO cycleapp;"
```

## 4. Apply the storefront catalog (this repo)

Loads the 6 extra subcategories + 114 curated products and de-duplicates the
original size-variants. Idempotent - safe to re-run.

```bash
sqlcmd -S localhost -U sa -P '<sa-password>' -b -i deploy/db/catalog_pivot.sql
```

## 5. Verify

```bash
sqlcmd -S localhost -U cycleapp -P '<cycleapp-password>' -d CYCLE_STORE -Q "SELECT COUNT(*) FROM Production.Product WHERE ProductID>=1000;"
```

Expect **114**.

> The `DB_USER` / `DB_PASSWORD` here must match `/etc/cyclestore/cyclestore.conf`
> on the web/app VM(s).

_(For an automated Windows/WinRM build of this whole tier, see the separate
`server-setup-automation-cyclestore` ops repo.)_
