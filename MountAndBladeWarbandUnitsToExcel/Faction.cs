using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MountAndBladeWarbandUnitsToExcel
{
    class Faction
    {
        public int ID { get; private set; }
        public string FactionID { get; private set; }
        public string EngName { get; private set; }
        public string LocName { get; set; }
        public bool isLocalized { get { return LocName != null; } }

        public Faction(int id, string[] txtFactionData)
        {
            ID = id;
            var fd = txtFactionData[0].Split(' ');
            int i = 0;
            for (; i < fd.Length; i++) 
            {
                if (fd[i].StartsWith("fac_"))
                {
                    FactionID = fd[i];
                    break;
                }
            }
            EngName = fd[i + 1].Replace('_', ' ');
        }
        public void SetLocName(Dictionary<string, string> locDict)
        {
            if (locDict.ContainsKey(FactionID))
                LocName = locDict[FactionID];
        }
    }
}
