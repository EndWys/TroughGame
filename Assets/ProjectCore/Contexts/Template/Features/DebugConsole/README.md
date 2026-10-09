# Debug Console

Debug Console is the persistent runtime UI for logs, typed cheats, Debug
Visualization, and performance metrics. It is installed by
`DebugToolsFeatureGroup` and is available only when Runtime Console is allowed
for the current build.

## Architecture

- `DebugConsoleSystem` owns the bounded log buffer, command history, Log
  handler subscription, Cheat catalog subscription, and asynchronous command
  execution. Its state does not depend on the console window.
- `DebugConsoleController` owns presentation state: visibility, active tab,
  filters, selected log, history navigation, autocomplete, visualization
  navigation, and refresh intervals.
- `DebugConsoleComponent` owns the `UIDocument`, keyboard input, launcher, and
  tab presentation. It forwards UI events through `IDebugConsoleView`.
- Widget Views only render supplied data and report user actions. They do not
  resolve or retain services.

When Runtime Console is allowed, the System collects up to `MaxLogs` entries
even while the window is closed or temporarily disabled. Enabling the tool
creates and attaches the View; disabling it removes only the View. System and
Controller state live until the Project context is destroyed.

Logs and command history use bounded circular buffers. Incoming log events
mark the view dirty; visible rows are refreshed at most once per frame and are
reused instead of recreated. Performance and visualization panels use their
own sampling intervals and do no global scene-object searches.

## Usage

Open or close the window with the configured `ToggleKey` or the draggable
launcher button. The tabs provide:

- `Logs`: filtering, details, history, autocomplete, and command input;
- `Cheats`: forms generated from registered Cheat descriptors;
- `DebugVisualization`: channel navigation and current inspected values;
- `Performance`: periodically sampled frame and memory metrics.

Build availability, initial enabled state, toggle key, and log capacity are
configured through `ProjectCore > Debug Tools`.
