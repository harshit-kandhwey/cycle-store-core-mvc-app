# Security Policy

## Supported versions
This project is developed on the `ui-revamp` line, which is the actively
maintained branch. Fixes are applied there.

| Version / branch | Supported |
|------------------|-----------|
| `ui-revamp` (current) | ✅ |
| older / upstream `master` | ❌ |

## Reporting a vulnerability
Please **do not** open a public issue for security problems.

- Preferred: use **GitHub → Security → "Report a vulnerability"** (private
  vulnerability reporting) on this repository.
- Alternatively, contact the maintainer privately.

Please include: affected component (web/app/db tier), steps to reproduce or a
proof of concept, and the impact you observed. We aim to acknowledge reports
within a few days.

## Scope & hardening notes
This is a demo storefront, but it applies several baseline protections:
- No secrets in source — the DB connection string comes only from configuration
  (`/etc/cyclestore/cyclestore.conf` → `cyclestore.env`).
- Security headers + a nonce-based Content-Security-Policy.
- Host-header allow-list (`AllowedHosts`).
- CSRF tokens on all state-changing requests.
- Bounded search input.

TLS is self-signed by default in the deployment kit; use a CA-issued certificate
and enable HSTS for any internet-facing deployment.
