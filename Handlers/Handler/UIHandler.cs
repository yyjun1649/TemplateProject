using System.Collections.Generic;
using UnityEngine;

namespace Library
{
    public class UIHandler : BaseHandler
    {
        [SerializeField] private Transform _trParent;
        private readonly Stack<PopupBase> popupStack = new Stack<PopupBase>();

        private int _layerIndex = 0;

        public T GetPopup<T>(string popupName = "") where T : PopupBase
        {
            var popupN = string.IsNullOrEmpty(popupName) ? typeof(T).ToString() : popupName;
            
            var popup = Handlers.Resource.GetPopup(popupN, out bool isNew);

            if (isNew)
            {
                var popupTr = popup.transform;
                popupTr.SetParent(_trParent);
                popupTr.localPosition = Vector3.zero;
                popupTr.localScale = Vector3.one;
                popup.SetAction(OnShow, OnHide);
            }

            return (T)popup;
        }

        public PopupBase GetPopup(string popupName)
        {
            var popup = Handlers.Resource.GetPopup(popupName, out bool isNew);

            if (isNew)
            {
                var popupTr = popup.transform;
                popupTr.SetParent(_trParent);
                popupTr.localPosition = Vector3.zero;
                popupTr.localScale = Vector3.one;
                popup.SetAction(OnShow, OnHide);
            }

            return popup;
        }

        public void Show<T>(string popupName = "") where T : PopupBase
        {
            var popupN = string.IsNullOrEmpty(popupName) ? typeof(T).ToString() : popupName;

            var popup = Handlers.Resource.GetPopup(popupN, out bool isNew);

            if (isNew)
            {
                var popupTr = popup.transform;
                popupTr.SetParent(_trParent);
                popupTr.localPosition = Vector3.zero;
                popupTr.localScale = Vector3.one;
                popup.SetAction(OnShow, OnHide);
            }

            popup.Show();
        }

        public void CloseAllPopup()
        {
            while (popupStack.Count > 0)
            {
                var popup = popupStack.Pop();

                popup.Hide();
            }
        }

        private void ClosePopup()
        {
            if (popupStack.Count == 0) return;

            PopupBase popup = popupStack.Pop();
            popup.Hide();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePopup();
            }
        }

        private void OnShow(PopupBase popup)
        {
            _layerIndex++;
            popup.SetSortIndex(_layerIndex);
            if (popup.IsOnStack)
            {
                popupStack.Push(popup);
            }
        }

        private void OnHide(PopupBase popup)
        {
            _layerIndex--;
            if (popup.IsOnStack)
            {
                if (popup == popupStack.Peek())
                {
                    popupStack.Pop();
                }
            }
        }

        public override void OnShutdown()
        {
            // 모든 팝업 닫기
            CloseAllPopup();

            // 레이어 인덱스 초기화
            _layerIndex = 0;
        }
    }
}