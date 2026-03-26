using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Model;

namespace Scv_Dal
{
	public class MoneyCut_Dal
	{
		public static List<MoneyCut> GetCuts(List<MoneyCut> list, int money)
		{
			List<MoneyCut> moneyCutList = new List<MoneyCut>();
			foreach (MoneyCut m in list)
				moneyCutList.Add(new MoneyCut(m.Cut));

			foreach (MoneyCut mc in moneyCutList.OrderByDescending(x => x.Cut).ToList())
			{
				if (money >= mc.Cut)
				{
					mc.Pieces = money / mc.Cut;
					money -= (mc.Cut * mc.Pieces);
				}
			}

			return moneyCutList;
		}
	}
}
