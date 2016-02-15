using System;

namespace BeRated.Cache
{
	class Vector
	{
		public int X { get; private set; }
		public int Y { get; private set; }
		public int Z { get; private set; }

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
