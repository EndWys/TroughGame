# Dynamic UI Collections

## Purpose

Choose a collection layout and update policy matching actual content and scale.

## Selection and Structure

| Need | Default Choice | Decision to Explain |
| --- | --- | --- |
| Small bounded composition with diverse children | ScrollView | Actual maximum count, layout cost and overflow |
| Large or unbounded repeatable data | ListView virtualization | Stable identity, row types, selection and refresh |
| Uniform row height | FixedHeight | Accurate item height and alignment |
| Content-dependent height | DynamicHeight | Measurement/reflow cost, variable text/images and scroll stability |

Mixed rows can use a stable compatible row host with explicit typed content or
appropriate templates. Explain the strategy instead of rebuilding the whole list
on each update. Do not assume CSS Grid, or fixed item height for expanding content.
For grids/carousels investigate supported controls and measured need before custom
virtualization. Hierarchy plans identify the viewport, content, row and empty state.

## Binding and Recycling

Create the row and stable callbacks in makeItem; bindItem assigns the current
item/context and resets all prior visible state. A callback reads the row's current
item identity, not a captured historical index. unbindItem detaches old item
notifications, references and owned row work; destroyItem releases the row's
remaining callbacks/resources. Verify the actual installed lifecycle APIs.

Track ownership explicitly: recycling does not dispose a borrowed model or
collection-owned item ViewModel. Cancel row-owned image/task work and reject stale
results after rebinding. Reset selection/disabled/error/tooltips and styles; do
not accumulate callbacks or allocate an independent VM on every repaint.

Use stable item IDs for updates/selection where appropriate; indexes may change
after sort/filter/remove. Define what selection and focus do when the item vanishes.
Use RefreshItem/RefreshItems for compatible updates; Rebuild is justified when
row construction/type configuration changes. Do not claim property notification
automatically applies insert/remove/reorder to the collection control.

## Verification

Exercise empty/single/dense/maximum content, mixed and long rows, scroll to both
ends, resize, rapid filter/sort/remove and selection. After repeated recycling,
activate rows and verify the command targets the currently shown item once.
Check late async results, close/reopen and retained scroll policy. Profile only
when required by scale/performance evidence; record measured conditions/results.

## Technical Sources

- [Unity 6.3 ListView and virtualization](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-uxml-element-ListView.html).
- [ListView unbindItem](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UIElements.ListView-unbindItem.html).
