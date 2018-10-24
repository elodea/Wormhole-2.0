﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForceUltra.src.GameModule.Entities;
using TaskForceUltra.src.GameModule.AI.strategies;
using SwinGameSDK;

namespace TaskForceUltra.src.GameModule.AI
{
	public abstract class AIStrategy
	{
		protected IAIEntity controlled;
		protected Vector targetDir;
		protected CooldownHandler shootCooldown;

		public AIStrategy(IAIEntity controlled, int shootCd) {
			this.controlled = controlled;

			//set random initial direction
			targetDir = Util.RandomUnitVector();
			shootCooldown = new CooldownHandler(shootCd * 1000);
		}

		public void Update() {
			if (controlled.IsDead)
				return;

			Shoot();
			ExecuteStrategy();
		}

		protected virtual void ExecuteStrategy() {
			if (controlled == null || controlled.IsDead)
				return;
		}

		protected void Shoot() {
			if (!shootCooldown.OnCooldown()) {
				controlled.Fire();
				shootCooldown.StartCooldown();
			}
		}
	}

	/// <summary>
	/// returns a randomised ai strategy based on some probability curve from a difficulty level
	/// </summary>
	/// /
	
	public class AIStrategyFactory
	{
		private int difficultyLevel;
		private int shootCooldown;

		public AIStrategyFactory(int difficultyLevel, int shootCooldown) {
			this.difficultyLevel = difficultyLevel;
			this.shootCooldown = shootCooldown;
		}

		public AIStrategy Create(IAIEntity aiShip, IHandlesEntities entHandler) {
			//generate random number up to the difficulty level
			int n = Util.Rand(difficultyLevel);

			//return the hardest strategy that the number can get
			if (n < 10) {
				return new StaticStrategy(aiShip, shootCooldown);
			}
			else if (n < 20) {
				return new ErraticStrategy(aiShip, shootCooldown);
			}
			else {
				return new ChaseStrategy(aiShip, entHandler, shootCooldown);
			}
		}
	}
}
