using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DeviceSimulatorTools.Editor
{
    // Unity exposes plugin UI/lifecycle, but not per-window selection, screen state
    // or render textures. Keep those version-sensitive accesses at this boundary.
    internal static class SimulatorCompatibility
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        static object Read(object target, string member)
        {
            if (target == null) throw new NotSupportedException("Simulator is not available.");
            var type = target.GetType();
            var property = type.GetProperty(member, Flags);
            if (property != null && property.CanRead) return property.GetValue(target);
            var field = type.GetField(member, Flags);
            if (field != null) return field.GetValue(target);
            throw new NotSupportedException($"Unity {Application.unityVersion}: Simulator member {member} is unavailable.");
        }
        static object Main(EditorWindow window) => Read(window, "main");
        static object Screen(EditorWindow window) => Read(Main(window), "ScreenSimulation");
        internal static UnityEngine.Object[] Devices(EditorWindow window) => ((Array)Read(Main(window), "devices")).Cast<UnityEngine.Object>().ToArray();
        internal static UnityEngine.Object Current(EditorWindow window) => (UnityEngine.Object)Read(Main(window), "currentDevice");
        internal static RenderTexture Preview(EditorWindow window) => Read(Read(Read(Main(window), "userInterface"), "DeviceView"), "PreviewTexture") as RenderTexture;
        internal struct ScreenState
        {
            internal int Width, Height;
            internal Rect SafeArea;
            internal ScreenOrientation Orientation;
        }
        internal static ScreenState State(EditorWindow window)
        {
            var screen = Screen(window);
            return new ScreenState { Width = (int)Read(screen, "width"), Height = (int)Read(screen, "height"),
                SafeArea = (Rect)Read(screen, "safeArea"), Orientation = (ScreenOrientation)Read(screen, "orientation") };
        }
        internal static void Select(EditorWindow window, UnityEngine.Object asset)
        {
            int index = Array.IndexOf(Devices(window), asset);
            if (index < 0) throw new InvalidOperationException("Device is no longer available.");
            var ui = Read(Main(window), "userInterface");
            var select = ui.GetType().GetMethod("OnDeviceSelected", Flags);
            if (select == null) throw new NotSupportedException("Device selection is unavailable in this Unity version. Use Unity's native selector.");
            select.Invoke(ui, new object[] { index });
            window.Repaint();
        }
        internal static void RestoreOrientation(EditorWindow window, ScreenOrientation orientation)
        {
            var screen = Screen(window);
            var property = screen.GetType().GetProperty("orientation", Flags);
            var apply = screen.GetType().GetMethod("ApplyChanges", Flags);
            if (property == null || !property.CanWrite || apply == null)
                throw new NotSupportedException("Simulator orientation restoration is unavailable.");
            property.SetValue(screen, orientation);
            apply.Invoke(screen, null);
        }
        internal static bool Available(EditorWindow window, out string reason)
        {
            try
            {
                if (window == null) { reason = "Waiting for the Simulator window."; return false; }
                var ui = Read(Main(window), "userInterface");
                if (ui.GetType().GetMethod("OnDeviceSelected", Flags) == null)
                    throw new NotSupportedException("Device selection is unavailable.");
                Current(window); State(window); Preview(window);
                reason = ""; return true;
            }
            catch (Exception e) { reason = "Simulator tools unavailable: " + e.GetBaseException().Message + " Use Unity's native controls."; return false; }
        }
    }
}
