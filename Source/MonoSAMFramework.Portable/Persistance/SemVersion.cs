﻿using System;

namespace MonoSAMFramework.Portable.Persistance
{
	public struct SemVersion : IComparable<SemVersion>, IEquatable<SemVersion>
	{
		public static readonly SemVersion VERSION_1_0_0 = new SemVersion(1, 0, 0);

		public readonly UInt16 Mayor;
		public readonly UInt16 Minor;
		public readonly UInt16 Patch;

		public SemVersion(UInt16 mayor)
		{
			Mayor = mayor;
			Minor = 0;
			Patch = 0;
		}

		public SemVersion(UInt16 mayor, UInt16 minor)
		{
			Mayor = mayor;
			Minor = minor;
			Patch = 0;
		}

		public SemVersion(UInt16 mayor, UInt16 minor, UInt16 patch)
		{
			Mayor = mayor;
			Minor = minor;
			Patch = patch;
		}

		#region Compare, Equals, Operators

		public int CompareTo(SemVersion other)
		{
			if (this.Mayor < other.Mayor) return -1;
			if (this.Mayor > other.Mayor) return +1;

			if (this.Minor < other.Minor) return -1;
			if (this.Minor > other.Minor) return +1;

			if (this.Patch < other.Patch) return -1;
			if (this.Patch > other.Patch) return +1;

			return 0;
		}

		public bool Equals(SemVersion other)
		{
			return 
				other.Mayor == this.Mayor && 
				other.Minor == this.Minor && 
				other.Patch == this.Patch;
		}
		
		public override bool Equals(object obj)
		{
			if (obj is SemVersion)
				return Equals((SemVersion) obj);
			else
				return false;
		}
		
		public override int GetHashCode()
		{
			return (293 * Mayor) ^ (17 * Minor) ^ (1 * Patch);
		}

		public static bool operator ==(SemVersion left, SemVersion right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SemVersion left, SemVersion right)
		{
			return !left.Equals(right);
		}

		public static bool operator <(SemVersion left, SemVersion right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator >(SemVersion left, SemVersion right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator <=(SemVersion left, SemVersion right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >=(SemVersion left, SemVersion right)
		{
			return left.CompareTo(right) >= 0;
		}

		#endregion

		public override string ToString()
		{
			return $"{Mayor}.{Minor}.{Patch}";
		}

		public bool IsLaterThan(SemVersion other)
		{
			return other > this;
		}
	}
}