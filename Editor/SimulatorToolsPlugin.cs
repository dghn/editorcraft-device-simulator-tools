using System;
using System.Linq;
using UnityEditor;
using UnityEditor.DeviceSimulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace DeviceSimulatorTools.Editor
{
    public sealed class SimulatorToolsPlugin : DeviceSimulatorPlugin
    {
        public override string title => "EditorCraft Device Tools";
        VisualElement root, controls;
        Label status;
        Button picker;
        EditorWindow owner;
        IVisualElementScheduledItem refresh;

        public override VisualElement OnCreateUI()
        {
            refresh?.Pause();
            root = new VisualElement { name = "editorcraft-device-tools" };
            status = new Label { name = "simulator-tools-status" };
            status.style.whiteSpace = WhiteSpace.Normal;
            root.Add(status);
            controls = new VisualElement();
            picker = new Button(() => Run(() => SimulatorDevicePicker.Open(owner, picker)))
                { name = "device-simulator-tools-device-picker", text = "Search devices & favorites…" };
            controls.Add(picker);
            root.Add(controls);
            // Scheduling is bound to this plugin's panel; detached panels do not poll.
            refresh = root.schedule.Execute(Refresh).Every(500);
            root.RegisterCallback<AttachToPanelEvent>(_ => Refresh());
            return root;
        }
        void Refresh()
        {
            if (owner == null)
                owner = Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(w => w.rootVisualElement.Contains(root));
            bool available = SimulatorCompatibility.Available(owner, out var reason);
            status.text = reason;
            status.style.display = available ? DisplayStyle.None : DisplayStyle.Flex;
            controls.SetEnabled(available);
            if (!available) return;
            Run(() =>
            {
                picker.text = SimulatorTestTools.DeviceName(SimulatorTestTools.Current(owner)) + " ▾";
                picker.SetEnabled(!SimulatorTestTools.Busy);
                SimulatorTestTools.Attach(owner, controls);
                SimulatorPreviewBackground.Attach(owner);
            });
        }
        void Run(Action action)
        {
            try { action(); }
            catch (Exception e) { status.text = e.GetBaseException().Message; status.style.display = DisplayStyle.Flex; }
        }
        public override void OnDestroy()
        {
            refresh?.Pause();
            try { SimulatorTestTools.CancelFor(owner); }
            finally
            {
                SimulatorPreviewBackground.Detach(owner);
                owner = null;
                root = null;
            }
        }
    }
}
