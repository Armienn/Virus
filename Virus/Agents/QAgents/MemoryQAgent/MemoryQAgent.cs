﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VirusNameSpace.Agents
{
	public partial class MemoryQAgent : Agent
	{
		public MemoryQAgent(
			byte player,
			double disc = 0.98,
			double lrmod = 1,
			double lrstrt = 0,
			double exmod = 1,
			double exstrt = 0,
			double lrpow = 1,
			double expow = 0.5,
			double initvalue = 0) {
			playerNumber = player;
			discount = disc;
			learningRateModifier = lrmod;
			learningRateStart = lrstrt;
			learningRatePower = lrpow;
			explorationModifier = exmod;
			explorationStart = exstrt;
			explorationPower = expow;
			this.initvalue = initvalue;
			RandomRate = 0;
			MinLearning = 0;
		}

		public MemoryQAgent(byte player = 1) {
			playerNumber = player;
		}

		public override void EndGame(Virus percept) {
			VirusBoard newState = percept.GetBoardCopy();
			double reward = 0;
			if (percept.Winner == playerNumber) reward = 1;
			else if (percept.Winner != playerNumber && percept.Winner != 0) reward = -1;

			ShortTermMemory.Add(new VirusMemory(prevState, prevAction, newState, reward));

			prevState = default(VirusBoard);
			prevAction = default(Move);
			prevReward = 0;
		}

		public override Move Move(Virus percept) {
			VirusBoard newState = percept.GetBoardCopy();

			if (!prevState.Equals(default(VirusBoard))) {
				ShortTermMemory.Add(new VirusMemory(prevState, prevAction, newState));
			}

			prevState = newState;
			prevAction = GetMaxExplorationFunctionA(newState);
			prevReward = 0;
			return prevAction;
		}
	}
}
