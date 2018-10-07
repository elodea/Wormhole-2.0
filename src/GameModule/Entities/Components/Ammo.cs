﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using TaskForceUltra.src.GameModule.AI.strategies;

namespace TaskForceUltra.src.GameModule.Entities
{
	public class Ammo : Component, ICollides
	{
		private float lifetime;
		private int mass;
		protected float maxVel;
		protected float thrustForce;
		protected float turnRate;
		protected bool thrusting;

		public override int Mass { get { return base.Mass + mass; } }
		public int Damage { get; private set; }

		protected bool sleep;

		private CooldownHandler cdHandler;

		public Ammo(
			string id, string filePath, Point2D refPos, Point2D offsetPos, Shape shape,
			List<Color> colors, int mass, int damage, float lifetime, float vel,
			float turnRate, BoundaryStrategy boundaryStrat, Team team
		) : base(id, filePath, refPos, offsetPos, shape, colors, 1, SwinGame.VectorTo(0, 0), SwinGame.VectorTo(0, -1), boundaryStrat, team)
		{
			Damage = damage;
			this.lifetime = lifetime < 0 ? 0 : lifetime;
			this.mass = mass <= 0 ? 1 : mass;
			maxVel = vel;
			thrustForce = 0;
			this.turnRate = turnRate;
		}

		public override void Update() {
			if (sleep)
				return;

			base.Update();
			if (cdHandler != null) {
				cdHandler.Update();
				if (cdHandler.OnCooldown()) {
					Thrust(Dir);
				}
				else Kill(Team.None);
			}
		}

		public virtual void Thrust(Vector vDir) {
			thrusting = true;
			Vector deltaV = Dir.Multiply(thrustForce / mass);
			Vel = (Vel.AddVector(deltaV)).LimitToMagnitude(maxVel);
		}

		public override void Draw() {
			if (sleep)
				return;

			if (cdHandler != null)
				base.Draw();
		}

		public virtual void Init(Point2D pos, Vector dir, Vector Vel) {
			TeleportTo(pos);
			theta = Dir.AngleTo(dir) * Math.PI / 180;

			maxVel += Vel.Magnitude;
			thrustForce = maxVel;
			cdHandler = new CooldownHandler(lifetime * 1000);
			cdHandler.StartCooldown();
		}

		public void ReactToCollision(int dmg, Vector collidingVel, int collidingMass, Team collider, bool forceReaction = false) {
			Kill(Team.None);
		}

		public void Sleep() {
			sleep = true;
		}
	}

	/// <summary>
	/// Ammo Factory
	/// </summary>
	public class AmmoFactory : ComponentFactory
	{
		public override Component Create(JObject ammoObj, string path, IHandlesEntities entHandler, BoundaryStrategy boundaryStrat, Team team, Point2D parentPos, float mod = 1) {
			string id = ammoObj.Value<string>("id");
			List<Color> colors = Util.LoadColors(ammoObj.Value<JArray>("colors"));
			int mass = ammoObj.Value<int>("mass");
			int damage = (int)(ammoObj.Value<int>("damage") * mod);
			float lifetime = ammoObj.Value<float>("lifetime") * mod;
			float vel = ammoObj.Value<float>("vel") * mod;
			float maxVel = ammoObj.Value<float>("maxVel") * mod;
			float turnRate = ammoObj.Value<float>("turnRate") * mod;
			float scale = ammoObj.Value<float>("scale");
			JObject shapeObj = ammoObj.Value<JObject>("shape");
			Shape shape = new ShapeFactory().Create(shapeObj, scale, parentPos);
			string behaviour = ammoObj.Value<string>("behaviour");

			if (team == Team.Computer)
				colors = new List<Color> { Color.Yellow };

			switch(behaviour) {
				case "seek":
					JArray emitterObj = ammoObj.Value<JArray>("emitters");
					List<Component> emitters = new EmitterFactory().CreateList(emitterObj, entHandler, boundaryStrat, team, parentPos, mod);
					SeekAmmo result = new SeekAmmo(id, path, SwinGame.PointAt(0, 0), parentPos, shape, colors, mass, damage, lifetime, vel, maxVel, turnRate, emitters, boundaryStrat, entHandler, team);
					result.AIStrat = new ChaseStrategy(result, entHandler, 0);
					return result;
				case "static":
					return new Ammo(id, path, SwinGame.PointAt(0, 0), parentPos, shape, colors, mass, damage, lifetime, vel, turnRate, boundaryStrat, team);
				default:
					return new Ammo(id, path, SwinGame.PointAt(0, 0), parentPos, shape, colors, mass, damage, lifetime, vel, turnRate, boundaryStrat, team);
			}
		}
	}
}
