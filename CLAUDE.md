# Phone Book

A full-stack application for storing phone numbers with role-based visibility. Users can add and view phone numbers after signing in through an identity provider. Entries are append-only: no edits or deletions are permitted.

## Technology Stack

- **Backend:** ASP.NET Core 10 (.NET 10), Entity Framework Core, PostgreSQL
- **Frontend:** Angular 22, TypeScript, Angular Material 3
- **Identity:** Keycloak 26.8.0 with OAuth 2.0 and OpenID Connect
- **Database:** PostgreSQL 18.6
- **Containerization:** Docker and Docker Compose
- **Testing:** xUnit (backend unit and integration), Vitest (frontend unit), Playwright (end-to-end)

## Project Structure

Detailed structure and architectural decisions are documented in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md). Key directories:

- `backend/` – ASP.NET Core API with layered architecture
- `frontend/` – Angular SPA with standalone components
- `keycloak/realm/` – Keycloak realm configuration
- `db/init/` – PostgreSQL initialization script
- `contracts/` – Shared test fixtures (validation cases)
- `e2e/` – Playwright end-to-end test suite

## Build and Run Commands

### Full Stack (Docker Compose)

```bash
docker-compose up
docker-compose up --build
docker compose down -v
```

Access the application at `http://localhost:4200`. Default credentials: `alice/alice` or `bob/bob`.

### Local Development

Start PostgreSQL and Keycloak:
```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d postgres keycloak
```

Run the backend on the host:
```bash
dotnet run --project backend/src/PhoneBook.Api
```

Run the frontend dev server:
```bash
npm start
```
in `frontend/`.

### Testing

```bash
cd backend && dotnet test --project tests/PhoneBook.Api.UnitTests
cd backend && dotnet test --project tests/PhoneBook.Api.IntegrationTests
npm test
npm run test:ci
npm run lint
```

in `frontend/`. End-to-end tests after `docker compose up`:
```bash
cd e2e
npm ci
npx playwright install --with-deps chromium
npm test
```

## Essential Rules

### Design Invariants

1. **No mutations.** The API and UI support only reads and creates. Update and delete operations do not exist anywhere: not in the API, not in the UI, not in the database layer.

2. **Owner from JWT.** The owner of an entry is always the `sub` claim from a validated access token. It is never read from the request body, query parameters, or headers.

3. **Company name restriction.** The task author's company name must never appear in any tracked file—not in code, documentation, configuration, comments, commit messages, or filenames. Verification is performed locally outside the repository.

### Code and Documentation

4. **English only.** All UI text, code identifiers, documentation, configuration, log messages, Keycloak display names and commit messages are in English. CI enforces this.

5. **No comments.** Source code contains no comments of any kind: not `//`, not `/* */`, not `///` XML documentation, not `<!-- -->` in templates, not `#` lines in YAML or shell scripts. Code explains itself through well-chosen names and structure. OpenAPI documentation comes from attributes, never from comment text.

6. **Follow conventions.** All implementation must adhere to the conventions and best practices documented in section 12 of [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md). This includes layering in the backend, standalone components and signals in the frontend, multi-stage Docker builds, and dependency injection patterns.

### Definition of Done

7. **Tests required.** Any new behavior or change must be accompanied by tests:
   - Domain rules get **unit tests** (backend: xUnit; frontend: Vitest).
   - HTTP behavior and database persistence get **integration tests** (backend: `WebApplicationFactory` with Testcontainers).
   - UI logic gets **frontend unit tests** (Vitest with TestBed).
   - User journeys and end-to-end flows get **e2e tests** (Playwright).

A step is done only when:
   - The behavior is covered by tests at the appropriate levels.
   - Every test suite is green locally.
   - CI passes.

Never skip or delete tests unless the behavior they cover was intentionally removed and this document explicitly approves the removal.

### Test Commands

All three test levels must pass:

```bash
cd backend && dotnet test --project tests/PhoneBook.Api.UnitTests
cd backend && dotnet test --project tests/PhoneBook.Api.IntegrationTests
cd frontend && npm run lint && npm run test:ci
cd e2e && npm test
```

## References

- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) – Complete architectural documentation
- [docs/ARCHITECTURE.md#11-testing-strategy](docs/ARCHITECTURE.md#11-testing-strategy) – Testing strategy and coverage
- [docs/ARCHITECTURE.md#12-conventions-and-best-practices](docs/ARCHITECTURE.md#12-conventions-and-best-practices) – Code conventions
