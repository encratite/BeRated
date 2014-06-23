using System;

namespace BeRated.Common
{
	public class Vector
	{
		public readonly int X;
		public readonly int Y;
		public readonly int Z;

		public Vector(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public double Distance(Vector vector)
		{
			int dx = X - vector.X;
			int dy = Y - vector.Y;
			int dz = Y - vector.Z;
			double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
			return distance;
		}
	}
}
