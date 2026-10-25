// Integration probe: copy this file into Editor/ of a temporary copy of the package,
// then use -executeMethod DeviceSimulatorTools.Editor.ReviewSmoke.Run (without -quit).
// Run only in an isolated empty project; creates temporary scene objects and windows.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DeviceSimulatorTools.Editor
{
    public static class ReviewSmoke
    {
        const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        static EditorWindow simulator;
        static SimulatorReviewWindow review;
        static GameObject fixture, camera;
        static RectTransform bad, ignored;
        static UnityEngine.Object original;
        static ScreenOrientation orientation;
        static UnityEngine.Object[] devices;
        static int phase;
        static double deadline;
        static string folder, captureFolder, callbackError;
        static readonly System.Collections.Generic.List<string> checks = new System.Collections.Generic.List<string>();
        static object Get(object o, string name) => o.GetType().GetField(name, F).GetValue(o);
        static void Set(object o, string name, object value) => o.GetType().GetField(name, F).SetValue(o, value);
        static void Invoke(object o, string name, params object[] args) => o.GetType().GetMethod(name, F).Invoke(o, args);
        static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        static bool Near(float a, float b) => Mathf.Abs(a - b) < .1f;
        public static void Run()
        {
            try
            {
                var type = TypeCache.GetTypesDerivedFrom<EditorWindow>().First(t => t.FullName == "UnityEditor.DeviceSimulation.SimulatorWindow");
                simulator = EditorWindow.GetWindow(type); simulator.Show(); simulator.Focus();
                original = SimulatorTestTools.Current(simulator); orientation = SimulatorCompatibility.State(simulator).Orientation;
                devices = new[] { "Redmi 13C", "POCO F7" }.Select(n => SimulatorTestTools.Devices(simulator).First(a => SimulatorTestTools.DeviceName(a) == n)).ToArray();
                SimulatorTestTools.Select(simulator, devices[0]); SimulatorCompatibility.RestoreOrientation(simulator, ScreenOrientation.Portrait);
                camera = new GameObject("Review probe camera", typeof(Camera));
                camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
                camera.GetComponent<Camera>().backgroundColor = new Color(.08f, .12f, .2f);
                fixture = new GameObject("Audit fixture", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                fixture.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var good = Child("Inside", fixture.transform, new Vector2(.5f,.5f), Vector2.zero, new Vector2(80,80));
                good.GetComponent<Image>().color = Color.green;
                bad = Child("Outside right", fixture.transform, new Vector2(1,.5f), new Vector2(30,0), new Vector2(100,100));
                bad.GetComponent<Image>().color = Color.red;
                ignored = Child("Excluded background", fixture.transform, Vector2.zero, new Vector2(-100,-100), new Vector2(300,300));
                Child("Excluded child", ignored, Vector2.zero, new Vector2(-200,-200), new Vector2(30,30));
                var hidden = Child("Hidden", fixture.transform, Vector2.one, new Vector2(100,100), Vector2.one * 100);
                hidden.gameObject.AddComponent<CanvasGroup>().alpha = 0;
                var inactive = Child("Inactive", fixture.transform, Vector2.one, Vector2.one * 100, Vector2.one * 100); inactive.gameObject.SetActive(false);
                SimulatorReviewWindow.Open(simulator, true);
                review = Resources.FindObjectsOfTypeAll<SimulatorReviewWindow>().First();
                review.position = new Rect(40,40,1100,800);
                Set(review,"root",fixture.GetComponent<RectTransform>());
                Set(review,"exclusions", new System.Collections.Generic.List<RectTransform> { ignored });
                phase = 0; deadline = EditorApplication.timeSinceStartup + 120;
                EditorApplication.update += Tick;
            }
            catch(Exception e) { Finish(e.ToString()); }
        }
        static RectTransform Child(string name, Transform parent, Vector2 anchor, Vector2 offset, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = obj.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor; rect.sizeDelta = size; rect.anchoredPosition = offset;
            return rect;
        }
        static SimulatorReviewReport Report => (SimulatorReviewReport)Get(review,"report");
        static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Probe timed out in phase " + phase);
                if (phase == 0)
                {
                    Canvas.ForceUpdateCanvases();
                    var s = SimulatorCompatibility.State(simulator);
                    if (!Near(fixture.GetComponent<Canvas>().pixelRect.width,s.Width)) { simulator.Focus(); simulator.Repaint(); return; }
                    var d = SimulatorUIAudit.Overflow(new Rect(-5,-9,120,130), new Rect(0,0,100,100),1);
                    Require(d == new Vector4(5,21,15,9),"Four edge pixel distances");
                    Require(SimulatorUIAudit.Overflow(new Rect(-1,0,101,100),new Rect(0,0,100,100),1)==Vector4.zero,"Tolerance boundary");
                    var frame = new SimulatorReviewFrame { width=s.Width,height=s.Height,safeArea=s.SafeArea };
                    SimulatorUIAudit.Inspect(fixture.GetComponent<RectTransform>(),new[]{ignored},false,1,frame);
                    Require(frame.checkedCount==2 && frame.skippedCount==0,"Active graphic count, alpha and exclusion filtering: "+frame.checkedCount+"/"+frame.skippedCount);
                    Require(frame.issues.Count==1 && frame.issues[0].path.EndsWith("Outside right"),"Expected out-of-bounds object only");
                    Require(Near(frame.issues[0].screenOverflow.z,80),"Right overflow distance");
                    bad.localScale=Vector3.one*2;
                    var scaled=new SimulatorReviewFrame { width=s.Width,height=s.Height,safeArea=s.SafeArea };
                    SimulatorUIAudit.Inspect(fixture.GetComponent<RectTransform>(),new[]{ignored},false,1,scaled);
                    Require(Near(scaled.issues.Single().screenOverflow.z,130),"Scaled UI projects to screen pixels");
                    bad.localScale=Vector3.one;
                    var world = new GameObject("World canvas",typeof(RectTransform),typeof(Canvas));
                    world.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
                    var worldChild=Child("World child",world.transform,Vector2.zero,Vector2.zero,Vector2.one*20);
                    Require(!SimulatorUIAudit.TryBounds(worldChild,out _,out var reason) && reason.Contains("World-space"),"World space must be skipped");
                    UnityEngine.Object.DestroyImmediate(world);
                    Require(SimulatorTestTools.ReviewJobs(devices,true).Length==8,"All stored orientations");
                    checks.Add("bounds math, tolerance, exclusions including descendants, hidden/inactive UI, unsupported Canvas, supported orientation jobs");
                    original=SimulatorTestTools.Current(simulator); orientation=SimulatorCompatibility.State(simulator).Orientation;
                    review.StartRun(devices); folder=(string)Get(review,"reportFolder"); phase=1; return;
                }
                if (phase==1)
                {
                    if (SimulatorTestTools.Busy) return;
                    Require(Report.status=="Complete","Audit failed: "+Report.status);
                    Require(Report.frames.Count==8,"Eight snapshots");
                    foreach(var frame in Report.frames)
                    {
                        Require(frame.checkedCount==2 && frame.skippedCount==0,"Settled UI on "+frame.device+" "+frame.orientation);
                        Require(frame.issues.Count==1 && Near(frame.issues[0].screenOverflow.z,80),"Correct warning on "+frame.device+" "+frame.orientation);
                        Require(File.Exists(Path.Combine(folder,frame.image)) && File.Exists(Path.Combine(folder,frame.thumbnail)),"PNG/thumbnail output");
                        var tex=new Texture2D(2,2); tex.LoadImage(File.ReadAllBytes(Path.Combine(folder,frame.thumbnail)));
                        Require(Mathf.Max(tex.width,tex.height)<=512,"Bounded gallery thumbnails"); UnityEngine.Object.DestroyImmediate(tex);
                    }
                    Require(SimulatorTestTools.Current(simulator)==original && SimulatorCompatibility.State(simulator).Orientation==orientation,"Completion restores original selection");
                    var saved=JsonUtility.FromJson<SimulatorReviewReport>(File.ReadAllText(Path.Combine(folder,"report.json")));
                    Require(saved.frames.Count==8 && saved.frames[0].issues[0].path.EndsWith("Outside right"),"Report serialization");
                    Invoke(review,"Navigate",Report.frames[0],Report.frames[0].issues[0]);
                    Require(Selection.activeTransform==bad,"Issue navigation selects scene object");
                    string exported=Path.Combine(Path.GetTempPath(),"editorcraft-review-export-"+Guid.NewGuid().ToString("N"));
                    review.ExportTo(exported);
                    Require(Directory.GetFiles(exported,"*.png").Length==16 && File.ReadAllText(Path.Combine(exported,"index.html")).Contains("Outside right"),"Export contains report details and images");
                    checks.Add("two-device/four-orientation audit, exact 80px overflow, PNG/thumbnail/JSON output, restored state, issue navigation");
                    review.Focus(); review.Repaint();
                    phase=2; deadline=EditorApplication.timeSinceStartup+120; return;
                }
                if(phase==2)
                {
                    // OnGUI has had an opportunity to load the visible page only.
                    var cache=(System.Collections.IDictionary)Get(review,"thumbnails");
                    Require(cache.Count<=4,"Gallery memory bounded to visible page");
                    Set(review,"allOrientations",false); Set(review,"audit",false);
                    review.StartRun(devices); phase=3; return;
                }
                if(phase==3)
                {
                    if(Report.frames.Count==0 && SimulatorTestTools.Busy)return;
                    Require(Report.frames.Count>0,"At least one completed capture before cancellation");
                    SimulatorTestTools.CancelFor(simulator);
                    Require(!SimulatorTestTools.Busy && Report.status.StartsWith("Incomplete:"),"Cancellation cleanup/status");
                    Require(Report.frames.Count>0 && File.Exists(Path.Combine((string)Get(review,"reportFolder"),Report.frames[0].image)),"Partial results retained");
                    Require(SimulatorTestTools.Current(simulator)==original && SimulatorCompatibility.State(simulator).Orientation==orientation,"Cancellation restores state");
                    checks.Add("comparison-only capture, bounded gallery cache, cancellation and partial retention");
                    review.StartRun(devices); review.Close();
                    Require(!SimulatorTestTools.Busy,"Closing review cancels owned batch");
                    checks.Add("window teardown cancels batch");
                    captureFolder=Path.Combine(Path.GetTempPath(),"editorcraft-legacy-"+Guid.NewGuid().ToString("N"));
                    SimulatorTestTools.Begin(simulator,devices,captureFolder);
                    phase=4;return;
                }
                if(phase==4)
                {
                    if(SimulatorTestTools.Busy)return;
                    Require(Directory.GetFiles(captureFolder,"*.png").Length==2 && File.Exists(Path.Combine(captureFolder,"index.html")),"Legacy PNG/HTML batch regression");
                    checks.Add("existing favorite PNG/HTML capture preserved");
                    SimulatorTestTools.BeginBatch(simulator,SimulatorTestTools.ReviewJobs(new[]{devices[1]},false),captureFolder+"-error",
                        (w,p)=>throw new InvalidOperationException("Intentional probe failure"),message=>callbackError=message);
                    phase=5;return;
                }
                if(phase==5)
                {
                    if(SimulatorTestTools.Busy)return;
                    Require(callbackError=="Intentional probe failure","Callback failures reported");
                    Require(SimulatorTestTools.Current(simulator)==original && SimulatorCompatibility.State(simulator).Orientation==orientation,"Failure restores state");
                    SimulatorTestTools.BeginBatch(simulator,SimulatorTestTools.ReviewJobs(devices,false),captureFolder+"-playmode",null,message=>callbackError=message);
                    typeof(SimulatorTestTools).GetMethod("OnPlayMode",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{PlayModeStateChange.ExitingEditMode});
                    Require(!SimulatorTestTools.Busy && callbackError.Contains("interrupted"),"Play mode transition cancels batch");
                    checks.Add("failure callback cleanup and restoration; play-mode transition interruption");
                    Finish(null);
                }
            }
            catch(Exception e){Finish(e.GetBaseException().ToString());}
        }
        static void Finish(string error)
        {
            EditorApplication.update-=Tick;
            if(simulator!=null)SimulatorTestTools.CancelFor(simulator);
            if(review!=null)review.Close();
            if(fixture!=null)UnityEngine.Object.DestroyImmediate(fixture);
            if(camera!=null)UnityEngine.Object.DestroyImmediate(camera);
            File.WriteAllText(Path.Combine(Path.GetTempPath(),"editorcraft-review-"+Application.unityVersion+".json"),
                new Newtonsoft.Json.Linq.JObject { ["passed"]=error==null,["unity"]=Application.unityVersion,["checks"]=new Newtonsoft.Json.Linq.JArray(checks),["error"]=error,["gallery"]=folder }.ToString());
            if(error!=null)Debug.LogError(error);
            EditorApplication.Exit(error==null?0:1);
        }
    }
}
