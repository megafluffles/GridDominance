﻿using Microsoft.Xna.Framework;
using MonoSAMFramework.Portable.Extensions;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace MonoSAMFramework.Portable.GameMath.Geometry
{
	[DataContract]
	[DebuggerDisplay("{" + nameof(DebugDisplayString) + ",nq}")]
	public struct FRotatedRectangle : IEquatable<FRotatedRectangle>, IFShape
	{
		public static readonly FRotatedRectangle Empty = new FRotatedRectangle(0, 0, 0, 0, FloatMath.RAD_000);

		[DataMember]
		public readonly float CenterX;
		[DataMember]
		public readonly float CenterY;
		[DataMember]
		public readonly float Width;
		[DataMember]
		public readonly float Height;
		[DataMember]
		public readonly float Rotation; //rads

		public FRotatedRectangle(float cx, float cy, float width, float height, float rot)
		{
			CenterX = cx;
			CenterY = cy;
			Width = width;
			Height = height;
			Rotation = rot;

			_cacheMostLeft   = null;
			_cacheMostRight  = null;
			_cacheMostTop    = null;
			_cacheMostBottom = null;
		}

		[Pure]
		public static bool operator ==(FRotatedRectangle a, FRotatedRectangle b)
		{
			return
				FloatMath.EpsilonEquals(a.CenterX, b.CenterX) &&
				FloatMath.EpsilonEquals(a.CenterY, b.CenterY) &&
				FloatMath.EpsilonEquals(a.Width, b.Width) &&
				FloatMath.EpsilonEquals(a.Height, b.Height) &&
				FloatMath.EpsilonEquals(a.Rotation, b.Rotation);
		}

		[Pure]
		public static bool operator !=(FRotatedRectangle a, FRotatedRectangle b)
		{
			return !(a == b);
		}
		
		public bool Contains(float x, float y)
		{
			return Contains(new FPoint(x, y));
		}

		[Pure]
		public bool Contains(FPoint p)
		{
			var rp = p.AsRotatedAround(Center, -Rotation);
			
			return
				rp.X >= CenterX - Width/2f  &&
				rp.Y >= CenterY - Height/2f &&
				rp.X <  CenterX + Width/2f  &&
				rp.Y <  CenterY + Height/2f ;
		}

		public bool Contains(Vector2 v)
		{
			var rp = v.RotateAround(Center, -Rotation);

			return
				rp.X >= CenterX - Width / 2f &&
				rp.Y >= CenterY - Height / 2f &&
				rp.X < CenterX + Width / 2f &&
				rp.Y < CenterY + Height / 2f;
		}

		[Pure]
		public FRotatedRectangle AsTranslated(float offsetX, float offsetY)
		{
			if (FloatMath.IsZero(offsetX) && FloatMath.IsZero(offsetY)) return this;

			return new FRotatedRectangle(CenterX + offsetX, CenterY + offsetY, Width, Height, Rotation);
		}

		IFShape IFShape.AsTranslated(float offsetX, float offsetY) => AsTranslated(offsetX, offsetY);

		[Pure]
		public FRotatedRectangle AsTranslated(Vector2 offset)
		{
			if (offset.IsZero()) return this;

			return new FRotatedRectangle(CenterX + offset.X, CenterY + offset.Y, Width, Height, Rotation);
		}

		IFShape IFShape.AsTranslated(Vector2 offset) => AsTranslated(offset);

		[Pure]
		public bool Contains(FPoint p, float delta)
		{
			var rp = p.AsRotatedAround(Center, -Rotation);

			return
				rp.X >= CenterX - Width/2f  - delta &&
				rp.Y >= CenterY - Height/2f - delta &&
				rp.X < (CenterX + Width/2f  + delta + delta) &&
				rp.Y < (CenterY + Height/2f + delta + delta);
		}

		[Pure]
		public override bool Equals(object obj)
		{
			return obj is FRotatedRectangle && this == (FRotatedRectangle)obj;
		}

		[Pure]
		public bool Equals(FRotatedRectangle other)
		{
			return this == other;
		}

		[Pure]
		public override int GetHashCode()
		{
			return Rotation.GetHashCode() ^ 7829 * CenterX.GetHashCode() ^ 7841 * CenterY.GetHashCode() ^ 7853 * Width.GetHashCode() ^ 7867 * Height.GetHashCode();
		}
		[Pure]
		public override string ToString()
		{
			return $"{{X:{CenterX} Y:{CenterY} Width:{Width} Height:{Height} Rotation:{FloatMath.ToDegree(Rotation)}°}}";
		}

		internal string DebugDisplayString => $"({CenterX}|{CenterY}):({Width}|{Height}):({FloatMath.ToDegree(Rotation)}°)";

		private float? _cacheMostLeft;
		private float? _cacheMostRight;
		private float? _cacheMostTop;
		private float? _cacheMostBottom;

		public float MostLeft   { get { if (_cacheMostLeft == null) CalcOuterCoords(); return _cacheMostLeft ?? 0; } }
		public float MostRight  { get { if (_cacheMostLeft == null) CalcOuterCoords(); return _cacheMostRight ?? 0; } }
		public float MostTop    { get { if (_cacheMostLeft == null) CalcOuterCoords(); return _cacheMostTop ?? 0; } }
		public float MostBottom { get { if (_cacheMostLeft == null) CalcOuterCoords(); return _cacheMostBottom ?? 0; } }

		private void CalcOuterCoords()
		{
			var p1 = new Vector2(+Width, -Width).Rotate(Rotation);
			var p2 = new Vector2(-Width, -Width).Rotate(Rotation);

			_cacheMostLeft   = FloatMath.Min(CenterX - p1.X, CenterX + p1.X, CenterX - p2.X, CenterX + p2.X);
			_cacheMostRight  = FloatMath.Max(CenterX - p1.X, CenterX + p1.X, CenterX - p2.X, CenterX + p2.X);
			_cacheMostTop    = FloatMath.Min(CenterY - p1.Y, CenterY + p1.Y, CenterY - p2.Y, CenterY + p2.Y);
			_cacheMostBottom = FloatMath.Max(CenterY - p1.Y, CenterY + p1.Y, CenterY - p2.Y, CenterY + p2.Y);
		}

		public bool IsEmpty => Math.Abs(Width) < FloatMath.EPSILON || Math.Abs(Height) < FloatMath.EPSILON;

		public FSize Size => new FSize(Width, Height);
		public float Area => Width * Height;
		public FPoint Center => new FPoint(CenterX, CenterY);
		public Vector2 VecCenter => new Vector2(CenterX, CenterY);
	}
}