---
applyTo: "src/**/*.cs"
description: "Clean Architecture boundaries, layering rules, and structural guidance for Beagl source code."
---

# Architecture Instructions

Use these rules for all source code changes.

## Project Shape

- Beagl is a modern CRM for animal centers.
- The solution follows Clean Architecture with strict layer separation:
  - Domain
  - Application
  - Infrastructure
  - WebApp

## Design Rules

- Respect SOLID principles.
- Prefer composition over inheritance.
- Avoid static state.
- Use dependency injection for all services.
- Keep methods small and cohesive.
- Limit file and function size.

## Layer Boundaries

- Domain must not reference Infrastructure or WebApp.
- Application must not reference WebApp.
- Infrastructure may reference Domain and Application.
- Mapping between layers must occur in Application.
- Do not expose EF Core entities outside Infrastructure.
- Repository interfaces live in Domain.
- Repository implementations live in Infrastructure.

## Project Structure

- Source projects:
  - `src/Beagl.Domain`
  - `src/Beagl.Application`
  - `src/Beagl.Infrastructure`
  - `src/Beagl.WebApp`
- Tests mirror source structure under `tests/`.
