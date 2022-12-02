using UnityEngine;

public class Cell : MonoBehaviour
{
	public CellType type;
	public bool IsYou;
	public bool IsWin;
	public bool CanPush;
}

public enum CellType
{
	Text, Wall, Baba, Flag
}