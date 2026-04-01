---
applyTo: "src/Beagl.WebApp/**/*.{razor,razor.cs,css,cshtml,cshtml.cs}"
description: "UI design system, brand tokens, CRUD interaction patterns, and detail panel structure for the WebApp."
---

# UI Design Instructions

Use these rules for WebApp UI changes.

## UI Surface Selection

- Default to Blazor components and pages for application UI in `Components/`, including dashboards, CRUD workflows, reusable widgets, and interactive business screens.
- Prefer Blazor when the feature belongs to the main app shell, shares component state, or should follow the same navigation and composition model as the rest of the WebApp.
- Use Razor Pages only for thin request/response endpoints that benefit from the standard ASP.NET Core HTTP pipeline, especially authentication and account flows such as sign-in, sign-out, access denied, external login callbacks, email confirmation, and password reset.
- Prefer Razor Pages when the flow is naturally form-post plus redirect and does not need rich component interactivity.
- Do not introduce Razor Pages for regular business CRUD or reporting screens unless there is a clear technical need that Blazor does not satisfy.
- Avoid splitting a single feature across Blazor and Razor Pages unless the Razor Pages portion is limited to a narrow account or authentication boundary.
- When both technologies coexist, keep business UI in Blazor and keep Razor Pages small, isolated, and infrastructure-oriented.

## Brand and Design System

- Use the official Beagl logo asset at `src/Beagl.WebApp/wwwroot/images/beagl_logo.png` for brand marks in the UI.
- Primary brand color is `#4EAAAB` and must be the main accent token.
- Build interfaces with a clear visual direction and reusable design tokens.
- Define and use CSS variables for color, spacing, radii, shadows, and motion timings.
- Prefer expressive type stacks and intentional hierarchy.
- Keep accessibility first: contrast, focus states, semantic structure, and keyboard navigation.
- Use responsive layouts by default with mobile-first breakpoints.
- Use meaningful motion such as page entrance, staggered reveal, and panel transitions.
- Preserve the established visual language when updating an existing surface unless a redesign is explicitly requested.
- Keep style logic centralized and avoid scattered one-off inline styles.

## CRUD Interaction Rules

- All data lists must include pagination by default.
- Pagination and filtering must be query-level, server-side by default.
- For data-heavy CRUD screens, avoid permanent narrow side panes and long inline bottom detail editors.
- Prefer modal dialogs for quick create, edit, and details flows.
- Prefer dedicated routes or pages for complex or long forms.
- For CRUD table row actions, prefer icon-only, borderless action buttons.
- Icon-only buttons must include `aria-label` and `title`.
- Use the primary brand color for neutral icon-only actions and a red destructive variant for delete actions.
- In CRUD dialogs, keep a single secondary dismissal action in the footer.
- Do not show both a top close button and a footer cancel or close button at the same time.

## Form Validation Rendering

- Do not use per-field `<ValidationMessage For="..." />` components. They render raw unlocalized strings.
- Use a single consolidated validation summary block that iterates `_editContext.GetValidationMessages()` and renders each message through `L.LocalizeValidationMessage(message)`.
- Place the validation summary inside `<EditForm>` after `<DataAnnotationsValidator />` and before the form fields.
- See `localization.instructions.md` for the full validation pattern, key naming, and code example.

## Detail Panel Standard

Detail panels must use grouped sections with module-scoped BEM classes.

### Required Markup Shape

- Wrap the panel in `<div class="{module}-detail-sections">`.
- Use one `<section class="{module}-detail-section">` per logical group.
- Each section must include a localized heading `<h3 class="{module}-detail-section__heading">`.
- Use a `<dl class="{module}-detail-grid">` with `<div>`, `<dt>`, and `<dd>` triplets for fields.

Example:

```html
<div class="users-detail-sections" data-testid="details-panel">
	<section class="users-detail-section">
		<h3 class="users-detail-section__heading">@L["Users.Details.Section.Identity"]</h3>
		<dl class="users-detail-grid">
			<div>
				<dt>@L["Users.Form.UserName"]</dt>
				<dd>@user.UserName</dd>
			</div>
		</dl>
	</section>
</div>
```

### Required CSS Shape

- `.{module}-detail-sections` is a vertical flex stack with spacing between sections.
- `.{module}-detail-section__heading` uses uppercase micro-heading styling and a trailing divider rule.
- `.{module}-detail-grid` is a responsive tile grid using `repeat(auto-fit, minmax(16rem, 1fr))`.
- Grid items use padded, rounded surface tiles.
- `dt` elements use subdued label styling.
- `dd` elements use stronger value styling.

Reference CSS:

```css
.{module}-detail-sections {
	display: flex;
	flex-direction: column;
	gap: 1.5rem;
}

.{module}-detail-section__heading {
	display: flex;
	align-items: center;
	gap: 0.75rem;
	margin: 0 0 0.75rem;
	color: var(--ink-soft);
	font-size: 0.78rem;
	font-weight: 600;
	text-transform: uppercase;
	letter-spacing: 0.1em;
}

.{module}-detail-section__heading::after {
	content: "";
	flex: 1;
	height: 1px;
	background: var(--surface-border);
}

.{module}-detail-grid {
	display: grid;
	gap: 1rem;
	grid-template-columns: repeat(auto-fit, minmax(16rem, 1fr));
}

.{module}-detail-grid > div {
	padding: 1rem;
	border-radius: 1rem;
	background: color-mix(in srgb, var(--surface-solid) 82%, #edf2fa 18%);
}

.{module}-detail-grid dt {
	margin-bottom: 0.35rem;
	color: var(--ink-soft);
	font-size: 0.82rem;
	text-transform: uppercase;
	letter-spacing: 0.08em;
}

.{module}-detail-grid dd {
	margin-bottom: 0;
	font-weight: 600;
}
```

### Group Order

Use this order and omit groups that do not apply:

1. Identity or Profile
2. Contact
3. Dates
4. Status or Account
5. Relations
6. System

- Consider collapsing long technical identifiers in the System group by default.
