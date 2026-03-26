using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.ComponentModel;

namespace Scv_Entities
{
	public partial class V_Articoli: EntityObject, INotifyPropertyChanged
	{

		public int EsistenzaUfficio
		{
			get { return ((EsistenzaArticolo)GetStrorage(Id_Articolo)).EsistenzaUfficio; }
		}

		public int EsistenzaMagazzino
		{
			get { return ((EsistenzaArticolo)GetStrorage(Id_Articolo)).EsistenzaMagazzino; }
		}

		public EsistenzaArticolo GetStrorage(int id)
		{
			EsistenzaArticolo ea = new EsistenzaArticolo();
			List<V_Movimento> list = new List<V_Movimento>();

			using (Scv_Entities.IN_VIAEntities _cntx = new IN_VIAEntities())
				list = _cntx.V_Movimento.Where(x => x.Id_Articolo == id).ToList();

			foreach (V_Movimento m in list)
			{
				switch (m.Segno_Maga)
				{
					case "+":
						ea.EsistenzaMagazzino += (int)m.Nr_Pezzi;
						break;
					case "-":
						ea.EsistenzaMagazzino -= (int)m.Nr_Pezzi;
						break;
				}
				switch (m.Segno_Uff)
				{
					case "+":
						ea.EsistenzaUfficio += (int)m.Nr_Pezzi;
						break;
					case "-":
						ea.EsistenzaUfficio -= (int)m.Nr_Pezzi;
						break;
				}
			}

			return ea;
		}

		private int copieMagazzino = 0;
		public int CopieMagazzino
		{
			get { return copieMagazzino; }
			set { copieMagazzino = value; OnPropertyChanged("CopieMagazzino"); }
		}

		private int copieUfficio = 0;
		public int CopieUfficio
		{
			get { return copieUfficio; }
			set { copieUfficio = value; OnPropertyChanged("CopieUfficio"); }
		}

		private int totaleCopie = 0;
		public int TotaleCopie
		{
			get { return totaleCopie; }
			set { totaleCopie = value; OnPropertyChanged("TotaleCopie"); }
		}

		public string Anomalie
		{
			get { return (CopieMagazzino < 0 || CopieUfficio < 0) ? "***" : string.Empty; }
		}


		event PropertyChangedEventHandler PropertyChanged;
		private void OnNotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
