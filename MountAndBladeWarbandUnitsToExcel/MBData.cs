using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;
using System.Windows.Controls;

namespace MountAndBladeWarbandUnitsToExcel
{
    class MBData
    {
        public List<Unit> Units { get; } = new List<Unit>();
        public List<Faction> Factions { get; } = new List<Faction>();
        public Dictionary<string, string> Localization { get; } = new Dictionary<string, string>();
        public ProgressBar progressBar;

        public void SetUnits(string[] txtUnitsData)
        {
            for (int i = 2; i < txtUnitsData.Length; i += 7)
            {
                string[] txtUnitData = new string[6];
                Array.Copy(txtUnitsData, i, txtUnitData, 0, 6);
                Units.Add(new Unit(Units.Count, txtUnitData));
            }
        }
        public void SetFactions(string[] txtFactionsData)
        {
            for (int i = 2; i + 2 < txtFactionsData.Length; i += 2)
            {
                string[] txtFactionData = new string[2];
                Array.Copy(txtFactionsData, i, txtFactionData, 0, 2);
                Factions.Add(new Faction(Factions.Count, txtFactionData));
            }
        }
        public void AddLocalizationInDictonary(string[] localizationFileData)
        {
            foreach (var locline in localizationFileData)
            {
                var splitline = locline.Split('|');
                if (Localization.ContainsKey(splitline[0]))
                    throw new Exception("Два одинаковых ID в файлах локализации.");
                Localization.Add(splitline[0], splitline[1]);
            }
        }
        public void FillUnitsAndFactionsLocalization()
        {
            foreach (var unit in Units)
                unit.SetLocNames(Localization);
            foreach (var faction in Factions)
                faction.SetLocName(Localization);
        }
        public void CreateExcelWorkbook()
        {
            Application oXL = new Application();
            _Workbook oWB = oWB = oXL.Workbooks.Add("");
            _Worksheet oSheet = (_Worksheet)oWB.ActiveSheet;
            oSheet.Name = "Units";
            Range oRng;
            object misvalue = System.Reflection.Missing.Value;
            int lr = Units.Count + 2;

            AddExcelMergedHeader(oSheet.Range["A1", "A2"], "ID");
            AddExternalThickBorders(oSheet.Range["A1", oSheet.Cells[lr, 1]], 2d);
            AddExcelMergedHeader(oSheet.Range["B1", "B2"], "TroopID");
            AddExternalThickBorders(oSheet.Range["B1", oSheet.Cells[lr, 2]], 2d);
            AddExcelMergedHeader(oSheet.Range["C1", "C2"], "Name");
            AddExternalThickBorders(oSheet.Range["C1", oSheet.Cells[lr, 3]], 2d);
            AddExcelMergedHeader(oSheet.Range["D1", "D2"], "Level");
            AddExternalThickBorders(oSheet.Range["D1", oSheet.Cells[lr, 4]], 2d);
            AddExcelMergedHeader(oSheet.Range["E1", "E2"], "Faction");
            AddExternalThickBorders(oSheet.Range["E1", oSheet.Cells[lr, 5]], 2d);
            AddExcelMergedHeader(oSheet.Range["F1", "F2"], "Sex");
            AddExternalThickBorders(oSheet.Range["F1", oSheet.Cells[lr, 6]], 2d);
            AddExcelMergedHeader(oSheet.Range["G1", "H1"], "Upgrade Path (ID)");
            AddExternalThickBorders(oSheet.Range["G1", oSheet.Cells[lr, 8]], 2d);
            oSheet.Range["G2"].Value = "1";
            oSheet.Range["H2"].Value = "2";
            int clmn = 9;
            oRng = oSheet.Cells[1, clmn];
            AddExcelHeadersForEnum(oSheet, ref clmn, typeof(Unit.Attribute));
            AddExternalThickBorders(oSheet.Range[oRng, oSheet.Cells[lr, clmn - 1]], 2d);
            oRng = oSheet.Cells[1, clmn];
            AddExcelHeadersForEnum(oSheet, ref clmn, typeof(Unit.Proficiency));
            AddExternalThickBorders(oSheet.Range[oRng, oSheet.Cells[lr, clmn - 1]], 2d);
            oRng = oSheet.Cells[1, clmn];
            AddExcelHeadersForEnum(oSheet, ref clmn, typeof(Unit.Skill));
            AddExternalThickBorders(oSheet.Range[oRng, oSheet.Cells[lr, clmn - 1]], 2d);
            oRng = oSheet.Cells[1, clmn];
            AddExcelHeadersForEnum(oSheet, ref clmn, typeof(Unit.Flag));
            AddExternalThickBorders(oSheet.Range[oRng, oSheet.Cells[lr, clmn - 1]], 2d);

            oRng = (Range)oSheet.Range["A1", oSheet.Cells[2, clmn - 1]];
            oRng.Font.Bold = true;
            oRng.VerticalAlignment = XlVAlign.xlVAlignCenter;
            oRng.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            oRng.Borders.LineStyle = XlLineStyle.xlContinuous;
            oRng.Borders.Weight = 2d;
            AddExternalThickBorders(oRng, 3d);

            AddExcelUnitsLines(oSheet, 3);
            oSheet.Columns.AutoFit();
            oXL.Visible = true;
        }


