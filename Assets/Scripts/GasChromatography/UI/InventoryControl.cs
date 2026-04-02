using System.Collections.Generic;
using System.Linq;
using GasChromatography.UI.DragDrop;
using GasChromatography.UI.Sampling;

namespace GasChromatography.UI
{
    public class InventoryControl : SamplingPopup
    {
        private List<UIParentSlot> _parentSlots = new();

        private void Awake()
        {
            _parentSlots = GetComponentsInChildren<UIParentSlot>().ToList();
            if(_parentSlots.Count > 0)
                _parentSlots.ForEach(x => x.OnItemDropped += OnItemDropped);
        }

        private void OnItemDropped(string arg1, string arg2)
        {
           RoleHandle.Instance.ChuanDaiZhuangBei(arg1, arg2);
        }
        
    }
}