﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wormhole
{
	public interface IControllableShip
	{
		void Forward();
		void Backward();
		void StrafeLeft();
		void StrafeRight();
		void TurnRight();
		void TurnLeft();
		void Shoot();
		void ActivatePowerup();
	}
}
