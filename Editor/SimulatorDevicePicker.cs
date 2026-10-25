using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DeviceSimulatorTools.Editor
{
    internal static class SimulatorDevicePicker
    {
        internal static void Open(EditorWindow owner, VisualElement anchor)
        {
            var popup = ScriptableObject.CreateInstance<SimulatorDevicePickerPopup>();
            popup.Initialize(owner);
            var rect = anchor.worldBound;
            rect.position = GUIUtility.GUIToScreenPoint(rect.position);
            popup.ShowAsDropDown(rect, new Vector2(320, 420));
        }
    }

    internal sealed class SimulatorDevicePickerPopup : EditorWindow
    {
        internal sealed class Device
        {
            internal UnityEngine.Object Asset;
            internal string Name, Platform, Kind, FavoriteId;
        }
        const string PrefKey = "DeviceSimulatorTools.SimulatorPicker.";
        static readonly string[] Platforms = { "All", "iOS", "Android" };
        static readonly string[] Kinds = { "All", "Phone", "Tablet", "Foldable" };
        readonly List<Device> devices = new();
        List<Device> visible = new();
        readonly HashSet<string> favorites = new(StringComparer.Ordinal);
        bool favoritesOnly;
        EditorWindow owner;
        UnityEngine.Object selected;
        int platform, kind;
        string query = "";
        ListView list;
        Label count, empty;
        ToolbarSearchField search;

        internal void Initialize(EditorWindow simulator)
        {
            owner = simulator;
            var nativeAssets = SimulatorCompatibility.Devices(owner);
            var assets = PreferredDevices(nativeAssets);
            selected = SimulatorCompatibility.Current(owner);
            foreach (var asset in assets)
            {
                devices.Add(ReadDevice(asset));
            }
            try
            {
                foreach (var id in JArray.Parse(EditorPrefs.GetString(PrefKey + "Favorites", "[]")))
                    if (id.Type == JTokenType.String) favorites.Add((string)id);
            }
            catch (Newtonsoft.Json.JsonException) { /* Ignore malformed personal preferences. */ }
            favoritesOnly = EditorPrefs.GetBool(PrefKey + "FavoritesOnly");
            query = EditorPrefs.GetString(PrefKey + "Search", "");
            platform = Mathf.Clamp(EditorPrefs.GetInt(PrefKey + "Platform"), 0, Platforms.Length - 1);
            kind = Mathf.Clamp(EditorPrefs.GetInt(PrefKey + "Kind"), 0, Kinds.Length - 1);
        }

        static Device ReadDevice(UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var json = File.Exists(path) ? JObject.Parse(File.ReadAllText(path)) : JObject.Parse(EditorJsonUtility.ToJson(asset));
            var data = json["deviceInfo"] ?? json;
            var name = (string)data["friendlyName"] ?? asset.name;
            var os = (string)data["systemInfo"]?["operatingSystem"] ?? "";
            return new Device { Asset = asset, Name = name, Platform = ClassifyPlatform(name, os), Kind = ClassifyKind(name), FavoriteId = string.IsNullOrEmpty(path) ? "preset:" + name : AssetDatabase.AssetPathToGUID(path) };
        }

        internal static UnityEngine.Object[] PreferredDevices(IEnumerable<UnityEngine.Object> assets)
        {
            // Keep Unity's installed package intact. Prefer our maintained profile only
            // when its normalized model name matches a bundled profile.
            string Key(Device d)
            {
                string name = Regex.Replace(d.Name, @"^Apple\s+", "", RegexOptions.IgnoreCase).Trim();
                name = name.Replace("th gen)", "th generation)");
                // Legacy names in Unity Devices 1.0.1, including its 10.15 typo.
                return name switch
                {
                    "iPad Pro 10.15" => "iPad Pro (10.5-inch)",
                    "iPad Pro 11" => "iPad Pro (11-inch) (1st generation)",
                    "iPad Pro 12.9 (2018)" => "iPad Pro (12.9-inch) (3rd generation)",
                    _ => name
                };
            }
            return assets.Select(ReadDevice).GroupBy(Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(d => AssetDatabase.GetAssetPath(d.Asset)
                    .StartsWith("Packages/com.daghandemirci.device-simulator-tools/", StringComparison.Ordinal)).First().Asset).ToArray();
        }

        internal static UnityEngine.Object[] SelectedFavorites(IEnumerable<UnityEngine.Object> assets)
        {
            var ids = JArray.Parse(EditorPrefs.GetString(PrefKey + "Favorites", "[]"))
                .Where(id => id.Type == JTokenType.String).Values<string>().ToHashSet(StringComparer.Ordinal);
            string selectedPlatform = Platforms[Mathf.Clamp(EditorPrefs.GetInt(PrefKey + "Platform"), 0, Platforms.Length - 1)];
            string selectedKind = Kinds[Mathf.Clamp(EditorPrefs.GetInt(PrefKey + "Kind"), 0, Kinds.Length - 1)];
            string searchQuery = EditorPrefs.GetString(PrefKey + "Search", "");
            // Always restrict captures to starred devices, even when the Favorites toggle is off.
            return PreferredDevices(assets).Select(ReadDevice)
                .Where(d => ids.Contains(d.FavoriteId) && Matches(d, selectedPlatform, selectedKind, searchQuery))
                .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase).Select(d => d.Asset).ToArray();
        }

        internal static string ClassifyPlatform(string name, string os)
        {
            if (os.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0) return "Android";
            if (Regex.IsMatch(os + " " + name, @"(?i)\bios\b|iphone|ipad|ipod")) return "iOS";
            return "Other"; // Unknown future profiles remain available under All.
        }
        internal static string ClassifyKind(string name)
        {
            if (Regex.IsMatch(name, @"(?i)fold|flip|\bduo\b|\b(inner|outer) screen\b")) return "Foldable";
            if (Regex.IsMatch(name, @"(?i)ipad|tablet|\btab\b|\bpad\b")) return "Tablet";
            return "Phone";
        }
        internal static bool Matches(Device device, string platform, string kind, string query) =>
            (platform == "All" || device.Platform == platform) && (kind == "All" || device.Kind == kind) &&
            query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).All(word => device.Name.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingTop = root.style.paddingBottom = 2;
            root.style.paddingLeft = root.style.paddingRight = 2;
            search = new ToolbarSearchField { name = "device-search" };
            search.style.marginBottom = 2;
            search.SetValueWithoutNotify(query);
            search.style.width = Length.Percent(100);
            search.style.maxWidth = StyleKeyword.None;
            search.RegisterValueChangedCallback(evt => { query = evt.newValue; EditorPrefs.SetString(PrefKey + "Search", query); Refresh(); });
            root.Add(search);
            AddFilters(Platforms, platform, "platform-filters", value => { platform = value; EditorPrefs.SetInt(PrefKey + "Platform", value); Refresh(); });
            AddFilters(Kinds, kind, "kind-filters", value => { kind = value; EditorPrefs.SetInt(PrefKey + "Kind", value); Refresh(); });
            var favoritesToggle = new ToolbarToggle { name = "favorites-only", text = "★ Favorites", value = favoritesOnly };
            favoritesToggle.tooltip = "Show starred devices within the current filters";
            favoritesToggle.RegisterValueChangedCallback(evt =>
            {
                favoritesOnly = evt.newValue;
                EditorPrefs.SetBool(PrefKey + "FavoritesOnly", favoritesOnly);
                Refresh();
            });
            var summary = new VisualElement { style = { flexDirection = FlexDirection.Row, height = 20 } };
            summary.Add(favoritesToggle);
            root.Add(summary);
            count = new Label { name = "device-count" };
            count.style.flexGrow = 1;
            count.style.unityTextAlign = TextAnchor.MiddleRight;
            count.style.fontSize = 11;
            count.style.marginRight = 4;
            count.style.color = new Color(.65f, .65f, .65f);
            summary.Add(count);
            list = new ListView { name = "filtered-devices", fixedItemHeight = 20, selectionType = SelectionType.Single };
            list.style.flexGrow = 1;
            list.makeItem = () =>
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                var star = new Button { name = "favorite-star", style = { width = 20, flexShrink = 0, fontSize = 13, paddingLeft = 0, paddingRight = 0, marginTop = 0, marginBottom = 0, marginLeft = 0, marginRight = 0, borderTopWidth = 0, borderBottomWidth = 0, borderLeftWidth = 0, borderRightWidth = 0, backgroundColor = Color.clear } };
                star.clicked += () => ToggleFavorite((Device)row.userData);
                row.Add(star);
                row.Add(new Label { name = "device-name", style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 0 } });
                return row;
            };
            list.bindItem = (element, index) =>
            {
                var device = visible[index];
                element.userData = device;
                element.Q<Label>("device-name").text = (device.Asset == selected ? "✓  " : "    ") + device.Name;
                var star = element.Q<Button>("favorite-star");
                bool favorite = favorites.Contains(device.FavoriteId);
                star.text = favorite ? "★" : "☆";
                star.tooltip = favorite ? "Remove from favorites" : "Add to favorites";
                star.style.color = favorite ? new Color(1f, .77f, .25f) : StyleKeyword.Null;
                element.tooltip = device.Name;
            };
            // One click, as with the native selector. Arrow keys only navigate; Enter selects.
            list.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (evt.button != 0) return;
                var label = evt.target as VisualElement;
                while (label != null && label != list)
                {
                    if (label is Button) return;
                    label = label.parent;
                }
                if (evt.target is Label && list.selectedIndex >= 0) SelectCurrent();
            });
            root.Add(list);
            empty = new Label("No matching devices. Change filters or clear the search.") { name = "empty-devices" };
            empty.style.whiteSpace = WhiteSpace.Normal;
            root.Add(empty);
            // TextField consumes Enter before bubbling; handle picker shortcuts on the way down.
            root.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape) { Close(); evt.StopPropagation(); }
                else if ((evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) && evt.target is not Button) { SelectCurrent(); evt.StopPropagation(); }
                else if (evt.keyCode == KeyCode.DownArrow && evt.target != list && !list.Contains(evt.target as VisualElement))
                {
                    if (visible.Count > 0) { list.selectedIndex = 0; list.Focus(); }
                    evt.StopPropagation();
                }
            }, TrickleDown.TrickleDown);
            Refresh();
            search.schedule.Execute(() =>
            {
                search.Focus();
                if (list.selectedIndex >= 0) list.ScrollToItem(list.selectedIndex);
            });
        }

        void AddFilters(string[] names, int active, string name, Action<int> changed)
        {
            var row = new VisualElement { name = name, style = { flexDirection = FlexDirection.Row, marginBottom = 1, height = 18 } };
            var toggles = new List<ToolbarToggle>();
            for (int i = 0; i < names.Length; i++)
            {
                int value = i;
                var toggle = new ToolbarToggle { text = names[i], value = i == active };
                toggle.style.flexGrow = 1;
                toggle.style.unityTextAlign = TextAnchor.MiddleCenter;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    foreach (var other in toggles) other.SetValueWithoutNotify(other == toggle);
                    changed(value);
                });
                toggles.Add(toggle);
                row.Add(toggle);
            }
            rootVisualElement.Add(row);
        }

        void ToggleFavorite(Device device)
        {
            if (device == null) return;
            if (!favorites.Remove(device.FavoriteId)) favorites.Add(device.FavoriteId);
            EditorPrefs.SetString(PrefKey + "Favorites", new JArray(favorites.OrderBy(id => id)).ToString(Newtonsoft.Json.Formatting.None));
            // Defer recycling the clicked button until its event has finished dispatching.
            rootVisualElement.schedule.Execute(Refresh);
        }

        void Refresh()
        {
            if (list == null) return;
            visible = devices.Where(d => Matches(d, Platforms[platform], Kinds[kind], query) && (!favoritesOnly || favorites.Contains(d.FavoriteId))).OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase).ToList();
            list.itemsSource = visible;
            list.RefreshItems();
            int selectedIndex = visible.FindIndex(d => d.Asset == selected);
            if (selectedIndex < 0 && query.Length > 0 && visible.Count > 0) selectedIndex = 0;
            list.SetSelectionWithoutNotify(selectedIndex < 0 ? Array.Empty<int>() : new[] { selectedIndex });
            count.text = $"{visible.Count} / {devices.Count} devices";
            empty.text = favoritesOnly ? "No matching favorites. Turn off Favorites and use ☆ to star devices." : "No matching devices. Change filters or clear the search.";
            list.style.display = visible.Count == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            empty.style.display = visible.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void SelectCurrent()
        {
            if (list.selectedIndex < 0 || list.selectedIndex >= visible.Count || owner == null) return;
            // Resolve by asset again in case an import changed the native array while open.
            var asset = visible[list.selectedIndex].Asset;
            if (SimulatorTestTools.Busy) return;
            try { SimulatorCompatibility.Select(owner, asset); }
            catch (Exception e) { Debug.LogWarning("Simulator selection: " + e.GetBaseException().Message); }
            Close();
        }
    }
}
