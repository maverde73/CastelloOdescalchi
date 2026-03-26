using System;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Presentation.Helpers
{
    public class CellElementCreatedEventArgs : EventArgs
    {
        public GridViewCell Cell { get; set; }
        public GridViewColumn Column { get; set; }
        public FrameworkElement Element { get; set; }
        public object DataItem { get; set; }
    }

    public class DynamicComboBoxColumn : GridViewComboBoxColumn
    {
		public event EventHandler<CellElementCreatedEventArgs> CellElementCreated;
		protected void OnCellElementCreated(CellElementCreatedEventArgs e)
        {
            if (CellElementCreated != null)
                CellElementCreated(this, e);
        }

		public override System.Windows.FrameworkElement CreateCellElement(GridViewCell cell, object dataItem)
        {
            var element = base.CreateCellElement(cell, dataItem);
            var e = new CellElementCreatedEventArgs { Cell = cell, Column = cell.Column, Element = element, DataItem = dataItem };
            OnCellElementCreated(e);
            return element;
        }

        public override System.Windows.FrameworkElement CreateCellEditElement(GridViewCell cell, object dataItem)
        {
            var element = base.CreateCellEditElement(cell, dataItem);
            var e = new CellElementCreatedEventArgs { Cell = cell, Column = cell.Column, Element = element, DataItem = dataItem };
            OnCellElementCreated(e);
            return element;
        }

    }
}
