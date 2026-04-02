using System;
using GasChromatography.UI.UIPopup;
using UnityEngine;

namespace GasChromatography.UI.Sampling
{
    [RequireComponent(typeof(UIPopupWindow))]
    public class SamplingPopup : MonoBehaviour
    {
        private UIPopupWindow _window;

        protected virtual void Start()
        {
            _window = GetComponent<UIPopupWindow>();
        }

        public virtual void Show()
        {
            if(!_window) _window = GetComponent<UIPopupWindow>();
            _window.Show();
        }

        public virtual void Hide()
        {
            _window.Hide();
        }

        public virtual void SetCallback(Action show, Action hide)
        {
            _window.OnShowComplete += show;
            _window.OnHideComplete += hide;
        }
    }
}