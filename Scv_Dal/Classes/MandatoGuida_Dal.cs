using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;
using System.Collections.ObjectModel;
using Scv_Model;

namespace Scv_Dal
{
	public class MandatoGuida_Dal
	{

		public MandatoGuida_Dal()
		{
		}

		private List<V_MandatoGuidaNecropoli> GetMandatoGuidaNecropoli(int year, int month)
		{
			//Parametri_Dal dalParameters = new Parametri_Dal();
			//Guida_Dal dalGuide = new Guida_Dal();

			List<V_MandatoGuidaNecropoli> list = new List<V_MandatoGuidaNecropoli>();
			//Guida g = null;
			//decimal price = 0;

			//decimal.TryParse(dalParameters.GetItem("compenso_guida").Valore, out price);

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				list = _context.V_MandatoGuidaNecropoli.Where(x => x.Yr == year && x.Mt == month).ToList();
			}

			return list;
		}

		private List<V_MandatoGuidaAltro> GetMandatoGuidaAltro(int year, int month)
		{
			List<V_MandatoGuidaAltro> obj = null;

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				obj = _context.V_MandatoGuidaAltro.Where(x => x.Yr == year && x.Mt == month).ToList();
			}

			return obj;
		}

		private List<V_MandatoGuidaAcconto> GetMandatoGuidaAcconto(int year, int month)
		{
			List<V_MandatoGuidaAcconto> obj = null;

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				obj = _context.V_MandatoGuidaAcconto.Where(x => x.Yr == year && x.Mt == month).ToList();
			}

			return obj;
		}

		public List<V_MandatoGuida> GetFilteredList_V_MandatoGuida(BaseFilter filter, int year, int month, ref List<MoneyCut> moneyCuts, bool groupByColleges)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_MandatoGuida> ItemsList = new List<V_MandatoGuida>();

				int id = 0;
				List<Guida> guideList = new Guida_Dal().GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out id);

				List<V_MandatoGuidaNecropoli> mNecropoli = GetMandatoGuidaNecropoli(year, month);
				List<V_MandatoGuidaAltro> mAltro = GetMandatoGuidaAltro(year, month);
				List<V_MandatoGuidaAcconto> mAcconto = GetMandatoGuidaAcconto(year, month);

				V_MandatoGuida obj = null;

				foreach (Guida g in guideList)
				{
					obj = new V_MandatoGuida();
					obj.Id_Guida = g.Id_Guida;
					obj.Id_Collegio = g.Id_Collegio;
					obj.Nome = g.Nome;
					obj.Cognome = g.Cognome;

					foreach (V_MandatoGuidaNecropoli mN in mNecropoli)
					{
						if (mN.Id_Guida == g.Id_Guida)
						{
							obj.YrNecropoli = mN.Yr;
							obj.MtNecropoli = mN.Mt;
							obj.NrNecropoli = mN.Nr;
							obj.TotaleNecropoli = mN.Totale;
							break;
						}
					}

					foreach (V_MandatoGuidaAltro mA in mAltro)
					{
						if (mA.Id_Guida == g.Id_Guida)
						{
							obj.YrAltro = mA.Yr;
							obj.MtAltro = mA.Mt;
							obj.NrAltro = mA.Nr;
							obj.TotaleAltro = mA.Totale;
							break;
						}
					}

					foreach (V_MandatoGuidaAcconto mga in mAcconto)
					{
						if (mga.Id_Guida == g.Id_Guida)
						{
							obj.AccontoAltro = mga.Acconto;
							break;
						}
					}

					if (!(obj.TotaleNecropoli == null && obj.TotaleAltro == null && obj.AccontoAltro == null))
						ItemsList.Add(obj);
				}

				List<List<MoneyCut>> moneyCutsList = new List<List<MoneyCut>>();

				foreach (V_MandatoGuida vmg in ItemsList)
					moneyCutsList.Add(MoneyCut_Dal.GetCuts(moneyCuts, (int)vmg.Saldo));

				foreach (List<MoneyCut> mcl in moneyCutsList)
				{
					foreach (MoneyCut mc in mcl)
					{
						foreach (MoneyCut m in moneyCuts)
						{
							if (mc.Cut == m.Cut)
							{
								m.Pieces += mc.Pieces;
								break;
							}
						}
					}
				}

				List<int> collegesIDs = new List<int>();
				List<V_MandatoGuida> filteredList = new List<V_MandatoGuida>();
				V_MandatoGuida mObj = null;
				LK_Collegio coll = null;
				List<V_MandatoGuida> collegeList = new List<V_MandatoGuida>();

				//acquisisco tutti gli eventuali collegi che fanno capo alle guide nella lista
				foreach (V_MandatoGuida mg in ItemsList)
					if (mg.Id_Collegio != null && (int)mg.Id_Collegio > 0)
						if (!collegesIDs.Contains((int)mg.Id_Collegio))
							collegesIDs.Add((int)mg.Id_Collegio);

				foreach (V_MandatoGuida mg in ItemsList)
				{
					if (mg.Id_Collegio != null && (int)mg.Id_Collegio > 0 && groupByColleges)
					{
						foreach (int cID in collegesIDs)
						{
							if ((int)mg.Id_Collegio == cID)
							{
								coll = new LK_Collegio_Dal().GetItem(cID);
								mObj = FindMandatoGuidaCollegio(collegeList, coll.Descrizione);

								if (mObj != null)
								{
									mObj.NrNecropoli += mg.NrNecropoli != null ? mg.NrNecropoli : 0;
									mObj.TotaleNecropoli += mg.TotaleNecropoli != null ? mg.TotaleNecropoli : 0;
									mObj.NrAltro += mg.NrAltro != null ? mg.NrAltro : 0;
									mObj.TotaleAltro += mg.TotaleAltro != null ? mg.TotaleAltro : 0;
									mObj.AccontoAltro += mg.AccontoAltro != null ? mg.AccontoAltro : 0;
								}
								else
								{
									mObj = new V_MandatoGuida();
									mObj.Cognome = coll.Descrizione;
									mObj.NrNecropoli = mg.NrNecropoli != null ? mg.NrNecropoli : 0;
									mObj.TotaleNecropoli = mg.TotaleNecropoli != null ? mg.TotaleNecropoli : 0;
									mObj.NrAltro = mg.NrAltro != null ? mg.NrAltro : 0;
									mObj.TotaleAltro = mg.TotaleAltro != null ? mg.TotaleAltro : 0;
									mObj.AccontoAltro = mg.AccontoAltro != null ? mg.AccontoAltro : 0;
									collegeList.Add(mObj);
								}
							}
						}
					}
					else
					{
						filteredList.Add(mg);
					}
				}

				filteredList.AddRange(collegeList);

				return filteredList;
			}
		}

		private V_MandatoGuida FindMandatoGuidaCollegio(List<V_MandatoGuida> list, string collegeName)
		{
			V_MandatoGuida obj = null;

			foreach (V_MandatoGuida mg in list)
			{
				if (mg.Cognome == collegeName)
				{
					obj = mg;
					break;
				}
			}

			return obj;
		}
	}
}
