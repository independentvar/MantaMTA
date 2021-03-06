﻿using System;
using System.Linq;
using OpenManta.Core;
using OpenManta.Data;

namespace OpenManta.Framework
{
	internal partial class BounceRulesManager
	{
		/// <summary>
		/// Holds a singleton instance of the BounceRulesManager.
		/// </summary>
		public static BounceRulesManager Instance { get { return _Instance; } }
		private static readonly BounceRulesManager _Instance = new BounceRulesManager();
		private BounceRulesManager() { }

		private static BounceRulesCollection _bounceRules = null;
		public static BounceRulesCollection BounceRules
		{
			get
			{
				if (_bounceRules == null || _bounceRules.LoadedTimestampUtc.AddMinutes(5) < DateTime.UtcNow)
				{
					// Would be nice to write to a log that we're updating.
					_bounceRules = EventDB.GetBounceRules();

					// Ensure the Rules are in the correct order.
					_bounceRules = new BounceRulesCollection(_bounceRules.OrderBy(r => r.ExecutionOrder));

					// Only set the LoadedTimestamp value after we're done assigning new values to _bounceRules.
					_bounceRules.LoadedTimestampUtc = DateTime.UtcNow;
				}

				return _bounceRules;
			}
		}
	}
}
