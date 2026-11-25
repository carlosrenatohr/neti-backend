# Versioning

Two versions coexist and are intentionally independent:

| Version | Where | Current |
|---------|-------|---------|
| Software version | Git tag and GitHub Release | `1.0.0` |
| HTTP API version | URL segment | `/api/v1` |

A patch release of the software does not change the HTTP contract. A new HTTP API version is only introduced when the public contract must break, and it ships as a new URL segment (`/api/v2`) alongside the old one during a transition.

## Semantic Versioning

The repository follows [Semantic Versioning 2.0.0](https://semver.org/):

- **MAJOR**: a breaking change for consumers. Renaming or removing a field, changing a status code, or changing the meaning of an existing response.
- **MINOR**: a backwards-compatible capability, such as a new endpoint or a new optional field.
- **PATCH**: a backwards-compatible fix, documentation correction or dependency update with no contract change.

Pre-release identifiers (`1.1.0-rc.1`) are allowed for candidates that are not yet deployed.

## Release rules

1. `main` must be green: Release build and the full test suite pass.
2. The deployed commit must match the commit being released.
3. Create an annotated tag on that commit: `git tag -a v1.0.0 -m "Marcas Autos API 1.0.0"`.
4. Publish a GitHub Release from the tag with notes based only on merged, verified changes.
5. Never move or delete a published tag or release. A correction ships as a new patch version.

## First release

`1.0.0` is the first formal version: the read-only contract is stable, the documented scope is complete, and the service is deployed and verified against PostgreSQL.
