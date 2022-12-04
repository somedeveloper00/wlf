using System;
using System.Collections.Generic;
using System.Linq;
using AnimFlex.Tweening;
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

	public void ExecuteCommand(string command) {
		var s = command.Split( ' ' );
		if ( s.Length < 3 || s.Length % 2 == 0 ) return;
		// check if it's a multi-statement command
		if ( s.Length > 3 ) {
			// turn them to multiple simple command string
			var last_is = s[s.Length-2];
			var assignment = s[s.Length-1];
			for (int i = 0; i < (s.Length - 1) / 2; i++) {
				ExecuteCommand( $"{s[i * 2]} {last_is} {assignment}" );
			}
			return;
		}
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
				case "block":
					Debug.Log( $"{type} {_is} block" );
					cells.SetIsBlock( type, _is );
					break;
			}
		}
	}

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
			if ( destCell.IsBlock || destCell.type == CellType.Border )
				return;

			if ( destCell.CanPush || destCell.IsYou ) {
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
						// Debug.Log( $"cant push {next.type}" );
						can = false;
						break;
					}
					if ( next.IsBlock || destCell.type == CellType.Border ) {
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
							.onComplete += () => onMoveEnd( c );
					} );
				}
				else {
					return;
				}
			}
		}
		
		_inMovings++;
		var anim = movable.transform.AnimPositionTo( dest * cellDistance, ease, duration, delay );
		anim.onComplete += () => onMoveEnd( movable, destCell );

		void onMoveEnd(Cell cell, Cell destCell = null) {
			_inMovings--;
			updateCommandsFromTexts();
			if (destCell == null) return;
			if ( cell.IsYou && destCell.IsWin || cell.IsWin && destCell.IsYou ) {
				onWin();
			} else if ( cell.IsYou && destCell.IsDie || cell.IsDie && destCell.IsYou ) {
				onDie();
			}
		}
	}

	void updateCommandsFromTexts() {
		// flushing previous results
		Debug.Log( "previous commands flushed." );
		cells.Where( c => c.type != CellType.Text ).ToList()
			.ForEach( c => c.CanPush = c.IsBlock = c.IsWin = c.IsYou = false );
		// finding new results 
		foreach (var cell in cells) {
			if (cell.type != CellType.Text) continue;
			var neighbors = cells.ToList().Where( c => c.type == CellType.Text && c != cell && Neighbor( c, cell ) ).ToList();
			neighbors.ForEach( c => process_neighbors_at( cell, c.transform.position - cell.transform.position ) );
		}

		void process_neighbors_at(Cell cell, Vector2 dir) {

			string r = cell.Text;
			int count = 1;
			
			while (true) {
				var next = cells.ToList().Find( c => c.type == CellType.Text && c != cell && Vector2.Distance(c.transform.position - cell.transform.position, dir) < 0.1f );
				if ( next == null ) break;
				r += " " + next.Text;
				count++;
				cell = next;
				if ( count > 2 && count % 2 == 1 ) 
					ExecuteCommand( r );
			}

		}
		bool Neighbor(Cell c1, Cell c2) => Mathf.Abs( Vector2.Distance( c1.transform.position, c2.transform.position ) - cellDistance ) < 0.1f;
	}
	private void onWin() {
		SceneManager.LoadScene( nextSceneName );
	}

	public void RestartLevel() {
		SceneManager.LoadScene( SceneManager.GetActiveScene().name );
		
	}
	private void onDie() {
		RestartLevel();
	}
}