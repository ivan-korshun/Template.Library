# Library Solution

A web service showcasing a hybrid ORM approach with Dapper (for data querying/modification) and Entity Framework (for code-first schema management).

## Main Endpoints

- API:
  - `/api/v1/authors`
  - `/api/v1/books`
  - `/api/v1/branches`

- Health checks:
  - `/api/status/healthz`
  - `/api/status/healthz/started`
  - `/api/status/healthz/ready`
  - `/api/status/healthz/live`
  - `/api/status/healthz/auth`
  - `/api/status/healthz/credentials`
  - `/api/status/healthz/db`
  - `/api/status/healthz/org-service`

- Metrics:
  - `/api/v2.0/prometheus`

- Documentation:
  - `/swagger`
  - `/openapi/v1.json`

## Local Setup

1. Execute `/scripts/init.hosts.ps1` to map the necessary DNS records to `127.0.0.1` in Windows `hosts` file.

2. Create `/.env` file from `/.env.example`.

3. Configure the following environment variables in `secrets.json` (to run from VS) and `.env` file (to run containers):

   - `Library__ClientCredentials__InternalApi__ClientId`
   - `Library__ClientCredentials__InternalApi__ClientSecret`
   - `LibraryTests__ClientCredentials__ClientId`
   - `LibraryTests__ClientCredentials__ClientSecret`

## Run Containers

Execute the appropriate `/scripts/run.*.ps1` script.
