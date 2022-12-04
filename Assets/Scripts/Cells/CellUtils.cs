using System.Linq;

public static class CellExtensions
{
	public static void SetYou(this Cell[] cells, CellType type, bool value) {
		cells.Where( c => c.type == type ).ToList().ForEach( c => c.IsYou = value );
	}

	public static void SetWin(this Cell[] cells, CellType type, bool value) {
		cells.Where( c => c.type == type ).ToList().ForEach( c => c.IsWin = value );
	}

	public static void SetCanPush(this Cell[] cells, CellType type, bool value) {
		cells.Where( c => c.type == type ).ToList().ForEach( c => c.CanPush = value );
	}
	public static void SetIsBlock(this Cell[] cells, CellType type, bool value) {
		cells.Where( c => c.type == type ).ToList().ForEach( c => c.IsBlock = true );
	}
	public static void SetIsDeath(this Cell[] cells, CellType type, bool value) {
		cells.Where( c => c.type == type ).ToList().ForEach( c => c.IsDeath = true );
	}
}