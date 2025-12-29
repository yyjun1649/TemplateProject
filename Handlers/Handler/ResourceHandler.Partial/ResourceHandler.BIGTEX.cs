using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;

namespace Library
{
    public sealed partial class ResourceHandler
    {
        #region public

        /// <summary>
        /// 런타임 SpriteAtlas에 캐싱된 Sprite 꺼내오기.
        /// </summary>
        /// <param name="textureName"> 큰사이즈 텍스쳐 이름 </param>
        /// <returns> [Nullable] Texture 객체 </returns>
        public Texture GetBigTexture(string textureName)
        {
            if (_textures.TryGetValue(textureName, out Texture texture))
            {
                return texture;
            }

            return null;
        }

        #endregion

        #region private

        /// <summary>
        /// 큰 사이즈 Texcute 캐싱
        /// </summary>
        private readonly Dictionary<string, Texture> _textures = new();

        /// <summary>
        /// Sprite Atlas 에셋 로드
        /// </summary>
        private async UniTask LoadBigTextureAsync()
        {
            _textures.Clear();
            IList<IResourceLocation> list = await Addressables.LoadResourceLocationsAsync(eAddressableLabel.BIGTEXTURE.ToString());

            for (var index = 0; index < list.Count; index++)
            {
                IResourceLocation location = list[index];
                var bigTexture = await Addressables.LoadAssetAsync<Texture>(location);
                if (bigTexture != null)
                {
                    _textures.TryAdd(bigTexture.name, bigTexture);
                }
            }

            Addressables.Release(list);
        }

        #endregion
    }
}
