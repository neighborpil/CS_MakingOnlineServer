using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FreeNet;
using VirusWarGameServer;

public class CBattleRoom : MonoBehaviour {

	enum GAME_STATE
	{
		READY = 0,
		STARTED
	}

	// ����, ���� ĭ ���� �ǹ��Ѵ�.
	public static readonly int COL_COUNT = 7;

	List<CPlayer> players;

	// �������� ���� �������� ���¸� ��Ÿ���� ������.
	List<short> board;

	// 0~49������ �ε����� ���� �ִ� ������ ������.
	List<short> table_board;

	// ���� ������ ������ ��Ÿ�� �� ����ϴ� ����Ʈ.
	List<short> available_attack_cells;

	// ���� ���� �������� �÷��̾� �ε���.
	byte current_player_index;

	// �������� �������� ������ �÷��̾� �ε���.
	byte player_me_index;

	// ��Ȳ�� ���� ��ġ �Է��� ó���ϱ� ���� ����.
	byte step;

	// ���� ���� �� �������� ���ư� �� ����ϱ� ���� MainTitle��ü�� ���۷���.
	CMainTitle main_title;

	// ��Ʈ��ũ ������ ��,������ ���� ��Ʈ��ũ �Ŵ��� ���۷���.
	CNetworkManager network_manager;

	// ���� ���¿� ���� ���� �ٸ� GUI����� �����ϱ� ���� �ʿ��� ���� ����.
	GAME_STATE game_state;

	// OnGUI�żҵ忡�� ȣ���� ��������Ʈ.
	// ���� ������ �żҵ带 ����� ���� ��Ȳ�� �°� draw�� �������ִ� ������� GUI�� �����Ų��.
	delegate void GUIFUNC();
	GUIFUNC draw;

	// �¸��� �÷��̾� �ε���.
	// ���º��϶��� byte.MaxValue�� ����.
	byte win_player_index;

	// ������ ǥ���ϱ� ���� �̹��� ���� ��ü.
	// �����ϰ� ���ڰ� ǥ���ϱ� ���� ��Ʈ ��� �̹����� ����� ����Ѵ�.
	CImageNumber score_images;

	// ���� �������� �÷��̾ ��Ÿ���� ��ü.
	CBattleInfoPanel battle_info;

	// ������ ����Ǿ������� ��Ÿ���� �÷���.
	bool is_game_finished;

	// ���� �̹��� �ؽ��ĵ�.
	List<Texture> img_players;
	Texture background;
	Texture blank_image;
	Texture game_board;

	Texture graycell;
	Texture focus_cell;

	Texture win_img;
	Texture lose_img;
	Texture draw_img;
	Texture gray_transparent;

	void Awake()
	{
		this.table_board = new List<short>();
		this.available_attack_cells = new List<short>();

		this.graycell = Resources.Load("images/graycell") as Texture;
		this.focus_cell = Resources.Load("images/border") as Texture;
		
		this.blank_image = Resources.Load("images/blank") as Texture;
		this.game_board = Resources.Load("images/gameboard") as Texture;
		this.background = Resources.Load("images/gameboard_bg") as Texture;
		this.img_players = new List<Texture>();
		this.img_players.Add(Resources.Load("images/red") as Texture);
		this.img_players.Add(Resources.Load("images/blue") as Texture);
		
		this.win_img = Resources.Load("images/win") as Texture;
		this.lose_img = Resources.Load("images/lose") as Texture;
		this.draw_img = Resources.Load("images/draw") as Texture;
		this.gray_transparent = Resources.Load("images/gray_transparent") as Texture;
		
		this.board = new List<short>();

		this.network_manager = GameObject.Find("NetworkManager").GetComponent<CNetworkManager>();

		this.game_state = GAME_STATE.READY;

		this.main_title = GameObject.Find("MainTitle").GetComponent<CMainTitle>();
		this.score_images = gameObject.AddComponent<CImageNumber>();

		this.win_player_index = byte.MaxValue;
		this.draw = this.on_gui_playing;
		this.battle_info = gameObject.AddComponent<CBattleInfoPanel>();
	}
	
