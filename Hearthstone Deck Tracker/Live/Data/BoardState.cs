using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Live.Data
{
	public class BoardState
	{
		[JsonProperty("player_hand")]
		public int[] PlayerHand { get; set; }

		[JsonProperty("player_board")]
		public int[] PlayerBoard { get; set; }

		[JsonProperty("player_deck")]
		public Dictionary<int, int[]> PlayerDeck { get; set; }

		[JsonProperty("opponent_board")]
		public int[] OpponentBoard { get; set; }

		[JsonProperty("player_hero")]
		public int PlayerHero { get; set; }

		public bool Equals(BoardState boardState)
		{
			if(PlayerHero != boardState?.PlayerHero)
				return false;
			if(PlayerDeck.Count != boardState.PlayerDeck.Count)
				return false;
			if(!PlayerDeck.All(pair => boardState.PlayerDeck.TryGetValue(pair.Key, out var value2) && ArrayEquals(pair.Value, value2)))
				return false;
			if(!ArrayEquals(PlayerHand, boardState.PlayerHand))
				return false;
			if(!ArrayEquals(PlayerBoard, boardState.PlayerBoard))
				return false;
			if(!ArrayEquals(OpponentBoard, boardState.OpponentBoard))
				return false;
			return true;
		}

		private bool ArrayEquals(IReadOnlyCollection<int> array1, IReadOnlyList<int> array2)
		{
			if(array1.Count != array2.Count)
				return false;
			return !array1.Where((item, i) => item != array2[i]).Any();
		}
	}
}
