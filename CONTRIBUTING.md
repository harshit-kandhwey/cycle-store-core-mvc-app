# Contributing

Thanks for your interest in improving the Cycle Store. Bug reports, features,
fixes, and documentation are all welcome.

## Reporting bugs / requesting features
Use the GitHub issue tracker (templates are provided). Before filing, please
check existing open/closed issues. Helpful details:
- A reproducible test case or series of steps
- The commit/branch you're on
- Deployment scenario (1 = Web+App/DB, 2 = Web/App/DB) and OS, if relevant
- Anything unusual about your environment

## Pull requests
1. Work against the latest `ui-revamp` branch (the actively maintained line).
2. Keep the change focused; avoid unrelated reformatting.
3. Build locally: `dotnet build AdventureWorksMVCCore.Web`.
4. Manually exercise the affected pages/flows.
5. Update docs if behaviour or setup changed (`README.md`, `docs/`, `deploy/`).
6. Use clear commit messages and open the PR (the template will guide you).

## Development
See [`README.md`](README.md) for local run instructions and
[`docs/`](docs/) for architecture, configuration, and the catalog model.
Never commit secrets — the DB connection string comes only from configuration.

## Security
Do **not** open public issues for vulnerabilities. See [`SECURITY.md`](SECURITY.md)
and use GitHub's private vulnerability reporting.

## Code of Conduct
This project follows the [Contributor Covenant](CODE_OF_CONDUCT.md).

## Licensing
See [`LICENSE`](LICENSE). This repository is a diverged, independently-maintained
fork of the archived `aws-samples/cycle-store-core-mvc-app`; contributions are
accepted under the same licence.
