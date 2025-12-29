using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Library
{
    public sealed partial class ResourceHandler
    {
        #region Public Methods

        /// <summary>
        /// 캐싱된 Mesh 가져오기.
        /// </summary>
        /// <param name="meshName"> Mesh 이름 </param>
        /// <returns> [Nullable] Mesh 객체 </returns>
        public Mesh GetMesh(string meshName)
        {
            if (_meshes.TryGetValue(meshName, out Mesh mesh))
            {
                return mesh;
            }

            return null;
        }

        /// <summary>
        /// 캐싱된 Material 가져오기.
        /// </summary>
        /// <param name="materialName"> Material 이름 </param>
        /// <returns> [Nullable] Material 객체 </returns>
        public Material GetMaterial(string materialName)
        {
            if (_materials.TryGetValue(materialName, out Material material))
            {
                return material;
            }

            return null;
        }

        /// <summary>
        /// 캐싱된 Shader 가져오기.
        /// </summary>
        /// <param name="shaderName"> Shader 이름 </param>
        /// <returns> [Nullable] Shader 객체 </returns>
        public Shader GetShader(string shaderName)
        {
            if (_shaders.TryGetValue(shaderName, out Shader shader))
            {
                return shader;
            }

            return null;
        }

        #endregion

        #region Private Fields

        // Mesh, Material, Shader 캐싱 딕셔너리
        private readonly Dictionary<string, Mesh> _meshes = new();
        private readonly Dictionary<string, Material> _materials = new();
        private readonly Dictionary<string, Shader> _shaders = new();

        #endregion

        #region Addressable Load Methods

        /// <summary>
        /// Mesh 에셋 로드
        /// </summary>
        private async UniTask LoadMeshesAsync()
        {
            _meshes.Clear();
            IList<IResourceLocation> list = await Addressables.LoadResourceLocationsAsync(eAddressableLabel.MESH.ToString());

            for (var index = 0; index < list.Count; index++)
            {
                IResourceLocation location = list[index];
                var mesh = await Addressables.LoadAssetAsync<Mesh>(location);
                if (mesh != null)
                {
                    _meshes.TryAdd(location.PrimaryKey, mesh);
                }
            }

            Addressables.Release(list);
        }

        /// <summary>
        /// Material 에셋 로드
        /// </summary>
        private async UniTask LoadMaterialsAsync()
        {
            _materials.Clear();
            IList<IResourceLocation> list = await Addressables.LoadResourceLocationsAsync(eAddressableLabel.MATERIAL.ToString());

            for (var index = 0; index < list.Count; index++)
            {
                IResourceLocation location = list[index];
                var material = await Addressables.LoadAssetAsync<Material>(location);
                if (material != null)
                {
                    _materials.TryAdd(location.PrimaryKey, material);
                }
            }

            Addressables.Release(list);
        }

        /// <summary>
        /// Shader 에셋 로드
        /// </summary>
        private async UniTask LoadShadersAsync()
        {
            _shaders.Clear();
            IList<IResourceLocation> list = await Addressables.LoadResourceLocationsAsync(eAddressableLabel.SHADER.ToString());

            for (var index = 0; index < list.Count; index++)
            {
                IResourceLocation location = list[index];
                var shader = Shader.Find(location.PrimaryKey);
                if (shader != null)
                {
                    _shaders.TryAdd(location.PrimaryKey, shader);
                }
            }

            Addressables.Release(list);
        }

        #endregion
    }
}