---
applyTo: "src/Beagl.WebApp/**/*.{razor,razor.cs,cs,resx}"
description: "Localization, resource organization, and bilingual content rules for the WebApp."
---

# Localization Instructions

Use these rules for user-facing text and resources.

This scope intentionally includes `.razor.cs` and WebApp `.cs` files so localization helper classes and code-behind logic receive the same i18n guidance.

- All backend errors and UI text, excluding database content, must be localization-ready.
- Always provide English and French resource entries for every user-facing string.
- Backend, domain, and application failures must expose stable error codes such as `users.invalid_email`.
- The UI should resolve backend error codes through localization resources, with backend message as fallback only.
- The app must select language from browser preferences via request localization with supported cultures `en` and `fr`.
- Keep localized text centralized in resource files organized by concern.
- Avoid hardcoded user-facing strings in components, pages, and services.

## Resource Convention

- Use concern-specific marker classes in `src/Beagl.WebApp/Resources/`.
- Use `IStringLocalizer<TConcernResource>` in each page, component, or service using the matching concern resource.
- Resource naming must follow this pattern:
  - `Resources.{Concern}Resource.resx`
  - `Resources.{Concern}Resource.fr.resx`
- Add new keys to the smallest relevant concern file.
- If a string changes in one language, update the matching key in both languages in the same change.
- Keep backend error code keys in the concern resources used by the UI.
- French translations must use proper French orthography with accents.

## Detail Section Keys

For CRUD detail panels, section headings should use keys under `{Module}.Details.Section.{GroupName}` in both English and French resources.

Examples:

- `Users.Details.Section.Identity` -> `Identity` / `Identité`
- `Users.Details.Section.Contact` -> `Contact` / `Contact`
- `Users.Details.Section.Account` -> `Account` / `Compte`
- `Users.Details.Section.System` -> `System` / `Système`
