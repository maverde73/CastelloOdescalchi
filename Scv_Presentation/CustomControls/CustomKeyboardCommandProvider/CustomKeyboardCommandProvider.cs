using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.GridView;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Presentation
{
	public class CustomKeyboardCommandProvider : DefaultKeyboardCommandProvider
	{
		private GridViewDataControl parentGrid = null;

		public CustomKeyboardCommandProvider(GridViewDataControl grid) 
			: base(grid)
		{
			this.parentGrid = grid;
		}

		public override IEnumerable<ICommand> ProvideCommandsForKey(Key key)
		{
			List<ICommand> commandsToExecute = base.ProvideCommandsForKey(key).ToList();

			if (key == Key.Escape)
				commandsToExecute.Add(RadGridViewCommands.CancelRowEdit);

			else if (key == Key.Enter)

				if (parentGrid.CurrentCell.IsInEditMode)
				{
					commandsToExecute.Clear();
					commandsToExecute.Remove(RadGridViewCommands.MoveDown);
				}

			return commandsToExecute;
		}
	}
}
