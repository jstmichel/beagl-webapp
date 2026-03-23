---
applyTo: "src/**/*.cs"
description: "Async, cancellation, and non-blocking I/O rules for Beagl source code."
---

# Async Instructions

Use these rules for all I/O and asynchronous workflows.

- All I/O operations must be asynchronous.
- Do not use `.Result`, `.Wait()`, or other blocking calls.
- Avoid `async void`.
- Use `CancellationToken` in application services and handlers.
- Async must flow from WebApp down to Infrastructure.