	void reset()
	{
		// ������ �����͸� ��� �ʱ�ȭ �Ѵ�.
		this.board.Clear();
		this.table_board.Clear();
		for (int i = 0; i < COL_COUNT * COL_COUNT; ++i)
		{
			this.board.Add(short.MaxValue);
			this.table_board.Add((short)i);
		}

		// �����ǿ� �� �÷��̾���� ��ġ�� �Է��Ѵ�.
		this.players.ForEach(obj =>
		{
			obj.cell_indexes.ForEach(cell =>
			{
				this.board[cell] = obj.player_index;
			});
		});
	}


	void clear()
	{
		this.current_player_index = 0;
		this.step = 0;
		this.draw = this.on_gui_playing;
		this.is_game_finished = false;
	}

	/// <summary>
	/// ���ӹ濡 ������ �� ȣ��ȴ�. ���ҽ� �ε��� �����Ѵ�.
	/// </summary>
	public void start_loading(byte player_me_index)
	{
		clear();

		this.network_manager.message_receiver = this;
		this.player_me_index = player_me_index;

		CPacket msg = CPacket.create((short)PROTOCOL.LOADING_COMPLETED);
		this.network_manager.send(msg);
	}


	/// <summary>
	/// ��Ŷ�� ���� ���� �� ȣ���.
	/// </summary>
	/// <param name="protocol"></param>
	/// <param name="msg"></param>
	void on_recv(CPacket msg)
	{
		PROTOCOL protocol_id = (PROTOCOL)msg.pop_protocol_id();

		switch (protocol_id)
		{
			case PROTOCOL.GAME_START:
				on_game_start(msg);
				break;

			case PROTOCOL.PLAYER_MOVED:
				on_player_moved(msg);
				break;

			case PROTOCOL.START_PLAYER_TURN:
				on_start_player_turn(msg);
				break;

			case PROTOCOL.ROOM_REMOVED:
				on_room_removed();
				break;

			case PROTOCOL.GAME_OVER:
				on_game_over(msg);
				break;
		}
	}


	void on_room_removed()
	{
		if (!is_game_finished)
		{
			back_to_main();
		}
	}


	void back_to_main()
	{
		this.main_title.gameObject.SetActive(true);
		this.main_title.enter();

		gameObject.SetActive(false);
	}


	void on_game_over(CPacket msg)
	{
		this.is_game_finished = true;
		this.win_player_index = msg.pop_byte();
		this.draw = this.on_gui_game_result;
	}


	void Update()
	{
		if (this.is_game_finished)
		{
			if (Input.GetMouseButtonDown(0))
			{
				back_to_main();
			}
		}
	}

	void on_game_start(CPacket msg)
	{
		this.players = new List<CPlayer>();

		byte count = msg.pop_byte();
		for (byte i = 0; i < count; ++i)
		{
			byte player_index = msg.pop_byte();

			GameObject obj = new GameObject(string.Format("player{0}", i));
			CPlayer player = obj.AddComponent<CPlayer>();
			player.initialize(player_index);
			player.clear();

			byte virus_count = msg.pop_byte();
			for (byte index = 0; index < virus_count; ++index)
			{
				short position = msg.pop_int16();
				player.add(position);
			}

			this.players.Add(player);
		}

		this.current_player_index = msg.pop_byte();
		reset();

		this.game_state = GAME_STATE.STARTED;
	}


	void on_player_moved(CPacket msg)
	{
		byte player_index = msg.pop_byte();
		short from = msg.pop_int16();
		short to = msg.pop_int16();

		StartCoroutine(on_selected_cell_to_attack(player_index, from, to));
	}


	void on_start_player_turn(CPacket msg)
	{
		phase_end();

		this.current_player_index = msg.pop_byte();
	}



