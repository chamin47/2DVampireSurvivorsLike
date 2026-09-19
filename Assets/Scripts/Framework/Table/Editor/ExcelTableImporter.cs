#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;
using UnityEngine;

namespace Framework.Table.Editor
{
    /// <summary>
    /// 외부 DLL(ExcelDataReader 등) 없이 C# 기본 ZipArchive 및 XmlDocument를 활용하여
    /// Excel(.xlsx) 파일을 2차원 셀 Grid(List<List<string>>)로 파싱하는 Editor 유틸리티 클래스입니다.
    /// </summary>
    public static class ExcelTableImporter
    {
        /// <summary>
        /// 지정한 경로의 .xlsx Excel 파일에서 첫 번째 워크시트 데이터를 로드합니다.
        /// </summary>
        public static List<List<string>> Import(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[ExcelTableImporter] File not found: '{filePath}'");
                return null;
            }

            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".csv")
            {
                return CsvTableImporter.Import(filePath);
            }
            if (ext != ".xlsx")
            {
                Debug.LogError($"[ExcelTableImporter] Unsupported file extension '{ext}'. Only .xlsx and .csv are supported. File: '{filePath}'");
                return null;
            }

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    List<string> sharedStrings = ReadSharedStrings(zip);
                    return ReadSheetData(zip, sharedStrings);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ExcelTableImporter] Failed to parse Excel file '{filePath}'. Exception: {ex.Message}");
                return null;
            }
        }

        private static List<string> ReadSharedStrings(ZipArchive zip)
        {
            List<string> sharedStrings = new List<string>();
            ZipArchiveEntry entry = zip.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return sharedStrings;

            using (Stream stream = entry.Open())
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);
                XmlNodeList siNodes = doc.GetElementsByTagName("si");
                foreach (XmlNode si in siNodes)
                {
                    sharedStrings.Add(si.InnerText);
                }
            }

            return sharedStrings;
        }

        private static List<List<string>> ReadSheetData(ZipArchive zip, List<string> sharedStrings)
        {
            List<List<string>> grid = new List<List<string>>();

            // 첫 번째 워크시트 찾기 (xl/worksheets/sheet1.xml)
            ZipArchiveEntry sheetEntry = zip.GetEntry("xl/worksheets/sheet1.xml");
            if (sheetEntry == null)
            {
                Debug.LogError("[ExcelTableImporter] 'xl/worksheets/sheet1.xml' not found in Excel archive.");
                return grid;
            }

            using (Stream stream = sheetEntry.Open())
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);

                XmlNodeList rowNodes = doc.GetElementsByTagName("row");
                foreach (XmlNode rowNode in rowNodes)
                {
                    List<string> rowCells = new List<string>();
                    XmlNodeList cNodes = rowNode.SelectNodes("*[local-name()='c']");
                    if (cNodes == null || cNodes.Count == 0)
                    {
                        cNodes = rowNode.ChildNodes;
                    }

                    if (cNodes != null)
                    {
                        foreach (XmlNode cNode in cNodes)
                        {
                            if (cNode.Name != "c" && cNode.LocalName != "c") continue;

                            string cellRef = cNode.Attributes?["r"]?.Value ?? "";
                            int colIdx = GetColumnIndexFromCellRef(cellRef);
                            if (colIdx < 0) colIdx = rowCells.Count;

                            string cellType = cNode.Attributes?["t"]?.Value ?? "";

                            string cellValue = "";
                            XmlNode vNode = null;
                            foreach (XmlNode child in cNode.ChildNodes)
                            {
                                if (child.Name == "v" || child.LocalName == "v")
                                {
                                    vNode = child;
                                    break;
                                }
                            }

                            if (vNode != null)
                            {
                                cellValue = vNode.InnerText;
                            }

                            if (cellType == "s" && int.TryParse(cellValue, out int sIdx))
                            {
                                if (sIdx >= 0 && sIdx < sharedStrings.Count)
                                {
                                    cellValue = sharedStrings[sIdx];
                                }
                            }
                            else if (cellType == "inlineStr")
                            {
                                cellValue = cNode.InnerText;
                            }

                            while (rowCells.Count <= colIdx)
                            {
                                rowCells.Add(string.Empty);
                            }
                            rowCells[colIdx] = cellValue;
                        }
                    }

                    grid.Add(rowCells);
                }
            }

            return grid;
        }

        private static int GetColumnIndexFromCellRef(string cellRef)
        {
            if (string.IsNullOrEmpty(cellRef)) return -1;

            int colIndex = 0;
            bool foundLetter = false;

            foreach (char c in cellRef)
            {
                if (char.IsLetter(c))
                {
                    foundLetter = true;
                    colIndex = colIndex * 26 + (char.ToUpper(c) - 'A' + 1);
                }
                else
                {
                    break;
                }
            }

            return foundLetter ? (colIndex - 1) : -1;
        }
    }
}
#endif
