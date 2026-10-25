# Android Simulator profiles

Additional phone and tablet profiles live in `Packages/com.daghandemirci.device-simulator-tools/Editor/Android`. Keep the `.device`, `.png`, directory and importer `.meta` files in Git. Teammates need neither Android Studio nor a Samsung account to use the checked-in output. Existing bundled Android devices are retained; new names must not duplicate them.

## Included models

92 Android screen profiles: 71 phones, 11 tablets and 10 foldable screens (5 inner/outer pairs). The October 25, 2026 expansion adds 59 profiles, including widely used older Galaxy models, 2025–2026 value-range successors and displays from Motorola, Redmi/POCO, OPPO, vivo, HONOR, realme, TECNO, Infinix, Nothing, Sony, Google and OnePlus.

### Market coverage and limits (2026-10-25)

This is a broad layout-testing catalog, **not a certified complete US/global top-30 ranking**. Public sources expose different metrics and limited lists; 2026 full-year sales are not yet available. No sales shares or rankings are inferred for unlisted models. `marketSources` are selection context; `coverageReason` distinguishes installed-base, successor and additional display coverage.

- [Counterpoint 2025 global sales](https://counterpointresearch.com/default.htm/insights/iphone-16-worlds-best-selling-smartphone-in-2025-apple-takes-7-spots-in-top-10-models): Galaxy A16 5G, A06 and S25 Ultra are covered.
- [Counterpoint 2025 LATAM sales](https://counterpointresearch.com/default.htm/insights/samsung-leads-latam-top10-Smartphones-list-with-4spots): A06, A16 LTE, Moto G15 and Redmi 14C are covered. Regional evidence is not presented as US sales.
- [AppBrain worldwide usage](https://www.appbrain.com/stats/top-android-phones), October 11 snapshot: all 18 named phone entries in its public top 20 are covered, including A12/A13/A14/A15/A16, A05/A06/A21s/A32/A34/A54/A55, S23/S24/S25 Ultra, Redmi A5/13C/14C. AFTMM and AFTSSS are TV devices and are excluded. LTE/5G variants are separately named where relevant.
- [AppBrain US usage](https://www.appbrain.com/stats/top-android-phones-tablets-by-country), October 15 snapshot: all ten publicly listed absolute-popularity phones are covered. The longer country list requires a subscription. Relative popularity measures where a model is used, not its rank in US sales; Moto G and Pixel additions provide further US coverage.

AppBrain measures its SDK audience over seven days, not the entire market. Older phones remain relevant to the 2025–2026 installed base. Successors and extra manufacturers are included for testing, without claiming top-30 placement. Obtaining a licensed model-level top-30 dataset is still necessary to certify exhaustive coverage against that specific ranking.

Foldable inner/outer profiles are separate selectable screens. OPPO Find N5 is included in the Foldable filter and paired-screen switch. Tablet additions include Xiaomi Pad 7 (2136 × 3200) and OnePlus Pad 3 (2400 × 3392). Sony Xperia 10 VI covers 21:9 without a display cutout. The list below records exact pixel aspect ratios rather than rounding everything to a marketed ratio.

## Sources and accuracy

`Catalog.json` records each resolution source and geometry provenance. These are layout profiles, not Android OS emulation or GPU/performance benchmarks. They use the native panel resolution; a device's default/lowered render resolution or display-size setting may differ. DPI is panel PPI, not Android's configurable logical density.

Existing native-skin Pixel phone and Pixel Tablet overlays use Google's actual Android Studio `back.webp`, `mask.webp` and `layout` resources. Native screen offsets and border dimensions are preserved when resampling to power-of-two Unity textures. Pixel Tablet's landscape artwork is rotated to portrait as the profile's base orientation. Sources: [Android Studio device artwork](https://android.googlesource.com/platform/tools/adt/idea/+/refs/heads/mirror-goog-studio-main/artwork/resources/device-art-resources/). Resource SHA-256 hashes are recorded to detect upstream changes. Google/Android artwork remains attributable to its original owners; this project does not claim authorship.

Native-skin Samsung phone and tablet overlays use the official [Galaxy Emulator skins](https://developer.samsung.com/galaxy-emulator-skin/). Native display offsets, foreground masks and camera artwork are retained. The 28 newly sourced Samsung skins omit separate side-button assets; existing ZIP-based skins retain them. The surrounding promotional poster is removed with a fitted exterior outline; this exterior crop is an approximation, not a Samsung-supplied transparent silhouette. Camera holes and the Tab Ultra side notch come from the original foreground masks. Slightly translucent corner pixels are backed with black to prevent game imagery leaking through the case. Samsung artwork remains attributable to Samsung.

### Geometry upgrade (2026-10-25)

Of the 59 newly added profiles, **30 now use official artwork**: 28 Samsung skins from a provenance-recorded public archive and two Pixel skins directly from Android Studio. Samsung direct ZIP downloads required sign-in; the public mirror is pinned to a commit, with original Samsung archive metadata and SHA-256 hashes. This is official artwork obtained through a third-party archive, not a fresh authenticated manufacturer download. Native screen masks are retained; the poster-removal body outline remains fitted rather than a manufacturer-supplied transparent silhouette.

**24 profiles use physical-device/Remote Test Lab cutout measurements** (20 at the same resolution, four explicitly scaled from a lower rendering resolution). Two Pixels use Android Emulator evidence, not physical Pixel measurements. Eight other profiles derive a conservative exclusion from the skin mask. Three profiles use community ROM cutout configurations (not stock-hardware measurements); the remaining 22 profiles still have unverified safe-area assumptions. The 29 profiles without matching native skins retain approximate frames; four of these nevertheless have hardware-matched inset measurements. See [the per-profile audit](GeometryAccuracy.md).

Raw captures are saved under `Measurements/`, with source URLs, hardware model codes, OS/build, timestamps, resolution scaling and hashes in `Catalog.json` / `GeometryEvidence.json`. Samsung capture evidence comes from [windowinsets.info](https://windowinsets.info/) and its [source repository](https://github.com/easyhooon/windowinsets.info). Only hardware-matched records are applied. 72 source orientation checks validate the generated output against the pinned raw captures.

Safe rectangles preserve this catalog's **immersive full-screen convention, with system bars hidden**: they use the measured `DisplayCutout.safeInsets`, not the source site's `max(systemBars, cutout)` rectangle. These are projections from WindowInsets captures, not recordings of a Unity game in immersive mode. Explicit `orientationGeometry` records override rotated portrait data where captures exist. Missing directions, including upside-down, remain derived rotations and are not labelled measured. Four S models scale all coordinates uniformly to their native panel resolution and round outward; they are not direct native-resolution measurements. The Pixel source captures are emulator-only and portrait-only.

Android cutout bounding rectangles often include space between a camera and the screen edge; they are distinct from the visible camera lens. Official mask artwork is not redrawn to match the larger OS exclusion rectangle. Unverified vector masks and corners remain estimates. Gesture/three-button navigation, display zoom, cutout policy and OS version can change the usable region. See [Android display-cutout guidance](https://developer.android.com/develop/ui/views/layout/display-cutout). Runtime applications must still use `Screen.safeArea`.

### Source discrepancies and cover-screen limits

Fold8: the Samsung download page lists 1828 x 2448, while its downloaded layout and mask are 2448 x 1848. The profile follows the actual skin resources. Physical panel resolution is not independently verified. Samsung cover-screen PPI is an explicit estimate; the download pages only specify the main panel density.

Pixel 10 Pro Fold outer: the native layout canvas is 1236 x 2554 but its background image is 1236 x 2495. Background artwork is aligned to the top of the layout, retaining the native display offset and neutral unused canvas. This prevents duplicated camera imagery and a shifted case.

Flip8 outer includes separate camera and flash cutout rectangles, with a conservative bottom exclusion. It is a layout stress test; it does not imply the OS supports launching an application on the cover display. Full folding transitions and simultaneous inner/outer rendering are not emulated.

## Regeneration and checks

From the repository root, provide a local cache folder for native skin downloads. For Samsung, sign in to Samsung Developer, download each catalog `skinDownload` ZIP and extract it into that cache, preserving its `Galaxy_*` directory. These source archives stay outside Git. The generator verifies all native resource SHA-256 hashes from the catalog before using them. Teammates only need these downloads if regenerating artwork:

```sh
python3 Tools/DeviceSimulator/Android/DownloadGoogleSkins.py <skin-cache>
python3 Tools/DeviceSimulator/Android/DownloadVerifiedSkins.py <skin-cache>
python3 Tools/DeviceSimulator/Android/GenerateProfiles.py
swift Tools/DeviceSimulator/Android/GenerateOverlays.swift <skin-cache>
unity command run_script Tools/DeviceSimulator/Android/ImportOverlays.cs --result-only
unity command run_script Tools/DeviceSimulator/Android/ValidateProfiles.cs --result-only
python3 Tools/DeviceSimulator/Android/ValidateGeometrySources.py
```

Keep existing GUIDs on regeneration. Texture import: readable, sRGB decoding disabled for Simulator GUI, no mipmaps, uncompressed, clamp, bilinear, max size 2048, no non-power-of-two scaling. Tall phone displays use 1024 x 2048; tablets and wide foldable displays use 1024 x 1024. The existing neutral Simulator backdrop prevents a mismatched exterior rectangle during Play mode.

The validator checks imported devices, unique names, loaded overlays, transparent screen centers, opaque frame/corners/cameras, and the actual Simulator resolution, DPI and safe rectangle in all four orientations. This confirms Unity agrees with these definitions; it does not independently validate physical Android insets.


To regenerate only approximate vector artwork without a native skin cache:

```sh
python3 Tools/DeviceSimulator/Android/GenerateProfiles.py
swift Tools/DeviceSimulator/Android/GenerateOverlays.swift --approximate-only
unity command run_script Tools/DeviceSimulator/Android/ImportOverlays.cs --result-only
```

## Added display coverage

| Profile | Native pixels | Long:short ratio | Specification source |
| --- | --- | --- | --- |
| Samsung Galaxy A12 | 720 × 1600 | 2.2222:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A13 5G | 720 × 1600 | 2.2222:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A14 5G | 1080 × 2408 | 2.2296:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A15 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A16 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A17 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A05 | 720 × 1600 | 2.2222:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A05s | 1080 × 2400 | 2.2222:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A06 | 720 × 1600 | 2.2222:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A07 | 720 × 1600 | 2.2222:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A21s | 720 × 1600 | 2.2222:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A25 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A26 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A27 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A32 | 1080 × 2400 | 2.2222:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A34 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A35 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A36 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A37 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A54 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy A57 5G | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html) |
| Samsung Galaxy S23 | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-s.html) |
| Samsung Galaxy S23+ | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-s.html) |
| Samsung Galaxy S23 Ultra | 1440 × 3088 | 2.1444:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-s.html) |
| Samsung Galaxy S24 | 1080 × 2340 | 2.1667:1 | [Specs](https://www.samsung.com/uk/support/mobile-devices/comparison-between-the-galaxy-s24-ultra-s24-plus-and-s24/) |
| Samsung Galaxy S24+ | 1440 × 3120 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-s.html) |
| Samsung Galaxy S24 Ultra | 1440 × 3120 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-s.html) |
| Samsung Galaxy S24 FE | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-s.html) |
| Samsung Galaxy S25 FE | 1080 × 2340 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-s.html) |
| Samsung Galaxy S25 Edge | 1440 × 3120 | 2.1667:1 | [Specs](https://developer.samsung.com/galaxy-emulator-skin/galaxy-s.html) |
| Samsung Galaxy A13 | 1080 × 2408 | 2.2296:1 | [Specs](https://news.samsung.com/us/samsung-galaxy-a13-reliable-smartphone-essentials-power-teams/) |
| Samsung Galaxy A14 | 1080 × 2408 | 2.2296:1 | [Specs](https://images.samsung.com/is/content/samsung/assets/global/ir/docs/2024_4Q_Interim_Report.pdf) |
| Samsung Galaxy A15 | 1080 × 2340 | 2.1667:1 | [Specs](https://www.dxomark.com/samsung-galaxy-a15-lte-battery-test/) |
| Samsung Galaxy A16 | 1080 × 2340 | 2.1667:1 | [Specs](https://devices.vodafone.com.au/samsung/galaxy-a16-4g-android-14/specifications/) |
| Redmi A5 | 720 × 1640 | 2.2778:1 | [Specs](https://www.mi.com/global/product/redmi-a5/specs/) |
| Redmi 13C | 720 × 1600 | 2.2222:1 | [Specs](https://www.mi.com/global/product/redmi-13c/specs/) |
| Redmi 15C | 720 × 1600 | 2.2222:1 | [Specs](https://www.mi.com/global/product/redmi-15c/specs/) |
| Redmi Note 14 | 1080 × 2400 | 2.2222:1 | [Specs](https://www.mi.com/global/product/redmi-note-14/specs/) |
| POCO F7 | 1280 × 2772 | 2.1656:1 | [Specs](https://www.mi.com/global/product/poco-f7/specs/) |
| Xiaomi Pad 7 | 2136 × 3200 | 1.4981:1 | [Specs](https://www.mi.com/global/product/xiaomi-pad-7/specs/) |
| Redmi 14C | 720 × 1640 | 2.2778:1 | [Specs](https://www.mi.com/pe/product/redmi-14-c/specs/) |
| Motorola Moto G 2025 | 720 × 1604 | 2.2278:1 | [Specs](https://www.t-mobile.com/cell-phone/motorola-moto-g-2025) |
| Motorola Moto G 2026 | 720 × 1604 | 2.2278:1 | [Specs](https://www.t-mobile.com/cell-phone/motorola-moto-g-2026) |
| Motorola Moto G Power 2025 | 1080 × 2388 | 2.2111:1 | [Specs](https://www.motorola.com/us/en/p/phones/moto-g/moto-g-power-2025/pmipmhi39mv) |
| Motorola Moto G Stylus 2025 | 1220 × 2712 | 2.2230:1 | [Specs](https://en-us.support.motorola.com/app/answers/detail/a_id/185268/~/specifications---moto-g-stylus---2025) |
| Motorola Moto G15 | 1080 × 2400 | 2.2222:1 | [Specs](https://en-emea.support.motorola.com/app/answers/detail/a_id/183973/p/12159) |
| Google Pixel 10 Pro | 1280 × 2856 | 2.2313:1 | [Specs](https://support.google.com/pixelphone/answer/7158570) |
| Google Pixel 10 Pro XL | 1344 × 2992 | 2.2262:1 | [Specs](https://support.google.com/pixelphone/answer/7158570) |
| OPPO A5 Pro | 720 × 1604 | 2.2278:1 | [Specs](https://www.oppo.com/ph/smartphones/series-a/a5-pro/specs/) |
| OPPO Find N5 Inner Screen | 2248 × 2480 | 1.1032:1 | [Specs](https://www.oppo.com/ph/smartphones/series-find-n/find-n5/specs/) |
| OPPO Find N5 Outer Screen | 1140 × 2616 | 2.2947:1 | [Specs](https://www.oppo.com/ph/smartphones/series-find-n/find-n5/specs/) |
| vivo X200 Pro | 1260 × 2800 | 2.2222:1 | [Specs](https://www.vivo.com/in/products/param/x200-pro) |
| HONOR Magic7 Pro | 1280 × 2800 | 2.1875:1 | [Specs](https://www.honor.com/sa-en/phones/honor-magic7-pro/spec/) |
| realme 14 Pro+ 5G | 1272 × 2800 | 2.2013:1 | [Specs](https://www.realme.com/sa-en/realme-14-pro-plus-5g/specs) |
| TECNO SPARK 40 Pro+ | 1224 × 2720 | 2.2222:1 | [Specs](https://www.tecno-mobile.com/phones/tech-specs/techspecs/spark-40-pro-%2B/) |
| Infinix NOTE 50 | 1080 × 2436 | 2.2556:1 | [Specs](https://wap.iq.infinixmobility.com/specs/note-50?cId=18&cName=Latest+Product) |
| Nothing Phone (3a) | 1080 × 2392 | 2.2148:1 | [Specs](https://ae.nothing.tech/en/pages/phone-3a) |
| Sony Xperia 10 VI | 1080 × 2520 | 2.3333:1 | [Specs](https://www.sony.com.my/electronics/support/mobile-phones-tablets-mobile-phones/xperia-10-vi/specifications) |
| OnePlus Pad 3 | 2400 × 3392 | 1.4133:1 | [Specs](https://www.oneplus.com/global/oneplus-pad-3/specs) |

Follow-up research: [GeometryResearch.md](GeometryResearch.md). Validate community ROM evidence separately with `python3 Tools/DeviceSimulator/Android/ValidateRomGeometry.py`.
