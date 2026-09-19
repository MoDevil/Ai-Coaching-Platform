# Milestone M3 Report — Scientific Knowledge Foundation

**Project:** AI Coach OS  
**Milestone:** M3 (Scientific Knowledge Foundation)  
**Status:** COMPLETED  
**Date:** September 19, 2026  
**Author:** Builder Agent  
**Deliverables Produced:**
1. Domain Entities, Enums, and Relationships (`AiCoachOs.Domain/Knowledge`)
2. Application DTOs, Service Contracts, and Validators (`AiCoachOs.Application/Knowledge`)
3. EF Core Configurations, Migration `20260919061104_AddScientificKnowledgeFoundation`, and Service Implementation (`AiCoachOs.Infrastructure`)
4. REST API Endpoints (`KnowledgeController`, `ExercisesController`)
5. Comprehensive Unit & Integration Test Suite (`tests/AiCoachOs.UnitTests`, `tests/AiCoachOs.IntegrationTests`)
6. Angular Frontend Feature Models & Service Integration (`src/frontend`)
7. `docs/milestones/M3-report.md`

---

## 1. Executive Summary

Milestone M3 establishes the evidence and scientific knowledge foundation for AI Coach OS. It introduces deterministic storage, retrieval, tracing, and versioning of scientific and coaching claims (`Claim → Evidence Source → Context → Status/Version`).

Core architectural principles strictly upheld:
- **No LLM, RAG, or Vector Databases**: M3 is a deterministic storage and traceability foundation.
- **Clear Domain Boundary**: Exercise Domain (M2) describes what an exercise is; Scientific Knowledge (M3) describes what source-backed literature asserts about a topic, method, or exercise.
- **Non-Mutating Exercise Linking**: Associating a claim with an exercise (`ExerciseId`) does **not** mutate `Exercise` properties or automatically promote `MetadataStatus.Provisional` to `Verified`.
- **No Fabricated Evidence**: Zero fake studies, DOIs, authors, or claims were seeded. Test fixtures in integration tests isolate test data.
- **Traceability & Versioning**: Claims link explicitly to supporting sources (`KnowledgeClaimSource`) and maintain supersession versioning (`SupersededByClaimId`, `SupersededAtUtc`, `SupersessionReason`).
- **Full Test Verification**: **119 automated tests** (89 unit + 30 integration) pass against live PostgreSQL with zero failures.

---

## 2. Domain Entities & Enums

### 2.1 Enums
- **`EvidenceLevel`**:
  - `Anecdotal = 1`
  - `Mechanistic = 2`
  - `ExpertConsensus = 3`
  - `RandomizedControlledTrial = 4`
  - `MetaAnalysis = 5`
- **`ClaimStatus`**:
  - `Provisional = 1`
  - `Active = 2`
  - `Superseded = 3`
  - `Rejected = 4`
- **`KnowledgeSourceType`**:
  - `ScientificPaper = 1`
  - `PositionStand = 2`
  - `ExpertConsensus = 3`
  - `Book = 4`
  - `PractitionerNote = 5`

### 2.2 Entities
- **`KnowledgeSource`**: External publication or consensus document with title, authors, publication year, evidence level, DOI, URL, and notes.
- **`KnowledgeClaim`**: Structured scientific/coaching claim with topic, question, claim text, evidence level, population context, limitations, practical application, status, optional `ExerciseId`, review metadata, and replacement link (`SupersededByClaimId`).
- **`KnowledgeClaimSource`**: Many-to-many join entity capturing the specific relevance note linking a claim to its supporting source.

---

## 3. Database Schema & Migrations

- Migration: `20260919061104_AddScientificKnowledgeFoundation`
- Tables Created:
  - `knowledge_sources`: Primary key `id`, indexes on `source_type` and `evidence_level`.
  - `knowledge_claims`: Primary key `id`, foreign key `exercise_id` (SetNull), self-referencing foreign key `superseded_by_claim_id` (Restrict), indexes on `topic`, `status`, `exercise_id`, and `superseded_by_claim_id`.
  - `knowledge_claim_sources`: Composite primary key `(claim_id, source_id)` with cascading foreign keys to claims and sources.
- Verified with `dotnet ef migrations has-pending-model-changes`: zero pending changes.

---

## 4. API Endpoints

| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/knowledge/sources` | Create a new knowledge source | Yes |
| `GET` | `/api/knowledge/sources` | List all knowledge sources | Yes |
| `GET` | `/api/knowledge/sources/{id}` | Get knowledge source details by ID | Yes |
| `POST` | `/api/knowledge/claims` | Create a new knowledge claim (general or exercise-linked) | Yes |
| `GET` | `/api/knowledge/claims` | Filter claims by topic, exerciseId, status, minEvidenceLevel | Yes |
| `GET` | `/api/knowledge/claims/{id}` | Get knowledge claim with supporting sources and supersession chain | Yes |
| `POST` | `/api/knowledge/claims/{id}/sources` | Associate supporting source with claim | Yes |
| `POST` | `/api/knowledge/claims/{id}/supersede` | Supersede claim with replacement claim | Yes |
| `GET` | `/api/exercises/{id}/claims` | Retrieve scientific claims associated with an exercise | Yes |

---

## 5. Testing & Verification

- **Unit Tests (`AiCoachOs.UnitTests`):** 89 passed, 0 failed.
  - Domain invariants for `KnowledgeClaim`, `KnowledgeSource`, and `KnowledgeClaimSource`.
  - Duplicate source association prevention.
  - Supersession invariants (self-supersession rejected, double-supersession rejected, reason required, replacement linked).
  - FluentValidation unit tests for sources, claims, source addition, and supersession.
- **Integration Tests (`AiCoachOs.IntegrationTests`):** 30 passed, 0 failed.
  - Source creation and retrieval against live PostgreSQL.
  - General claims (`ExerciseId == null`) and exercise-linked claims (`ExerciseId` valid).
  - Many-to-many source association and retrieval.
  - Deterministic supersession and state verification.
  - Unauthorized request rejection (`401 Unauthorized`).
- **Frontend Verification:**
  - Angular production build (`npm run build`) passed with zero errors and zero warnings (`Initial Total: 421.83 kB`).
