using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DeviceSimulatorTools.Editor
{
    internal static class SimulatorTestTools
    {
        internal static UnityEngine.Object[] Devices(EditorWindow w) => SimulatorCompatibility.Devices(w);
        internal static UnityEngine.Object Current(EditorWindow w) => SimulatorCompatibility.Current(w);
        internal static JObject Spec(UnityEngine.Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            var json = JObject.Parse(File.Exists(path) ? File.ReadAllText(path) : EditorJsonUtility.ToJson(asset));
            return (JObject)(json["deviceInfo"] ?? json);
        }
        internal static string DeviceName(UnityEngine.Object a) => (string)Spec(a)["friendlyName"] ?? a.name;
        internal static RenderTexture Preview(EditorWindow w) => SimulatorCompatibility.Preview(w);
        internal static void Select(EditorWindow w, UnityEngine.Object asset) => SimulatorCompatibility.Select(w, asset);
        internal static UnityEngine.Object Partner(EditorWindow w)
        {
            string name = DeviceName(Current(w));
            string target = name.Contains("Inner Screen") ? name.Replace("Inner Screen", "Outer Screen") : name.Contains("Outer Screen") ? name.Replace("Outer Screen", "Inner Screen") : null;
            return target == null ? null : Devices(w).FirstOrDefault(d => DeviceName(d) == target);
        }
        internal static void Attach(EditorWindow w, VisualElement parent)
        {
            var swap = parent.Q<ToolbarButton>("simulator-screen-swap");
            if (swap == null)
            {
                swap = new ToolbarButton { name = "simulator-screen-swap", tooltip = "Switch between this device's inner and outer display" };
                swap.clicked += () => { var target = Partner(w); if (target != null && !Busy) Select(w, target); };
                parent.Add(swap);
            }
            var paired = Partner(w);
            swap.style.display = paired == null ? DisplayStyle.None : DisplayStyle.Flex;
            swap.text = paired != null && DeviceName(paired).Contains("Inner Screen") ? "Inner ↔" : "Outer ↔";
            swap.SetEnabled(!Busy);
            if (parent.Q("simulator-test-tools") != null) return;
            var button = new ToolbarButton { name = "simulator-test-tools", text = "Test Tools ▾" };
            button.clicked += () =>
            {
                var menu = new GenericDropdownMenu();
                var partner = Partner(w);
                if (partner != null && !Busy) menu.AddItem("Switch to " + DeviceName(partner), false, () => Select(w, partner));
                if (partner != null) menu.AddSeparator("");
                if (!Busy)
                {
                    menu.AddItem("Screenshot — Clean PNG", false, () => SaveOne(w, false));
                    menu.AddItem("Screenshot — Framed PNG", false, () => SaveOne(w, true));
                    menu.AddItem("Capture selected favorites…", false, () => StartFavorites(w));
                }
                else menu.AddItem("Cancel capture", false, () => Finish("Capture cancelled; completed images retained."));
                menu.AddItem("Safe area inspector", false, () => SimulatorSafeAreaInspector.Open(w));
                menu.AddSeparator("");
                menu.AddItem("Settings…", false, SimulatorCaptureSettings.Open);
                // Keep the dropdown in the plugin's UI Toolkit panel. Native GenericMenu
                // did not open reliably in floating Simulator windows on 2022.3.
                menu.DropDown(button.worldBound, button, true);
            };
            parent.Add(button);
        }
        internal static Texture2D Capture(EditorWindow w, bool framed)
        {
            var rt = Preview(w);
            var screen = SimulatorCompatibility.State(w);
            int width = screen.Width, height = screen.Height;
            if (rt == null || rt.width != width || rt.height != height) throw new InvalidOperationException("Simulator frame is not ready at device resolution. Focus Simulator and retry.");
            var old = RenderTexture.active;
            var clean = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try { RenderTexture.active = rt; clean.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                if (SystemInfo.graphicsUVStartsAtTop)
                {
                    var pixels = clean.GetPixels32(); var row = new Color32[width];
                    for (int y = 0; y < height / 2; y++)
                    {
                        Array.Copy(pixels, y * width, row, 0, width);
                        Array.Copy(pixels, (height - 1 - y) * width, pixels, y * width, width);
                        Array.Copy(row, 0, pixels, (height - 1 - y) * width, width);
                    }
                    clean.SetPixels32(pixels);
                }
                clean.Apply(); }
            catch { UnityEngine.Object.DestroyImmediate(clean); throw; }
            finally { RenderTexture.active = old; }
            if (!framed) return clean;
            try
            {
                var assetPath = AssetDatabase.GetAssetPath(Current(w));
                var overlayPath = (string)Spec(Current(w))["screens"][0]["presentation"]?["overlayPath"];
                var overlay = string.IsNullOrEmpty(overlayPath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(overlayPath)
                    ?? AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(Path.GetDirectoryName(assetPath) ?? "", overlayPath).Replace('\\', '/'));
                if (overlay == null) throw new InvalidOperationException("This device has no frame artwork. Use Clean PNG.");
                var spec = Spec(Current(w))["screens"][0];
                var border = spec["presentation"]["borderSize"];
                int bw = (int)spec["width"], bh = (int)spec["height"];
                int l = Mathf.RoundToInt((float)border["x"]), t = Mathf.RoundToInt((float)border["y"]), r = Mathf.RoundToInt((float)border["z"]), b = Mathf.RoundToInt((float)border["w"]);
                int cw = bw + l + r, ch = bh + t + b;
                var orientation = screen.Orientation;
                bool sideways = orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight;
                var result = new Texture2D(sideways ? ch : cw, sideways ? cw : ch, TextureFormat.RGBA32, false);
                // Read the imported overlay through the GPU; bundled textures need not be readable.
                var tmp = RenderTexture.GetTemporary(cw, ch, 0, RenderTextureFormat.ARGB32, overlay.isDataSRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
                var frame = new Texture2D(cw, ch, TextureFormat.RGBA32, false);
                try
                {
                    Graphics.Blit(overlay, tmp); RenderTexture.active = tmp; frame.ReadPixels(new Rect(0, 0, cw, ch), 0, 0); frame.Apply();
                    var colors = frame.GetPixels32(); var pixels = clean.GetPixels32(); var output = new Color32[colors.Length];
                    // Our preview overlays use opaque neutral backing to prevent framebuffer
                    // leakage. Remove only boundary-connected backing in exported PNGs.
                    var exterior = AssetDatabase.GetAssetPath(overlay).StartsWith("Packages/com.daghandemirci.device-simulator-tools/", StringComparison.Ordinal)
                        ? ExteriorBacking(colors, cw, ch) : null;
                    for (int y = 0; y < ch; y++) for (int x = 0; x < cw; x++)
                    {
                        var color = colors[y * cw + x];
                        if (x >= l && x < l + bw && y >= b && y < b + bh)
                        {
                            var p = Rotate(x - l, y - b, bw, bh, orientation);
                            Color under = pixels[p.y * width + p.x]; Color over = color;
                            color = Color.Lerp(under, new Color(over.r, over.g, over.b, 1), over.a);
                        }
                        if (exterior != null && exterior[y * cw + x]) color = new Color32(0, 0, 0, 0);
                        var q = Rotate(x, y, cw, ch, orientation); output[q.y * result.width + q.x] = color;
                    }
                    result.SetPixels32(output); result.Apply(); return result;
                }
                catch { UnityEngine.Object.DestroyImmediate(result); throw; }
                finally { RenderTexture.active = old; RenderTexture.ReleaseTemporary(tmp); UnityEngine.Object.DestroyImmediate(frame); }
            }
            finally { UnityEngine.Object.DestroyImmediate(clean); }
        }
        // Never chroma-key the composed screenshot: UI and enclosed hardware may
        // legitimately contain the same gray. Walk the overlay from its perimeter.
        internal static bool[] ExteriorBacking(Color32[] colors, int width, int height)
        {
            var exterior = new bool[colors.Length];
            var pending = new Queue<int>();
            void Visit(int index)
            {
                if (exterior[index]) return;
                var c = colors[index];
                if (c.a < 253 || Math.Abs(c.r - 56) > 2 || Math.Abs(c.g - 56) > 2 || Math.Abs(c.b - 56) > 2) return;
                exterior[index] = true;
                pending.Enqueue(index);
            }
            for (int x = 0; x < width; x++) { Visit(x); Visit((height - 1) * width + x); }
            for (int y = 0; y < height; y++) { Visit(y * width); Visit(y * width + width - 1); }
            while (pending.Count > 0)
            {
                int i = pending.Dequeue(), x = i % width, y = i / width;
                if (x > 0) Visit(i - 1);
                if (x + 1 < width) Visit(i + 1);
                if (y > 0) Visit(i - width);
                if (y + 1 < height) Visit(i + width);
            }
            return exterior;
        }
        static Vector2Int Rotate(int x, int y, int w, int h, ScreenOrientation orientation) => orientation switch
        {
            ScreenOrientation.PortraitUpsideDown => new Vector2Int(w - 1 - x, h - 1 - y),
            ScreenOrientation.LandscapeLeft => new Vector2Int(h - 1 - y, x),
            ScreenOrientation.LandscapeRight => new Vector2Int(y, w - 1 - x),
            _ => new Vector2Int(x, y)
        };
        static string FileName(EditorWindow w) => string.Concat(DeviceName(Current(w)).Select(c => char.IsLetterOrDigit(c) ? c : '_')) + "_" + SimulatorCompatibility.State(w).Orientation;
        static void SaveOne(EditorWindow w, bool framed)
        {
            string destination = SimulatorCaptureSettings.ResolveFolder(false);
            if (destination.Length == 0) return;
            string path = SimulatorCaptureSettings.UniquePath(destination, FileName(w) + (framed ? "_framed" : ""), ".png");
            try { Write(w, path, framed); EditorUtility.RevealInFinder(path); }
            catch (Exception e) { EditorUtility.DisplayDialog("Simulator capture", e.Message, "OK"); }
        }
        internal static void Write(EditorWindow w, string path, bool framed)
        {
            var texture = Capture(w, framed);
            try { File.WriteAllBytes(path, texture.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }
        internal static bool Busy => batchWindow != null;
        static EditorWindow batchWindow;
        static UnityEngine.Object original;
        static ScreenOrientation originalOrientation;
        static Queue<UnityEngine.Object> queue;
        static string folder;
        static double readyAt, deadline;
        static bool waiting;
        static int total;
        static readonly List<string> images = new();
        static void StartFavorites(EditorWindow w)
        {
            UnityEngine.Object[] assets;
            try { assets = SimulatorDevicePickerPopup.SelectedFavorites(Devices(w)); }
            catch (Newtonsoft.Json.JsonException) { EditorUtility.DisplayDialog("Favorites", "Favorite preferences or device data could not be read.", "OK"); return; }
            if (assets.Length == 0) { EditorUtility.DisplayDialog("Favorites", "No favorites match the current platform, device type and search filters. Change filters or star matching devices.", "OK"); return; }
            string destination = SimulatorCaptureSettings.ResolveFolder(true);
            if (destination.Length == 0) return;
            Begin(w, assets, SimulatorCaptureSettings.UniquePath(destination, "Simulator", ""));
        }
        internal static void Begin(EditorWindow w, UnityEngine.Object[] assets, string destination)
        {
            if (Busy || assets.Length == 0) return;
            Directory.CreateDirectory(destination); folder = destination; batchWindow = w;
            original = Current(w); originalOrientation = SimulatorCompatibility.State(w).Orientation;
            queue = new Queue<UnityEngine.Object>(assets); total = assets.Length; images.Clear(); waiting = false;
            EditorApplication.update += Tick; AssemblyReloadEvents.beforeAssemblyReload += Cancel;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }
        internal static void CancelFor(EditorWindow owner) { if (owner != null && batchWindow == owner) Cancel(); }
        static void Cancel() => Finish("Capture interrupted.");
        static void OnPlayMode(PlayModeStateChange state) { if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.ExitingEditMode) Cancel(); }
        static void Tick()
        {
            try
            {
                if (batchWindow == null) { Finish("Simulator closed."); return; }
                if (EditorUtility.DisplayCancelableProgressBar("Favorite device captures", "The game continues running; these are sequential layout snapshots.", (float)images.Count / total)) { Cancel(); return; }
                batchWindow.Repaint();
                if (!waiting)
                {
                    if (queue.Count == 0) { Finish(null); return; }
                    Select(batchWindow, queue.Dequeue()); batchWindow.Focus();
                    readyAt = EditorApplication.timeSinceStartup + 1.5; deadline = readyAt + 10; waiting = true; return;
                }
                if (EditorApplication.timeSinceStartup < readyAt) return;
                var rt = Preview(batchWindow); var s = SimulatorCompatibility.State(batchWindow);
                if (rt == null || rt.width != s.Width || rt.height != s.Height)
                { if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Timed out waiting for a full-resolution Simulator frame."); return; }
                string file = images.Count.ToString("D3") + "_" + FileName(batchWindow) + ".png";
                Write(batchWindow, Path.Combine(folder, file), false); images.Add(file); waiting = false;
            }
            catch (Exception e) { Finish(e.Message); }
        }
        static void Finish(string error)
        {
            EditorApplication.update -= Tick; AssemblyReloadEvents.beforeAssemblyReload -= Cancel; EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorUtility.ClearProgressBar();
            try
            {
                if (batchWindow != null && original != null)
                {
                    try
                    {
                        Select(batchWindow, original);
                        SimulatorCompatibility.RestoreOrientation(batchWindow, originalOrientation);
                    }
                    catch (Exception e)
                    {
                        error = (error == null ? "" : error + " ") + "Could not restore Simulator: " + e.GetBaseException().Message;
                    }
                }
                if (!string.IsNullOrEmpty(folder))
                {
                    string html = "<!doctype html><meta charset='utf-8'><title>Simulator captures</title><style>body{background:#282828;color:white;font:14px sans-serif}main{display:flex;flex-wrap:wrap;gap:16px}figure{margin:0;width:280px}img{max-width:280px;max-height:560px}figcaption{overflow-wrap:anywhere}</style><h1>Favorite device captures</h1><p>Sequential snapshots, not frozen game state or automatic UI pass/fail.</p><main>";
                    foreach (string file in images) html += "<figure><img src='" + file + "'><figcaption>" + file + "</figcaption></figure>";
                    File.WriteAllText(Path.Combine(folder, "index.html"), html + "</main>");
                    if (error == null) EditorUtility.RevealInFinder(Path.Combine(folder, "index.html"));
                }
                if (error != null) Debug.LogWarning("Simulator capture: " + error);
            }
            finally { batchWindow = null; original = null; queue = null; folder = null; }
        }
    }

    internal sealed class SimulatorCaptureSettings : EditorWindow
    {
        static string Key(bool gallery) => "DeviceSimulatorTools.SimulatorTools." + (gallery ? "GalleryFolder" : "ScreenshotFolder");
        internal static string SavedFolder(bool gallery) => EditorPrefs.GetString(Key(gallery), "");
        internal static string ChooseFolder(bool gallery)
        {
            string path = EditorUtility.OpenFolderPanel(gallery ? "Choose HTML gallery export folder" : "Choose screenshot folder", SavedFolder(gallery), "");
            if (!string.IsNullOrEmpty(path)) EditorPrefs.SetString(Key(gallery), path);
            return path;
        }
        internal static string ResolveFolder(bool gallery)
        {
            string path = SavedFolder(gallery);
            return Directory.Exists(path) ? path : ChooseFolder(gallery);
        }
        internal static string UniquePath(string directory, string name, string extension)
        {
            string stem = name + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string path = Path.Combine(directory, stem + extension);
            for (int n = 2; File.Exists(path) || Directory.Exists(path); n++)
                path = Path.Combine(directory, stem + "_" + n + extension);
            return path;
        }
        internal static void Open()
        {
            var window = GetWindow<SimulatorCaptureSettings>("Simulator Settings");
            window.minSize = new Vector2(360, 300);
            window.Show();
        }
        void OnGUI()
        {
            EditorGUILayout.LabelField("Export folders", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose each folder once. Future exports save there automatically with unique names. These settings are personal to this computer.", MessageType.Info);
            DrawFolder(false, "Screenshots (PNG)");
            GUILayout.Space(8);
            DrawFolder(true, "Favorite captures (HTML + PNG)");
        }
        static void DrawFolder(bool gallery, string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            string path = SavedFolder(gallery);
            EditorGUILayout.LabelField(string.IsNullOrEmpty(path) ? "Ask on first export" : path, EditorStyles.wordWrappedLabel);
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                EditorGUILayout.HelpBox("Folder unavailable. Choose another folder or reconnect it before exporting.", MessageType.Warning);
            if (GUILayout.Button(string.IsNullOrEmpty(path) ? "Choose folder…" : "Change folder…", GUILayout.Width(130))) ChooseFolder(gallery);
        }
    }

    internal sealed class SimulatorSafeAreaInspector : EditorWindow
    {
        EditorWindow simulator;
        internal static void Open(EditorWindow owner) { var w = GetWindow<SimulatorSafeAreaInspector>("Safe Area"); w.simulator = owner; w.minSize = new Vector2(320, 400); w.Show(); }
        void OnInspectorUpdate() => Repaint();
        void OnGUI()
        {
            if (simulator == null) { EditorGUILayout.HelpBox("Open from Simulator > Test Tools.", MessageType.Info); return; }
            if (!SimulatorCompatibility.Available(simulator, out var reason)) { EditorGUILayout.HelpBox(reason, MessageType.Info); return; }
            var s = SimulatorCompatibility.State(simulator);
            int width = s.Width, height = s.Height;
            Rect safe = s.SafeArea;
            EditorGUILayout.LabelField(SimulatorTestTools.DeviceName(SimulatorTestTools.Current(simulator)), EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{width} × {height} px");
            EditorGUILayout.LabelField($"L {safe.xMin:0}   T {height-safe.yMax:0}   R {width-safe.xMax:0}   B {safe.yMin:0} px");
            var rt = Selection.activeTransform as RectTransform; Rect? selected = null;
            if (rt != null && rt.gameObject.scene.IsValid() && rt.gameObject.activeInHierarchy)
            {
                var canvas = rt.GetComponentInParent<Canvas>();
                if (canvas != null) canvas = canvas.rootCanvas;
                if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
                {
                    var corners = new Vector3[4]; rt.GetWorldCorners(corners);
                    var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                    var pts = corners.Select(p => RectTransformUtility.WorldToScreenPoint(cam, p)).ToArray();
                    selected = Rect.MinMaxRect(pts.Min(p=>p.x),pts.Min(p=>p.y),pts.Max(p=>p.x),pts.Max(p=>p.y));
                    var r = selected.Value;
                    bool inside = r.xMin >= safe.xMin && r.yMin >= safe.yMin && r.xMax <= safe.xMax && r.yMax <= safe.yMax;
                    EditorGUILayout.HelpBox(rt.name + (inside ? ": inside safe area" : ": extends outside safe area"), inside ? MessageType.Info : MessageType.Warning);
                }
            }
            if (!selected.HasValue) EditorGUILayout.LabelField("Select an active scene screen-space UI RectTransform to inspect its bounds.", EditorStyles.wordWrappedLabel);
            float top = GUILayoutUtility.GetLastRect().yMax + 8;
            var available = new Rect(4, top, Mathf.Max(1, position.width - 8), Mathf.Max(1, position.height - top - 4));
            float scale = Mathf.Min(available.width/width, available.height/height);
            var preview = new Rect(available.x+(available.width-width*scale)/2,available.y,width*scale,height*scale);
            var texture = SimulatorTestTools.Preview(simulator);
            if (texture != null) GUI.DrawTextureWithTexCoords(preview, texture, SystemInfo.graphicsUVStartsAtTop ? new Rect(0, 1, 1, -1) : new Rect(0, 0, 1, 1), false);
            Rect Map(Rect r) => new Rect(preview.x+r.x*scale,preview.y+(height-r.yMax)*scale,r.width*scale,r.height*scale);
            Handles.BeginGUI(); Outline(Map(safe), Color.green); if(selected.HasValue) Outline(Map(selected.Value), Color.yellow); Handles.EndGUI();
        }
        static void Outline(Rect r, Color color) => Handles.DrawSolidRectangleWithOutline(r, new Color(color.r,color.g,color.b,.06f),color);
    }
}
