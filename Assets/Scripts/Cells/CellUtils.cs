using System;
using Unity.VisualScripting;

public static class CellExtensions
{
	public static void SetYou<T>(this Cell[] cells, bool value) where T : Cell =>
		SetYou( typeof(T), cells, value );

	private static void SetYou(Type type, Cell[] cells, bool value) {
		foreach (var c in cells)
			if ( c.GetType() == type )
				c.IsYou = value;
	}

	public static void RemoveAllYous(this Cell[] cells) {
		foreach (var type in typeof(This).Assembly.GetTypes())
			if ( type.IsAssignableFrom( typeof(Cell) ) )
				SetYou( type, cells, false );
	}
}