using System.Windows.Controls;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace LoadProfileGenerator
{
    public static class Extensions
    {

        public static void ResizeColummns([NotNull] this ListView lv, [CanBeNull] double? maxWidth = null)
        {
            if (lv == null)
            {
                throw new LPGException("Listview == null");
            }
            var actualmaxwidth = lv.ActualWidth;
            if (maxWidth != null)
            {
                actualmaxwidth = (double)maxWidth;
            }
            var gv = (GridView)lv.View;

            foreach (var column in gv.Columns)
            {
                column.Width = column.ActualWidth;
                column.Width = double.NaN;
            }
            double currentwidth = 0;
            foreach (var column in gv.Columns)
            {
                currentwidth += column.ActualWidth;
            }
            if (currentwidth > actualmaxwidth)
            {
                var factor = currentwidth / actualmaxwidth;
                foreach (var column in gv.Columns)
                {
                    column.Width = column.ActualWidth / factor;
                }
            }
        }


    }
}
