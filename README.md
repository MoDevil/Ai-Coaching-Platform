# AI Coach OS

Evidence-based coaching operating system for human gym coaches in Egypt.

## Architecture & Technology Stack
- **Backend**: .NET 8 (ASP.NET Core), C# 12, Entity Framework Core 8, PostgreSQL
- **Frontend**: Angular 18 (Standalone Components, TypeScript)
- **AI Engine**: Multi-Provider Router with round-robin key rotation and automated fallback (Anthropic, Gemini, Groq, OpenRouter)
- **Architecture**: Modular Monolith with clean domain separation (M0–M20)

## Quick Start (Development)

### Prerequisites
- .NET 8 SDK
- Node.js (v20+) & npm
- PostgreSQL 16 (or running via `docker-compose -f docker/docker-compose.dev.yml up -d`)

### Backend Setup
```bash
# Navigate to backend and run
dotnet restore
dotnet build
dotnet run --project src/AiCoachOs.Api
```
API runs on `http://localhost:5000` with Swagger available at `http://localhost:5000/swagger`.

### Frontend Setup
```bash
cd src/frontend
npm install
npm start
```
Frontend runs on `http://localhost:4200`.

### Health Check
- `GET http://localhost:5000/health` returns `200 OK` (`Healthy`).

## Testing & Verification
```bash
# Run all unit and integration tests
dotnet test

# Build Angular frontend
cd src/frontend
npm run build
```

For detailed production configuration and operations guidelines, see [Production Hardening Guide](docs/production-hardening.md).
