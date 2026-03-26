using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Model;
using Scv_Entities;

namespace Scv_Dal
{
	public class Receipt_Dal
	{
		public List<ReceiptVisitPage> GetVisitPages(List<V_VisiteProgrammate> list)
		{
			List<ReceiptVisitPage> vList = new List<ReceiptVisitPage>();
			ReceiptVisitPage obj = null;

			int maxVisitPerSide = 2;
			int MaxSides = 2;

			int visitPerSide = 0;
			int side = 0;

			foreach(V_VisiteProgrammate vp in list)
			{

				switch (visitPerSide)
				{
					case 0:
						switch (side)
						{
							case 0:
								obj = new ReceiptVisitPage();
								obj.VisitHour1 = vp.Ora_Visita;
								obj.VisitLanguage1 = vp.LinguaVisita;
								obj.VisitInt1 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
								obj.VisitRid1 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
                                obj.VisitScont1 = vp.Nr_Scontati != null ? ((int)vp.Nr_Scontati).ToString() : string.Empty;
                                obj.VisitCum1 = vp.Nr_Cumulativi != null ? ((int)vp.Nr_Cumulativi).ToString() : string.Empty;
								obj.VisitOma1 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
								break;

							case 1:
								obj.VisitHour4 = vp.Ora_Visita;
								obj.VisitLanguage4 = vp.LinguaVisita;
								obj.VisitInt4 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
								obj.VisitRid4 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
                                obj.VisitScont4 = vp.Nr_Scontati != null ? ((int)vp.Nr_Scontati).ToString() : string.Empty;
                                obj.VisitCum4 = vp.Nr_Cumulativi != null ? ((int)vp.Nr_Cumulativi).ToString() : string.Empty;
								obj.VisitOma4 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;								
								break;

							case 2:
								obj.VisitHour7 = vp.Ora_Visita;
								obj.VisitLanguage7 = vp.LinguaVisita;
								obj.VisitInt7 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
                                obj.VisitRid7 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;                                
                                obj.VisitScont7 = vp.Nr_Scontati != null ? ((int)vp.Nr_Scontati).ToString() : string.Empty;
                                obj.VisitCum7 = vp.Nr_Cumulativi != null ? ((int)vp.Nr_Cumulativi).ToString() : string.Empty;
								obj.VisitOma7 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
								break;

						}
						break;

					case 1:
						switch (side)
						{
							case 0:
								obj.VisitHour2 = vp.Ora_Visita;
								obj.VisitLanguage2 = vp.LinguaVisita;
								obj.VisitInt2 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
								obj.VisitRid2 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
                                obj.VisitScont2 = vp.Nr_Scontati != null ? ((int)vp.Nr_Scontati).ToString() : string.Empty;
                                obj.VisitCum2 = vp.Nr_Cumulativi != null ? ((int)vp.Nr_Cumulativi).ToString() : string.Empty;
								obj.VisitOma2 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
								break;

							case 1:
								obj.VisitHour5 = vp.Ora_Visita;
								obj.VisitLanguage5 = vp.LinguaVisita;
								obj.VisitInt5 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
								obj.VisitRid5 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
                                obj.VisitScont5 = vp.Nr_Scontati != null ? ((int)vp.Nr_Scontati).ToString() : string.Empty;
                                obj.VisitCum5 = vp.Nr_Cumulativi != null ? ((int)vp.Nr_Cumulativi).ToString() : string.Empty;
								obj.VisitOma5 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
								break;

							case 2:
								obj.VisitHour8 = vp.Ora_Visita;
								obj.VisitLanguage8 = vp.LinguaVisita;
								obj.VisitInt8 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
								obj.VisitRid8 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
                                obj.VisitScont8 = vp.Nr_Scontati != null ? ((int)vp.Nr_Scontati).ToString() : string.Empty;
                                obj.VisitCum8 = vp.Nr_Cumulativi != null ? ((int)vp.Nr_Cumulativi).ToString() : string.Empty;
								obj.VisitOma8 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
								break;

						}
						break;

					case 2:
						switch (side)
						{
							case 0:
								obj.VisitHour3 = vp.Ora_Visita;
								obj.VisitLanguage3 = vp.LinguaVisita;
								obj.VisitInt3 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
								obj.VisitRid3 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
                                obj.VisitScont3 = vp.Nr_Scontati != null ? ((int)vp.Nr_Scontati).ToString() : string.Empty;
                                obj.VisitCum3 = vp.Nr_Cumulativi != null ? ((int)vp.Nr_Cumulativi).ToString() : string.Empty;
								obj.VisitOma3 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
								break;

							case 1:
								obj.VisitHour6 = vp.Ora_Visita;
								obj.VisitLanguage6 = vp.LinguaVisita;
								obj.VisitInt6 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
								obj.VisitRid6 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
                                obj.VisitScont6 = vp.Nr_Scontati != null ? ((int)vp.Nr_Scontati).ToString() : string.Empty;
                                obj.VisitCum6 = vp.Nr_Cumulativi != null ? ((int)vp.Nr_Cumulativi).ToString() : string.Empty;
								obj.VisitOma6 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
								break;

							case 2:
								obj.VisitHour9 = vp.Ora_Visita;
								obj.VisitLanguage9 = vp.LinguaVisita;
								obj.VisitInt9 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
								obj.VisitRid9 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
                                obj.VisitScont9 = vp.Nr_Scontati != null ? ((int)vp.Nr_Scontati).ToString() : string.Empty;
                                obj.VisitCum9 = vp.Nr_Cumulativi != null ? ((int)vp.Nr_Cumulativi).ToString() : string.Empty;
								obj.VisitOma9 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
								break;

						}
						break;

					//case 3:
					//    switch (side)
					//    {
					//        case 0:
					//            obj.VisitHour4 = vp.Ora_Visita;
					//            obj.VisitLanguage4 = vp.LinguaVisita;
					//            obj.VisitInt4 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
					//            obj.VisitRid4 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
					//            obj.VisitOma4 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
					//            break;

					//        case 1:
					//            obj.VisitHour10 = vp.Ora_Visita;
					//            obj.VisitLanguage10 = vp.LinguaVisita;
					//            obj.VisitInt10 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
					//            obj.VisitRid10 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
					//            obj.VisitOma10 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
					//            break;
					//    }
					//    break;

					//case 4:
					//    switch (side)
					//    {
					//        case 0:
					//            obj.VisitHour5 = vp.Ora_Visita;
					//            obj.VisitLanguage5 = vp.LinguaVisita;
					//            obj.VisitInt5 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
					//            obj.VisitRid5 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
					//            obj.VisitOma5 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
					//            break;

					//        case 1:
					//            obj.VisitHour11 = vp.Ora_Visita;
					//            obj.VisitLanguage11 = vp.LinguaVisita;
					//            obj.VisitInt11 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
					//            obj.VisitRid11 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
					//            obj.VisitOma11 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
					//            break;
					//    }
					//    break;

					//case 5:
					//    switch (side)
					//    {
					//        case 0:
					//            obj.VisitHour6 = vp.Ora_Visita;
					//            obj.VisitLanguage6 = vp.LinguaVisita;
					//            obj.VisitInt6 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
					//            obj.VisitRid6 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
					//            obj.VisitOma6 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
					//            break;

					//        case 1:
					//            obj.VisitHour12 = vp.Ora_Visita;
					//            obj.VisitLanguage12 = vp.LinguaVisita;
					//            obj.VisitInt12 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
					//            obj.VisitRid12 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
					//            obj.VisitOma12 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
					//            break;
					//    }
					//    break;

					//case 6:
					//    switch (side)
					//    {
					//        case 0:
					//            obj.VisitHour7 = vp.Ora_Visita;
					//            obj.VisitLanguage7 = vp.LinguaVisita;
					//            obj.VisitInt7 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
					//            obj.VisitRid7 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
					//            obj.VisitOma7 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
					//            break;

					//        case 1:
					//            obj.VisitHour19 = vp.Ora_Visita;
					//            obj.VisitLanguage19 = vp.LinguaVisita;
					//            obj.VisitInt19 = vp.Nr_Interi != null ? ((int)vp.Nr_Interi).ToString() : string.Empty;
					//            obj.VisitRid19 = vp.Nr_Ridotti != null ? ((int)vp.Nr_Ridotti).ToString() : string.Empty;
					//            obj.VisitOma19 = vp.Nr_Omaggio != null ? ((int)vp.Nr_Omaggio).ToString() : string.Empty;
					//            break;
					//    }
					//    break;
				}

				if (visitPerSide++ == maxVisitPerSide)
				{
					visitPerSide = 0;
					if (side++ == MaxSides)
					{
						if (obj != null)
							vList.Add(obj);

						obj = new ReceiptVisitPage();
						visitPerSide = 0;
						side = 0;
					}
				}
			}

			if (obj != null)
				vList.Add(obj);

			return vList;
		}
	}
}
