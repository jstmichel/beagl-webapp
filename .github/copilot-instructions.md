# Project Overview

Beagl is a modern CRM for animal centers. It centralizes management of animals, owners, and employees, provides a self-service portal, facilitates adoption and annual fee payments, and offers reporting tools and a secure web interface.

## Technologies Used

- .NET 10 / C#
- ASP.NET Core
 - Blazor static server-side rendering (SSR) and interactive server-side Blazor (real-time UI updates via SignalR)
- Entity Framework Core
- Docker (deployment)
- PostgreSQL (development and production)
- Identity with EF Core for user and role management

## Main Features & Workflows

- Domain entity and role management
- Data access and migrations
- Authentication and authorization
- Employee management (profiles, details)
- Animal registration and tracking
- Adoption workflow
- Owner portal (self-service, annual fee payment)
- Payment processing
- Data centralization
- Reporting tools

## Design Patterns & Architecture

- Layered architecture: Domain, Infrastructure, Application, WebApp
- SOLID principles and Clean Architecture
- Patterns used: Repository, Dependency Injection, Factory, Singleton, Strategy, Adapter, Observer, Mediator, Command, CQRS, Specification

## Naming and Formatting Conventions

- PascalCase for classes, methods, properties
- camelCase for local variables and parameters
- Prefix 'I' for interfaces (e.g., `IAnimalRepository`)
- Explicit and descriptive names
- Tab indentation (per `.editorconfig`)
- Line length: max 120 characters
- Braces on new line

## Error Handling and Logging

**Error Handling Guidelines:**
- Always use the result pattern (e.g., `Result<T>`) for error handling in all layers, including domain, application services, use cases, and handlers, except for critical exceptions in domain logic.
- Only throw exceptions in aggregates, entities, and value objects for critical domain invariant violations. Such exceptions must inherit from `DomainException`.
- Do not use exceptions for expected business or validation errors; always prefer the result pattern.
- Only catch exceptions if they are explicitly handled.
- Use custom exception types if relevant.
- Logging via ASP.NET Core `ILogger<T>`.
- Do not log sensitive information.
- Use structured logging for complex data.

## Data Sources

- PostgreSQL database for all environments

## Configuration and Deployment

- Dockerfile for web app deployment
- EF Core migrations in `src/Beagl.Infrastructure/Migrations/`
- Configuration files in `src/Beagl.WebApp/`


## Commit Message Guidelines

- Use Conventional Commits, lowercase (e.g., `feat: add login form partial`)
- Include a clear summary of the change
- For breaking changes, add `BREAKING CHANGE:` in the message body and describe the impact

### Commit Message Generation for Multiple Changes

- When committing multiple related changes, generate a single Conventional Commit message that summarizes all changes.
- Use only one header (type/scope/subject) per commit.
- List all relevant details in the commit body as bullet points.
- Do not include multiple headers or separate commit messages in a single commit.
- Example:

```
chore: add .gitignore, security policy, and test .editorconfig

- include common build results and user-specific files
- add settings for VSCode and Rider
- exclude environment and database files
- cover logs, npm packages, and temporary files
- outline supported versions and reporting process
- include security best practices for users
- emphasize responsible disclosure of vulnerabilities
- allow underscores in test method names for readability
```

## Code Standards and Style

- Source code must be written in English (variable, function, class names, etc.)
- Comments and documentation must be in English, clear and precise
- Follow clean code principles: simplicity, readability, modularity, avoid duplication
- Follow Microsoft style recommendations for the language used
- Indentation must be tabs, per `.editorconfig`
- Use `.editorconfig` as the main linter for code quality and consistency
- C# analyzers are used and all warnings are treated as errors during compilation
- Document all public functions and main classes
- Avoid code duplication: prefer reuse via functions or classes
- Favor simplicity and readability: code understandable by all team members
- Modularize code: separate responsibilities into distinct modules or classes
- Respect Single Responsibility Principle
- Use explicit names for variables, functions, and classes
- Add unit tests for critical functions
- Limit file and function size for maintainability
- Document entry points and public APIs
- Commit messages must follow Conventional Commits for clarity and traceability
- Use file-scoped namespace declarations for C# source files

## Folder and Project Structure

The repository contains the following projects:

- `src/Beagl.WebApp`: main web app (Blazor SSR + interactive Blazor)
    - Should contain: controllers, viewmodels, pages, UI components, integration services, UI-specific config
- `src/Beagl.Infrastructure`: infrastructure layer (data access, external services)
    - Should contain: repositories, data contexts, persistence DTOs, external integrations, technical config
- `src/Beagl.Application`: application logic (use cases, business services)
    - Should contain: application services, exchange DTOs, use case management, domain/infrastructure mapping
- `src/Beagl.Domain`: domain model and business rules
    - Should contain: entities, aggregates, value objects, repository interfaces, business rules, domain exceptions

Tests are organized similarly:

- `tests/Beagl.WebApp.Tests`: web app tests (controllers, UI, integration)
- `tests/Beagl.Infrastructure.Tests`: infrastructure tests (repositories, data access)
- `tests/Beagl.Application.Tests`: application logic tests (services, use cases)
- `tests/Beagl.Domain.Tests`: domain tests (entities, aggregates, business rules)

All repositories must include:
- A `README.md` file with specific instructions
- Source code in the `src/` folder as described
- Unit tests in the `tests/` folder as described
- Required config files (`.editorconfig`, etc.)

**Important: All code generation must strictly follow the rules defined in the `.editorconfig` file at the project root.**
This includes:
- Indentation (spaces or tabs per config)
- UTF-8 encoding
- LF line endings
- No trailing whitespace
- Naming and documentation conventions
- C# style preferences (explicit types, private fields prefixed, `readonly`, initializers, etc.)
- Explicit access modifiers
- Remove unused `using` statements
- XML documentation for public members
- Other analyzer rules and recommendations in `.editorconfig`
- File header based on `.editorconfig` settings
