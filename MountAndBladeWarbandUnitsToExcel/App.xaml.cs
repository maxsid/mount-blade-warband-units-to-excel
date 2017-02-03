using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MountAndBladeWarbandUnitsToExcel
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static int[] GetDivRsltAndRest(long dividend, int divider)
        {
            var res = new int[2];
            res[0] = (int)dividend / divider;
            res[1] = (int)dividend % divider;
            return res;
        }
    }
}
