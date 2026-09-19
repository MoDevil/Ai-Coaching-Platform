# Milestone M2 Report — Exercise & Training Domain

**Project:** AI Coach OS  
**Milestone:** M2 (Exercise & Training Domain)  
**Status:** COMPLETED  
**Date:** September 19, 2026  
**Author:** Builder Agent  
**Deliverables Produced:**
1. Domain Entities, Value Objects, and Enums (`AiCoachOs.Domain`)
2. Application DTOs, Service Contracts, and Validators (`AiCoachOs.Application`)
3. EF Core Configurations, Migrations, Seeders, and Services (`AiCoachOs.Infrastructure`)
4. REST API Endpoints (`AiCoachOs.Api`)
5. Comprehensive Unit & Integration Test Suite (`tests/AiCoachOs.UnitTests`, `tests/AiCoachOs.IntegrationTests`)
6. Angular Frontend Feature Views & Services (`src/frontend`)
7. `docs/milestones/M2-report.md`

---

## 1. Executive Summary

Milestone M2 implements the foundational domain layer for evidence-based exercise libraries and client training profiles in AI Coach OS.

In accordance with the project philosophy:
- **No LLMs, RAG, or Vector Databases** were used.
- **No arbitrary numeric scores** (e.g. stimulus = 8.7) were introduced. All stimulus, fatigue, and technical complexity metrics are strictly qualitative (`Low`, `Moderate`, `High`). Resistance profiles explicitly follow biomechanical curves (`Lengthened`, `MidRange`, `Shortened`, `Even`).
- **No M3 scientific evidence citations** or **M4 workout program generation** were implemented prematurely.
- The `Client` aggregate is preserved as clean and focused; `ClientTrainingProfile` models client training parameters referencing `ClientId: Guid` as a value identifier.
- Multi-tenancy and Coach-Client data isolation are strictly enforced on all profile and exercise queries.

---

## 2. Implemented Domain Model

### 2.1 Enums
- `QualitativeRating`: `Low`, `Moderate`, `High`
- `ResistanceProfile`: `Lengthened`, `MidRange`, `Shortened`, `Even`
- `ExerciseCategory`: `Compound`, `Isolation`, `Machine`, `Bodyweight`
- `TrainingExperienceLevel`: `Beginner`, `Intermediate`, `Advanced`
- `MetadataStatus`: `Provisional` (heuristic coaching estimates), `Verified` (evidence-backed knowledge in M3)

### 2.2 Core Entities & Value Objects
- **`Muscle`**: Canonical muscle representation with anatomical group and function.
- **`MovementPattern`**: Biomechanical movement classification (e.g. Squat, Hinge, Horizontal Push, Horizontal Pull, Vertical Push, Vertical Pull).
- **`Equipment`**: Training equipment inventory with category.
- **`Exercise`**: Core exercise entity with movement pattern, resistance profile, technical demand, qualitative coaching estimates, and explicit `MetadataStatus` tracking. Medical contraindications are strictly excluded and deferred to M8 Safety & Medical Awareness.
- **`ExerciseMuscle`**: Explicit join entity tracking target muscles with `IsPrimary` designation.
- **`ExerciseEquipment`**: Explicit join entity linking exercises to required equipment with `IsRequired` flag.
- **`ExerciseSubstitution`**: Relational substitutions between exercises preserving mechanical intent.
- **`ClientTrainingProfile`**: Training profile for a specific client (`ClientId`), capturing experience level, equipment, preferences, constraints, and notes.
- **`ClientTrainingPriority`**: Focus-area priority assignments per profile.
- **`TrainingAvailability`**: Embedded value object encapsulating sessions per week, available days, and session duration bounds.

---

## 3. Database Schema & Migrations

- Migration: `20260919052955_AddExerciseAndTrainingProfile`
- Tables Created:
  - `muscles`
  - `movement_patterns`
  - `equipment`
  - `exercises`
  - `exercise_muscles`
  - `exercise_equipment`
  - `exercise_substitutions`
  - `client_training_profiles`
  - `client_training_priorities`
- Converted Value Objects:
  - `TrainingAvailability` mapped as owned entity columns (`days_per_week`, `available_days`, `session_duration_minutes`, `schedule_notes`).
  - JSON conversions with custom `ValueComparer` for list properties (`List<Guid>`, `IReadOnlyList<DayOfWeek>`).
- Database Seeder:
  - `ExerciseLibrarySeeder` seeds 14 foundational compound and isolation exercises with complete biomechanical patterns, muscle maps, equipment, and substitution links, thread-synchronized with `SemaphoreSlim` for safe parallel test runners.

---

## 4. API Endpoints

### 4.1 Exercises (`/api/exercises`)
- `GET /api/exercises`: Filterable exercise library search (by search term, movement pattern, muscle, equipment, category).
- `GET /api/exercises/{id}`: Detailed exercise inspection with muscles, equipment, and metadata.
- `GET /api/exercises/{id}/substitutions`: List viable substitutions with biomechanical reasoning and similarity.
- `GET /api/exercises/meta/muscles`: Canonical muscle catalog.
- `GET /api/exercises/meta/movement-patterns`: Movement pattern catalog.
- `GET /api/exercises/meta/equipment`: Equipment catalog.

### 4.2 Client Training Profiles (`/api/clients/{clientId}/training-profile`)
- `GET /api/clients/{clientId}/training-profile`: Retrieves full training profile with priorities and availability (coach-tenant secured).
- `PUT /api/clients/{clientId}/training-profile`: Creates or updates training profile baseline.
- `PUT /api/clients/{clientId}/training-profile/availability`: Dedicated schedule and availability update.
- `PUT /api/clients/{clientId}/training-profile/priorities`: Update muscle-specific focus priorities.

---

## 5. Testing & Verification

- **Unit Tests (`AiCoachOs.UnitTests`):** 62 passed, 0 failed.
  - Exercise domain invariants, validation rules, rating validations, substitution creation, and `MetadataStatus` state transitions.
  - Client training profile domain invariants, duplicate priority checks, availability boundaries.
  - FluentValidation profile validator rules.
- **Integration Tests (`AiCoachOs.IntegrationTests`):** 24 passed, 0 failed.
  - Seeded exercise retrieval, filtering, substitution verification, and `MetadataStatus.Provisional` assertion against live PostgreSQL.
  - Training profile creation, update, schedule modification, and priority replacement.
  - Multi-tenant Coach isolation enforcement: returning `403 Forbidden` or `404 Not Found` when a coach accesses another coach's client training profile.
- **Frontend Verification:**
  - Angular build completed with zero errors and zero warnings (`Initial Total: 418.04 kB`).
  - UI routes: `/exercises`, `/exercises/:id`, and `/clients/:id/training-profile`.
  - Displaying explicit `Provisional Estimates` badges and contextual disclaimers.
