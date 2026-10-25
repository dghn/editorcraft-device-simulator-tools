"""Offline UPM integrity check; does not certify artwork rights or Editor behavior."""
import json
import math
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[2]
PACKAGE = ROOT


def validate():
    errors = []

    def check(condition, message):
        if not condition:
            errors.append(message)

    def read_json(path):
        return json.loads(path.read_text(encoding="utf-8"))

    manifest = read_json(PACKAGE / "package.json")
    check(manifest["name"] == "com.daghandemirci.device-simulator-tools", "Unexpected package identity")
    check(bool(re.fullmatch(r"\d+\.\d+\.\d+", manifest["version"])), "Expected numeric release version")
    for name in ("README.md", "LICENSE.md", "THIRD_PARTY_NOTICES.md", "CHANGELOG.md"):
        check((PACKAGE / name).is_file(), f"Missing {name}")
    check("com.unity.pipeline" not in manifest["dependencies"], "Pipeline must remain host-only")
    assembly = read_json(PACKAGE / "Editor/DeviceSimulatorTools.Editor.asmdef")
    check(assembly.get("includePlatforms") == ["Editor"], "Assembly must be Editor-only")

    guids = {}
    for path in sorted(PACKAGE.rglob("*")):
        relative = path.relative_to(PACKAGE)
        if any(part.startswith(".") or part.endswith("~") for part in relative.parts):
            continue
        if path.suffix != ".meta":
            check(Path(str(path) + ".meta").is_file(), f"Missing meta: {relative}")
            continue
        check(Path(str(path)[:-5]).exists(), f"Orphan meta: {relative}")
        if path.name.endswith('.png.meta'):
            check(re.search(r'^\s+sRGBTexture: 0$', path.read_text(), re.MULTILINE) is not None,
                  f"Overlay must disable sRGB decoding for Simulator GUI: {relative}")
        matches = re.findall(r"^guid: ([0-9a-f]{32})$", path.read_text(), re.MULTILINE)
        check(len(matches) == 1, f"Invalid GUID: {relative}")
        for guid in matches:
            check(guid not in guids, f"Duplicate GUID: {relative} and {guids.get(guid)}")
            guids[guid] = str(relative)

    profiles = sorted(PACKAGE.rglob("*.device"))
    check(bool(profiles), "No device profiles")
    names = set()
    used_overlays = set()
    orientation_count = 0
    for path in profiles:
        data = read_json(path)
        name = data["friendlyName"].casefold()
        check(name not in names, f"Duplicate device name: {name}")
        names.add(name)
        check(bool(data["screens"]), f"No screens: {path.name}")
        for screen in data["screens"]:
            check(all(math.isfinite(screen[k]) and screen[k] > 0 for k in ("width", "height", "dpi")), f"Invalid dimensions: {path.name}")
            overlay = (path.parent / screen["presentation"]["overlayPath"]).resolve()
            check(overlay.is_relative_to(PACKAGE.resolve()) and overlay.is_file(), f"Missing/external overlay: {path.name}")
            used_overlays.add(overlay)
            orientations = set()
            check(bool(screen["orientations"]), f"No orientations: {path.name}")
            for item in screen["orientations"]:
                orientation_count += 1
                orientation = item["orientation"]
                check(orientation in (1, 2, 3, 4) and orientation not in orientations, f"Invalid/duplicate orientation: {path.name}")
                orientations.add(orientation)
                width, height = screen["width"], screen["height"]
                if orientation in (3, 4):
                    width, height = height, width
                area = item["safeArea"]
                check(all(math.isfinite(area[k]) and area[k] >= 0 for k in ("x", "y", "width", "height"))
                      and area["x"] + area["width"] <= width
                      and area["y"] + area["height"] <= height, f"Safe area outside screen: {path.name}/{orientation}")
    overlays = {p.resolve() for p in (PACKAGE / "Editor").rglob("*.png")}
    check(overlays == used_overlays, "Overlay inventory differs from referenced overlays")
    return {"passed": not errors, "profiles": len(profiles), "orientations": orientation_count,
            "overlays": len(overlays), "errors": errors,
            "limits": "Static integrity only; artwork clearance, Unity behavior and Git installation require separate review."}


if __name__ == "__main__":
    try:
        result = validate()
    except (OSError, ValueError, KeyError, TypeError) as error:
        result = {"passed": False, "errors": [str(error)]}
    print(json.dumps(result, indent=2))
    sys.exit(0 if result["passed"] else 1)
