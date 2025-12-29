using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Library
{
    public sealed partial class ResourceHandler
    {
        private readonly Dictionary<string, PopupBase> _cachePopups = new();

        // 동기 메서드 - 일반 사용
        public PopupBase GetPopup(string popupName, out bool isNew)
        {
            isNew = false;

            if (!_cachePopups.TryGetValue(popupName, out var popup))
            {
                isNew = true;
                popup = Instantiate(LoadPopupSync(popupName));
                _cachePopups.Add(popupName, popup);
            }

            return popup;
        }

        // 비동기 메서드 - 프리로딩용
        public async UniTask<(PopupBase popup, bool isNew)> GetPopupAsync(string popupName)
        {
            bool isNew = false;

            if (!_cachePopups.TryGetValue(popupName, out var popup))
            {
                isNew = true;
                var prefabPopup = await LoadPopupAsync(popupName);
                popup = Instantiate(prefabPopup);
                _cachePopups.Add(popupName, popup);
            }

            return (popup, isNew);
        }

        public void ClearPopupCache()
        {
            foreach (var preafab in _cachePrefabs.Cache)
            {
                Addressables.Release(preafab.Value);
            }

            _cachePopups.Clear();
        }

        // 동기 로드
        private PopupBase LoadPopupSync(string popupName)
        {
            var prefab = Addressables.LoadAssetAsync<GameObject>(popupName).WaitForCompletion();
            return prefab.GetComponent<PopupBase>();
        }

        // 비동기 로드
        private async UniTask<PopupBase> LoadPopupAsync(string popupName)
        {
            var prefab = await Addressables.LoadAssetAsync<GameObject>(popupName).ToUniTask();
            return prefab.GetComponent<PopupBase>();
        }
    }
}