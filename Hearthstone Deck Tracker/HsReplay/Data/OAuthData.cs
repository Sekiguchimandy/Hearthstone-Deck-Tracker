﻿using System;
using System.Collections.Generic;
using HSReplay.OAuth;

namespace Hearthstone_Deck_Tracker.HsReplay.Data
{
	internal class OAuthData
	{
		public string Code { get; set; }
		public string RedirectUrl { get; set; }
		public TokenData TokenData { get; set; }
		public DateTime TokenDataCreatedAt { get; set; }
		public List<TwitchUser> TwitchUsers { get; set; }
	}
}
