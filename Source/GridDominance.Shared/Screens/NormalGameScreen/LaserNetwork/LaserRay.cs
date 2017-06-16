﻿using System;
using System.Collections.Generic;
using GridDominance.Shared.Screens.NormalGameScreen.Entities;
using MonoSAMFramework.Portable.GameMath.Geometry;

namespace GridDominance.Shared.Screens.NormalGameScreen.LaserNetwork
{
	public enum LaserRayTerminator { OOB, VoidObject, Portal, Glass, Mirror, Target, LaserMultiTerm, LaserSelfTerm, LaserFaultTerm }
	
	public sealed class LaserRay
	{
		public FPoint Start;
		public FPoint End;

		public LaserRayTerminator Terminator;

		public Cannon TerminatorCannon;

		public List<Tuple<LaserRay, LaserSource>> TerminatorRays;         // Rays that directcollide with this one
		public List<LaserRay> SelfCollRays = new List<LaserRay>(); // Rays that [[LaserSelfTerm]] with this one

		public readonly LaserRay Source;
		public readonly int Depth;
		public readonly bool InGlass;
		public readonly object StartIgnoreObj;
		public readonly object EndIgnoreObj;
		public readonly float SourceDistance; // At [[Start]]

		public float Length => (End - Start).Length();

		public LaserRay(FPoint s, FPoint e, LaserRay src, LaserRayTerminator t, int d, bool g, object sign, object eign, float sd, Cannon tc)
		{
			Depth = d;
			InGlass = g;
			StartIgnoreObj = sign;
			EndIgnoreObj   = eign;
			Start = s;
			End = e;
			Source = src;
			Terminator = t;
			TerminatorCannon = tc;
			TerminatorRays = new List<Tuple<LaserRay, LaserSource>>();
			SourceDistance = sd;
		}

		public void SetLaserIntersect(FPoint e, LaserRay otherRay, LaserSource otherSource, LaserRayTerminator t)
		{
			End = e;
			Terminator = t;
			TerminatorCannon = null;

			TerminatorRays.Add(Tuple.Create(otherRay, otherSource));
		}

		public void SetLaserCollisionlessIntersect(FPoint e, LaserRay otherRay, LaserSource otherSource, LaserRayTerminator t)
		{
			End = e;
			Terminator = t;
			TerminatorCannon = null;

			TerminatorRays.Clear();
		}
	}
}
