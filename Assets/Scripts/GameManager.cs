using System;
using System.Collections.Generic;
using System.Linq;
using AnimFlex.Tweening;
using TMPro;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public Main input;
	
	public Cell[] cells = Array.Empty<Cell>();

	[SerializeField] string nextSceneName;
	[SerializeField] float cellDistance = 10;
	[SerializeField] float duration = 1;
	[SerializeField] float delay = 0.2f;
	[SerializeField] Ease ease = Ease.InOutCirc;

	private int _inMovings = 0;


	void Awake() {
		cells = FindObjectsOfType<Cell>();
		input = new();
		input.player.up.performed += onMoveUpInput;
		input.player.down.performed += onMoveDownInput;
		input.player.right.performed += onMoveRightInput;
		input.player.left.performed += onMoveLeftInput;
	}

	private void Start() {
		updateCommandsFromTexts();
	}

	void OnEnable() => input.player.Enable();
	void OnDisable() => input.player.Disable();

#region cell change commands

	public void ExecuteCommand(string command) {
		var s = command.Split( ' ' );
		if ( s.Length != 3 ) return;
		if ( Enum.TryParse( typeof(CellType), s[0], true, out var t) ) {
			var type = (CellType)t;
			bool _is;
			if ( s[1] == "is" ) _is = true;
			else if ( s[1] == "not" ) _is = false;
			else return;
			
			switch (s[2].ToLower()) {
				case "you":
					Debug.Log( $"{type} {_is} you" );
					cells.SetYou( type, _is );
					break;
				case "win":
					Debug.Log( $"{type} {_is} win" );
					cells.SetWin( type, _is );
					break;
				case "canpush":
					Debug.Log( $"{type} {_is} canpush" );
					cells.SetCanPush( type, _is );
					break;
			}
		}
	}

#endregion

	void onMoveUpInput(InputAction.CallbackContext _) => moveYous( Vector2.up );
	void onMoveDownInput(InputAction.CallbackContext _) => moveYous( Vector2.down );
	void onMoveRightInput(InputAction.CallbackContext _) => moveYous( Vector2.right );
	void onMoveLeftInput(InputAction.CallbackContext _) => moveYous( Vector2.left );

	void moveYous(Vector2 dir) {
		if(_inMovings > 0) return;
		foreach (var movable in cells.Where( c => c.IsYou )) 
			moveCell( dir * cellDistance, movable );
	}

	private void moveCell(Vector2 dir, Cell movable) {

		var dest = (Vector2)movable.transform.position + dir;
		
		// check if destination is valid
		var destCell = cells.ToList().Find( c => Vector2.Distance( dest, c.transform.position ) < 0.1f );
		
		if ( destCell != null ) {
			if ( destCell.type == CellType.Wall ) {
				return;
			}
			if( destCell.IsWin ) {
				onWin();
				return;
			}

			if ( destCell.CanPush ) {
				var pushingCells = new List<Cell>();
				pushingCells.Add( destCell );
				
				// check if can be pushed further
				bool can;
				Cell n = destCell;
				while (true) {
					var pos = (Vector2)n.transform.position + dir;
					var next = cells.ToList().Find( c => Vector2.Distance( c.transform.position, pos ) < 0.1f );
					if ( next == null ) {
						can = true;
						break;
					}

					if ( !next.CanPush ) {
						Debug.Log( $"cant push {next.type}" );
						can = false;
						break;
					}
					if ( next.type == CellType.Wall ) {
						Debug.Log( $"cant push wall" );
						can = false;
						break;
					}
					pushingCells.Add( next );
					n = next;
				}

				if ( can ) {
					Debug.Log( $"pushing {string.Join( ", ", pushingCells.Select( c => c.type ) )}" );
					pushingCells.ForEach( c => {
						_inMovings++;
						var dest = (Vector2)c.transform.position + dir;
						c.transform.AnimPositionTo( dest * cellDistance, ease, duration, delay )
							.onComplete += () => _inMovings--;
					} );
				}
				else {
					return;
				}
			}
		}
		
		_inMovings++;
		var anim = movable.transform.AnimPositionTo( dest * cellDistance, ease, duration, delay );
		anim.onComplete += () => {
			_inMovings--;
			updateCommandsFromTexts();
		};
	}

	void updateCommandsFromTexts() {
		// flushing previous results
		Debug.Log( "previous commands flushed." );
		cells.Where( c => c.type != CellType.Text ).ToList()
			.ForEach( c => c.CanPush = c.IsWin = c.IsYou = false );
		
		foreach (var cell in cells) {
			// finding 2nd match
			foreach (var ncell in cells) {
				if( cell == ncell ) continue;
				// if dist is 1
				if ( Math.Abs( Vector2.Distance( cell.transform.position, ncell.transform.position ) - 1 ) < 0.1f ) {
					// finding 3rd match with the same distance pattern
					var pos = ncell.transform.position * 2 - cell.transform.position;
					var nncell = cells.ToList().Find( c => Vector2.Distance( c.transform.position, pos ) < 0.1f );
					if (nncell == null) continue;
					// making text
					var t1 = cell.GetComponentInChildren<TMP_Text>();
					var t2 = ncell.GetComponentInChildren<TMP_Text>();
					var t3 = nncell.GetComponentInChildren<TMP_Text>();
					if ( t1 == null || t2 == null || t3 == null ) continue;
					Debug.Log( $"text match : {t1.text} + {t2.text} + {t3.text}" );
					ExecuteCommand( $"{t1.text} {t2.text} {t3.text}" );
				}
			}
		}
	}
	private void onWin() {
		SceneManager.LoadScene( nextSceneName );
	}
}