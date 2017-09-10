using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.HsReplay.Data;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HSReplay.OAuth;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	// ReSharper disable once InconsistentNaming
	internal sealed class HSReplayNetOAuth
	{
		private static readonly JsonSerializer<OAuthData> Serializer;
		private static readonly Lazy<OAuthClient> Client;
		private static readonly Lazy<OAuthData> Data;


		static HSReplayNetOAuth()
		{
			Serializer = new JsonSerializer<OAuthData>("hsreplay_oauth", true);
			Data = new Lazy<OAuthData>(Serializer.Load);
			Client = new Lazy<OAuthClient>(LoadClient);
		}

		private static OAuthClient LoadClient()
		{
			return new OAuthClient("jIpNwuUWLFI6S3oeQkO3xlW6UCnfogw1IpAbFXqq", Helper.GetUserAgent(), Data.Value.TokenData);
		}

		public static void Save() => Serializer.Save(Data.Value);

		public static async Task<bool> Authenticate()
		{
			var url = Client.Value.GetAuthenticationUrl(new[] { Scope.Webhooks }, null, new[] { 17784, 17785, 17786 });
			var callbackTask = Client.Value.ReceiveAuthenticationCallback("", "");
			if(!Helper.TryOpenUrl(url))
				ErrorManager.AddError("Could not open browser to complete authentication.", $"Please go to '{url}' to continue authentication.", true);
			var data = await callbackTask;
			Data.Value.Code = data.Code;
			Data.Value.RedirectUrl = data.RedirectUrl;
			Save();
			return true;
		}

		public static async Task<bool> UpdateToken()
		{
			var data = Data.Value;
			if(string.IsNullOrEmpty(data.Code) || string.IsNullOrEmpty(data.RedirectUrl))
			{
				Log.Error("Could not update token, we don't have a code or redirect url.");
				return false;
			}
			if(!string.IsNullOrEmpty(data.TokenData?.RefreshToken))
			{
				try
				{
					var tokenData = await Client.Value.RefreshToken();
					if(tokenData != null)
					{
						UpdateTokenData(tokenData);
						return true;
					}
				}
				catch(Exception e)
				{
					Log.Error(e);
				}
			}
			try
			{
				var tokenData = await Client.Value.GetToken(data.Code, data.RedirectUrl);
				if(tokenData == null)
				{
					Log.Error("We did not receive any token data.");
					return false;
				}
				UpdateTokenData(tokenData);
				return true;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public static async Task<bool> UpdateTwitchUsers()
		{
			try
			{
				//var twitchAccounts = await Client.Value.GetTwitchAccounts();
				List<TwitchUser> twitchAccounts = null;
				if(twitchAccounts == null)
					return false;
				Data.Value.TwitchUsers = twitchAccounts;
				Save();
				return true;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public static bool IsAuthenticated => !string.IsNullOrEmpty(Data.Value.Code);

		public static List<TwitchUser> TwitchUsers => Data.Value.TwitchUsers;

		public static void UpdateTokenData(TokenData data)
		{
			Data.Value.TokenData = data;
			Data.Value.TokenDataCreatedAt = DateTime.Now;
			Save();
		}
	}
}
