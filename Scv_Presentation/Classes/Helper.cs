using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace Presentation
{	
	public class Helper
	{
		public static Dictionary<object, object> GetControlValue(object control)
		{			
			Dictionary<object, object> value = new Dictionary<object,object>();
			object cnt = null;

			switch (control.GetType().Name)
			{
				case "RadAutoCompleteBox":
					cnt = control as RadAutoCompleteBox;
					if (cnt != null)
					{
						value.Add("name", ((RadAutoCompleteBox)cnt).Name);
						value.Add("value", ((RadAutoCompleteBox)cnt).SearchText.ToString());
					}
					break;

				case "TextBox":
					cnt = control as TextBox;
					if (cnt != null)
					{
						value.Add("name", ((TextBox)cnt).Name);
						value.Add("value", ((TextBox)cnt).Text.ToString());
					}
					break;

				case "RadComboBox":
					cnt = control as RadComboBox;
					if (cnt != null)
					{
						value.Add("name", ((RadComboBox)cnt).Name);
						value.Add("value", ((RadComboBox)cnt).SelectedValue);
					}
					break;

				case "ComboBox":
					cnt = control as ComboBox;
					if (cnt != null)
					{
						value.Add("name", ((ComboBox)cnt).Name);
						value.Add("value", ((ComboBox)cnt).SelectedValue);
					}
					break;

				case "CheckBox" :
					cnt = control as CheckBox;
					if (cnt != null)
					{
						value.Add("name", ((CheckBox)cnt).Name);
						value.Add("value", (bool)((CheckBox)cnt).IsChecked);
					}
					break;

				case "RadRadioButton":
					cnt = control as RadRadioButton;
					if (cnt != null)
					{
						value.Add("name", ((RadRadioButton)cnt).Name);
						value.Add("value", (bool)((RadRadioButton)cnt).IsChecked);
					}
					break;

				case "RadioButton":
					cnt = control as RadioButton;
					if (cnt != null)
					{
						value.Add("name", ((RadioButton)cnt).Name);
						value.Add("value", (bool)((RadioButton)cnt).IsChecked);
					}
					break;
			}

			return value;
		}
	}
}
