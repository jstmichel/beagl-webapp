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

## Form Validation

- Never use bare `[Required]`, `[MaxLength]`, or other data-annotation attributes on Blazor form models for user-facing validation. Default .NET data annotations produce English-only messages that bypass the localization pipeline.
- Implement `IValidatableObject` on form models. Yield `ValidationResult` objects whose `ErrorMessage` is a resx key (e.g., `citizen_profile.first_name_required`).
- In the Razor template, render errors with a consolidated validation summary block using `_editContext.GetValidationMessages()` and `L.LocalizeValidationMessage(message)`. Do not use per-field `<ValidationMessage For="..." />` components, as they render the raw (unlocalized) message.
- The `LocalizeValidationMessage` extension method looks up the key in `IStringLocalizer` and falls back to the raw string only when the resource is not found. Always add keys to both `.resx` and `.fr.resx` concern files.

### Validation Summary Pattern

```razor
@if (_editContext.GetValidationMessages().Any())
{
    <div class="alert alert-danger mb-3" role="alert">
        <ul class="mb-0 ps-3">
            @foreach (string message in _editContext.GetValidationMessages().Distinct())
            {
                <li>@L.LocalizeValidationMessage(message)</li>
            }
        </ul>
    </div>
}
```

Place this block inside the `<EditForm>` after `<DataAnnotationsValidator />` and before the form fields.

### Validation Key Naming

- Use the pattern `{concern}.{field}_{rule}` for validation keys.
- Examples: `citizen_profile.first_name_required`, `citizen_profile.street_too_long`, `setup.organization_name_required`.

## Detail Section Keys

For CRUD detail panels, section headings should use keys under `{Module}.Details.Section.{GroupName}` in both English and French resources.

Examples:

- `Users.Details.Section.Identity` -> `Identity` / `Identité`
- `Users.Details.Section.Contact` -> `Contact` / `Contact`
- `Users.Details.Section.Account` -> `Account` / `Compte`
- `Users.Details.Section.System` -> `System` / `Système`
