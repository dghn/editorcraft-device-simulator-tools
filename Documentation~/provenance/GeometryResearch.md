# Follow-up geometry source research

Checked 2026-10-25. Scope: the 29 approximate frames / 25 previously unsourced safe areas from the 59-device expansion.

No additional **verified exact-model official emulator skins** or physical WindowInsets captures were located. Official-artwork coverage stays at 30/59. Exterior frames remain approximate for 29/59. Three unsourced exclusions were replaced with explicitly labelled community ROM configurations; 22/59 remain unsourced. Community configurations can differ from shipping firmware and are not OEM artwork or hardware validation.

## Applied configuration evidence

| Profile / matched model | Previous → source-derived top exclusion | Basis |
| --- | --- | --- |
| POCO F7 / 25053PC47G | 109 → 126 px | `onyx` framework rectangle ±41 × 126 px. Visual camera is separately defined as radius 40, center y=85; software corner radius is 156 px. |
| Redmi 13C / 23106RN0DA | 61 → 68 px | `gale` framework notch rectangle ±40 × 68 px. Curved notch rendering remains an approximation of this software bound. |
| Galaxy A25 5G / SM-A256E | 92 → 77 px | `a25x` cutout ±22.4 × 27.37777777777778 dp, at the inherited ROM density of 450 dpi → ±63 × 77 px. This is logical Android density, not the panel PPI. Curved notch rendering remains approximate. |

Sources, immutable commits, hashes, previous values and transformations are stored in [RomGeometryEvidence.json](RomGeometryEvidence.json); corresponding files are preserved under `RomSources/`. `ValidateRomGeometry.py` verifies model strings, source hashes, dp conversion, XML geometry and the 12 derived orientation rectangles. The camera's visible shape is kept separate from the larger Android exclusion rectangle. All three use system-bars-hidden projection. No additional landscape measurement was obtained.

## Sources reviewed and exclusions

- [Samsung official Galaxy A skins](https://developer.samsung.com/galaxy-emulator-skin/galaxy-a.html): A25 is listed, but the downloadable archive requires sign-in; no exact-model accessible replacement was verified. The earlier A13 5G archive has an incompatible 1080×2408 layout. It is not reassigned to A13 LTE merely because that resolution matches. A14/A15/A16 LTE do not inherit 5G skins based on similar appearance.
- [Xiaomi Vela emulator skins](https://iot.mi.com/vela/quickapp/en/tools/debug/multi-screens.html): this is the wearable/IoT emulator, not evidence for these Android phones/tablets. [POCO F7 specifications](https://www.mi.com/global/product/poco-f7/specs/) and [Redmi 13C specifications](https://www.mi.com/global/product/redmi-13c/specs/) establish native screen dimensions but do not supply runtime insets or a native skin.
- [Nothing Asteroids configuration](https://github.com/AKoskovich/android_device_nothing_asteroids/blob/39e64d1b3da64fbddb6577e08547e8da6f89d631/rro_overlays/AsteroidsFrameworksOverlay/res/values/config.xml): visual circle is centered at x=540, radius 21.5, but its reported rectangle spans x=454..540. The rectangle does not enclose the circle. Rejected pending corroborating stock capture; neither contradictory value is applied.
- [Redmi 14C lake tree](https://github.com/Toramadscientist/android_device_xiaomi_lake_lineage/tree/b9d4071ce5a8a91b372e371c789c413464d7a2da): product identity exists, but no usable matching display-cutout resource was found. Recovery-only configurations are not runtime inset evidence.
- [Xiaomi Pad 7 uke tree](https://github.com/Xiaomi-Pad-7-Pro-Resources/android_device_xiaomi_uke/tree/f00e869c183191a35462e61aefda7f6867b37efe), [OnePlus Pad 3 erhai tree](https://github.com/LineageOS/android_device_oneplus_erhai/tree/fff6dba6dfc8b8eada8fd85d92b45920da3add85), [Sony Xperia 10 VI pdx246 tree](https://github.com/johanneszab/android_device_sony_pdx246/tree/f4946e1cd5a8b409d74cb5cf291e23a2113087c2): model-specific trees found; absence of a cutout override is not proof of runtime zero insets because inherited resources and policy still apply. Zero values remain unverified. [OnePlus's model listing](https://www.oneplus.com/uk/pstisoc) confirms Pad 3 = OPD2415, but supplies no geometry.
- [Moto G 2025 kansas tree](https://github.com/councilcj/android_device_motorola_kansas/tree/0accbd2cd8dddc393e9e9d0762a0e05a1163482c): no usable Android runtime cutout resource found. Old MOTODEV or Moto X4 skins do not match current Moto G devices.
- [Redmi Note 14 Pro GSI modification](https://github.com/moloo4ni/statusbar-fix-obsidian): wrong variant, and deliberately clears display-cutout awareness. Not a valid source for Redmi Note 14 or a zero safe area.
- Targeted manufacturer/exact-model skin and DisplayCutout/safeInsets searches for OPPO, HONOR, vivo, realme, TECNO, Infinix and the remaining Motorola/Redmi devices did not yield an importable exact-model OEM skin or traceable measurement. This records a search result, not a claim that those manufacturers have never published skins. Product photographs and display resolution alone are insufficient to promote geometry to measured status.

## Remaining approximate-frame inventory

Each row retains its specification link in Catalog.json. A manufacturer/model-matched skin with layout/mask, or a stock-device capture with model, build, resolution, density, display state and rotation, is still needed. Find N5 inner and outer displays must be verified independently.

| Profile | Current safe-area basis | Outstanding frame/source requirement |
| --- | --- | --- |
| Samsung Galaxy A13 5G | measured-cutout | Exact-model OEM artwork; existing captured insets retained |
| Samsung Galaxy A25 5G | community-rom-configuration | Exact-model OEM artwork; stock capture also needed |
| Samsung Galaxy A13 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Samsung Galaxy A14 | measured-cutout | Exact-model OEM artwork; existing captured insets retained |
| Samsung Galaxy A15 | measured-cutout | Exact-model OEM artwork; existing captured insets retained |
| Samsung Galaxy A16 | measured-cutout | Exact-model OEM artwork; existing captured insets retained |
| Redmi A5 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Redmi 13C | community-rom-configuration | Exact-model OEM artwork; stock capture also needed |
| Redmi 15C | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Redmi Note 14 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| POCO F7 | community-rom-configuration | Exact-model OEM artwork; stock capture also needed |
| Xiaomi Pad 7 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Redmi 14C | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Motorola Moto G 2025 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Motorola Moto G 2026 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Motorola Moto G Power 2025 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Motorola Moto G Stylus 2025 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Motorola Moto G15 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| OPPO A5 Pro | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| OPPO Find N5 Inner Screen | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| OPPO Find N5 Outer Screen | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| vivo X200 Pro | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| HONOR Magic7 Pro | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| realme 14 Pro+ 5G | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| TECNO SPARK 40 Pro+ | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Infinix NOTE 50 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Nothing Phone (3a) | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| Sony Xperia 10 VI | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
| OnePlus Pad 3 | unverified-estimate | Exact-model OEM artwork; stock capture also needed |
