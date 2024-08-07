using System;
using System.IO;
using System.Text;
using System.Globalization;

using ClassicUO.Game.Data;

namespace Assistant
{
	internal class Utility
	{
		private static Random m_Random = new Random();

		internal static int Random(int min, int max)
		{
			return m_Random.Next(max - min + 1) + min;
		}

		internal static int Random(int num)
		{
			return m_Random.Next(num);
		}

		public static bool InRange(IPoint2D from, IPoint2D to, int range)
		{
			return (to.X >= (from.X - range))
				   && (to.X <= (from.X + range))
				   && (to.Y >= (from.Y - range))
				   && (to.Y <= (from.Y + range));
		}

		public static bool InRangeZ(short from, short to, int range = 8)
		{
			return (to >= (from - range))
				   && (to <= (from + range))
				   && (to >= (from - range))
				   && (to <= (from + range));
		}

		public static int Distance(int fx, int fy, int tx, int ty)
		{
			int xDelta = Math.Abs(fx - tx);
			int yDelta = Math.Abs(fy - ty);

			return (xDelta > yDelta ? xDelta : yDelta);
		}

		public static int Distance(IPoint2D from, IPoint2D to)
		{
			int xDelta = Math.Abs(from.X - to.X);
			int yDelta = Math.Abs(from.Y - to.Y);

			return (xDelta > yDelta ? xDelta : yDelta);
		}

		public static double DistanceSqrt(IPoint2D from, IPoint2D to)
		{
			float xDelta = Math.Abs(from.X - to.X);
			float yDelta = Math.Abs(from.Y - to.Y);

			return Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
		}

		internal static void Offset(Direction d, ref int x, ref int y)
		{
			switch (d & Direction.Up)
			{
				case Direction.North: --y; break;
				case Direction.South: ++y; break;
				case Direction.West: --x; break;
				case Direction.East: ++x; break;
				case Direction.Right: ++x; --y; break;
				case Direction.Left: --x; ++y; break;
				case Direction.Down: ++x; ++y; break;
				case Direction.Up: --x; --y; break;
			}
		}

		private static char[] pathChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
		internal static string PathDisplayStr(string path, int maxLen)
		{
			if (path == null || path.Length <= maxLen || path.Length < 5)
				return path;

			int first = (maxLen - 3) / 2;
			int last = path.LastIndexOfAny(pathChars);
			if (last == -1 || last < maxLen / 4)
				last = path.Length - first;
			first = maxLen - last - 3;
			if (first < 0)
				first = 1;
			if (last < first)
				last = first;

			return String.Format("{0}...{1}", path.Substring(0, first), path.Substring(last));
		}

		internal static string FormatSize(long size)
		{
			if (size < 1024) // 1 K
				return String.Format("{0:#,##0} B", size);
			else if (size < 1048576) // 1 M
				return String.Format("{0:#,###.0} KB", size / 1024.0);
			else
				return String.Format("{0:#,###.0} MB", size / 1048576.0);
		}

		internal static string FormatTime(int sec)
		{
			int m = sec / 60;
			int h = m / 60;
			m = m % 60;
			return String.Format("{0:#0}:{1:00}:{2:00}", h, m, sec % 60);
		}

		internal static string FormatTimeMS(int ms)
		{
			int s = ms / 1000;
			int m = s / 60;
			int h = m / 60;

			ms = ms % 1000;
			s = s % 60;
			m = m % 60;

			if (h > 0 || m > 55)
				return String.Format("{0:#0}:{1:00}:{2:00}.{3:000}", h, m, s, ms);
			else
				return String.Format("{0:00}:{1:00}.{2:000}", m, s, ms);
		}

		internal static int ToInt32(string str, int def)
		{
			if (str == null)
				return def;

			int val;
			if(str.StartsWith("0x"))
			{
				if (int.TryParse(str.Substring(2), NumberStyles.HexNumber, UOScript.Interpreter.Culture, out val))
					return val;
			}
			else if (int.TryParse(str, out val))
				return val;

			return def;
		}

		public static uint ToUInt32(string str, uint def)
		{
			if (str == null)
				return def;

			uint val;
			if (str.StartsWith("0x"))
			{
				if (uint.TryParse(str.Substring(2), NumberStyles.HexNumber, UOScript.Interpreter.Culture, out val))
					return val;
			}
			else if (uint.TryParse(str, out val))
				return val;

			return def;
		}

		public static double ToDouble(string str, double def)
		{
			if (str == null)
				return def;

			if (double.TryParse(str, out double d))
				return d;

			return def;
		}

		public static ushort ToUInt16(string str, ushort def)
		{
			if (str == null)
				return def;

			ushort val;
			if (str.StartsWith("0x"))
			{
				if (ushort.TryParse(str.Substring(2), NumberStyles.HexNumber, UOScript.Interpreter.Culture, out val))
					return val;
			}
			else if (ushort.TryParse(str, out val))
				return val;

			return def;
		}

		private static DateTime _NextMessageTime = DateTime.MinValue;
		public static void SendTimedWarning(string message, MsgLevel level = MsgLevel.Warning)
        {
			if(DateTime.UtcNow >= _NextMessageTime)
            {
				_NextMessageTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(20));
				UOSObjects.Player?.SendMessage(level, message);
            }
        }
	}
}
