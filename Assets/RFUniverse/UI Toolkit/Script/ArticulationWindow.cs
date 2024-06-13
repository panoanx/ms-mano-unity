using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RFUniverse
{
    public class ArticulationWindow : VisualElement
    {
        ScrollView articulationView;
        VisualTreeAsset articulationItem;
        public new class UxmlFactory : UxmlFactory<ArticulationWindow> { }

        public ArticulationWindow()
        {
            Resources.Load<VisualTreeAsset>("articulation-window").CloneTree(this);
            articulationView = this.Q<ScrollView>("articulation-view");
            articulationItem = Resources.Load<VisualTreeAsset>("articulation-item");
        }
        Dictionary<ArticulationBody, Slider> currentBodys = new();
        public void Refresh(List<ArticulationBody> bodys)
        {
            articulationView.Clear();
            currentBodys.Clear();
            foreach (var item in bodys)
            {
                TemplateContainer oneArticulationItem = articulationItem.Instantiate();
                articulationView.Add(oneArticulationItem);
                Label label = oneArticulationItem.Q<Label>("articulation-name");
                label.text = item.name;
                Slider slider = oneArticulationItem.Q<Slider>("value-slider");
                currentBodys.Add(item, slider);
                Label min = oneArticulationItem.Q<Label>("min-value");
                Label max = oneArticulationItem.Q<Label>("max-value");
                switch (item.linearLockX)
                {
                    case ArticulationDofLock.LimitedMotion:
                        slider.lowValue = item.xDrive.lowerLimit;
                        min.text = item.xDrive.lowerLimit.ToString("f2");
                        slider.highValue = item.xDrive.upperLimit;
                        max.text = item.xDrive.upperLimit.ToString("f2");
                        break;
                    case ArticulationDofLock.FreeMotion:
                        slider.lowValue = -999;
                        min.text = "-999";
                        slider.highValue = 999;
                        max.text = "999";
                        break;
                }

                slider.SetValueWithoutNotify(item.xDrive.target);
                slider.RegisterValueChangedCallback((f) =>
                {
                    item.GetUnit().SetJointPosition(f.newValue);
                });
            }
        }

        void Update()
        {
        }
    }
}
