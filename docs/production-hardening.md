# AI Coach OS — Production Operations & Hardening Guide

## 1. Overview
AI Coach OS is a modular monolith built on .NET 8 (ASP.NET Core) and Angular, utilizing PostgreSQL for persistence and multi-provider AI routing (Anthropic, Gemini, Groq, OpenRouter) with deterministic safety and evidence gating.

---

## 2. Configuration & Environment Variables

### Core Configuration
| Key / Variable | Description | Example / Default |
| :--- | :--- | :--- |
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=aicoachos;Username=...;Password=...;` |
| `Jwt:SecretKey` | HMAC-SHA256 secret for signing authentication tokens | (Change in production to $\ge$ 32 char random key) |
| `Jwt:Issuer` | JWT Issuer claim | `AiCoachOs` |
| `Jwt:Audience` | JWT Audience claim | `AiCoachOsApp` |
| `Jwt:ExpirationMinutes` | JWT lifetime in minutes | `120` |
| `Cors:AllowedOrigins` | Array of authorized frontend origins | `["https://app.aicoachos.com"]` |

### AI Provider Keys
AI provider keys are loaded via multi-key environment variables supporting round-robin rotation and HTTP 429 exponential backoff:
- `ANTHROPIC_API_KEYS`: Comma-separated list of Anthropic API keys (e.g. `sk-ant-key1,sk-ant-key2`).
- `GEMINI_API_KEYS`: Comma-separated list of Google Gemini API keys.
- `GROQ_API_KEYS`: Comma-separated list of Groq API keys.
- `OPENROUTER_API_KEYS`: Comma-separated list of OpenRouter API keys.

---

## 3. Database Migrations
Migrations are managed using EF Core and executed against PostgreSQL:
```bash
# Check for pending model changes
dotnet ef migrations has-pending-model-changes --project src/AiCoachOs.Infrastructure --startup-project src/AiCoachOs.Api

# Apply pending migrations to target database
dotnet ef database update --project src/AiCoachOs.Infrastructure --startup-project src/AiCoachOs.Api
```

---

## 4. Health Checks & Monitoring
The API provides an unauthenticated liveness and health endpoint:
- **Endpoint**: `GET /health`
- **Response**: `200 OK` (`Healthy`)

---

## 5. Security & Privacy Hardening
1. **Server-Side Authorization**: All client-scoped resources (`/api/clients`, `/api/workouts`, `/api/programs`, `/api/adaptations`, `/api/safety`, `/api/rehab`, `/api/memory`, `/api/photos`, `/api/videos`, `/api/expert-ingestions`, `/api/reasoning`) strictly enforce coach ownership server-side.
2. **Error Masking**: 500 Internal Server Errors return sanitized `ProblemDetails` (`"An unexpected error occurred while processing your request."`), preventing stack trace or internal database connection leakage.
3. **Media Privacy**: Uploaded physique photos undergo automated EXIF/metadata stripping to purge device and geolocation data. Storage keys use non-guessable GUIDs.

---

## 6. Build & Test Procedures
```bash
# Backend test suite
dotnet test

# Frontend production build
cd src/frontend
npm ci
npm run build
```

---

## 7. Open Infrastructure & Product Decisions
The following items remain designated for Product Owner review:
- **Backup Strategy & Provider**: `BACKUP DECISION REQUIRED — NOT INVENTED` (Cloud database automated snapshot retention and recovery schedule to be selected with target hosting provider).
- **Client Data Retention Policy**: Explicit retention timeframes for inactive client records remain subject to coach agreement policies.
- **Production Cloud Host**: Selection between cloud container services (AWS ECS, Azure App Service, GCP Cloud Run, VPS) remains neutral and decoupled.
