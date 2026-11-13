using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.otm.utility
{
	public static class Extensions
	{
		public static Color SetAlpha(this Color color, float alpha)
		{
			color.a = alpha;
			return color;
		}

		public static IEnumerable<T> GetEnumValues<T>()
		{
			if (typeof(T).BaseType != typeof(Enum))
			{
				throw new ArgumentException("T must be of type System.Enum");
			}
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static string ToOrdinal(int number)
		{
			if (number <= 0)
				return number.ToString();

			int lastTwo = number % 100;

			if (lastTwo >= 11 && lastTwo <= 13)
				return number + "th";

			return (number % 10) switch
			{
				1 => number + "st",
				2 => number + "nd",
				3 => number + "rd",
				_ => number + "th"
			};
		}
	}
}
