# AI Coach OS

Evidence-based coaching operating system for human gym coaches in Egypt.

## Architecture & Technology Stack
- **Backend**: .NET 8 (ASP.NET Core), C# 12, Entity Framework Core 8, PostgreSQL
- **Frontend**: Angular 16 (mixed standalone + NgModule components), TypeScript 5.1
- **AI Engine**: Multi-Provider Router with round-robin key rotation and automated fallback (Anthropic, Gemini, xAI, Groq, Cerebras, SambaNova, HuggingFace, OpenRouter)
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

The router walks the chain in the `Priority` order configured under `AiProviders:Providers` in
`src/AiCoachOs.Api/appsettings.json` and moves to the next entry whenever the current provider fails
or exhausts all of its keys. Set keys only for the providers you intend to use.

| Priority | Variable | Provider | Default model |
| :--- | :--- | :--- | :--- |
| 1 | `ANTHROPIC_API_KEYS` | Anthropic | `claude-sonnet-4-6` |
| 2 | `GEMINI_API_KEYS` | Gemini | `gemini-2.0-flash` |
| 3 | `XAI_API_KEYS` | xAI | `grok-4.6` |
| 4 | `GROQ_API_KEYS` | Groq | `llama-3.3-70b-versatile` |
| 5 | `CEREBRAS_API_KEYS` | Cerebras | `qwen-3.8-27b` |
| 6 | `SAMBANOVA_API_KEYS` | SambaNova | `gpt-oss-120b` |
| 7 | `HUGGINGFACE_API_KEYS` | HuggingFace | `openai/gpt-oss-120b` |
| 8 | `OPENROUTER_API_KEYS` | OpenRouter | `openai/gpt-4o-mini` |

Reorder or disable entries by editing `AiProviders:Providers` in `src/AiCoachOs.Api/appsettings.json`.
Set `"Enabled": false` to skip a provider without deleting it.

Then select a provider with `dotnet user-secrets set "AiSettings:Provider" "Router" --project src/AiCoachOs.Api`.

`Mock` is accepted **only** in the Development environment (or when `AiSettings:AllowMockProvider` is
explicitly set). It returns fabricated recommendations and must never be used for real client data.

### Backend Setup
```bash
# Apply EF Core migrations to the local database
dotnet ef database update --project src/AiCoachOs.Infrastructure --startup-project src/AiCoachOs.Api

# Build and run
dotnet build
dotnet run --project src/AiCoachOs.Api
```

The API listens on `http://localhost:5076` and `https://localhost:7194`. Swagger UI is at
`http://localhost:5076/swagger` (Development only). `http://localhost:5076/health` returns `Healthy`.

If you get `Jwt:SecretKey is not configured`, the user-secrets step above was skipped or was run from
a different folder. User secrets are per-project and per-user, so run it from the repository root.

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
