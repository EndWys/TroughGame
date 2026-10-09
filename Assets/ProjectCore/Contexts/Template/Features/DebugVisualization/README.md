# Debug Visualization

Debug Visualization provides runtime debug drawing, channel filtering, object
inspection, and a UI Toolkit HUD for editor and development builds.

`DebugVisualizationUtility` is a stateless static facade used by gameplay
code. `DebugVisualizationSystem` owns the runtime runner, drawing state, and
lifecycle through DI. `DebugVisualizationRegistry` owns registered inspection
targets and reflection caches. When the feature is disabled or unavailable,
the facade routes calls to a no-op backend.

`DebugVisualizationFeature` is installed in `ProjectContext`. It connects the
facade to the context-owned system, initializes the feature assets when Debug
Visualization is enabled, and shuts the runtime down when it is disabled or
the context is destroyed.

## Drawing API

Use `DebugVisualizationUtility` for event-driven or one-off visualization:

```csharp
using ProjectCore.Template;
using UnityEngine;

public sealed class DamageDebugComponent : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Vector3 point = collision.GetContact(0).point;

        DebugVisualizationUtility.DrawNumber(point, 25f, "Combat/Damage");
        DebugVisualizationUtility.DrawRadius(
            transform.position,
            2.5f,
            "Combat/AttackRange");
    }
}
```

Available operations include labels, numbers, lines, rays, radius rings,
filled discs, channel visibility, clearing, and manual object registration.
Callers do not inject the runtime system; internal feature UI receives it
through DI.

Duration values mean:

- `null`: visible until explicitly cleared;
- `0f`: current update cycle;
- positive value: unscaled seconds.

Draw calls are compiled for `UNITY_EDITOR`, `DEVELOPMENT_BUILD`, or
`DEBUG_VISUALS`.

## Attribute inspection

Use `[DebugValue]` for continuously inspected fields or properties and
`[DebugRadius]` for numeric ranges associated with a scene object:

```csharp
public sealed class EnemyDebugComponent : BaseDebugInspectableComponent
{
    [SerializeField, DebugValue(
        "AI/Stats",
        Label = "Move Speed",
        Display = DebugVisualizationValueDisplay.HudAndWorld)]
    private float _moveSpeed = 4f;

    [SerializeField, DebugRadius(
        "AI/Vision",
        Label = "Vision Radius",
        Filled = true,
        ValueDisplay = DebugVisualizationValueDisplay.World)]
    private float _visionRadius = 8f;
}
```

`BaseDebugInspectableComponent` registers and unregisters itself automatically.
Other Unity objects must use `DebugVisualizationUtility.Register` and
`DebugVisualizationUtility.Unregister` explicitly. The feature never scans a
scene for components, so registration has no global discovery cost.

Registered targets and `IDebugDrawable` world output are evaluated every
frame. Hitboxes, radii, lines, world labels, their positions, and their values
therefore follow runtime objects without throttling. Only the textual value
list inside the UI Toolkit HUD is refreshed at 10 Hz.

## Custom drawing

Implement `IDebugDrawable` when visualization combines multiple values or
requires conditional logic:

```csharp
public sealed class AgentDebugComponent : MonoBehaviour, IDebugDrawable
{
    [SerializeField] private float _visionRadius = 8f;

    public void DrawDebug(DebugVisualizationContextAdapter context)
    {
        context.Radius(transform.position, _visionRadius, "AI/Vision");
        context.Value("AI/State", "Enabled", enabled);
    }
}
```

Objects implementing `IDebugDrawable` can inherit
`BaseDebugInspectableComponent` or be registered manually.

Transient draw commands use a bounded circular buffer. Geometry is rendered
in batches grouped by depth-test mode; preview and reflection cameras are
ignored. Disabling the tool detaches the static facade, clears registered
targets and draw state, and destroys the runtime runner.

## Feature assets

- `DataResources/Config_Template_DebugVisualization_Settings.asset` controls
  HUD startup, draw limits, default style, and channel styles.
- `DataResources/Config_Template_DebugVisualization_PanelSettings.asset`
  configures the UI Toolkit panel.
- `GraphicResources/USS/Document_Template_DebugVisualization_HUD.uss`
  defines HUD presentation.
- `GraphicResources/Scenes/Scene_Template_DebugVisualization_Example.unity`
  contains the example scene.

Use `Tools > Debug Visualization > Create Example Objects` to create example
objects in the active scene.
