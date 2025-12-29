using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace Library
{
    public sealed partial class ResourceHandler : BaseHandler
    {
        private bool IsInitialize;

        public async UniTask InitializeAddressable()
        {
            if (IsInitialize)
            {
                DebugUtil.LogError("ResourceHandler의 어드레서블 리소스의 초기화는 이미 진행되었습니다.");
                return;
            }
            
            await LoadSpriteAtlasAsync();

            // await LoadBigTextureAsync();

            IsInitialize = true;
            
            DebugUtil.Log("[App Initialize] ResourceHandler 초기화");
        }

        public override void OnShutdown()
        {
            if (!IsInitialize)
            {
                return;
            }
            
            ClearCache();
            
            ClearPopupCache();

            ClearCacheScriptable();

            ClearSpriteLibraryCache();

            // 6. Sprite 및 Atlas 정리
            _sprites.Clear();
            foreach (var atlas in _atlases)
            {
                if (atlas != null)
                {
                    Addressables.Release(atlas);
                }
            }
            _atlases.Clear();

            // 7. BigTexture 정리
            foreach (var texture in _textures)
            {
                if (texture.Value != null)
                {
                    Addressables.Release(texture.Value);
                }
            }
            _textures.Clear();

            // 8. Mesh 정리
            foreach (var mesh in _meshes)
            {
                if (mesh.Value != null)
                {
                    Addressables.Release(mesh.Value);
                }
            }
            _meshes.Clear();

            // 9. Material 정리
            foreach (var material in _materials)
            {
                if (material.Value != null)
                {
                    Addressables.Release(material.Value);
                }
            }
            _materials.Clear();

            // 10. Shader 정리 (Shader.Find로 로드한 것은 Release 불필요)
            _shaders.Clear();

            IsInitialize = false;
        }
    }
}
