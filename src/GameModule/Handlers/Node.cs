﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;

namespace TaskForceUltra.src.GameModule.Handlers
{
	public class Node
	{
		private Node parent;
		private Node[] childNodes;
		private Rectangle grid;
		private int minWidth;
		public List<ICollides> ICollidesList { get; private set; }
		public List<ICollides> CheckedList { get; private set; }

		public Node(Node parent, Rectangle grid, int minWidth) {
			this.parent = parent;
			this.grid = grid;
			this.minWidth = minWidth;
			ICollidesList = new List<ICollides>();
			CheckedList = new List<ICollides>();

			if (Math.Abs(grid.Width) > minWidth)
				CreateChildren();
		}

		private void CreateChildren() {
			childNodes = new Node[4];
			Rectangle[] grids = CreateGrids();

			for(int i=0; i<4; ++i) {
				childNodes[i] = new Node(this, grids[i], minWidth);
			}
		}

		private Rectangle[] CreateGrids() {
			Point2D gridCenter = SwinGame.PointAt(grid.X + (grid.Width / 2), grid.Y + (grid.Height / 2));

			return new Rectangle[4] {
				SwinGame.CreateRectangle(grid.TopLeft, gridCenter), //top left
				SwinGame.CreateRectangle(grid.CenterTop, grid.CenterRight), //top right
				SwinGame.CreateRectangle(gridCenter, grid.BottomRight), //bottom right
				SwinGame.CreateRectangle(grid.CenterLeft, grid.CenterBottom) //bottom left
			};
		}

		/// <summary>
		/// check collision against all other collidables in the node
		/// that haven't already been checked in the current tick
		/// </summary>
		public ICollides Collide(ICollides self) {
			Node n = FetchContaining(self);
			
			//guard
			if (n == null)
				return null;

			foreach (ICollides other in n.ICollidesList) {
				if (self != other && self.Team != other.Team && !CheckedList.Contains(other)) {
					CheckedList.Add(other);
					if (Colliding(self.BoundingBox, other.BoundingBox)) {
						return other;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Checks collision between line segments
		/// </summary>
		private bool Colliding(List<LineSegment> bounds1, List<LineSegment> bound2) {
			foreach(LineSegment l1 in bounds1) {
				foreach(LineSegment l2 in bound2) {
					if (SwinGame.LineSegmentsIntersect(l1, l2))
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Register collidable to the right node
		/// </summary>
		public void Register(ICollides obj) {
			//guard
			if (obj == null)
				return;

			//end condition
			if (childNodes == null) {
				ICollidesList.Add(obj);
				return;
			}

			//find the node which contains the entiites position
			foreach(Node n in childNodes) {
				//only explore traverse relevant nodes
				if (n.ContainsPos(obj.RealPos)) {
					n.Register(obj);
				}
			}
		}

		public void Deregister(ICollides obj) {
			Node n = FetchContaining(obj);

			if (n != null)
				n.ICollidesList.Remove(obj);
		}

		public void Clear() {
			ICollidesList?.Clear();

			//end condition
			if (childNodes == null)
				return;

			//traversing
			foreach (Node n in childNodes) {
				n.Clear();
			}
		}

		//traverse to find the node that contains the Collidable object
		private Node FetchContaining(ICollides obj) {
			//guard
			if (obj == null)
				return null;

			//end condition (no more child nodes)
			if (childNodes == null) {
				if (ICollidesList.Contains(obj))
					return this;
				else return null;
			}
			else {
				//traverse
				foreach (Node n in childNodes) {
					if (n.ContainsPos(obj.RealPos))
						return n.FetchContaining(obj);
				}

				return null;
			}
		}

		public bool ContainsPos(Point2D pos) {
			return pos.InRect(grid);
		}
	}
}
