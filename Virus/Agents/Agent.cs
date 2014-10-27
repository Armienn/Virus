using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirusNameSpace
{
	public abstract class Agent
	{
		protected byte playerNumber = 1;

		public abstract Move Move(Virus percept);

		public abstract void EndGame(Virus percept);
	}
}
