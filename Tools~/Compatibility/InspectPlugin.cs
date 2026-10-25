using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
public static class InspectPlugin {
 public static object Main() {
  var t=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.DeviceSimulation.SimulatorWindow")).First(x=>x!=null);
  var w=EditorWindow.GetWindow(t);w.Show();
  var root=w.rootVisualElement.Q("editorcraft-device-tools");
  return new { found=root!=null, status=root?.Q<Label>("simulator-tools-status")?.text, controls=root?.Query<Button>().ToList().Select(b=>b.text).ToArray(), nativePicker=w.rootVisualElement.Q("device-list-menu")?.style.display.ToString() };
 }
}
