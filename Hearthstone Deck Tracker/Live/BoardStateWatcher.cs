using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Live.Data;

namespace Hearthstone_Deck_Tracker.Live
{
	internal class BoardStateWatcher
	{
		private bool _update;
		private bool _running;
		private BoardState _currentBoardState;

		public event Action<BoardState> OnNewBoardState;

		public void Stop()
		{
			_update = false;
			_currentBoardState = null;
		}

		public async void Start()
		{
			if(_running)
				return;
			_running = true;
			_update = true;
			while(_update)
			{
				var boardState = GetBoardState();
				if(!boardState?.Equals(_currentBoardState) ?? false)
				{
					OnNewBoardState?.Invoke(boardState);
					_currentBoardState = boardState;
				}
				await Task.Delay(1000);
			}
			_running = false;
		}

		private BoardState GetBoardState()
		{
			if(Core.Game.PlayerEntity == null || Core.Game.OpponentEntity == null)
				return null;

			int ZonePosition(Entity e) => e.GetTag(GameTag.ZONE_POSITION);
			int DbfId(Entity e) => e?.Card.DbfIf ?? 0;
			int[] SortedDbfIds(IEnumerable<Entity> entities) => entities.OrderBy(ZonePosition).Select(DbfId).ToArray();
			int HeroId(Entity playerEntity) => playerEntity.GetTag(GameTag.HERO_ENTITY);
			Entity Find(Player p, int entityId) => p.PlayerEntities.FirstOrDefault(x => x.Id == entityId);

			var player = Core.Game.Player;
			var opponent = Core.Game.Opponent;

			var playerHero = HeroId(Core.Game.PlayerEntity);

			var fullDeckList = DeckList.Instance.ActiveDeckVersion?.Cards.ToDictionary(x => x.DbfIf, x => x.Count);
			int FullCount(int dbfId) => fullDeckList == null ? 0 : fullDeckList.TryGetValue(dbfId, out var count) ? count : 0;

			return new BoardState
			{
				PlayerDeck = player.PlayerCardList.ToDictionary(x => x.DbfIf, x => new []{x.Count, FullCount(x.DbfIf)}),
				PlayerBoard = SortedDbfIds(player.Board.Where(x => x.IsMinion)),
				PlayerHand = SortedDbfIds(player.Hand),
				OpponentBoard = SortedDbfIds(opponent.Board.Where(x => x.IsMinion)),
				PlayerHero = DbfId(Find(player, playerHero)),
			};
		}
	}
}
