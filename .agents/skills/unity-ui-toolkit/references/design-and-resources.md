# UI Design and Resource Planning

## Purpose

Translate a design into a reviewable hierarchy and resource plan before edits.

## Analysis and Decomposition

1. Inspect the supplied mockup at its actual resolution. Identify regions, order,
   alignment, spacing, typography, colors, overlays, scroll boundaries and states.
   Separate measured facts, inferred intent and unresolved decisions. Do not claim
   pixel measurements from an image that was not inspected.
2. Map behavior: navigation, submit/cancel, focus/default action, loading/empty/
   error/disabled states, selection, validation, localization and long content.
3. Inspect existing host/views/templates/tokens and actual asset owners. Present
   a tree naming UXML elements, reusable widgets, list templates and binding points.
   Explain each meaningful container and sizing/overflow responsibility.
4. Specify exact new/changed/reused files and resource requirements before approval.
   Use conventions for UXML/USS, definitions, catalogs and sprites. Confirm paths;
   example names are proposals, not existing resources.

## Resource Search and Decision

Use [the canonical common layout](../../../../docs/CONVENTIONS.md#common-ui-resource-layout)
and [promotion rules](../../../../docs/UI_TOOLKIT_STANDARD.md#common-ui-resources).
Search in this order:

1. The owning feature's existing UI and resource references.
2. Root CommonUI groups: `Assets/ProjectCore/Assets/UI/UXML/CommonUI`,
   `UI/USS/CommonUI`, `Sprites/CommonUI` and `Fonts/CommonUI`, with the latter
   three paths relative to `Assets/ProjectCore/Assets`.
3. Resources of the same context and relevant generic Template capabilities.
4. Other active features as promotion candidates. Another concrete context's
   resources cannot become direct dependencies; propose approved root promotion.

Confirm which directories actually exist; a reserved location is not an available
asset. Inspect references and visual/import metadata; filename matches alone do
not prove suitability. For concrete Editor asset lookups use the installed Unity
Search skill and targeted Editor inspection. Do not use legacy Prototype as a
supported runtime dependency.

For each required sprite/font/icon list actual path, intended use, dimensions,
visual states, ownership, reuse fit, import/slice requirements and decision: reuse,
supplied, missing, proposed root promotion or generated placeholder. Reuse a suitable common button before
requesting one. If nothing fits, explain the search and mismatch, then ask whether
the developer has a ready resource or wants the proposed prototype generation.
Do not ask again when generation/resources are already authorized for this scope.

Use USS for simple fills/borders/shapes and image art for textured/illustrated
elements. Keep editable/localized text as UI text. Check 9-slice borders and
pixels-per-unit with the actual panel policy. Prefer scalable composition over
one baked screenshot. Do not demand art that the current prototype does not need.

## Prototype Art and Implementation

After plan/resource approval, reuse supplied assets or generate only agreed missing
prototype art with `imagegen`, reading that skill first. Inspect edits' sources and
preserve requested transparency. Generate consistent states/style with sufficient
source resolution; disclose generated provenance and temporary status.

Import through the connected Editor, preserve `.meta`, confirm texture/sprite
settings and rendered appearance. Store feature-owned placeholders under the
approved semantic asset folder. Include exact paths and remaining replacement
work in review; do not treat placeholder acceptance as production art approval.

Prototype with representative long/empty/mixed data and actual PanelSettings.
Compare renders to the approved source, identify concrete differences, and request
visual acceptance with next functional work stated. Correct feedback in the same
scope; changed design/resource decisions revise the relevant plan.

## Technical Source

[Unity 6.3 background styling and 9-slicing](https://docs.unity3d.com/6000.3/Documentation/Manual/UIB-styling-ui-backgrounds.html).
