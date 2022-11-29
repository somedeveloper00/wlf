﻿using UnityEngine;

public interface ICell
{
	public bool IsYou { get; set; }
	public bool CanPass { get; set; }
	public bool CanPush { get; set; }
	public Vector2Int Position { get; set; }
}