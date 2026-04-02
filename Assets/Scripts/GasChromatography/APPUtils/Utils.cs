using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GasChromatography.APPUtils
{
    public static class Utils
    {
        public static bool IsPointerInsideTarget(Vector2 screenPos, RectTransform target)
        {
            if (target == null) return false;

            var canvas = target.GetComponentInParent<Canvas>();
            var cam = Camera.main;
            return RectTransformUtility.RectangleContainsScreenPoint(target, screenPos, cam);
        }

        public static void AddEventTriggerListener(GameObject obj, EventTriggerType type,
            UnityAction<BaseEventData> callback)
        {
            var trigger = obj.GetComponent<EventTrigger>();

            if (trigger == null)
                trigger = obj.AddComponent<EventTrigger>();

            var entry = new EventTrigger.Entry
            {
                eventID = type
            };
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);
        }
    }
}