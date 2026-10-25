# Clean UPM installation verification

Verified on 2026-10-25 with Unity **6000.6.5f1** on macOS.

**Result: passed.** Unity resolved the private Git package, compiled the Editor
assembly and loaded its tooling type. The final batch process exited with code 0
and logged `EDITORCRAFT_CLEAN_UPM_PASS`, with no C# compiler errors or warnings.

## Tested revision

- Commit: `18449199b8524c5decd71df4c18fb906f26c8252`
- Package: `com.daghandemirci.device-simulator-tools` version `0.1.0`
- Dependency: `ssh://git@github-dghn/dghn/editorcraft-device-simulator-tools.git#main`
- `github-dghn` is a local SSH alias for `github.com`; use your own authenticated host.
- UPM lock source: `git`; lock hash matches the commit above.

## Procedure and evidence

1. Created a new empty project with Unity's `-createProject` command. No host Assets,
   ProjectSettings or Library content was copied into it.
2. Ran the manifest installer against that project using the authenticated SSH alias.
3. Started Unity in batch mode; UPM fetched the package into `Library/PackageCache`.
4. Compiled and ran an Editor verification probe that checked the registered package,
   loaded `DeviceSimulatorTools.Editor` assembly and `SimulatorTestTools` type,
   dependencies and imported asset counts.

| Check | Result |
| --- | --- |
| Registered package source | Git |
| Embedded package copy | Absent |
| Pipeline dependency | Absent |
| Device Simulator Devices | 1.0.1, registry |
| Newtonsoft JSON | 3.2.2, registry |
| uGUI | 2.6.0, built-in |
| Device profiles | 122 |
| Overlay textures | 71 |
| Final verification exit code | 0 |

The machine used its existing Unity license, Git credentials and registry cache.
This was a fresh project, not a fresh operating system or cleared global package cache.
The temporary probe needed two corrections (an ambiguous PackageInfo type name and
Unity's virtual Packages path handling); neither required changing package code.

## Scope

This verifies Git installation, dependency resolution and Editor compilation/loading.
It ran with `-batchmode -nographics`: picker interaction, foldable switching, rendered
captures and safe-area visuals were not repeated in this clean project. Their earlier
development-host checks remain separate. No player build or other Unity version was
tested, and no release tag was created.


## Public v0.1.0 release verification

On 2026-10-25 the public HTTPS URL was resolved in the same isolated test project:

```text
https://github.com/dghn/editorcraft-device-simulator-tools.git#v0.1.0
```

UPM locked commit `ba0c77e2154f1944c5f3f27ec6ff9f3214b28dfb`. The final Unity 6000.6.5f1
batch verification exited 0 with `passed=true`, the Editor assembly loaded, 122
profiles and 71 overlays, and no C# compiler errors or warnings. The project remained
free of embedded copies and Pipeline. This follow-up reused the isolated project's
Library and registry cache from the earlier fresh-project verification.
