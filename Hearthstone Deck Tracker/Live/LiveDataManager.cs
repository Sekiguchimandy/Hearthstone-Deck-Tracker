using System;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Live.Data;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Twitch;

namespace Hearthstone_Deck_Tracker.Live
{
	internal class LiveDataManager
	{
		private static BoardStateWatcher _boardStateWatcher;
		private static BoardStateWatcher BoardStateWatcher => _boardStateWatcher ?? (_boardStateWatcher = GetBoardStateWatcher());

		private static BoardStateWatcher GetBoardStateWatcher()
		{
			var boardStateWatcher = new BoardStateWatcher();
			boardStateWatcher.OnNewBoardState += OnNewBoardState;
			return boardStateWatcher;
		}

		public static async void WatchBoardState()
		{
			if(Config.Instance.SendLiveUpdates)
			{
				var twitchUserId = 0; // get via oauth
				var streaming = await TwitchApi.IsStreaming(twitchUserId);
			}
			BoardStateWatcher.Start();
			//PayloadDump.Clear();
		}

		public static void Stop()
		{
			BoardStateWatcher.Stop();
			SendUpdate(PayloadFactory.GameEnd());
			//var json = JsonConvert.SerializeObject(PayloadDump, Formatting.Indented);
			//using(var wr = new StreamWriter("D:/hdt-payload-dump.json"))
			//	wr.Write(json);
		}

		private static DateTime _lastSent = DateTime.MinValue;
		private static int _currentHash;
		private static async void SendUpdate(Payload payload)
		{
			var hash = payload.GetHashCode();
			_currentHash = hash;
			await Task.Delay(Math.Max(0, 1000 - (int)(DateTime.Now - _lastSent).TotalMilliseconds));
			if(_currentHash == hash)
			{
				//PayloadDump.Add(payload);
				_lastSent = DateTime.Now;
				Log.Debug($"Sending payload {hash} (type={payload.Type})");
			}
			else
			{
				Log.Debug($"Skipped payload {hash} (type={payload.Type})");
			}
		}

		private static void OnNewBoardState(BoardState boardState)
		{
			SendUpdate(PayloadFactory.BoardState(boardState));
		}

		//public List<Payload> PayloadDump { get; set; } = new List<Payload>();
	}
}
