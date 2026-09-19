using System.IO;
using Example;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace ExampleEditor
{
    [InitializeOnLoad]
    internal static class AddressablePrefabDemoSetup
    {
        private const string PrefabDirectory = "Assets/Prefabs";
        private const string PrefabPath = PrefabDirectory + "/AddressableDemo.prefab";
        private const string MaterialPath = PrefabDirectory + "/AddressableDemoMaterial.mat";

        static AddressablePrefabDemoSetup()
        {
            EditorApplication.delayCall += EnsureDemoIsReady;
        }

        [MenuItem("Tools/Addressables/Setup Framework Demo")]
        private static void EnsureDemoIsReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Directory.CreateDirectory(PrefabDirectory);

            CreateOrUpdatePrefab();

            RegisterAddressable();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateOrUpdatePrefab()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Unlit/Color")
                    ?? Shader.Find("Sprites/Default");

                if (shader == null)
                {
                    Debug.LogError("[AddressablePrefabDemoSetup] No compatible unlit shader was found.");
                    return;
                }

                material = new Material(shader)
                {
                    name = "Addressable Demo Material",
                    color = new Color(0.15f, 0.85f, 1f, 1f)
                };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null;
            GameObject temporary = prefabExists
                ? PrefabUtility.LoadPrefabContents(PrefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            temporary.name = "Addressable Demo Prefab";
            temporary.transform.localScale = Vector3.one * 1.5f;

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(temporary);
            if (temporary.GetComponent<AddressableDemoMotion>() == null)
            {
                temporary.AddComponent<AddressableDemoMotion>();
            }

            if (temporary.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.sharedMaterial = material;
            }

            PrefabUtility.SaveAsPrefabAsset(temporary, PrefabPath);
            if (prefabExists)
            {
                PrefabUtility.UnloadPrefabContents(temporary);
            }
            else
            {
                Object.DestroyImmediate(temporary);
            }

            Debug.Log($"[AddressablePrefabDemoSetup] Ensured prefab at '{PrefabPath}'.");
        }

        private static void RegisterAddressable()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[AddressablePrefabDemoSetup] AddressableAssetSettings is missing.");
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(PrefabPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"[AddressablePrefabDemoSetup] Could not resolve the GUID for '{PrefabPath}'.");
                return;
            }

            AddressableAssetGroup group = settings.DefaultGroup;
            if (group == null)
            {
                Debug.LogError("[AddressablePrefabDemoSetup] The default Addressables group is missing.");
                return;
            }

            var entry = settings.FindAssetEntry(guid);
            if (entry == null || entry.parentGroup != group)
            {
                entry = settings.CreateOrMoveEntry(guid, group, false, false);
            }

            if (entry.address != AddressablePrefabDemo.AssetKey)
            {
                entry.address = AddressablePrefabDemo.AssetKey;
                EditorUtility.SetDirty(settings);
            }

            Debug.Log(
                $"[AddressablePrefabDemoSetup] Registered '{PrefabPath}' as " +
                $"'{AddressablePrefabDemo.AssetKey}'.");
        }
    }
}
