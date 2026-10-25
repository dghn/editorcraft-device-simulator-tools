// Copy into Editor/ in a temporary package copy; run only in an isolated project.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace DeviceSimulatorTools.Editor
{
    public static class FavoriteSmoke
    {
        const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic;
        const string Prefix = "DeviceSimulatorTools.SimulatorPicker.";
        static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        public static void Run()
        {
            var keys = new[] { "Favorites", "Search", "Platform", "Kind", "FavoritesOnly" };
            var had = keys.ToDictionary(k => k, k => EditorPrefs.HasKey(Prefix + k));
            var favorite = EditorPrefs.GetString(Prefix + "Favorites", "[]");
            var search = EditorPrefs.GetString(Prefix + "Search", "");
            int platform = EditorPrefs.GetInt(Prefix + "Platform"), kind = EditorPrefs.GetInt(Prefix + "Kind");
            bool only = EditorPrefs.GetBool(Prefix + "FavoritesOnly");
            SimulatorDevicePickerPopup popup = null;
            SimulatorReviewWindow review = null;
            var result = new JObject { ["unity"] = Application.unityVersion, ["passed"] = false };
            try
            {
                var simulatorType = TypeCache.GetTypesDerivedFrom<EditorWindow>().First(t => t.FullName == "UnityEditor.DeviceSimulation.SimulatorWindow");
                var simulator = EditorWindow.GetWindow(simulatorType);
                var assets = SimulatorTestTools.Devices(simulator);
                EditorPrefs.SetString(Prefix + "Favorites", "[]"); EditorPrefs.SetString(Prefix + "Search", "");
                EditorPrefs.SetInt(Prefix + "Platform", 0); EditorPrefs.SetInt(Prefix + "Kind", 0);
                EditorPrefs.SetBool(Prefix + "FavoritesOnly", true);
                SimulatorReviewWindow.Open(simulator, false);
                review = Resources.FindObjectsOfTypeAll<SimulatorReviewWindow>().First();
                typeof(SimulatorReviewWindow).GetMethod("StartReview", F).Invoke(review, null);
                Require(!string.IsNullOrEmpty((string)typeof(SimulatorReviewWindow).GetField("error", F).GetValue(review)), "Empty favorites should report a warning");
                popup = ScriptableObject.CreateInstance<SimulatorDevicePickerPopup>();
                popup.Initialize(simulator); popup.CreateGUI();
                var rows = (System.Collections.Generic.List<SimulatorDevicePickerPopup.Device>)typeof(SimulatorDevicePickerPopup).GetField("devices", F).GetValue(popup);
                var names = new[] { "Apple iPod Touch 7th Gen", "Asus ROG Phone", "Google Pixel 10 Pro", "Google Pixel 2", "iPhone 13" };
                foreach (string name in names)
                {
                    var device = rows.First(d => d.Name == name);
                    typeof(SimulatorDevicePickerPopup).GetMethod("ToggleFavorite", F).Invoke(popup, new object[] { device });
                }
                typeof(SimulatorDevicePickerPopup).GetMethod("Refresh", F).Invoke(popup, null);
                var visible = (System.Collections.Generic.List<SimulatorDevicePickerPopup.Device>)typeof(SimulatorDevicePickerPopup).GetField("visible", F).GetValue(popup);
                var selected = SimulatorDevicePickerPopup.SelectedFavorites(assets);
                result["pickerFavorites"] = visible.Count; result["batchFavorites"] = selected.Length;

                typeof(SimulatorReviewWindow).GetMethod("OnInspectorUpdate", F).Invoke(review, null);
                Require((int)typeof(SimulatorReviewWindow).GetField("matchingFavorites", F).GetValue(review)==5, "Gallery live favorite count");
                Require(string.IsNullOrEmpty((string)typeof(SimulatorReviewWindow).GetField("error", F).GetValue(review)), "Old no-match warning must clear after starring devices");
                // Unity hot reload can retain an old error string from a version without the flag.
                typeof(SimulatorReviewWindow).GetField("error", F).SetValue(review, "No favorites match the current picker filters. Star devices or change the search/platform/type filters.");
                typeof(SimulatorReviewWindow).GetField("selectionError", F).SetValue(review, false);
                typeof(SimulatorReviewWindow).GetField("selectionPreferences", F).SetValue(review, null);
                typeof(SimulatorReviewWindow).GetMethod("OnInspectorUpdate", F).Invoke(review, null);
                Require(string.IsNullOrEmpty((string)typeof(SimulatorReviewWindow).GetField("error", F).GetValue(review)), "Legacy warning after hot reload");
                Require(visible.Count == 5, "Picker should show all five starred devices");
                Require(selected.Length == 5, "Picker shows five favorites but batch found " + selected.Length);
                Require(selected.Select(SimulatorTestTools.DeviceName).OrderBy(n => n).SequenceEqual(names.OrderBy(n => n)), "Batch identities differ from picker");
                EditorPrefs.SetString(Prefix + "Search", "PIXEL 10");
                Require(SimulatorDevicePickerPopup.SelectedFavorites(assets).Length == 1, "Multi-word case-insensitive search");
                EditorPrefs.SetInt(Prefix + "Platform", 1);
                Require(SimulatorDevicePickerPopup.SelectedFavorites(assets).Length == 0, "iOS filter excludes Pixel");
                EditorPrefs.SetString(Prefix + "Search", "");
                Require(SimulatorDevicePickerPopup.SelectedFavorites(assets).Length == 2, "iOS favorites");
                EditorPrefs.SetInt(Prefix + "Platform", 0); EditorPrefs.SetInt(Prefix + "Kind", 3);
                Require(SimulatorDevicePickerPopup.SelectedFavorites(assets).Length == 0, "Foldable filter excludes phone favorites");
                EditorPrefs.SetInt(Prefix + "Kind", 0); EditorPrefs.SetBool(Prefix + "FavoritesOnly", false);
                Require(SimulatorDevicePickerPopup.SelectedFavorites(assets).Length == 5, "Batch remains restricted to favorites with toggle off");
                EditorPrefs.SetString(Prefix + "Favorites", "[]");
                Require(SimulatorDevicePickerPopup.SelectedFavorites(assets).Length == 0, "Empty favorites");
                result["passed"] = true;
                result["checks"] = new JArray("actual picker star handler -> saved preferences -> batch selection", "five screenshot devices", "search/platform/type filters", "favorites toggle off", "empty favorites", "stale no-match warning clears and count refreshes");
            }
            catch (Exception e) { result["error"] = e.GetBaseException().Message; }
            finally
            {
                if (review != null) review.Close();
                if (popup != null) UnityEngine.Object.DestroyImmediate(popup);
                EditorPrefs.SetString(Prefix + "Favorites", favorite); EditorPrefs.SetString(Prefix + "Search", search);
                EditorPrefs.SetInt(Prefix + "Platform", platform); EditorPrefs.SetInt(Prefix + "Kind", kind);
                EditorPrefs.SetBool(Prefix + "FavoritesOnly", only);
                foreach (var key in keys.Where(k => !had[k])) EditorPrefs.DeleteKey(Prefix + key);
            }
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "editorcraft-favorites-" + Application.unityVersion + ".json"), result.ToString());
            EditorApplication.Exit((bool)result["passed"] ? 0 : 1);
        }
    }
}
