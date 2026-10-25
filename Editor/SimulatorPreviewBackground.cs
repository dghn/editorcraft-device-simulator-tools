using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DeviceSimulatorTools.Editor
{
    // Device overlays must cover the rectangular framebuffer outside rounded cases.
    // Overlay imports disable sRGB decoding for the gamma-space IMGUI renderer.
    // Keep their #383838 backing and the surrounding preview in the same color space,
    // independent of Editor theme and the user's Play Mode tint. No global preferences
    // or game rendering settings are changed. The plugin owns this decoration and its lifecycle.
    internal static class SimulatorPreviewBackground
    {
        static readonly Color Background = new Color32(56, 56, 56, 255);
        internal static void Attach(EditorWindow window)
        {
            // Re-query after Editor rebuilds the UI on reload or Play transitions.
            var preview = window.rootVisualElement.Q<VisualElement>("preview-panel");
            if (preview == null) return;
            var backdrop = preview.Q<VisualElement>("device-simulator-tools-preview-backdrop");
            if (backdrop != null) return;
            backdrop = new VisualElement
            {
                name = "device-simulator-tools-preview-backdrop",
                pickingMode = PickingMode.Ignore
            };
#if UNITY_6000_0_OR_NEWER
            backdrop.disablePlayModeTint = true;
#endif
            backdrop.style.position = Position.Absolute;
            backdrop.style.left = backdrop.style.right = 0;
            backdrop.style.top = backdrop.style.bottom = 0;
            backdrop.style.backgroundColor = Background;
            // Disable tint only on this background leaf; the Simulator's IMGUI
            // device renderer must retain its own render state.
            preview.Insert(0, backdrop);
            window.Repaint();
        }
        internal static void Detach(EditorWindow window)
        {
            if (window != null) window.rootVisualElement.Q("device-simulator-tools-preview-backdrop")?.RemoveFromHierarchy();
        }
    }
}
