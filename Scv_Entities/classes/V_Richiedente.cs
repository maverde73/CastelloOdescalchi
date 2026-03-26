using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Entities
{
	public partial class V_Richiedente
	{
		public V_Richiedente()
		{
			this.Nome = string.Empty;
			this.Cognome = string.Empty;
			this.Id_Titolo = 1;
			this.Id_LinguaAbituale = 1;
		}
		public V_Richiedente(string nome, string cognome, int idTitolo, int idLinguaAbituale)
		{
			this.Nome = nome;
			this.Cognome = cognome;
			this.Id_Titolo = idTitolo;
			this.Id_LinguaAbituale = idLinguaAbituale;
		}
	}
}
