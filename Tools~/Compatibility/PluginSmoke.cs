// Run in an isolated Editor project with -executeMethod PluginSmoke.Run (no -quit),
// or through Pipeline run_script. Reflection here tests the package's internal boundary.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
public static class PluginSmoke
{
    const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    static Type bridge, tools, picker;
    static EditorWindow window;
    static UnityEngine.Object original;
    static ScreenOrientation orientation;
    static double start;
    static int phase, profiles, rotations, captures;
    static bool exit;
    static GameObject camera;
    static string gallery;
    static UnityEngine.Object batchOriginal;
    static ScreenOrientation batchOrientation;
    static readonly List<string> checks = new List<string>();
    static Type T(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
    static object Call(Type t, string method, params object[] args) => t.GetMethod(method, F).Invoke(null, args);
    static object Field(object o, string name) => o.GetType().GetField(name, F).GetValue(o);
    static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    static UnityEngine.Object[] Devices() => (UnityEngine.Object[])Call(bridge, "Devices", window);
    static string Name(UnityEngine.Object asset) => (string)Call(tools, "DeviceName", asset);
    static string Report => Path.Combine(Path.GetTempPath(), "editorcraft-plugin-" + Application.unityVersion + ".json");
    public static object Main() { Start(false); return "Started; report: " + Report; }
    public static void Run() { Start(true); }
    static void Start(bool shouldExit)
    {
        exit = shouldExit; phase = profiles = rotations = captures = 0; checks.Clear();
        bridge = T("DeviceSimulatorTools.Editor.SimulatorCompatibility");
        tools = T("DeviceSimulatorTools.Editor.SimulatorTestTools"); picker = T("DeviceSimulatorTools.Editor.SimulatorDevicePickerPopup");
        window = EditorWindow.GetWindow(T("UnityEditor.DeviceSimulation.SimulatorWindow")); window.Show(); window.Focus();
        original = (UnityEngine.Object)Call(bridge, "Current", window);
        orientation = (ScreenOrientation)Field(Call(bridge, "State", window), "Orientation");
        if (exit && Camera.allCamerasCount == 0)
        {
            camera = new GameObject("Compatibility test camera");
            var c = camera.AddComponent<Camera>(); c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = new Color(.15f,.35f,.55f);
        }
        start = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
    }
    static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup - start > 90) throw new Exception("Timed out in phase " + phase);
            if (EditorApplication.timeSinceStartup - start < 2) return;
            if (phase == 0)
            {
                var root = window.rootVisualElement.Q("editorcraft-device-tools");
                Require(root != null, "Unity did not discover plugin UI");
                Require(root.Q<Label>("simulator-tools-status").text == "", "Compatibility unavailable: " + root.Q<Label>("simulator-tools-status").text);
                Require(root.Q<Button>("device-simulator-tools-device-picker") != null, "Plugin picker missing");
                Require(root.Q<Button>("simulator-test-tools") != null, "Capture tools missing");
                Require(window.rootVisualElement.Q("device-list-menu").resolvedStyle.display != DisplayStyle.None, "Native selector hidden");
                checks.Add("official plugin discovered; controls present; native selector retained");
                var preferred = (UnityEngine.Object[])Call(picker,"PreferredDevices",new object[]{Devices()});
                Require(preferred.Select(Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == preferred.Length,"Duplicate maintained profiles");
                foreach (var asset in preferred.Where(a => AssetDatabase.GetAssetPath(a).StartsWith("Packages/com.daghandemirci.device-simulator-tools/")))
                {
                    Call(bridge,"Select",window,asset);
                    var spec = (JObject)Call(tools,"Spec",asset); var screen = spec["screens"][0];
                    foreach (var o in screen["orientations"])
                    {
                        int rotation = (int)o["orientation"];
                        Call(bridge,"RestoreOrientation",window,(ScreenOrientation)rotation);
                        var state=Call(bridge,"State",window);var safe=(Rect)Field(state,"SafeArea");var expected=o["safeArea"];
                        bool sideways=rotation==3 || rotation==4;
                        Require((int)Field(state,"Width")== (int)screen[sideways?"height":"width"],Name(asset)+" width");
                        Require((int)Field(state,"Height")== (int)screen[sideways?"width":"height"],Name(asset)+" height");
                        Require(Mathf.Abs(safe.x-(float)expected["x"])<.1f && Mathf.Abs(safe.y-(float)expected["y"])<.1f && Mathf.Abs(safe.width-(float)expected["width"])<.1f && Mathf.Abs(safe.height-(float)expected["height"])<.1f,Name(asset)+" safe area");
                        rotations++;
                    }
                    profiles++;
                }
                Require(profiles==181,"Incomplete package profile coverage: "+profiles);
                checks.Add("all package profiles selected; dimensions and safe areas matched every stored orientation");
                Require((string)Call(picker,"ClassifyKind","OPPO Find N5 Inner Screen")=="Foldable","Foldable filter");
                Require((string)Call(picker,"ClassifyKind","OnePlus Pad 3")=="Tablet","Tablet filter");
                Require((string)Call(picker,"ClassifyPlatform","POCO F7","Android")=="Android","Platform filter");
                TestFavorites();
                var fold = preferred.First(a=>Name(a)=="OPPO Find N5 Inner Screen");
                Call(bridge,"Select",window,fold);
                Require(Name((UnityEngine.Object)Call(tools,"Partner",window))=="OPPO Find N5 Outer Screen","Paired screen switch");
                Call(bridge,"Select",window,preferred.First(a=>Name(a)=="POCO F7"));
                Call(bridge,"RestoreOrientation",window,ScreenOrientation.Portrait);
                var popup = ScriptableObject.CreateInstance(picker) as EditorWindow;
                picker.GetMethod("Initialize",F).Invoke(popup,new object[]{window});
                popup.Show(); if(popup.rootVisualElement.Q("device-search")==null) picker.GetMethod("CreateGUI",F).Invoke(popup,null);
                Require(popup.rootVisualElement.Q("device-search")!=null && popup.rootVisualElement.Q("favorites-only")!=null,"Picker UI");popup.Close();
                Call(T("DeviceSimulatorTools.Editor.SimulatorSafeAreaInspector"),"Open",window);
                var inspector=Resources.FindObjectsOfTypeAll<EditorWindow>().First(w=>w.GetType().Name=="SimulatorSafeAreaInspector");inspector.Close();
                checks.Add("picker UI; favorites/filter preferences; paired displays; safe-area inspector");
                phase=1;start=EditorApplication.timeSinceStartup;window.Focus();window.Repaint();return;
            }
            var rt=Call(bridge,"Preview",window) as RenderTexture;
            var stateNow=Call(bridge,"State",window);
            if(rt==null || rt.width!=(int)Field(stateNow,"Width") || rt.height!=(int)Field(stateNow,"Height")) { window.Repaint();return; }
            if(phase<=4)
            {
                foreach(bool framed in new[]{false,true})
                {
                    var texture=(Texture2D)Call(tools,"Capture",window,framed);
                    Require(texture.width>0 && texture.height>0,"Empty capture");
                    if(!framed) Require(texture.width==rt.width && texture.height==rt.height,"Clean dimensions");
                    Require(texture.EncodeToPNG().Length>100,"Invalid PNG");
                    UnityEngine.Object.DestroyImmediate(texture); captures++;
                }
                if(phase<4) Call(bridge,"RestoreOrientation",window,(ScreenOrientation)(phase+1));
                phase++;start=EditorApplication.timeSinceStartup;window.Repaint();return;
            }
            if (phase == 5)
            {
                checks.Add("clean and framed PNG captured in all four orientations");
                gallery = Path.Combine(Path.GetTempPath(), "editorcraft-gallery-" + Guid.NewGuid().ToString("N"));
                batchOriginal = (UnityEngine.Object)Call(bridge, "Current", window);
                batchOrientation = (ScreenOrientation)Field(Call(bridge, "State", window), "Orientation");
                var assets = new[] { "Redmi 13C", "POCO F7" }.Select(n => Devices().First(a => Name(a) == n)).ToArray();
                Call(tools, "Begin", window, assets, gallery);
                phase = 6; start = EditorApplication.timeSinceStartup; return;
            }
            if ((bool)tools.GetProperty("Busy", F).GetValue(null)) return;
            Require(Directory.GetFiles(gallery, "*.png").Length == 2 && File.Exists(Path.Combine(gallery, "index.html")), "Batch HTML/PNG export");
            Require((UnityEngine.Object)Call(bridge,"Current",window) == batchOriginal, "Batch device restoration");
            Require((ScreenOrientation)Field(Call(bridge,"State",window),"Orientation") == batchOrientation,"Batch orientation restoration");
            Call(tools,"Begin",window,new[]{batchOriginal},gallery+"-cancelled");
            Call(tools,"CancelFor",window);
            Require(!(bool)tools.GetProperty("Busy",F).GetValue(null),"Batch cancel cleanup");
            checks.Add("two-device HTML gallery; device/orientation restored; cancellation cleanup");
            // Teardown must pause the panel schedule without touching native controls.
            var pluginType=T("DeviceSimulatorTools.Editor.SimulatorToolsPlugin");
            var plugin=Activator.CreateInstance(pluginType);
            var ui=(VisualElement)pluginType.GetMethod("OnCreateUI").Invoke(plugin,null);
            window.rootVisualElement.Add(ui);
            pluginType.GetMethod("OnDestroy").Invoke(plugin,null);ui.RemoveFromHierarchy();
            Require(window.rootVisualElement.Q("device-list-menu")!=null,"Native UI lost after teardown");
            checks.Add("plugin teardown leaves native controls intact");
            Finish(null);
        }
        catch(Exception e){Finish(e.GetBaseException().ToString());}
    }
    static void TestFavorites()
    {
        const string key="DeviceSimulatorTools.SimulatorPicker.";
        var names=new[]{"Favorites","Search","Platform","Kind"};
        var had=names.ToDictionary(n=>n,n=>EditorPrefs.HasKey(key+n));
        var savedStrings=new[]{"Favorites","Search"}.ToDictionary(n=>n,n=>EditorPrefs.GetString(key+n));
        var savedInts=new[]{"Platform","Kind"}.ToDictionary(n=>n,n=>EditorPrefs.GetInt(key+n));
        try
        {
            var asset=Devices().First(a=>Name(a)=="POCO F7");
            EditorPrefs.SetString(key+"Favorites",new JArray(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset))).ToString());
            EditorPrefs.SetString(key+"Search","POCO F7");EditorPrefs.SetInt(key+"Platform",2);EditorPrefs.SetInt(key+"Kind",1);
            Require(((UnityEngine.Object[])Call(picker,"SelectedFavorites",new object[]{Devices()})).Length==1,"Favorite match");
            EditorPrefs.SetInt(key+"Platform",1);
            Require(((UnityEngine.Object[])Call(picker,"SelectedFavorites",new object[]{Devices()})).Length==0,"Favorite platform exclusion");
        }
        finally
        {
            foreach(var n in names) if(!had[n])EditorPrefs.DeleteKey(key+n); else if(savedStrings.ContainsKey(n))EditorPrefs.SetString(key+n,savedStrings[n]);else EditorPrefs.SetInt(key+n,savedInts[n]);
        }
    }
    static void Finish(string error)
    {
        EditorApplication.update-=Tick;
        try { if(window!=null && original!=null){Call(bridge,"Select",window,original);Call(bridge,"RestoreOrientation",window,orientation);} }
        catch(Exception e){error=(error??"")+" Restore: "+e.GetBaseException().Message;}
        if(camera!=null)UnityEngine.Object.DestroyImmediate(camera);
        File.WriteAllText(Report,new JObject{["unity"]=Application.unityVersion,["passed"]=error==null,["profiles"]=profiles,["orientationChecks"]=rotations,["pngCaptures"]=captures,["checks"]=new JArray(checks),["error"]=error}.ToString());
        if(error!=null)Debug.LogError(error);
        if(exit)EditorApplication.Exit(error==null?0:1);
    }
}
