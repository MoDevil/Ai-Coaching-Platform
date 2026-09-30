# AI Coach OS

Evidence-based coaching operating system for human gym coaches in Egypt.

## Architecture & Technology Stack
- **Backend**: .NET 8 (ASP.NET Core), C# 12, Entity Framework Core 8, PostgreSQL
- **Frontend**: Angular 16 (mixed standalone + NgModule components), TypeScript 5.1
- **AI Engine**: Multi-Provider Router with round-robin key rotation and automated fallback (Anthropic, Gemini, Groq, OpenRouter)
- **Architecture**: Modular Monolith with clean domain separation (M0–M20)

## Quick Start (Development)
### Prerequisites
- .NET 8 SDK
- Node.js (v20+) & npm
- PostgreSQL 16 (or running via `docker-compose -f docker/docker-compose.dev.yml up -d`)

### Required Secrets

No secret is committed to this repository. The API refuses to start without them, so set them once
per machine using .NET user-secrets (stored outside the repo, in `%APPDATA%`):

```bash
# 32+ character HMAC-SHA256 signing key — generate with: openssl rand -base64 48
dotnet user-secrets set "Jwt:SecretKey" "<your-random-key>" --project src/AiCoachOs.Api

# Match docker/docker-compose.dev.yml
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=aicoachos;Username=postgres;Password=postgrespassword;" \
  --project src/AiCoachOs.Api
```

AI provider keys are read from comma-separated environment variables at call time, never from
configuration files. Set only the providers you intend to use:

```bash
# Windows PowerShell
$env:GROQ_API_KEYS="key1,key2"
$env:GEMINI_API_KEYS="key1,key2"
$env:OPENROUTER_API_KEYS="key1,key2"
```

| Variable | Provider |
| :--- | :--- |
| `ANTHROPIC_API_KEYS` | Anthropic |
| `GEMINI_API_KEYS` | Gemini |
| `GROQ_API_KEYS` | Groq |
| `OPENROUTER_API_KEYS` | OpenRouter |

Then select a provider with `dotnet user-secrets set "AiSettings:Provider" "Router" --project src/AiCoachOs.Api`.

`Mock` is accepted **only** in the Development environment (or when `AiSettings:AllowMockProvider` is
explicitly set). It returns fabricated recommendations and must never be used for real client data.

### Backend Setup
```bash
# Navigate to backend and run
dotnet restore
dotnet build
dotnet run --project src/AiCoachOs.Api
```

API runs on the port configured in `src/AiCoachOs.Api/Properties/launchSettings.json` with Swagger
available at `/swagger` (Development only).

### Frontend Setup
```bash
cd src/frontend
npm install
npm start
```

Frontend runs on `http://localhost:4200` and proxies `/api` to the backend via `proxy.conf.json`.

### Health Check
- `GET /health` returns `200 OK` (`Healthy`).

## Testing & Verification
```bash
# Run all unit and integration tests.
# Integration tests use a real PostgreSQL instance (no in-memory provider), so they need a database:
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=aicoachos;Username=postgres;Password=postgrespassword;"
dotnet test

# Build Angular frontend
cd src/frontend
npm run build

# Frontend unit tests
cd src/frontend
npm test                 # watch mode for local development
npm run test:ci          # headless single run, as used by CI
```

For detailed production configuration and operations guidelines, see [Production Hardening Guide](docs/production-hardening.md).
