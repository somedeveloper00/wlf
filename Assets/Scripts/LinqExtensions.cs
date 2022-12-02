using System;
using System.Linq;

namespace UIFlex
{
	internal static class LinqExtensions
	{
		
		public static void ForEach<T>(this T[] array, Action<T> callback)
		{
			if (array == null) throw new NullReferenceException(nameof(array));
			if(callback == null) return;
			
			for (int i = 0; i < array.Length; i++) 
				callback(array[i]);
		}
	}
}