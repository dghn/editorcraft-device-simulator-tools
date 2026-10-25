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
Applicable source agreement:
https://www.apple.com/legal/sla/docs/xcode.pdf

## Android overlays

Catalog provenance and hashes are in Documentation~/provenance/Android.json.
Google-derived frames originate from Android Studio device-art resources:
https://android.googlesource.com/platform/tools/adt/idea/+/refs/heads/mirror-goog-studio-main/artwork/resources/device-art-resources/
Samsung-derived frames originate from Galaxy Emulator Skins:
https://developer.samsung.com/galaxy-emulator-skin/

Original source ownership and applicable terms remain in effect. Public redistribution
of these particular derived image files has not been verified. Attribution alone is
not a license. Approximate vector frames are identified in the catalog. Keep downloaded
source archives outside the repository. The maintainer authorized publication with these materials on 2026-10-25.
That publication decision does not establish a redistribution license or extend
the MIT grant to third-party artwork.

## Device measurements

Source links and assumptions are recorded in Documentation~/provenance. Safe rectangles depend on
OS, orientation and system-UI settings; they are test profiles, not universal hardware
constants. Manufacturer names identify compatibility and do not imply endorsement.
