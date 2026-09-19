#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.Table.Editor
{
    public sealed class TableAutoImporter : AssetPostprocessor
    {
        private const string SourceRoot = "Assets/Datas/";
        private const string TargetRoot = "Assets/Resources/Tables";
        private static bool isProcessing;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (isProcessing) return;
            isProcessing = true;
            try
            {
                foreach (string path in importedAssets) TryConvert(path);
                foreach (string path in movedAssets) TryConvert(path);
                foreach (string path in deletedAssets) TryDeleteGeneratedJson(path);
                foreach (string path in movedFromAssetPaths) TryDeleteGeneratedJson(path);
            }
            finally
            {
                isProcessing = false;
            }
        }

        private static void TryConvert(string assetPath)
        {
            if (!IsTableSource(assetPath) || Path.GetFileName(assetPath).StartsWith("~$", StringComparison.Ordinal)) return;
            if (TableJsonExporter.ConvertFileToJson(assetPath, TargetRoot))
            {
                Debug.Log($"[TableAutoImporter] Auto-import complete: {assetPath}");
            }
        }

        private static void TryDeleteGeneratedJson(string sourcePath)
        {
            if (!IsTableSource(sourcePath)) return;
            string generatedPath = TableJsonExporter.GetTargetPath(sourcePath, TargetRoot);
            if (AssetDatabase.DeleteAsset(generatedPath))
            {
                Debug.Log($"[TableAutoImporter] Removed generated JSON because its source was removed: {generatedPath}");
            }
        }

        private static bool IsTableSource(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string normalized = path.Replace('\\', '/');
            if (!normalized.StartsWith(SourceRoot, StringComparison.OrdinalIgnoreCase)) return false;
            string extension = Path.GetExtension(normalized);
            return string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase);
        }
    }
}
#endif
