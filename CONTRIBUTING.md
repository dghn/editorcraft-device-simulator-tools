# Contributing

Keep code, comments, documentation, UI strings and contribution text in English.
Keep this project independent: use neutral samples and do not introduce content,
branding, configuration or credentials from unrelated applications.

## Report a problem

Include your exact Unity version, operating system, device profile, orientation,
reproduction steps and expected/actual behavior. For capture problems, confirm that
Device Simulator is active and rendering. Use a minimal neutral scene and remove
private content from attachments.

## Make a change

1. Use Unity 6000.6.5f1 for the currently verified Editor baseline.
2. Keep installable functionality inside the UPM package and Editor-only.
3. Preserve existing asset GUIDs and include metadata for new package assets.
4. Run `python3 .github/scripts/validate_package.py` with Python 3.9+.
5. For profile or Simulator changes, verify in a separate Unity host and describe
   manual checks. Record the Editor version and any limitations.
6. Update documentation when behavior changes.

Device measurements need source references and explicit OS/system-UI assumptions.
New artwork needs documented redistribution rights. Attribution alone is insufficient.
Do not commit downloaded manufacturer archives, generated captures, local preferences,
private keys or credentials.
