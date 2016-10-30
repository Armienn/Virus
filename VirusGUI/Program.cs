using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using VirusNameSpace;

namespace VirusGUI
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application. Du må ikke kigge kristjan !
		/// </summary>
		[STAThread]
		static void Main(String[] args) {
			int boardsize = 8;
			int tilesize = 40;
			bool immediateness = true;

			String[] players = new String[] { "AnnAI", "MixedAI" };

			if (args.Length > 0) {
				boardsize = int.Parse(args[0]);
			}
			if (args.Length > 1) {
				tilesize = int.Parse(args[1]);
			}
			if (args.Length > 2) {
				immediateness = bool.Parse(args[2]);
			}
			if (args.Length > 3) {
				players = new String[args.Length - 3];
				for (int i = 0; i < players.Length; i++) {
					players[i] = args[i + 3];
				}
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Virus virus = new Virus(players.Length, boardsize);
			Application.Run(new VirusInterface(virus, tilesize, immediateness, players));
		}
	}
}
