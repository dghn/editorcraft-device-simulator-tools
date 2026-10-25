# Release checklist

Package-only repository preparation, 2026-10-25. The maintainer explicitly authorized public distribution with existing artwork
and publication of v0.1.0 on 2026-10-25. Manufacturer-derived artwork remains intact.

## Verified

- Root UPM manifest and Editor-only assembly; original asset GUIDs preserved.
- 122 custom profiles, 440 orientations and 71 overlays pass static validation.
- Development host on Unity 6000.6.5f1 passed Apple 89/308 and Android 33/132 checks.
- Ten foldable framed captures were rendered and visually checked in that host.
- Unity host Assets, ProjectSettings, caches and local SSH instructions are excluded.

## Before publication

- [ ] Resolve artwork redistribution for overlays and framed screenshots; see the
  root THIRD_PARTY_NOTICES.md. Permission remains unverified; the maintainer
  authorized publication with the existing materials. This is not license clearance.
- [x] Repository selected: `dghn/editorcraft-device-simulator-tools` (public release authorized).
- [ ] Choose and verify a release tag before adding a release installation URL.
- [x] Initial commit and private upload authorized by the maintainer.
- [x] Obtain authorization before changing visibility to public or publishing a release.
- [x] Git installation and Editor compilation in a clean Unity 6000.6.5f1 project;
  see [verification](INSTALL_VERIFICATION.md), tested commit `18449199b852`.
- [ ] Repeat interactive picker, foldable, capture and safe-area checks in the
  clean Git-installed project (not covered by the batch compilation test).
- [x] Record the tested commit and installation results; retest the selected release
  candidate before tagging.

Embedded-host validation does not by itself verify a Git-installed package.
