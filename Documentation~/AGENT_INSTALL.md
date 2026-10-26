# Install with a coding agent


Run this from the **target Unity project root** (the folder containing `Assets`,
`Packages` and `ProjectSettings`). Requires Python 3.9+ and Git. No GitHub account or SSH key is needed. Use Bash/zsh on macOS/Linux or PowerShell with `python3`
available on Windows. Close Unity while editing its manifest, then reopen it.

```sh
python3 -c 'import os, pathlib, subprocess, sys, tempfile; url=os.environ.get("EDITORCRAFT_GIT_URL", "https://github.com/dghn/editorcraft-device-simulator-tools.git"); tmp=tempfile.TemporaryDirectory(prefix="editorcraft-"); repo=pathlib.Path(tmp.name)/"package"; subprocess.run(["git", "clone", "--depth", "1", "--branch", "v0.4.1", url, str(repo)], check=True); subprocess.run([sys.executable, str(repo/"Tools~"/"install.py"), "--project", os.getcwd(), "--url", url+"#v0.4.1"], check=True)'
```

This clones the installer into a temporary directory and registers the Git dependency
in `Packages/manifest.json`. Other dependencies and manifest fields are preserved.
It saves an exact backup as `Packages/manifest.json.editorcraft-backup`, leaves an
identical installation unchanged, and stops on a conflicting dependency or embedded
package. It does not copy this repository into `Assets`, modify project settings,
or edit `packages-lock.json`. Unity resolves dependencies when the project opens.
Manifest registration is not confirmation that UPM resolution or compilation passed.

For a dedicated SSH alias, set `EDITORCRAFT_GIT_URL` to
`ssh://git@github-dghn/dghn/editorcraft-device-simulator-tools.git` before running the
command. In Bash/zsh you can prefix the command with
`EDITORCRAFT_GIT_URL=ssh://git@github-dghn/dghn/editorcraft-device-simulator-tools.git`.
In PowerShell set `$env:EDITORCRAFT_GIT_URL` first. The value is the repository URL
without a `#ref`; this command uses `v0.4.1`.

If the repository is already cloned, use the installer directly:

```sh
python3 "/path/to/editorcraft-device-simulator-tools/Tools~/install.py" --project "/path/to/UnityProject" --url "https://github.com/dghn/editorcraft-device-simulator-tools.git#v0.4.1"
```

Coding agents: run the command only in the intended project, inspect the manifest
diff, then let Unity resolve the package and check the Console for compilation
errors. Report those results separately. Do not claim success from the manifest
change alone. The package needs no agent plugin, Unity CLI or Pipeline installation.

Clean-project Git installation, dependency resolution and Editor compilation were
verified on 2026-10-25 at commit `18449199b852`.
[Read the verification report](Documentation~/INSTALL_VERIFICATION.md) for evidence and scope.
