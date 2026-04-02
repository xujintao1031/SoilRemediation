using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GasChromatography.UI.DragDrop
{
    public class UIParentSlot : MonoBehaviour, IDropHandler
    {
        public UIDragItem targetDragItem;
        public Action<string, string> OnItemDropped;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                var dragObject = eventData.pointerDrag;
                var originalParent = dragObject.GetComponent<RectTransform>().parent;
                if (originalParent == transform) return;


                var dragItem = dragObject.GetComponent<UIDragItem>();
                if (dragItem != null && targetDragItem != dragItem) return;
                var image = dragObject.GetComponent<Image>();
                var parent = transform.parent;
                var firstOrDefault = dragObject.GetComponentsInParent<Transform>().FirstOrDefault(x=>x.name == image.sprite.name);
                dragObject.transform.SetParent(transform);
                var rt = dragObject.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                dragObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                dragItem.IsComplete = true;
                if (firstOrDefault != null) firstOrDefault.gameObject.SetActive(false);
                OnItemDropped?.Invoke(parent.name, image.sprite.name);
            }
        }
    }
}