        private void AddExcelHeadersForEnum(_Worksheet oSheet, ref int clmn, Type type)
        {
            Range oRng;
            var elms = Enum.GetNames(type);
            oRng = oSheet.Range[oSheet.Cells[1, clmn], oSheet.Cells[1, clmn + elms.Length - 1]];
            oRng.Merge();
            oRng.Value = type.Name;
            for (int i = 0; i < elms.Length; i++, clmn++)
                oSheet.Cells[2, clmn] = elms[i];
        }
        private void AddExcelMergedHeader(Range oRng, string value)
        {
            oRng.Merge();
            oRng.Value = value;
        }
        private void AddExcelDataFromDictonary<T>(_Worksheet oSheet, ref int clmn, ref int row, Dictionary<T, byte> dict)
        {
            var values = Enum.GetValues(typeof(T));
            foreach (var value in values)
            {
                oSheet.Cells[row, clmn] = dict[(T)value];
                clmn++;
            }
        }
        private int AddExcelUnitsLines(_Worksheet oShet, int row)
        {
            AddExcelUnitsLines(oShet, ref row);
            return row;
        }
        private void AddExcelUnitsLines(_Worksheet oSheet, ref int row)
        {
            progressBar.Dispatcher.Invoke(delegate () 
            {
                progressBar.Maximum = Units.Count;
                progressBar.Value = 0;
            });
            var pba = new System.Action(delegate 
            {
                progressBar.Value++;
            });
            for (int i = 0; i < Units.Count; i++, row++)
            {
                oSheet.Cells[row, 1] = Units[i].ID;
                oSheet.Cells[row, 2] = Units[i].TroopID;
                oSheet.Cells[row, 3] = Units[i].LocSingleName != null ?
                    Units[i].LocSingleName : Units[i].EngSingleName;
                oSheet.Cells[row, 4] = Units[i].Level;
                var faction = Factions[Units[i].FactionID];
                oSheet.Cells[row, 5] = faction.LocName != null ?
                    faction.LocName : faction.EngName;
                oSheet.Cells[row, 6] = Units[i].Sex.ToString();
                oSheet.Cells[row, 7] = Units[i].UpgradePath[0];
                oSheet.Cells[row, 8] = Units[i].UpgradePath[0];
                int clmn = 9;
                AddExcelDataFromDictonary(oSheet, ref clmn, ref row, Units[i].Attributes);
                AddExcelDataFromDictonary(oSheet, ref clmn, ref row, Units[i].Proficiencies);
                AddExcelDataFromDictonary(oSheet, ref clmn, ref row, Units[i].Skills);
                AddExcelDataFromDictonary(oSheet, ref clmn, ref row, Units[i].Flags);
                progressBar.Dispatcher.Invoke(pba);
            }
        }
        private void AddExternalThickBorders(Range oRng, double weight)
        {
            var bis = new XlBordersIndex[] 
            {
                XlBordersIndex.xlEdgeTop,
                XlBordersIndex.xlEdgeRight,
                XlBordersIndex.xlEdgeBottom,
                XlBordersIndex.xlEdgeLeft
            };
            foreach (var bor in bis)
            {
                oRng.Borders[bor].LineStyle = XlLineStyle.xlContinuous;
                oRng.Borders[bor].Weight = weight;
            }
        }
    }
}
