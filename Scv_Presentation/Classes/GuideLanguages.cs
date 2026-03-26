using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Presentation
{
	public class GuideLanguages : INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Events

		#region Private Fields

		private int language1ID = 0;
		private string language1Description = string.Empty;

		private int language2ID = 0;
		private string language2Description = string.Empty;

		private int language3ID = 0;
		private string language3Description = string.Empty;

		#endregion// Private Fields


		#region Public Properties

		public int Language1ID 
		{
			get { return language1ID; }
			set { language1ID = value; OnPropertyChanged("Language1ID"); }
		}

		public string Language1Description
		{
			get { return language1Description; }
			set { language1Description = value; OnPropertyChanged("Language1Description"); }
		}

		public int Language2ID
		{
			get { return language2ID; }
			set { language2ID = value; OnPropertyChanged("Language2ID"); }
		}

		public string Language2Description
		{
			get { return language2Description; }
			set { language2Description = value; OnPropertyChanged("Language2Description"); }
		}

		public int Language3ID
		{
			get { return language3ID; }
			set { language3ID = value; OnPropertyChanged("Language3ID"); }
		}

		public string Language3Description
		{
			get { return language3Description; }
			set { language3Description = value; OnPropertyChanged("Language3Description"); }
		}



		#endregion// Public Properties


		#region Event Handling

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handling
	}
}
