using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DeviceSimulatorTools.Editor
{
    // Device overlays must cover the rectangular framebuffer outside rounded cases.
    // Keep their #383838 backing and the surrounding preview in the same color space,
    // independent of Editor theme and the user's Play Mode tint. No global preferences
    // or game rendering settings are changed. This timer is Editor-only state.
    [InitializeOnLoad]
    internal static class SimulatorPreviewBackground
    {
        static readonly Color Background = new Color32(56, 56, 56, 255);
        static double nextCheck;

        static SimulatorPreviewBackground()
        {
            EditorApplication.update += Update;
        }

        static void Update()
        {
            if (EditorApplication.timeSinceStartup < nextCheck) return;
            nextCheck = EditorApplication.timeSinceStartup + .5;
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window.GetType().FullName != "UnityEditor.DeviceSimulation.SimulatorWindow") continue;
                // Re-query after Editor rebuilds the UI on reload or Play transitions.
                var preview = window.rootVisualElement.Q<VisualElement>("preview-panel");
                if (preview == null) continue;
                var backdrop = preview.Q<VisualElement>("device-simulator-tools-preview-backdrop");
                if (backdrop != null) continue;
                backdrop = new VisualElement
                {
                    name = "device-simulator-tools-preview-backdrop",
                    pickingMode = PickingMode.Ignore,
                    disablePlayModeTint = true
                };
                backdrop.style.position = Position.Absolute;
                backdrop.style.left = backdrop.style.right = 0;
                backdrop.style.top = backdrop.style.bottom = 0;
                backdrop.style.backgroundColor = Background;
                // Disable tint only on this background leaf; the Simulator's IMGUI
                // device renderer must retain its own render state.
                preview.Insert(0, backdrop);
                window.Repaint();
            }
        }
    }
}
