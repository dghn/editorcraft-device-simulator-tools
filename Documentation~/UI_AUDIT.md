# Batch UI audit and comparison gallery

Open **Device Simulator → Control Panel → EditorCraft Device Tools → Test Tools →
Batch UI audit & comparison…**. For screenshots without a UI audit, choose
**Comparison gallery…** instead.

1. Star the devices to test in the device picker. The batch uses favorites matching
   the current search, platform and device-type filters.
2. Choose an active scene **UI root**. The default scope is active uGUI Graphics
   (including TMP UI graphics) and Selectables beneath that root, including the root
   itself when applicable. Enable **Include layout containers** to inspect every
   active RectTransform in the subtree.
3. Add intentional backgrounds or other objects under **Exclude objects and their
   descendants**. Exclusions apply to the whole subtree. Inactive objects and graphics
   with effectively zero inherited CanvasRenderer alpha are omitted.
4. Leave **All profile orientations** enabled to test each orientation recorded in
   every selected device profile. Disable it to capture the orientation applied by
   Simulator when each device is selected. Player orientation restrictions may prevent
   a requested orientation; the batch stops with an incomplete report rather than
   labelling the wrong orientation.
5. Choose **Audit & capture selected favorites**. Each snapshot reports how many
   objects were checked or skipped and lists any screen/safe-area overflows as left,
   top, right and bottom distances in simulated screen pixels. The default tolerance
   is one pixel; smaller excursions are ignored.

## Compare and investigate

The Unity gallery displays up to four snapshots side by side. **Compare orientation**
filters the gallery so the same orientation on different devices can be compared;
Previous/Next pages through larger batches. Images fit their tile and are not shown
at physical device size. Green outlines mark safe areas; orange outlines mark objects
with geometric warnings. Only the current page's thumbnails are held in memory.

Click an object path to select its captured device/orientation and select/ping the
object in Unity. If a scene object was deleted or is no longer loaded, navigation
reports that instead of selecting a similarly named object. Paths are descriptive;
object references and GlobalObjectIds provide identity. **Open device in Simulator**
also works for comparison-only captures.

**Export report & images…** writes a new folder containing a browsable `index.html`,
`report.json`, full-resolution PNGs and thumbnails. HTML includes object paths and
per-edge warnings; JSON additionally includes geometric bounds, safe rectangles,
object identities, exclusions, scope, tolerance and completion status. Working
results are cached in `Library/EditorCraft/Reviews`; export results you want to keep.
The latest report in an open gallery is recovered across script reloads. Old cache
folders are not automatically deleted.

## State and accuracy

The runner shares the existing PNG capture, device selection, render wait and
restoration infrastructure. A batch is exclusive: the plugin's picker and competing
batch commands are disabled while it runs. Leave native Simulator controls unchanged
until completion. Completion, cancellation, play-mode changes, script reloads and
window teardown restore the original device/orientation when the Simulator is still
available. Completed results remain available after cancellation or a failure, with
an **Incomplete** status. Closing the gallery cancels a batch owned by that gallery.

This is a geometric layout audit, not an automated visual pass/fail test. It compares
projected RectTransform bounding boxes against the screen and recorded profile safe
area. Rotation and Canvas scaling affect those projected bounds. It does not evaluate
text clipping, masks, overlap, occlusion, shader effects, rounded screen corners or
hardware touch targets. Scrolled content can legitimately produce warnings; exclude
that subtree when appropriate. Safe rectangles reflect the selected profile's OS/UI
configuration and source accuracy.

World-space canvases, additional displays, camera canvases without a supported active
camera, and stale overlay Canvas dimensions are reported as **skipped**, never as
passes. A frame with no supported UI checked has an explicit warning. Applications
keep running during capture; animations, timers and game state can change between
screenshots. The tool does not modify scene layouts or install runtime components.

## Maintainer verification

`Tools/DeviceSimulator/Compatibility/ReviewSmoke.cs` is an integration probe for an
isolated empty project. Copy it into `Editor/` in a **temporary copy** of this package
so it can access the internal APIs. Launch Unity with
`-executeMethod DeviceSimulatorTools.Editor.ReviewSmoke.Run` and without `-quit`;
the asynchronous probe exits itself and saves `editorcraft-review-<unity>.json` in
`Path.GetTempPath()`. Do not install this probe in a user's package.

The probe covers overflow distances and tolerance, scaled UI, exclusions, hidden and
inactive UI, unsupported canvases, two devices/four orientations, PNG/thumbnail/JSON
and HTML export, object navigation, cancellation with partial results, restoration,
window teardown, callback failure, play-mode interruption, and the original PNG/HTML
batch. Actual native gallery presentation is checked separately.

Verified on 2026-10-25 in **Unity 2022.3.62f2** and **Unity 6000.6.5f1**: the
integration probe passed on both versions. The play-mode transition handler was
invoked directly by the probe; it did not enter Play mode. A native Unity 6 gallery
was also visually inspected with two portrait captures, safe-area outlines and the
expected 80-pixel right-edge warning. Unity 6 still reports the existing obsolete
GenericDropdownMenu overload warning; no new C# errors remain.
