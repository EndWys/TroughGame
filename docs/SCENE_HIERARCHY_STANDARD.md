# Scene Hierarchy Standard

This document defines the authored Unity hierarchy for project-owned gameplay
scenes. It complements the ownership, DI, and lifecycle rules in
[`ARCHITECTURE_RULES.md`](ARCHITECTURE_RULES.md); it does not replace them.

`Scene_TechnicalPrototype` is the current reference implementation of this
standard.

## Scope And Exceptions

The standard applies to every new project-owned gameplay scene under
`Assets/ProjectCore/Contexts/<Context>/Assets/Scenes`.

The following are explicit exceptions:

- `ProjectContext.prefab` is the Zenject application-lifetime composition root.
- `Scene_Preloader` is a temporary application-entry scene, not a gameplay
  scene.
- Third-party samples, Template examples, and legacy `Prototype` experiments
  are reference content and do not define the standard.

## Root Hierarchy

Gameplay scenes must contain exactly these authored root containers, in this
order. A container is an organizational `GameObject` with only a `Transform`.

```text
===== CONTEXT =====
  SceneContext
  SceneFeatures

===== NETWORK =====
  NetworkBootstrap
  NetworkRunner
  NetworkObjectProvider

===== CAMERAS =====
  Main Camera

===== UI =====
  EventSystem
  ScreenNavigationUI
  PopupUI

===== GAMEPLAY =====
===== ENVIRONMENT =====
```

Container names, uppercase spelling, separator characters, and root order are
part of the standard. Do not place authored scene objects directly at the root
or create additional root containers. Empty `GAMEPLAY` and `ENVIRONMENT`
containers are kept so a scene can grow without changing its overall shape.

## Container Ownership

### `===== CONTEXT =====`

- `SceneContext` owns only the Zenject `SceneContext` component and its
  serialized installer list. It does not host scene feature components.
- `SceneFeatures` hosts the scene context installer and every
  `BaseMonoBehaviourFeature` registered through `AddFeatureFromComponent<TFeature>()`.
  They share this object because the framework resolves each feature from its
  installer's `GameObject`.

### `===== NETWORK =====`

- This container owns scene-level Fusion setup only.
- In a networked gameplay scene it contains `NetworkBootstrap`, `NetworkRunner`,
  and `NetworkObjectProvider` as direct children.
- `NetworkRunner` owns its Fusion callback, scene-manager, visibility, physics,
  and debugging components. Do not scatter these components across unrelated
  objects.
- The current project flow uses the standard Fusion connection menu and a room
  name. Custom lobby objects do not belong in a scene until that feature is
  explicitly introduced.

### `===== CAMERAS =====`

- This container owns all scene cameras and their camera-local components.
- The gameplay camera is named `Main Camera` and has the `MainCamera` tag.
- A gameplay scene must provide a camera when it uses UI Toolkit or IMGUI,
  including Fusion's standard connection menu.

### `===== UI =====`

- This container owns authored UI input and presentation objects.
- `EventSystem` is the single scene input-event root.
- UI Toolkit `UIDocument` objects and their presentation components live under
  descriptive direct children such as `ScreenNavigationUI` and `PopupUI`.
- Screen Navigation and Popup feature components remain under `SceneFeatures`;
  their serialized references point to their presentation components under
  `UI`.
- A project-owned UI object must not be placed under `SceneContext`,
  `NetworkRunner`, or a gameplay object merely for convenience.

At runtime Unity may add children named after `PanelSettings` assets under
`EventSystem`. They contain UI Toolkit's `PanelEventHandler` and
`PanelRaycaster`, which bridge Input System events to a runtime UI panel. They
are engine-generated, are not authored scene hierarchy, and must not be moved,
saved, or treated as project-owned UI objects.

### `===== GAMEPLAY =====`

- This container owns scene-specific gameplay roots, test actors, spawn
  markers, and presentation objects that are part of gameplay.
- Group objects by a stable gameplay responsibility when the scene grows; do
  not introduce a new top-level container for a feature.

### `===== ENVIRONMENT =====`

- This container owns non-gameplay world presentation: terrain, static level
  geometry, lighting, volumes, environment effects, and decoration.
- Environment content does not contain gameplay services, UI, or networking
  objects.

## Lifecycle And References

Scene entry, registration and initialization follow the architecture's
[Runtime Entry Flow](ARCHITECTURE_RULES.md#runtime-entry-flow) and
[Initialization Flow](ARCHITECTURE_RULES.md#initialization-flow). Runtime dependencies
follow its [DI rules](ARCHITECTURE_RULES.md#dependency-injection).

Serialized references may cross containers only for Unity presentation or
composition: for example, `SceneContext` to its installer, a feature to its
UI component, or a bootstrap component to its runner prefab.

## New Gameplay Scene Checklist

Before considering a new gameplay scene ready for feature work, verify that:

- its name follows [Unity Resource Naming](CONVENTIONS.md#unity-resource-naming);
- its authored hierarchy follows [Root Hierarchy](#root-hierarchy) and
  [Container Ownership](#container-ownership), including required camera, UI and
  network objects;
- every required scene feature is registered through the context installer;
- the scene is registered through the typed Scene Flow definition and is
  reached through the Preloader flow;
- the relevant Play Mode startup or scene-composition test asserts the new
  scene's required structure.

## Changes To The Standard

Do not make one scene an ad-hoc exception. If a new recurring scene concern
does not fit one of the six containers, update this document and the reference
scene intentionally before applying the change to new scenes.
