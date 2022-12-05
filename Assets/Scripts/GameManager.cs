using System;
using System.Collections.Generic;
using System.Linq;
using AnimFlex.Sequencer;
using AnimFlex.Sequencer.UserEnd;
using AnimFlex.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public Main input;
	
	public Cell[] cells = Array.Empty<Cell>();

	public WinCondition winCondition;
	public SequenceAnim onLoseAnim;
	public SequenceAnim onWinAnim;
	
	
	[Serializable]
	public class WinCondition
	{
		public string commandMet = "blah is you";
		public CellType[] fromTypes;
		public CellType[] toTypes;
	}

	private void OnValidate() {
		winCondition ??= new();
		winCondition.commandMet = winCondition.commandMet.ToLower();
	}

	[SerializeField] public string nextSceneName;
	[SerializeField] float cellDistance = 10;
	[SerializeField] float duration = 1;
	[SerializeField] float delay = 0.2f;
	[SerializeField] Ease ease = Ease.InOutCirc;

	private int _inMovings = 0;


	void Awake() {
		cells = FindObjectsOfType<Cell>();
		input = new();
		input.player.up.performed += (_) => onMoveUpInput();
		input.player.down.performed += (_) => onMoveDownInput();
		input.player.right.performed += (_) => onMoveRightInput();
		input.player.left.performed += (_) => onMoveLeftInput();
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
				case "death":
					Debug.Log( $"{type} {_is} death" );
					cells.SetIsDeath( type, _is );
					break;
			}
		}
	}

	public void onMoveUpInput() => moveYous( Vector2.up );
	public void onMoveDownInput() => moveYous( Vector2.down );
	public void onMoveRightInput() => moveYous( Vector2.right );
	public void onMoveLeftInput() => moveYous( Vector2.left );

	public void moveYous(Vector2 dir) {
		if(_inMovings > 0) return;
		foreach (var movable in cells.Where( c => c.IsYou )) 
			moveCell( dir * cellDistance, movable );
	}

	private void moveCell(Vector2 dir, Cell movable) {
		
		var dest = (Vector2)movable.transform.position + dir;
		var destCells = cells.ToList().Where( c => Vector2.Distance( dest, c.transform.position ) < 0.1f ).ToList();

		var nextYou = destCells.Find( c => c.IsYou );
		if ( nextYou ) if ( !canPush( nextYou, dir ) ) return;

		if ( !canPush( movable, dir ) ) return;
		
		move( movable );
		
		// push all pushable cells
		Cell next = movable;
		while (true) {
			var pos = dir * cellDistance + (Vector2)next.transform.position;
			var pushables = cells.Where( c => c.CanPush && Vector2.Distance( c.transform.position, pos ) < 0.1f ).ToList();
			if ( pushables.Count == 0 ) return;
			if ( !canPush( pushables[0], dir ) ) return;
			pushables.ForEach( move );
			next = pushables[0];
		}

		void move(Cell cell) {
			_inMovings++;
			var anim = cell.transform.AnimPositionTo( (Vector2)cell.transform.position + dir * cellDistance, ease, duration, delay );
			anim.onComplete += () => {
				_inMovings--;
				updateCommandsFromTexts();
				checkWinLose();
			};
		}
	}

	bool canPush(Cell cell, Vector2 dir) {
		Vector2 pos = (Vector2)cell.transform.position + dir;
		while (true) {
			var nexts = cells.Where( c => Vector2.Distance( c.transform.position, pos) < 0.1f ).ToArray();
			if ( nexts.Length == 0 ) return true;
			if ( nexts.Any( c => c.IsBlock ) ) return false;
			if ( nexts.Any( c => c.CanPush || c.IsYou ) ) {
				pos = (Vector2)nexts[0].transform.position + dir;
				continue;
			}
			return true;
		}
	}

	
	
	void updateCommandsFromTexts() {
		// flushing previous results
		Debug.Log( "previous commands flushed." );
		cells.Where( c => c.type != CellType.Text && c.type != CellType.Border ).ToList()
			.ForEach( c => c.IsDeath = c.CanPush = c.IsBlock = c.IsWin = c.IsYou = false );
		var commands = getCommandsFromTexts().ToList();
		commands.ForEach( ExecuteCommand );
	}
	
	/// <summary>gets all the possible matching commands from text cells </summary>
	IEnumerable<string> getCommandsFromTexts() {
		foreach (var cell in cells) {
			if (cell.type != CellType.Text) continue;
			var neighbors = cells.ToList().Where( c => c.type == CellType.Text && c != cell && Neighbor( c, cell ) ).ToList();
			
			foreach (var c in neighbors) {
				Cell cell1 = cell;
				Vector2 dir = c.transform.position - cell.transform.position;
				string r = cell1.Text;
				int count = 1;
			
				while (true) {
					var next = cells.ToList().Find( c1 => c1.type == CellType.Text && c1 != cell1 && Vector2.Distance(c1.transform.position - cell1.transform.position, dir) < 0.1f );
					if ( next == null ) break;
					r += " " + next.Text;
					count++;
					cell1 = next;
					if ( count > 2 && count % 2 == 1 ) 
						yield return r.ToLower();
				}
			}
		}

		bool Neighbor(Cell c1, Cell c2) => Mathf.Abs( Vector2.Distance( c1.transform.position, c2.transform.position ) - cellDistance ) < 0.1f;
	}
	
	private void checkWinLose() {
		if ( is_lost() ) {
			onLose();
		} else if ( is_won() ) {
			onWin();
		}
	}

	bool is_lost() {
		for (int i = 0; i < cells.Length - 1; i++) {
			for (int j = i; j < cells.Length; j++) {
				if ( !((cells[i].IsYou && cells[j].IsDeath) || (cells[i].IsDeath && cells[j].IsYou)) ) continue;
				if ( Vector2.Distance( cells[i].transform.position, cells[j].transform.position ) > 0.1f ) continue;
				return true;
			}
		}

		return false;
	}
	
	bool is_won() {
		var commands = getCommandsFromTexts().ToList();
		// if command condition is met
		if ( commands.Any( c => c == winCondition.commandMet ) ) {
			// if any of the two cells are colliding
			var tfrom = cells.Where( c => winCondition.fromTypes.Contains( c.type ) );
			var tto = cells.Where( c => winCondition.toTypes.Contains( c.type ) );
			// check if any of them are in same pos
			if ( tfrom.Any( c1 =>
				    tto.Any( c2 => Vector2.Distance( c1.transform.position, c2.transform.position ) < 0.1f ) ) ) {
				// it'll be enough 
				return true;
			}
		}
		return false;
	}

	
	private void onWin() {
		Debug.Log( "won" );
		onWinAnim.PlaySequence();
		enabled = false;
	}

	public void GoNextLevel() => SceneManager.LoadScene( nextSceneName );

	public void RestartLevel() {
		SceneManager.LoadScene( SceneManager.GetActiveScene().name );
		
	}
	private void onLose() {
		// onLoseAnim.onComplete += RestartLevel;
		onLoseAnim.PlaySequence();
		Debug.Log( "lost" );
		enabled = false;
	}
}