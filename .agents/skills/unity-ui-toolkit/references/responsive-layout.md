# Responsive UI Layout

## Purpose

Define and verify scaling separately from adaptive composition.

## Panel Policy

Inspect the active UIDocument's PanelSettings, scale mode, reference resolution,
match policy, DPI behavior, target texture and sorting. Record actual values.
Do not assume a serialized reference resolution means ScaleWithScreenSize.
Existing project panels observed during skill preparation use ConstantPhysicalSize;
re-inspect before relying on that observation. Do not silently rewrite shared panels.

Propose ScaleWithScreenSize and an appropriate reference/match policy only when
the target requires it and ownership/change is approved. Explain consequences
for every document using that panel. At identical aspect ratio a chosen screen
scale can preserve logical layout across 1920x1080 and 3840x2160; asset resolution,
font rendering and actual panel behavior must still be checked.

## Adaptive Composition

Use Yoga/Flexbox, relative/flexible sizes, shrink/grow and deliberate min/max bounds.
Constrain popup width/content height; keep essential actions reachable while
overflowing content scrolls. Reserve absolute positioning for justified overlays.
Do not mechanically double every USS value for UHD or simulate browser CSS APIs
that the installed Unity version does not support.

When width requires alternate composition, use a scoped class policy driven by
actual panel/container geometry. Handle GeometryChangedEvent without layout
feedback loops, repeated callback registration or constant rebuilding. Update
classes only when the relevant size band changes. Account for initial layout
and unregister at owner release. Keep readability and input target sizes explicit.

## Resolution and Content Matrix

| Case | Required Observation |
| --- | --- |
| FHD 1920x1080, 16:9 | Reference composition, readable text, complete actions |
| UHD 3840x2160, 16:9 | Intended logical scale, crisp resources/text, stable proportions |
| Relevant 16:10 / ultrawide | Reflow/alignment and bounded content, no unintended stretching |
| Minimum supported window | No inaccessible actions; defined overflow and scroll |
| Long/localized text, empty and dense data | No clipping/overlap; meaningful empty/loading/error states |

Select supported cases from actual target requirements; proposed unsupported
formats are questions, not invented guarantees. Record panel policy, screen/panel
size, data set and screenshot for each executed case. Editor source inspection
alone does not prove a rendered case passed.

## Technical Sources

- [Unity 6.3 Panel Settings](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-Runtime-Panel-Settings.html).
- [Unity adaptive layouts](https://docs.unity3d.com/6000.3/Documentation/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/layouts.html).
- [GeometryChangedEvent](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UIElements.GeometryChangedEvent.html).
