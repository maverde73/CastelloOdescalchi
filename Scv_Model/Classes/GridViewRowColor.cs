using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media;

namespace Scv_Model
{
	public class GridViewRowColor
	{
		#region Private Fields
		private System.Windows.Media.Color sGroup = new System.Windows.Media.Color();
		private System.Windows.Media.Color lGroup = new System.Windows.Media.Color();
		private System.Windows.Media.Color sChild = new System.Windows.Media.Color();
		private System.Windows.Media.Color lChild = new System.Windows.Media.Color();
		private SolidColorBrush strongGroup = null;
		private SolidColorBrush strongChild = null;
		private SolidColorBrush lightGroup = null;
		private SolidColorBrush lightChild = null;

		private int colorIndex = 0;

		private List<SolidColorBrush> groupColors = null;
		private List<SolidColorBrush> childColors = null;

		#endregion// Private Fields


		#region Public Properties

		public int ColorIndex
		{
			get { return colorIndex; }
			set { colorIndex = value; }
		}

		public SolidColorBrush CurrentGroupColor
		{
			get
			{
				if (groupColors == null)
				{
					sGroup.A = 255;
					sGroup.R = 230;
					sGroup.G = 230;
					sGroup.B = 230;
					lGroup.A = 255;
					lGroup.R = 255;
					lGroup.G = 255;
					lGroup.B = 255;

					strongGroup = new SolidColorBrush(sGroup);
					lightGroup = new SolidColorBrush(lGroup);

					groupColors = new List<SolidColorBrush>() { strongGroup, lightGroup };
				}
				return groupColors[ColorIndex];
			}
		}

		public SolidColorBrush CurrentChildColor
		{
			get
			{

				if (childColors == null)
				{
					sChild.A = 255;
					sChild.R = 230;
					sChild.G = 230;
					sChild.B = 230;
					lChild.A = 255;
					lChild.R = 255;
					lChild.G = 255;
					lChild.B = 255;

					strongChild = new SolidColorBrush(sChild);
					lightChild = new SolidColorBrush(lChild);

					childColors = new List<SolidColorBrush> { lightChild, strongChild };
				}
				return childColors[ColorIndex];
			}
		}

		#endregion// Public Properties
	}
}
