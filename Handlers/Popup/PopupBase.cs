
    using System;
    using System.ComponentModel;
    using Library;
    using Sigtrap.Relays;
    using UnityEngine;

    public abstract class PopupBase : MonoBehaviour ,IPopup, IEvent
    {
        public bool IsOnStack;
        
        protected bool _isActive;
        public bool IsActive => _isActive;

        private Relay<PopupBase> OnShow = new Relay<PopupBase>();

        private Relay<PopupBase> OnHide = new Relay<PopupBase>();
        
        [SerializeField] protected Canvas _canvas;
        
        public void SetAction(Action<PopupBase> onShow, Action<PopupBase>  onHide)
        {
            OnShow.AddListener(onShow);
            OnHide.AddListener(onHide);
        }

        public void SetSortIndex(int index)
        {
            _canvas.sortingOrder = index;
        }

        public virtual void Show()
        {
            _isActive = true;
            
            gameObject.SetActive(true);
            
            OnShow?.Dispatch(this);
            
            Handlers.Event.Subscribe(this);
        }

        public virtual void Hide()
        {
            _isActive = false;
            gameObject.SetActive(false);
            
            OnHide?.Dispatch(this);
            
            Handlers.Event.Unsubscribe(this);
        }

        private void Reset()
        {
#if UNITY_EDITOR
            _canvas = GetComponent<Canvas>();
#endif
        }

        public abstract void HandleEvent(eEventType eventType);
    }
