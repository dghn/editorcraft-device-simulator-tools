# Changelog

## 0.4.1 — 2026-10-26

- Replace the mismatched split-pane search demo with a chronological, whole-window tour: iPhone Duo, Pixel Fold, iPhone 16, then Duo again.
- Show UI walkthroughs without unrelated gameplay and host the actual HTML reports on GitHub Pages.
- Keep falling Jelly Garden tiles above slot backgrounds and update their click targets after gravity.

## 0.4.0 — 2026-10-26

- Add the optional Jelly Garden playable puzzle sample with portrait, foldable and landscape layouts.
- Replace the application-style showcase with fresh puzzle captures and 15-second, 180-frame walkthrough GIFs.
- Feature iPhone Duo in the hero and primary gameplay/inner–outer switching demos, retaining real device surrounds and camera masks.
- Reorganize the README around installation, finding devices, layout audits and capture comparison.

## 0.3.1 — 2026-10-25

- Refresh the gallery’s matching-favorite count when picker preferences change and
  clear an obsolete no-match warning once favorite devices match again.
- Cover the real picker star/save/batch-selection path and stale-warning recovery
  with an Editor regression probe.

## 0.3.0 — 2026-10-25

- Add batch UI/safe-area audits using the existing device/capture runner: selected
  favorite devices, profile orientations, root/exclusion scope, per-edge pixel
  warnings, skipped-object diagnostics and scene-object navigation.
- Add an in-Editor side-by-side comparison gallery with orientation filtering,
  paged thumbnails and exportable HTML/JSON reports plus PNGs.
- Share projected-bound checks with the single-object safe-area inspector. Preserve
  original device/orientation and partial results on cancellation and failure.

## 0.2.0 — 2026-10-25

- Add six workflow GIFs and a sample HTML capture gallery; show tested support for
  Unity 2022.3.62f2 (2022 LTS) and Unity 6 (6000.6.5f1) across presentation materials.

- Handle Enter from the device search field before the text input consumes it.
- Use an anchored UI Toolkit menu for Test Tools so capture commands also open
  in floating Simulator windows on Unity 2022.3. Verify search, favorites,
  paired displays and PNG/HTML exports through the 2022.3 Editor UI.

- Move tools into the official DeviceSimulatorPlugin control panel, preserving
  search, filters, favorites, paired displays, captures, settings and safe-area
  inspection. Keep the native toolbar intact; remove global UI-injection polling.
- Isolate unavoidable internal selection/screen/render access in one compatibility
  adapter and load overlay artwork through AssetDatabase. Preserve capture state
  on cancellation and plugin teardown.
- Support Unity 2022.3 with compatible uGUI dependencies and version-guarded UI
  properties; validate 2022.3.62f2 and 6000.6.5f1 in the Editor.

- Refine POCO F7, Redmi 13C and Galaxy A25 5G cutout masks and immersive
  safe areas using pinned, model-matched community ROM configurations. Keep
  source-derived configuration distinct from measured stock hardware. Separate
  visual camera masks from OS exclusion rectangles; document the remaining
  22 unsourced safe areas and 29 approximate exterior frames.

- Replace 30 newly added approximate frames with official Samsung/Google artwork.
  Use hardware-matched cutout measurements for 24 profiles and explicitly labelled
  Pixel emulator data for 2; preserve raw evidence and model/rotation validation.
  Reject mismatched LTE/5G sources. Document remaining unverified safe-area profiles
  and all remaining artwork, crop and resolution-scaling limitations.


- Expand Android from 33 to 92 screen profiles with market-selection and display
  provenance, additional manufacturers, separate LTE/5G models, OPPO Find N5
  inner/outer screens and two tablet aspect ratios. New frame/inset geometry is
  explicitly approximate; full US/global top-30 ranking coverage is not certified.
- Recognize named inner/outer screens in the Foldable filter; add cache-free
  approximate overlay regeneration with notch/capsule and off-center camera masks.

- Correct overlay texture color handling in the Simulator GUI to remove dark
  rectangles outside rounded devices in Linear projects. Preserve raw overlay
  colors during framed PNG readback and validate the import setting.

- Framed PNG exports now remove the package overlay’s exterior preview backing,
  preserving enclosed gray pixels and keeping Simulator preview rendering intact.

## 0.1.0

Initial standalone UPM package: device catalog, filtered picker and favorites, paired
displays, PNG/HTML capture, persistent export settings and safe-area inspection.
