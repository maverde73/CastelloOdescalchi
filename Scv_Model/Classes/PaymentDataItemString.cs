using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class PaymentDataItemString
	{
		#region Private Fields

		PaymentDataMaster master = null;

		List<PaymentDataDetails> details = null;

		List<string> masterStructure = null;

		List<string> detailStructure = null;

		#endregion// Private Fields



		#region Public Properties

		public PaymentDataMaster Master
		{
			get
			{
				if (master == null)
					master = new PaymentDataMaster();
				return master;
			}
			set { master = value; }
		}

		public List<PaymentDataDetails> Details
		{
			get
			{
				if (details == null)
					details = new List<PaymentDataDetails>();
				return details;
			}
			set { details = value; }
		}

		public List<string> MasterStructure
		{
			get
			{
				if (masterStructure == null)
					masterStructure = new List<string>();
				return masterStructure;
			}
			set { masterStructure = value; }
		}

		public List<string> DetailStructure
		{
			get
			{
				if (detailStructure == null)
					detailStructure = new List<string>();
				return detailStructure;
			}
			set { detailStructure = value; }
		}

		#endregion// Public Properties



		#region Constructors

		public PaymentDataItemString()
		{ }

		public PaymentDataItemString(PaymentDataMaster master)
		{
			this.Master = master;
		}

		public PaymentDataItemString(PaymentDataMaster master, List<PaymentDataDetails> details)
		{
			this.Master = master;
			this.Details = details;
		}

		#endregion// Constructors



		#region Public Methods

		public void AddMasterField(string fieldname, string fieldData)
		{
			Master.AddField(fieldname, fieldData);
		}

		public void AddDetail(List<PaymentDataField> detailField)
		{
			PaymentDataDetails d = new PaymentDataDetails();
			foreach (PaymentDataField f in detailField)
				d.AddField(f.FieldName, f.FieldData);
			Details.Add(d);
		}

		#endregion// Public Methods

		public class PaymentDataMaster
		{
			#region Private Fields

			private List<PaymentDataField> fieldList = null;

			#endregion// Private Fields



			#region Public Properties

			public List<PaymentDataField> FieldList
			{
				get
				{
					if (fieldList == null)
						fieldList = new List<PaymentDataField>();
					return fieldList;
				}
			}

			#endregion// Public Properties



			#region Constructors

			public PaymentDataMaster()
			{ }

			#endregion// Constructors



			#region Public Methods

			public void AddField(string fieldName, string fieldData)
			{
				if (!FieldList.Select(x => x.FieldName).Contains(fieldName))
					FieldList.Add(new PaymentDataField(fieldName, fieldData));
			}

			public void RemoveField(string fieldName)
			{
				foreach (PaymentDataField f in FieldList)
					if (f.FieldName.ToUpper() == fieldName.ToUpper())
					{
						FieldList.Remove(f);
						break;
					}
			}

			public void SetField(string fieldName, string fieldData)
			{
				foreach (PaymentDataField f in FieldList)
					if (f.FieldName.ToUpper() == fieldName.ToUpper())
					{
						f.FieldData = fieldData;
						break;
					}
			}

			public string GetField(string fieldName)
			{
				string data = string.Empty;

				foreach (PaymentDataField f in FieldList)
					if (f.FieldName.ToUpper() == fieldName.ToUpper())
					{
						data = f.FieldData;
						break;
					}

				return data;
			}

			#endregion// Public Methods
		}

		public class PaymentDataDetails
		{
			#region Private Fields

			private List<PaymentDataField> fieldList = null;

			#endregion// Private Fields



			#region Public Properties

			public List<PaymentDataField> FieldList
			{
				get
				{
					if (fieldList == null)
						fieldList = new List<PaymentDataField>();
					return fieldList;
				}
			}

			#endregion// Public Properties



			#region Constructors

			public PaymentDataDetails()
			{ }

			#endregion// Constructors



			#region Public Methods

			public void AddField(string fieldName, string fieldData)
			{
				if (!FieldList.Select(x => x.FieldName).Contains(fieldName))
					FieldList.Add(new PaymentDataField(fieldName, fieldData));
			}

			public void RemoveField(string fieldName)
			{
				foreach(PaymentDataField f in FieldList)
					if(f.FieldName.ToUpper() == fieldName.ToUpper())
					{
						FieldList.Remove(f);
						break;
					}
			}

			public void SetField(string fieldName, string fieldData)
			{
				foreach (PaymentDataField f in FieldList)
					if (f.FieldName.ToUpper() == fieldName.ToUpper())
					{
						f.FieldData = fieldData;
						break;
					}
			}

			public string GetField(string fieldName)
			{
				string data = string.Empty;

				foreach (PaymentDataField f in FieldList)
					if (f.FieldName.ToUpper() == fieldName.ToUpper())
					{
						data = f.FieldData;
						break;
					}

				return data;
			}

			#endregion// Public Methods

		}

		public class PaymentDataField
		{
			#region Private Fields



			#endregion// Private Fields



			#region Public Properties

			public string FieldName { get; set; }
			public string FieldData { get; set; }

			#endregion//Public Properties



			#region Constructors

			public PaymentDataField()
			{ }

			public PaymentDataField(string fieldName, string fieldData)
			{
				this.FieldName = FieldName;
				this.FieldData = FieldData;
			}


			#endregion// Constructors
		}
	}
}
