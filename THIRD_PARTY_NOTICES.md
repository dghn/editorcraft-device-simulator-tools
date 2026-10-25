# Third-party materials

## Unity device package

Unity's Device Simulator Devices 1.0.1 is a registry dependency, not redistributed
inside this package. Unity's original license and notices apply to that dependency.
The custom picker filters matching names without modifying the installed catalog.

## Apple-derived overlays

Editor/Apple/Overlays contains textures generated using local
CoreSimulator framebuffer masks and DeviceKit artwork, plus simplified case contours.
Apple retains rights to its materials. The Xcode agreement does not establish a
blanket open-source redistribution grant for these generated textures.
Review the applicable agreement or replace/remove derived artwork before publication:
https://www.apple.com/legal/sla/docs/xcode.pdf

## Android overlays

Catalog provenance and hashes are in Tools/DeviceSimulator/Android/Catalog.json
in the source repository. This maintainer folder is not included in the installed
UPM package; consult the source repository at the same revision as your package.
Google-derived frames originate from Android Studio device-art resources:
https://android.googlesource.com/platform/tools/adt/idea/+/refs/heads/mirror-goog-studio-main/artwork/resources/device-art-resources/
Samsung-derived frames originate from Galaxy Emulator Skins:
https://developer.samsung.com/galaxy-emulator-skin/

Original source ownership and applicable terms remain in effect. Public redistribution
of these particular derived image files has not been verified. Attribution alone is
not a license. Approximate vector frames are identified in the catalog. Keep downloaded
source archives outside the repository. Resolve artwork distribution separately before
publishing this local copy as an open-source repository.

## Device measurements

Source links and assumptions are recorded in the catalogs. Safe rectangles depend on
OS, orientation and system-UI settings; they are test profiles, not universal hardware
constants. Manufacturer names identify compatibility and do not imply endorsement.


## Android geometry evidence update

Additional Samsung artwork is obtained from the provenance-recorded public mirror at
https://github.com/easyhooon/windowinsets.info/tree/3884efc51ee56798d139ae64d730719b1083573c/public/skins
Original source/archive metadata and image hashes are retained in the Android catalog.
Original Samsung ownership remains unchanged; the mirror is not the manufacturer.
Pixel 10 Pro/Pro XL artwork comes directly from Google's Android Studio resources.

Hardware/RTL and explicitly labelled emulator measurements are attributed to
windowinsets.info / easyhooon. Raw captures, source URLs, build conditions and SHA-256
hashes are recorded in the Android geometry evidence and measurement snapshots.
Only matching hardware variants are used; lower-resolution captures are marked scaled.

## Community ROM geometry references

POCO F7 (crDroid/LineageOS onyx), Redmi 13C (Mayuri-Chan/LineageOS gale),
and Galaxy A25 5G (exynos1280/LineageOS a25x and s5e8825-common) cutout
configurations are attributed to their repository authors and The LineageOS Project.
Pinned source files, original copyright headers, hashes and URLs are retained in
RomSources and RomGeometryEvidence.json (under Android tools / exported provenance).
Those sources are Apache-2.0; the full license is included in RomSources/LICENSE-Apache-2.0.txt.
These are community ROM configurations, not official OEM skins or physical measurements.
