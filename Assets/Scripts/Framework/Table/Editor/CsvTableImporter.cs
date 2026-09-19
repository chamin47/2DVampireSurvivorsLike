#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Framework.Table.Editor
{
    /// <summary>
    /// CSV 파일을 파싱하여 2차원 셀 데이터 Grid(List<List<string>>)로 변환하는 Editor 유틸리티 클래스입니다.
    /// 큰따옴표(""), 콤마(,), 줄바꿈 예외 처리를 지원합니다.
    /// </summary>
    public static class CsvTableImporter
    {
        /// <summary>
        /// 지정한 경로의 CSV 파일을 로드하여 셀 Grid 목록으로 파싱합니다.
        /// </summary>
        public static List<List<string>> Import(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[CsvTableImporter] CSV File not found at path: '{filePath}'");
                return null;
            }

            string text = File.ReadAllText(filePath, Encoding.UTF8);
            return ParseCsvText(text);
        }

        /// <summary>
        /// CSV 텍스트 문자열을 2차원 셀 Grid로 파싱합니다.
        /// </summary>
        public static List<List<string>> ParseCsvText(string csvText)
        {
            List<List<string>> grid = new List<List<string>>();
            if (string.IsNullOrEmpty(csvText))
            {
                return grid;
            }

            List<string> currentRow = new List<string>();
            StringBuilder currentCell = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char c = csvText[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                        {
                            currentCell.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        currentCell.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        currentRow.Add(currentCell.ToString());
                        currentCell.Clear();
                    }
                    else if (c == '\r')
                    {
                        // CR 무시 (\n에서 행 종단 처리)
                    }
                    else if (c == '\n')
                    {
                        currentRow.Add(currentCell.ToString());
                        currentCell.Clear();
                        grid.Add(currentRow);
                        currentRow = new List<string>();
                    }
                    else
                    {
                        currentCell.Append(c);
                    }
                }
            }

            if (currentCell.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentCell.ToString());
                grid.Add(currentRow);
            }

            return grid;
        }
    }
}
#endif
