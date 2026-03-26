using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class ConflictResult
	{
		public ConflictType Conflict { get; set; }
		public List<Conflict> Conflicts = new List<Conflict>();
	}
	public enum ConflictType
	{
		NoConflict = 0,
		Acceptable,
		Unacceptable
	}
	public class Conflict
	{
		public Conflict() { }

		public Conflict(int id, ConflictType conflictType)
		{
			this.ID = id;
			this.ConflictType = conflictType;
		}

		public int ID { get; set; }
		public ConflictType ConflictType { get; set; }
	}
}
