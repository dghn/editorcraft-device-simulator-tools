using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DeviceSimulatorTools.Editor
{
    internal sealed class SimulatorReviewWindow : EditorWindow
    {
        [SerializeField] EditorWindow simulator;
        [SerializeField] RectTransform root;
        [SerializeField] List<RectTransform> exclusions = new List<RectTransform>();
        [SerializeField] bool audit = true, allOrientations = true, includeContainers;
        [SerializeField] float tolerance = 1;
        [SerializeField] string reportFolder;
        [SerializeField] int page, orientationFilter;
        [SerializeField] bool showOptions = true;
        [NonSerialized] SimulatorReviewReport report;
        readonly Dictionary<string, Texture2D> thumbnails = new Dictionary<string, Texture2D>();
        Vector2 scroll;
        bool running;
        string error, selectionPreferences;
        bool selectionError;
        int matchingFavorites = -1;
        const int PageSize = 4;
        const string NoFavoritesMessage = "No favorites match the current picker filters. Star devices or change the search/platform/type filters.";

        internal static void Open(EditorWindow owner, bool enableAudit)
        {
            var window = GetWindow<SimulatorReviewWindow>("UI Audit & Gallery");
            window.simulator = owner;
            if (!window.running) window.audit = enableAudit;
            window.selectionPreferences = null;
            window.OnInspectorUpdate();
            if (window.root == null) window.root = Selection.activeTransform as RectTransform;
            window.minSize = new Vector2(680, 520); window.Show();
        }
        void OnEnable()
        {
            selectionPreferences = null;
            if (!string.IsNullOrEmpty(reportFolder) && File.Exists(Path.Combine(reportFolder, "report.json")))
            {
                try { report = JsonUtility.FromJson<SimulatorReviewReport>(File.ReadAllText(Path.Combine(reportFolder, "report.json"))); }
                catch (Exception e) { error = e.Message; }
            }
        }
        void OnDisable()
        {
            if (running) SimulatorTestTools.CancelFor(simulator);
            ClearThumbnails();
        }
        void ClearThumbnails()
        {
            foreach (var texture in thumbnails.Values) if (texture != null) DestroyImmediate(texture);
            thumbnails.Clear();
        }
        void OnInspectorUpdate()
        {
            string snapshot = SimulatorDevicePickerPopup.CapturePreferenceSnapshot();
            if (!running && simulator != null && snapshot != selectionPreferences)
            {
                try
                {
                    matchingFavorites = SimulatorDevicePickerPopup.SelectedFavorites(SimulatorTestTools.Devices(simulator)).Length;
                    selectionPreferences = snapshot;
                    if ((selectionError || error == NoFavoritesMessage) && matchingFavorites > 0) { error = null; selectionError = false; }
                }
                catch { matchingFavorites = -1; } // StartReview reports actionable read errors.
            }
            Repaint();
        }
        void OnGUI()
        {
            EditorGUILayout.LabelField("Device comparison & UI audit", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Capture the favorites matching the device picker's current filters. Snapshots are sequential, not live or frozen game state. Audit checks geometric bounds; clipping and text readability are not assessed.", MessageType.Info);
            showOptions = EditorGUILayout.Foldout(showOptions, "Capture settings", true);
            if (showOptions)
            using (new EditorGUI.DisabledScope(SimulatorTestTools.Busy))
            {
                audit = EditorGUILayout.Toggle("Audit UI bounds", audit);
                allOrientations = EditorGUILayout.Toggle("All profile orientations", allOrientations);
                if (audit)
                {
                    root = (RectTransform)EditorGUILayout.ObjectField("UI root", root, typeof(RectTransform), true);
                    includeContainers = EditorGUILayout.Toggle("Include layout containers", includeContainers);
                    tolerance = Mathf.Max(0, EditorGUILayout.FloatField("Tolerance (screen px)", tolerance));
                    EditorGUILayout.LabelField("Exclude objects and their descendants", EditorStyles.miniBoldLabel);
                    for (int i = 0; i < exclusions.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            exclusions[i] = (RectTransform)EditorGUILayout.ObjectField(exclusions[i], typeof(RectTransform), true);
                            if (GUILayout.Button("Remove", GUILayout.Width(65))) { exclusions.RemoveAt(i); break; }
                        }
                    }
                    if (GUILayout.Button("Add exclusion", GUILayout.Width(120))) exclusions.Add(null);
                }
                if (matchingFavorites >= 0) EditorGUILayout.LabelField($"Matching favorite devices: {matchingFavorites}", EditorStyles.miniLabel);
                if (GUILayout.Button(audit ? "Audit & capture selected favorites" : "Capture selected favorites for comparison")) StartReview();
            }
            if (running && GUILayout.Button("Cancel batch (keep completed results)")) SimulatorTestTools.CancelFor(simulator);
            if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Warning);
            if (report == null) return;
            EditorGUILayout.LabelField($"{report.frames.Count} snapshots · {report.status}", EditorStyles.boldLabel);
            if (report.audit) EditorGUILayout.LabelField($"Root: {report.root} · tolerance: {report.tolerance:0.##} px", EditorStyles.miniLabel);
            int filter = EditorGUILayout.Popup("Compare orientation", orientationFilter,
                new[] { "All orientations", "Portrait", "Portrait upside down", "Landscape left", "Landscape right" });
            if (filter != orientationFilter) { orientationFilter = filter; page = 0; ClearThumbnails(); }
            var matching = report.frames.Where(f => orientationFilter == 0 || (int)f.orientation == orientationFilter).ToArray();
            page = Mathf.Clamp(page, 0, Mathf.Max(0, (matching.Length - 1) / PageSize));
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(running || report.frames.Count == 0))
                    if (GUILayout.Button("Export report & images…")) Export();
                if (GUILayout.Button("Show cached files")) EditorUtility.RevealInFinder(reportFolder);
                using (new EditorGUI.DisabledScope(page == 0))
                    if (GUILayout.Button("Previous", GUILayout.Width(75))) { page--; ClearThumbnails(); }
                GUILayout.Label($"{page + 1} / {Mathf.Max(1, Mathf.CeilToInt(matching.Length / (float)PageSize))}", GUILayout.Width(55));
                using (new EditorGUI.DisabledScope((page + 1) * PageSize >= matching.Length))
                    if (GUILayout.Button("Next", GUILayout.Width(55))) { page++; ClearThumbnails(); }
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            var visible = matching.Skip(page * PageSize).Take(PageSize).ToArray();
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var frame in visible) DrawFrame(frame, Mathf.Max(230, (position.width - 40) / Mathf.Max(1, visible.Length)));
            }
            EditorGUILayout.EndScrollView();
        }
        void StartReview()
        {
            error = null; selectionError = false;
            try
            {
                if (!SimulatorCompatibility.Available(simulator, out var reason)) throw new InvalidOperationException(reason);
                if (audit && (root == null || !root.gameObject.scene.IsValid() || !root.gameObject.activeInHierarchy))
                    throw new InvalidOperationException("Choose an active scene UI root to audit.");
                if (float.IsNaN(tolerance) || float.IsInfinity(tolerance)) throw new InvalidOperationException("Tolerance must be a finite number.");
                var assets = SimulatorDevicePickerPopup.SelectedFavorites(SimulatorTestTools.Devices(simulator));
                matchingFavorites = assets.Length;
                selectionPreferences = SimulatorDevicePickerPopup.CapturePreferenceSnapshot();
                if (assets.Length == 0)
                {
                    selectionError = true;
                    error = NoFavoritesMessage;
                    return;
                }
                StartRun(assets);
            }
            catch (Exception e) { error = e.GetBaseException().Message; }
        }
        // Shared entry point also allows the Editor integration probe to supply explicit devices.
        internal void StartRun(UnityEngine.Object[] assets)
        {
            var jobs = SimulatorTestTools.ReviewJobs(assets, allOrientations);
            if (SimulatorTestTools.Busy) throw new InvalidOperationException("Another Simulator batch is running.");
            string destination = SimulatorCaptureSettings.UniquePath(Path.GetFullPath("Library/EditorCraft/Reviews"), "Review", "");
            var next = new SimulatorReviewReport { createdUtc = DateTime.UtcNow.ToString("O"), status = "Running", audit = audit,
                root = audit ? SimulatorUIAudit.HierarchyPath(root) : "", tolerance = tolerance,
                scope = includeContainers ? "All active RectTransforms" : "Active UI Graphics and Selectables",
                exclusions = exclusions.Where(e => e != null).Select(e => SimulatorUIAudit.HierarchyPath(e)).ToList() };
            var capturedRoot = root;
            var capturedExclusions = exclusions.Where(e => e != null).ToArray();
            bool containers = includeContainers;
            SimulatorTestTools.BeginBatch(simulator, jobs, destination, (owner, path) =>
            {
                var state = SimulatorCompatibility.State(owner);
                var device = SimulatorTestTools.Current(owner);
                var frame = new SimulatorReviewFrame { device = SimulatorTestTools.DeviceName(device),
                    deviceGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(device)),
                    orientation = state.Orientation, width = state.Width, height = state.Height, safeArea = state.SafeArea,
                    image = Path.GetFileName(path), thumbnail = Path.GetFileNameWithoutExtension(path) + "_thumb.png" };
                if (next.audit) SimulatorUIAudit.Inspect(capturedRoot, capturedExclusions, containers, next.tolerance, frame);
                WriteThumbnail(path, Path.Combine(destination, frame.thumbnail));
                next.frames.Add(frame); SaveReport(); Repaint();
            }, message =>
            {
                running = false; showOptions = false; next.status = message == null ? "Complete" : "Incomplete: " + message;
                error = message;
                try { SaveReport(); }
                catch (Exception e) { error = "Could not save report: " + e.Message; }
                Repaint();
            });
            ClearThumbnails(); page = 0; scroll = Vector2.zero; error = null;
            report = next; reportFolder = destination; running = true; SaveReport();
        }
        void SaveReport()
        {
            File.WriteAllText(Path.Combine(reportFolder, "report.json"), JsonUtility.ToJson(report, true));
            string H(string value) => WebUtility.HtmlEncode(value ?? "");
            var html = new StringBuilder("<!doctype html><meta charset='utf-8'><title>Device UI review</title><style>body{background:#20242b;color:#eef0f4;font:14px sans-serif;margin:24px}main{display:flex;flex-wrap:wrap;gap:20px}article{width:320px;background:#303640;padding:16px;border-radius:8px}img{max-width:100%;height:340px;object-fit:contain}li{margin:8px 0;overflow-wrap:anywhere}.warn{color:#ffc77b}</style><h1>Device UI review</h1>");
            html.Append("<p>").Append(H(report.status)).Append(" · ").Append(H(report.createdUtc)).Append("</p><p>").Append(H(report.limitations)).Append("</p>");
            if (report.audit) html.Append("<p>Root: ").Append(H(report.root)).Append(" · ").Append(H(report.scope)).Append(" · tolerance: ").Append(report.tolerance.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(" px</p><p>Exclusions: ").Append(H(string.Join(", ", report.exclusions))).Append("</p>");
            html.Append("<main>");
            foreach (var frame in report.frames)
            {
                html.Append("<article><h2>").Append(H(frame.device)).Append("</h2><p>").Append(frame.orientation).Append(" · ").Append(frame.width).Append(" × ").Append(frame.height).Append("</p><a href='").Append(H(frame.image)).Append("'><img alt='Captured device screen' src='").Append(H(frame.image)).Append("'></a>");
                if (report.audit)
                {
                    html.Append("<p>").Append(frame.checkedCount).Append(" checked · ").Append(frame.issues.Count(i => !i.skipped)).Append(" warnings · ").Append(frame.skippedCount).Append(" skipped</p><ul>");
                    if (frame.checkedCount == 0) html.Append("<li class='warn'>No supported UI was checked.</li>");
                    foreach (var item in frame.issues)
                    {
                        html.Append("<li class='warn'>").Append(H(item.path)).Append(": ");
                        if (item.skipped) html.Append("Skipped — ").Append(H(item.message));
                        else
                        {
                            if (item.safeOverflow != Vector4.zero) html.Append("Safe area: ").Append(H(Distances(item.safeOverflow))).Append("; ");
                            if (item.screenOverflow != Vector4.zero) html.Append("Screen: ").Append(H(Distances(item.screenOverflow)));
                        }
                        html.Append("</li>");
                    }
                    html.Append("</ul>");
                }
                html.Append("</article>");
            }
            File.WriteAllText(Path.Combine(reportFolder, "index.html"), html.Append("</main>").ToString());
        }
        static void WriteThumbnail(string input, string output)
        {
            var source = new Texture2D(2, 2);
            Texture2D thumbnail = null; RenderTexture temporary = null;
            var previous = RenderTexture.active;
            try
            {
                if (!source.LoadImage(File.ReadAllBytes(input))) throw new IOException("Could not read captured PNG.");
                float scale = Mathf.Min(1, 512f / Mathf.Max(source.width, source.height));
                int width = Mathf.Max(1, Mathf.RoundToInt(source.width * scale)), height = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));
                temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(source, temporary); RenderTexture.active = temporary;
                thumbnail = new Texture2D(width, height, TextureFormat.RGB24, false);
                thumbnail.ReadPixels(new Rect(0, 0, width, height), 0, 0); thumbnail.Apply();
                File.WriteAllBytes(output, thumbnail.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                if (temporary != null) RenderTexture.ReleaseTemporary(temporary);
                if (thumbnail != null) DestroyImmediate(thumbnail);
                DestroyImmediate(source);
            }
        }
        Texture2D Thumbnail(SimulatorReviewFrame frame)
        {
            string path = Path.Combine(reportFolder, frame.thumbnail);
            if (thumbnails.TryGetValue(path, out var texture)) return texture;
            if (!File.Exists(path)) return null;
            texture = new Texture2D(2, 2) { hideFlags = HideFlags.HideAndDontSave };
            if (!texture.LoadImage(File.ReadAllBytes(path))) { DestroyImmediate(texture); return null; }
            thumbnails.Add(path, texture); return texture;
        }
        void DrawFrame(SimulatorReviewFrame frame, float width)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(width)))
            {
                GUILayout.Label(frame.device, EditorStyles.boldLabel);
                GUILayout.Label($"{frame.orientation} · {frame.width} × {frame.height}", EditorStyles.miniLabel);
                var texture = Thumbnail(frame);
                Rect slot = GUILayoutUtility.GetRect(width - 12, 300);
                float scale = Mathf.Min(slot.width / frame.width, slot.height / frame.height);
                var imageRect = new Rect(slot.center.x - frame.width * scale / 2, slot.y, frame.width * scale, frame.height * scale);
                if (texture != null) GUI.DrawTexture(imageRect, texture, ScaleMode.StretchToFill);
                Rect Map(Rect r) => new Rect(imageRect.x + r.x * scale, imageRect.y + (frame.height - r.yMax) * scale, r.width * scale, r.height * scale);
                if (Event.current.type == EventType.Repaint && report.audit)
                {
                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(Map(frame.safeArea), Color.clear, Color.green);
                    foreach (var issue in frame.issues.Where(i => !i.skipped))
                        Handles.DrawSolidRectangleWithOutline(Map(issue.bounds), new Color(1, .5f, 0, .06f), new Color(1, .6f, 0));
                    Handles.EndGUI();
                }
                using (new EditorGUI.DisabledScope(SimulatorTestTools.Busy || simulator == null))
                    if (GUILayout.Button("Open device in Simulator")) Navigate(frame, null);
                if (!report.audit) return;
                int warnings = frame.issues.Count(i => !i.skipped);
                GUILayout.Label($"{frame.checkedCount} checked · {warnings} warnings · {frame.skippedCount} skipped", EditorStyles.wordWrappedMiniLabel);
                if (frame.checkedCount == 0) EditorGUILayout.HelpBox("No supported UI was checked. Review the root, exclusions and Canvas configuration.", MessageType.Warning);
                foreach (var item in frame.issues)
                {
                    GUILayout.Space(5);
                    using (new EditorGUI.DisabledScope(SimulatorTestTools.Busy || simulator == null))
                        if (GUILayout.Button(new GUIContent(item.path, item.path), EditorStyles.linkLabel)) Navigate(frame, item);
                    if (item.skipped) GUILayout.Label("Skipped: " + item.message, EditorStyles.wordWrappedMiniLabel);
                    else
                    {
                        if (item.safeOverflow != Vector4.zero) GUILayout.Label("Safe area: " + Distances(item.safeOverflow), EditorStyles.wordWrappedMiniLabel);
                        if (item.screenOverflow != Vector4.zero) GUILayout.Label("Screen: " + Distances(item.screenOverflow), EditorStyles.wordWrappedMiniLabel);
                    }
                }
            }
        }
        static string Distances(Vector4 d) => $"L {d.x:0.##}  T {d.y:0.##}  R {d.z:0.##}  B {d.w:0.##} px";
        void Navigate(SimulatorReviewFrame frame, SimulatorAuditItem item)
        {
            try
            {
                var device = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(frame.deviceGuid));
                if (device == null) throw new InvalidOperationException("This captured device profile is no longer available.");
                SimulatorTestTools.Select(simulator, device);
                SimulatorCompatibility.RestoreOrientation(simulator, frame.orientation); simulator.Focus();
                if (item == null) return;
                UnityEngine.Object target = item.target;
                if (target == null && GlobalObjectId.TryParse(item.objectId, out var id)) target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                if (target == null) throw new InvalidOperationException("The captured UI object no longer exists in the loaded scene.");
                Selection.activeObject = target; EditorGUIUtility.PingObject(target);
            }
            catch (Exception e) { error = e.GetBaseException().Message; }
        }
        internal void ExportTo(string destination)
        {
            if (running || report == null) throw new InvalidOperationException("Finish or cancel the batch before exporting.");
            Directory.CreateDirectory(destination);
            foreach (var name in report.frames.SelectMany(f => new[] { f.image, f.thumbnail }).Concat(new[] { "report.json", "index.html" }))
                File.Copy(Path.Combine(reportFolder, name), Path.Combine(destination, name));
        }
        void Export()
        {
            string parent = EditorUtility.OpenFolderPanel("Export UI report and comparison images", "", "");
            if (string.IsNullOrEmpty(parent)) return;
            try
            {
                string destination = SimulatorCaptureSettings.UniquePath(parent, "DeviceReview", "");
                ExportTo(destination);
                EditorUtility.RevealInFinder(destination);
            }
            catch (Exception e) { error = "Export failed: " + e.Message; }
        }
    }
}
