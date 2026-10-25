# Changelog

## 0.1.0 — 2026-10-25

- Correct overlay texture color handling in the Simulator GUI to remove dark
  rectangles outside rounded devices in Linear projects. Preserve raw overlay
  colors during framed PNG readback and validate the import setting.

- Framed PNG exports now remove the package overlay’s exterior preview backing,
  preserving enclosed gray pixels and keeping Simulator preview rendering intact.


Initial standalone UPM package: device catalog, filtered picker and favorites, paired
displays, PNG/HTML capture, persistent export settings and safe-area inspection.