	float ratio = 1.0f;
	void OnGUI()
	{
		this.draw();
	}


	/// <summary>
	/// ���� ���� ȭ�� �׸���.
	/// </summary>
	void on_gui_playing()
	{
		if (this.game_state != GAME_STATE.STARTED)
		{
			return;
		}

		this.ratio = Screen.width / 800.0f;

		draw_board();
	}


	/// <summary>
	/// ��� ȭ�� �׸���.
	/// </summary>
	void on_gui_game_result()
	{
		on_gui_playing();

		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), this.gray_transparent);
		GUI.BeginGroup(new Rect(Screen.width / 2 - 173, Screen.height / 2 - 84, 
			this.win_img.width, this.win_img.height));
		{
			if (this.win_player_index == byte.MaxValue)
			{
				GUI.DrawTexture(new Rect(0, 0, this.draw_img.width, this.draw_img.height), this.draw_img);
			}
			else
			{
				// win, lose�̹��� ���.
				if (this.player_me_index == this.win_player_index)
				{
					GUI.DrawTexture(new Rect(0, 0, 346, 169), this.win_img);
				}
				else
				{
					GUI.DrawTexture(new Rect(0, 0, 346, 169), this.lose_img);
				}
			}

			// �ڱ� �ڽ��� �÷��̾� �̹��� ���.
			Texture character = this.img_players[this.player_me_index];
			GUI.DrawTexture(new Rect(28, 43, character.width, character.height), character);
		}
		GUI.EndGroup();
	}
	
	void draw_board()
	{
		float scaled_height = 480.0f * ratio;
		float gap_height = Screen.height - scaled_height;
		
		float outline_left = 0;
		float outline_top = gap_height * 0.5f;
		float outline_width = Screen.width;
		float outline_height = scaled_height;
		
		float hor_center = outline_width * 0.5f;
		float ver_center = outline_height * 0.5f;

		GUI.BeginGroup(new Rect(0, 0, outline_width, Screen.height));
		
		// Draw background to full of the screen.
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), this.background);

		// ���� ǥ��.
		// red(index 0)���� ����,
		// blue(index 1)���� �����ʿ� ǥ��.
		Rect redteam_rect = new Rect(outline_left + 20 * ratio, ver_center - 60 * ratio, 100 * ratio, 60 * ratio);
		Rect blueteam_rect = new Rect(outline_width - 120 * ratio, ver_center - 60 * ratio, 100 * ratio, 60 * ratio);
		this.score_images.print(this.players[0].get_virus_count(), redteam_rect.xMin, redteam_rect.yMin, ratio);
		this.score_images.print(this.players[1].get_virus_count(), blueteam_rect.xMin, blueteam_rect.yMin, ratio);

		// Draw a board(alignment : center).
		GUI.DrawTexture(new Rect(0, outline_top, outline_width, outline_height), this.game_board);

		// ���� �������� �� ǥ��.
		this.battle_info.draw_turn_info(this.current_player_index, ratio);
		this.battle_info.draw_myinfo(this.player_me_index, ratio);

		int width = (int)(60 * ratio);
		int celloutline_width = width * CBattleRoom.COL_COUNT;
		float half_celloutline_width = celloutline_width*0.5f;
		
		GUI.BeginGroup(new Rect(hor_center-half_celloutline_width, 
			ver_center-half_celloutline_width + outline_top, celloutline_width, celloutline_width));
		
		List<int> current_turn = new List<int>();
		short index = 0;
		for (int row=0; row<CBattleRoom.COL_COUNT; ++row)
		{
			int gap_y = 0;//(row * 1);
			for (int col=0; col<CBattleRoom.COL_COUNT; ++col)
			{
				int gap_x = 0;//(col * 1);
				
				Rect cell_rect = new Rect(col * width + gap_x, row * width + gap_y, width, width);
				if (GUI.Button(cell_rect, ""))
				{
					on_click(index);
				}
				
				if (this.board[index] != short.MaxValue)
				{
					int player_index = this.board[index];
					GUI.DrawTexture(cell_rect, this.img_players[player_index]);
					
					if (this.current_player_index == player_index)
					{
						GUI.DrawTexture(cell_rect, this.focus_cell);
					}
				}
				
				if (this.available_attack_cells.Contains(index))
				{
					GUI.DrawTexture(cell_rect, this.focus_cell);
				}
				
				++index;
			}
		}
		GUI.EndGroup();
		GUI.EndGroup();
	}

	short selected_cell = short.MaxValue;
	void on_click(short cell)
	{
		// �ڽ��� ���ʰ� �ƴϸ� ó������ �ʰ� �����Ѵ�.
		if (this.player_me_index != this.current_player_index)
		{
			return;
		}

		//Debug.Log(cell);
		
		switch(this.step)
		{
		case 0:
			if (validate_begin_cell(cell))
			{
				this.selected_cell = cell;
				Debug.Log("go to step2");
				this.step = 1;
				
				refresh_available_cells(this.selected_cell);
			}
			break;

		case 1:
			{
				// When you touched your cell again.
				if (this.players[this.current_player_index].cell_indexes.Exists(obj => obj == cell))
				{
					this.selected_cell = cell;
					refresh_available_cells(this.selected_cell);
					break;
				}

				// Cannot touch other player's cell.
				foreach (CPlayer player in this.players)
				{
					if (player.cell_indexes.Exists(obj => obj == cell))
					{
						return;
					}
				}


				if (CHelper.get_distance(this.selected_cell, cell) > 2)
				{
					// 2ĭ�� �ʰ��ϴ� �Ÿ��� �̵��� �� ����.
					return;
				}

				CPacket msg = CPacket.create((short)PROTOCOL.MOVING_REQ);
				msg.push(this.selected_cell);
				msg.push(cell);
				this.network_manager.send(msg);

				this.step = 2;
			}
			break;
		}
	}

	IEnumerator on_selected_cell_to_attack(byte player_index, short from, short to)
	{
		byte distance = CHelper.howfar_from_clicked_cell(from, to);
		if (distance == 1)
		{
			// copy to cell
			yield return StartCoroutine(reproduce(to));
		}
		else if (distance == 2)
		{
			// move
			this.board[from] = short.MaxValue;
			this.players[player_index].remove(from);
			yield return StartCoroutine(reproduce(to));
		}

		CPacket msg = CPacket.create((short)PROTOCOL.TURN_FINISHED_REQ);
		this.network_manager.send(msg);

		yield return 0;
	}
	
	void phase_end()
	{
		this.step = 0;
		this.available_attack_cells.Clear();
	}
	

	void refresh_available_cells(short cell)
	{
		this.available_attack_cells = CHelper.find_available_cells(cell, this.table_board, this.players);
	}
	
	void clear_available_attacking_cells()
	{
		this.available_attack_cells.Clear();
	}
	
	//IEnumerator moving()
	//{
	//}
	
	IEnumerator reproduce(short cell)
	{
		CPlayer current_player = this.players[this.current_player_index];
		CPlayer other_player = this.players.Find(obj => obj.player_index != this.current_player_index);
		
		clear_available_attacking_cells();
		//yield return new WaitForSeconds(0.5f);
		
		this.board[cell] = current_player.player_index;
		current_player.add(cell);

		yield return new WaitForSeconds(0.5f);
		
		// eat.
		List<short> neighbors = CHelper.find_neighbor_cells(cell, other_player.cell_indexes, 1);
		foreach (short obj in neighbors)
		{
			this.board[obj] = current_player.player_index;
			current_player.add(obj);
			
			other_player.remove(obj);
			
			yield return new WaitForSeconds(0.2f);
		}
	}
	
	bool validate_begin_cell(short cell)
	{
		return this.players[this.current_player_index].cell_indexes.Exists(obj => obj == cell);
	}
}
