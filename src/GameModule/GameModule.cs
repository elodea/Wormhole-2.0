﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForceUltra.src.GameModule.Entities;
using SwinGameSDK;

namespace TaskForceUltra.src.GameModule
{
	public class GameModule {
		public Level Level { get; private set; }

		private EntityHandler entityHandler;
		private CollisionHandler collisionHandler;
		private CameraHandler cameraHandler;
		private InputController inputController;
		private AISpawner aiSpawner;
		private Ship player;

		private Scoresheet scoresheet;
		private GameSendData gameSendData;
		private Timer gameTimer;

		private bool IsGameEnd { get { return player.IsDead; } }

		/*
		 * public Difficulty(string id, float spawnTimer, float rewardMod, float aiMod, int maxMass) {
			this.id = id;
			this.spawnTimer = spawnTimer;
			this.rewardMod = rewardMod;
			this.aiMod = aiMod;
			this.maxMass = maxMass;
			}
		*/

		public GameModule(Ship p, Level level, AISpawner aiSpawner, EntityHandler entHandler, CollisionHandler collHandler,
			CameraHandler camHandler, GameSendData gameSendData, Scoresheet scoresheet, InputController inpController
		)
		{
			player = p;
			Level = level;
			entityHandler = entHandler;
			collisionHandler = collHandler;
			cameraHandler = camHandler;
			inputController = inpController;
			this.scoresheet = scoresheet;
			this.aiSpawner = aiSpawner;
			this.gameSendData = gameSendData;

			gameTimer = SwinGame.CreateTimer();
			gameTimer.Start();
		}

		public void Update() {
			Level.Update();
			entityHandler.Update();
			collisionHandler.Update();
			cameraHandler.Update();
			inputController.Update();
			aiSpawner.Update();

			if (IsGameEnd)
				EndGame();
		}

		public void Draw() {
			Level.Draw();
			entityHandler.Draw();
		}

		public void EndGame() {
			gameSendData.Add(GameResultType.Points, scoresheet.FetchTeamScore(player.Team));
			gameSendData.Add(GameResultType.Result, player.IsDead ? 1 : 0);
			gameSendData.Add(GameResultType.Time, (int)gameTimer.Ticks);

			SwinGame.FreeTimer(gameTimer);

			gameSendData.Send();
		}
	}

	/// <summary>
	/// Game Module Factory
	/// </summary>
	public class GameModuleFactory
	{
		public GameModule Create(string shipId, Difficulty diff, Level level, ShipFactory shipFac, GameSendData gameSendData) {
			Scoresheet scoreSheet = new Scoresheet();

			EntityHandler entHandler = new EntityHandler(scoreSheet);
			CollisionHandler collHandler = new CollisionHandler(level.PlayArea, entHandler);

			Ship p = shipFac.Create(shipId, SwinGame.PointAt(100, 100), new WrapBoundaryBehaviour(level.PlayArea), ControllerType.Player1, diff, entHandler);
			entHandler.Track(p);

			CameraHandler camHandler = new CameraHandler(p, level.PlayArea);

			//ai spawning
			AISpawner aiSpawner = new AISpawner(diff, level.PlayArea, shipFac, entHandler);


			InputController inpController;
			if (p is IControllable) {
				InputControllerFactory inpContFac = new InputControllerFactory();
				inpController = inpContFac.Create(p as IControllable, ControllerType.Player1);
			}
			else {
				inpController = null;
			}

			return new GameModule(p, level, aiSpawner, entHandler, collHandler, camHandler, gameSendData, scoreSheet, inpController);
		}
	}

}
