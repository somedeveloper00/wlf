using System;
using TMPro;
using UnityEngine;

public class Cell : MonoBehaviour
{
	public CellType type;
	public bool IsYou;
	public bool IsWin;
	public bool CanPush;

	 TMP_Text text;
	public string Text => text != null ? text.text : string.Empty;

	private void Awake() => text = GetComponentInChildren<TMP_Text>();
}

public enum CellType
{
	Text, Wall, Baba, Flag, PapaNoel
}