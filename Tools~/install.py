#!/usr/bin/env python3
"""Register EditorCraft in an existing Unity project's UPM manifest (Python 3.9+)."""
import argparse
import json
import os
from pathlib import Path
import sys
import tempfile

PACKAGE = "com.daghandemirci.device-simulator-tools"
DEFAULT_URL = "https://github.com/dghn/editorcraft-device-simulator-tools.git#v0.3.1"


def unique_keys(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"Duplicate JSON key: {key}")
        result[key] = value
    return result


def install(project, url=DEFAULT_URL):
    project = Path(project).resolve()
    manifest = project / "Packages/manifest.json"
    if not (project / "ProjectSettings/ProjectVersion.txt").is_file() or not manifest.is_file():
        raise ValueError("Expected an existing Unity project with Packages/manifest.json and ProjectSettings/ProjectVersion.txt")
    if (project / "Packages" / PACKAGE).exists():
        raise ValueError("An embedded copy exists. Remove or relocate it deliberately before installing from Git.")
    if not url.startswith(("ssh://", "https://")) or ".git" not in url:
        raise ValueError("Use an SSH or HTTPS Git package URL")
    original = manifest.read_bytes()
    data = json.loads(original.decode("utf-8-sig"), object_pairs_hook=unique_keys)
    if not isinstance(data, dict) or not isinstance(data.get("dependencies"), dict):
        raise ValueError("Manifest must contain a dependencies object")
    dependencies = data["dependencies"]
    if PACKAGE in dependencies:
        if dependencies[PACKAGE] == url:
            return "Already registered; no files changed."
        raise ValueError("A different package dependency already exists; review it manually before replacing it.")
    dependencies[PACKAGE] = url
    backup = manifest.with_name("manifest.json.editorcraft-backup")
    # Never replace an earlier backup. Save exact bytes before changing the manifest.
    with backup.open("xb") as file:
        file.write(original)
    temporary = None
    try:
        with tempfile.NamedTemporaryFile(mode="w", encoding="utf-8", newline="\n", dir=manifest.parent, delete=False) as file:
            temporary = Path(file.name)
            json.dump(data, file, indent=2, ensure_ascii=False)
            file.write("\n")
        os.chmod(temporary, manifest.stat().st_mode)
        if manifest.read_bytes() != original:
            raise ValueError("Manifest changed during installation; no overwrite performed")
        os.replace(temporary, manifest)
    finally:
        if temporary is not None and temporary.exists():
            temporary.unlink()
    return f"Registered {PACKAGE}. Backup: {backup}\nOpen Unity to resolve the Git dependency. Package resolution and compilation are not verified by this command."


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", default=".", help="Unity project root (default: current directory)")
    parser.add_argument("--url", default=DEFAULT_URL, help="Git dependency URL, including optional ref")
    args = parser.parse_args()
    try:
        print(install(args.project, args.url))
    except (OSError, ValueError) as error:
        print(f"Installation stopped: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
