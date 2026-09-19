#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.Table.Editor
{
    /// <summary>
    /// Unity Editor 메뉴 (Tools > Framework > Table)를 통해 Excel 및 CSV 파일을 JSON으로 변환하고
    /// Resources/Tables/ 디렉토리에 저장하는 Export 유틸리티 클래스입니다.
    /// </summary>
    public static class TableJsonExporter
    {
        private const string DefaultSourceDir = "Assets/Datas";
        private const string DefaultTargetDir = "Assets/Resources/Tables";

        [MenuItem("Tools/Framework/Table/Convert All Tables to JSON", false, 1)]
        public static void ConvertAllTables()
        {
            if (!Directory.Exists(DefaultSourceDir))
            {
                Directory.CreateDirectory(DefaultSourceDir);
                Debug.LogWarning($"[TableJsonExporter] Source directory '{DefaultSourceDir}' did not exist. Created empty folder.");
                return;
            }

            string[] csvFiles = Directory.GetFiles(DefaultSourceDir, "*.csv", SearchOption.AllDirectories);
            string[] xlsxFiles = Directory.GetFiles(DefaultSourceDir, "*.xlsx", SearchOption.AllDirectories);

            List<string> allFiles = new List<string>();
            allFiles.AddRange(csvFiles);
            foreach (var file in xlsxFiles)
            {
                if (!Path.GetFileName(file).StartsWith("~$"))
                {
                    allFiles.Add(file);
                }
            }

            if (allFiles.Count == 0)
            {
                Debug.LogWarning($"[TableJsonExporter] No .csv or .xlsx files found under '{DefaultSourceDir}'.");
                return;
            }

            int successCount = 0;
            foreach (string filePath in allFiles)
            {
                if (ConvertFileToJson(filePath, DefaultTargetDir))
                {
                    successCount++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[TableJsonExporter] Table JSON Export Completed! ({successCount}/{allFiles.Count} converted to '{DefaultTargetDir}').");
        }

        [MenuItem("Tools/Framework/Table/Convert Selected File to JSON", false, 2)]
        public static void ConvertSelectedFile()
        {
            var selectedObj = Selection.activeObject;
            if (selectedObj == null)
            {
                Debug.LogWarning("[TableJsonExporter] No asset file selected in Project window.");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(selectedObj);
            string ext = Path.GetExtension(assetPath).ToLowerInvariant();

            if (ext != ".csv" && ext != ".xlsx")
            {
                Debug.LogWarning($"[TableJsonExporter] Selected asset '{assetPath}' is not a .csv or .xlsx file.");
                return;
            }

            if (ConvertFileToJson(assetPath, DefaultTargetDir))
            {
                AssetDatabase.Refresh();
                Debug.Log($"[TableJsonExporter] Selected file '{assetPath}' successfully converted to JSON!");
            }
        }

        /// <summary>
        /// 단일 CSV/Excel 파일 경로를 받아 JSON으로 변환하여 저장합니다.
        /// </summary>
        public static bool ConvertFileToJson(string sourceFilePath, string targetDirectory)
        {
            List<List<string>> grid = null;
            string ext = Path.GetExtension(sourceFilePath).ToLowerInvariant();

            if (ext == ".csv")
            {
                grid = CsvTableImporter.Import(sourceFilePath);
            }
            else if (ext == ".xlsx")
            {
                grid = ExcelTableImporter.Import(sourceFilePath);
            }

            if (grid == null || grid.Count < 3)
            {
                Debug.LogError($"[TableJsonExporter] Failed to read grid data from '{sourceFilePath}'. Minimum 3 rows required (Header Names, Data Types, Data Rows).");
                return false;
            }

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceFilePath);
            string tableName = fileNameWithoutExt;

            string jsonContent = TableFileConverter.ConvertGridToJson(grid, sourceFilePath);

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            string targetPath = Path.Combine(targetDirectory, $"{tableName}.json");
            File.WriteAllText(targetPath, jsonContent, System.Text.Encoding.UTF8);

            Debug.Log($"[TableJsonExporter] Exported JSON: '{targetPath}'");
            return true;
        }
    }
}
#endif
