# Device Simulator plugin compatibility

**Verified versions: Unity 2022.3.62f2 and Unity 6000.6.5f1.** These are test results, not minimum requirements. Earlier Unity versions may work but have not been verified. Compatibility depends on the available Device Simulator APIs and package dependencies.

The tools use Unity's official `UnityEditor.DeviceSimulation.DeviceSimulatorPlugin`.
Open **Window > General > Device Simulator**, show its **Control Panel**, then expand
**EditorCraft Device Tools**. The searchable picker, favorites, paired-screen switch
and Test Tools menu live there. Unity's native toolbar and device selector remain intact.
Existing favorite/filter/export-folder EditorPrefs keys are unchanged.

## Public API and the remaining boundary

`SimulatorToolsPlugin` provides `title`, `OnCreateUI` and `OnDestroy`. UI Toolkit
scheduling is tied to that panel's attachment, rather than a global editor update
loop searching for toolbar elements. Teardown pauses refresh and cancels this
window's capture job. The preview background decoration is plugin-owned as well.

The picker reads device JSON and GUIDs through public file/AssetDatabase APIs.
Frame captures resolve overlay textures through AssetDatabase; they no longer read
`m_OverlayTexture`. UI creation, preferences, filtering and compositing use public APIs.

Only `Editor/SimulatorCompatibility.cs` uses reflection in the shipped package:

| Internal operation | Reason |
| --- | --- |
| Window `main`, `devices`, `currentDevice` | Public plugin API has no per-window device enumeration or active-device accessor. |
| UI `OnDeviceSelected` | No public API changes the simulated device. Using the native handler also updates Unity's UI. |
| `ScreenSimulation` dimensions, safe area and orientation | Values must belong to the owning Simulator window, not a global active-screen shim. |
| UI `DeviceView.PreviewTexture` | No public plugin screenshot/render-texture API. |
| Orientation setter and `ApplyChanges` | Restore the user's previous orientation after a batch. |

These are unsupported implementation details, isolated behind typed methods.
A failed compatibility probe disables the custom controls with a status message;
Unity's native controls remain usable. Not all reflection can be removed while
preserving selection and capture. The preview backdrop still queries the internal
`preview-panel` UI element; if absent it is simply skipped.

## Version support

Verified on 2026-10-25 in **2022.3.62f2** and **6000.6.5f1** on macOS.
Both passed 181 profile selections, 676 orientation checks, eight PNG captures
and a two-device HTML gallery, including state restoration and cancellation.

The UPM manifest does not declare a minimum Editor version. Removing the `unity`
field avoids treating the oldest tested Editor as a requirement; it is not proof
of compatibility with every older version. Device Simulator APIs and dependencies
still need to be available in the target Editor. The package requests uGUI **1.0.0**.
See the [Unity manifest reference](https://docs.unity3d.com/Manual/upm-manifestPkg.html#unity). The Unity 6 host retains its directly requested uGUI 2.6.0.
The Unity 6-only play-mode tint property is conditionally compiled.
The host project itself remains a Unity 6000.6 project; do not downgrade it to test
2022.3. Use an empty project and install the package from disk.

Behavior results and exact tested patch versions are stored in
`Tools/DeviceSimulator/Compatibility/Results/`. Validation covers plugin discovery,
native selector preservation, all 181 package devices / 676 stored orientations,
search/filter/favorite behavior, paired screen selection, inspector creation,
clean/framed PNGs in four orientations, batch HTML/PNG export, state restoration,
cancellation and plugin teardown. These are Editor integration checks, not physical
hardware tests or a guarantee for every Unity patch release.

## Reproducing

1. Create a separate empty project using the desired editor version.
2. Add this package using a local `file:` dependency; let UPM resolve its dependencies.
3. Copy `Tools/DeviceSimulator/Compatibility/PluginSmoke.cs` into that project's
   `Assets/Editor/`. The probe uses reflection only to exercise package-internal tests.
4. Run Unity with `-batchmode -projectPath <test-project> -executeMethod PluginSmoke.Run
   -logFile <log-path>`. Do not pass `-quit` or `-nographics`: the test waits for actual
   render frames and exits when finished. Reports use `Path.GetTempPath()` and the
   filename `editorcraft-plugin-<Unity-version>.json`.
5. For an existing Pipeline-enabled development host, run the same script through
   `unity command run_script` and read that report after completion.

The probe restores device/orientation and favorite preferences. Its generated test
galleries remain in the OS temporary directory. It opens/closes test popup windows;
prefer an isolated project for routine verification.

References: [Adding a device](https://docs.unity3d.com/2022.3/Documentation/Manual/device-simulator-adding-a-device.html),
[Extending Device Simulator](https://docs.unity3d.com/2022.3/Documentation/Manual/device-simulator-plugins.html),
[DeviceSimulatorPlugin](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/DeviceSimulation.DeviceSimulatorPlugin.html).
