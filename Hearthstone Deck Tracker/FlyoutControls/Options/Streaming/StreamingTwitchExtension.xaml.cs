using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.HsReplay.Data;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Twitch;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Streaming
{
	public partial class StreamingTwitchExtension : INotifyPropertyChanged
	{
		private bool _oAuthSuccess;
		private bool _oAuthError;
		private string _hsreplayUserName;
		private TwitchUser _twitchAccount;
		private bool _twitchAccountLinked;
		private bool _twitchStreamLive;
		private bool _multipleTwitchAccounts;
		private List<TwitchUser> _availableTwitchAccounts;
		private TwitchUser _selectedTwitchUser;

		public StreamingTwitchExtension()
		{
			InitializeComponent();
		}

		public SolidColorBrush SelectedColor => Helper.BrushFromHex(Config.Instance.StreamingOverlayBackground);

		public ICommand AuthenticateCommand => new Command(async () =>
		{
			var success = await HSReplayNetOAuth.Authenticate();
			OAuthSuccess = success;
			OAuthError = !success;
		});

		public bool OAuthSuccess
		{
			get => _oAuthSuccess; set
			{
				_oAuthSuccess = value; 
				OnPropertyChanged();
				OnPropertyChanged(nameof(SetupComplete));
			}
		}

		public bool OAuthError
		{
			get => _oAuthError; set
			{
				_oAuthError = value; 
				OnPropertyChanged();
			}
		}

		// ReSharper disable once InconsistentNaming
		public string HSReplayUserName
		{
			get => _hsreplayUserName;
			set
			{
				_hsreplayUserName = value; 
				OnPropertyChanged();
			}
		}

		public bool TwitchExtensionEnabled
		{
			get => Config.Instance.SendLiveUpdates;
			set
			{
				Config.Instance.SendLiveUpdates = value;
				Config.Save();
				OnPropertyChanged();
			}
		}

		public TwitchUser TwitchAccount
		{
			get => _twitchAccount;
			set
			{
				_twitchAccount = value;
				OnPropertyChanged();
			}
		}

		public bool TwitchAccountLinked
		{
			get => _twitchAccountLinked;
			set
			{
				_twitchAccountLinked = value; 
				OnPropertyChanged();
				OnPropertyChanged(nameof(SetupComplete));
			}
		}

		public bool SetupComplete => OAuthSuccess && TwitchAccountLinked;

		public bool TwitchStreamLive
		{
			get => _twitchStreamLive;
			set
			{
				_twitchStreamLive = value; 
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		internal TwitchUser SelectedTwitchUser
		{
			get => _selectedTwitchUser;
			set
			{
				if(_selectedTwitchUser != value)
				{
					_selectedTwitchUser = value; 
					OnPropertyChanged();
					var newId = value?.Id ?? 0;
					if(Config.Instance.SelectedTwitchUser != newId)
					{
						Config.Instance.SelectedTwitchUser = newId;
						Config.Save();
					}
				}
			}
		}

		public List<TwitchUser> AvailableTwitchAccounts
		{
			get => _availableTwitchAccounts;
			set
			{
				_availableTwitchAccounts = value; 
				OnPropertyChanged();
				OnPropertyChanged(nameof(MultipleTwitchAccounts));
			}
		}

		public bool MultipleTwitchAccounts => AvailableTwitchAccounts?.Count > 1;

		internal async void OnSelected()
		{
			OAuthSuccess = HSReplayNetOAuth.IsAuthenticated;
			AvailableTwitchAccounts = HSReplayNetOAuth.TwitchUsers;

			var selected = Config.Instance.SelectedTwitchUser;
			SelectedTwitchUser = AvailableTwitchAccounts?.FirstOrDefault(x => x.Id == selected || selected == 0);
			if(SelectedTwitchUser?.Id > 0)
				TwitchStreamLive = await TwitchApi.IsStreaming(SelectedTwitchUser.Id);
		}
	}
}
