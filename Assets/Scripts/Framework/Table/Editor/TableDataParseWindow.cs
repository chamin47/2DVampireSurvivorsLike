#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.Table.Editor
{
    /// <summary>
    /// Tools > Framework > Table > Data Parse 메뉴를 통해 실행되는 EditorWindow입니다.
    /// CSV 및 Excel(.xlsx) 파일을 선택하여 데이터를 읽어오고 파싱된 결과를 하위 GUI 테이블 뷰에 미리보기로 표시합니다.
    /// </summary>
    public class TableDataParseWindow : EditorWindow
    {
        private string selectedFilePath = string.Empty;
        private List<List<string>> parsedGrid = null;
        private Vector2 scrollPosition = Vector2.zero;
        private string statusMessage = "파일을 선택하고 'Parse Data' 버튼을 누르세요.";
        private MessageType statusMessageType = MessageType.Info;

        [MenuItem("Tools/Framework/Table/Data Parse", false, 10)]
        public static void OpenWindow()
        {
            var window = GetWindow<TableDataParseWindow>("Table Data Parser");
            window.minSize = new Vector2(650, 450);
            window.Show();
        }

        private void OnGUI()
        {
            DrawHeaderArea();
            EditorGUILayout.Space(10);
            DrawFileSelectionArea();
            EditorGUILayout.Space(10);
            DrawStatusArea();
            EditorGUILayout.Space(10);
            DrawGridDataPreviewArea();
        }

        private void DrawHeaderArea()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("📊 Table Data Parser & Viewer", EditorStyles.boldLabel);
            GUILayout.Label("Excel(.xlsx) 또는 CSV(.csv) 파일을 선택하고 데이터를 파싱하여 미리봅니다.", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawFileSelectionArea()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Target Data File Selection", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            selectedFilePath = EditorGUILayout.TextField("File Path", selectedFilePath);

            if (GUILayout.Button("Browse...", GUILayout.Width(75)))
            {
                string openPath = string.IsNullOrEmpty(selectedFilePath) ? Application.dataPath : Path.GetDirectoryName(selectedFilePath);
                string path = EditorUtility.OpenFilePanel("Select CSV or Excel File", openPath, "csv,xlsx");
                if (!string.IsNullOrEmpty(path))
                {
                    selectedFilePath = path;
                    ParseSelectedFile();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(selectedFilePath));
            if (GUILayout.Button("Parse Data", GUILayout.Height(26)))
            {
                ParseSelectedFile();
            }
            if (GUILayout.Button("Export to JSON", GUILayout.Height(26)))
            {
                ExportSelectedFileToJson();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawStatusArea()
        {
            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.HelpBox(statusMessage, statusMessageType);
            }
        }

        private void DrawGridDataPreviewArea()
        {
            if (parsedGrid == null || parsedGrid.Count == 0)
            {
                EditorGUILayout.HelpBox("파싱된 데이터가 없습니다. CSV 또는 Excel 파일을 지정한 후 'Parse Data'를 누르세요.", MessageType.None);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            int dataRowCount = Mathf.Max(0, parsedGrid.Count - 2);
            int colCount = parsedGrid.Count > 0 ? parsedGrid[0].Count : 0;
            EditorGUILayout.LabelField($"Data Preview (Total Rows: {parsedGrid.Count} | Data Rows: {dataRowCount} | Columns: {colCount})", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, true, true);

            // 헤더 스타일 설정
            GUIStyle headerNameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };

            GUIStyle headerTypeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.3f, 0.7f, 1.0f) }
            };

            GUIStyle cellStyle = new GUIStyle(EditorStyles.textField)
            {
                alignment = TextAnchor.MiddleLeft
            };

            const float colWidth = 130f;
            const float rowHeight = 22f;

            // Row 0: Field Names Header
            if (parsedGrid.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box("Row# / Field", EditorStyles.toolbarButton, GUILayout.Width(80), GUILayout.Height(rowHeight));
                for (int c = 0; c < parsedGrid[0].Count; c++)
                {
                    string colName = parsedGrid[0][c];
                    GUILayout.Box(colName, EditorStyles.toolbarButton, GUILayout.Width(colWidth), GUILayout.Height(rowHeight));
                }
                EditorGUILayout.EndHorizontal();
            }

            // Row 1: Data Types Header
            if (parsedGrid.Count > 1)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box("Type", EditorStyles.toolbarButton, GUILayout.Width(80), GUILayout.Height(rowHeight));
                for (int c = 0; c < parsedGrid[1].Count; c++)
                {
                    string colType = parsedGrid[1][c];
                    GUILayout.Box($"<{colType}>", EditorStyles.toolbarButton, GUILayout.Width(colWidth), GUILayout.Height(rowHeight));
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(2);

            // Row 2+: Actual Data Rows
            for (int r = 2; r < parsedGrid.Count; r++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box($"Row {r + 1}", EditorStyles.toolbarButton, GUILayout.Width(80), GUILayout.Height(rowHeight));

                List<string> rowData = parsedGrid[r];
                int maxCols = parsedGrid[0].Count;

                for (int c = 0; c < maxCols; c++)
                {
                    string cellVal = c < rowData.Count ? rowData[c] : string.Empty;
                    EditorGUILayout.TextField(cellVal, GUILayout.Width(colWidth), GUILayout.Height(rowHeight));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void ParseSelectedFile()
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                statusMessage = "유효한 파일 경로가 아닙니다. 경로를 다시 확인해주세요.";
                statusMessageType = MessageType.Error;
                parsedGrid = null;
                return;
            }

            string ext = Path.GetExtension(selectedFilePath).ToLowerInvariant();
            if (ext == ".csv")
            {
                parsedGrid = CsvTableImporter.Import(selectedFilePath);
            }
            else if (ext == ".xlsx")
            {
                parsedGrid = ExcelTableImporter.Import(selectedFilePath);
            }
            else
            {
                statusMessage = $"지원하지 않는 확장자입니다 ('{ext}'). .csv 또는 .xlsx 파일만 지원됩니다.";
                statusMessageType = MessageType.Warning;
                parsedGrid = null;
                return;
            }

            if (parsedGrid == null || parsedGrid.Count == 0)
            {
                statusMessage = "파싱 실패: 파일 내용이 비어있거나 읽을 수 없습니다.";
                statusMessageType = MessageType.Error;
            }
            else
            {
                statusMessage = $"파싱 성공! 파일: '{Path.GetFileName(selectedFilePath)}' (전체 {parsedGrid.Count}행 로드됨)";
                statusMessageType = MessageType.Info;
            }
        }

        private void ExportSelectedFileToJson()
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                statusMessage = "유효한 파일 경로가 지정되지 않았습니다.";
                statusMessageType = MessageType.Error;
                return;
            }

            bool success = TableJsonExporter.ConvertFileToJson(selectedFilePath, "Assets/Resources/Tables");
            if (success)
            {
                AssetDatabase.Refresh();
                statusMessage = $"JSON 내보내기 성공! 경로: Assets/Resources/Tables/{Path.GetFileNameWithoutExtension(selectedFilePath)}.json";
                statusMessageType = MessageType.Info;
            }
            else
            {
                statusMessage = "JSON 내보내기에 실패했습니다. 콘솔 에러 로그를 확인하세요.";
                statusMessageType = MessageType.Error;
            }
        }
    }
}
#endif
