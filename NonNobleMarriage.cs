﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using static TaleWorlds.CampaignSystem.Romance;
using System.Reflection;

namespace MB2CustomCommands
{
	class NonNobleMarriage
	{
		public static string Apply(Hero firstHero, Hero secondHero)
		{
			firstHero.Spouse = secondHero;
			secondHero.Spouse = firstHero;
			ChangeRelationAction.ApplyRelationChangeBetweenHeroes(firstHero, secondHero, Campaign.Current.Models.MarriageModel.GetEffectiveRelationIncrease(firstHero, secondHero), showQuickNotification: false);

			var t = secondHero.CharacterObject.GetType();
			t.GetProperty("Occupation").SetValue(secondHero.CharacterObject, Occupation.Lord);
			secondHero.IsNoble = true;
			secondHero.CompanionOf = null;
			secondHero.CharacterObject.StringId = "lord_2_10";
			secondHero.Clan = firstHero.Clan;

			EndAllCourtships(firstHero);
			EndAllCourtships(secondHero);

			//CampaignEventDispatcher.Instance.OnHeroesMarried(firstHero, secondHero, true);
			var methodInfo = CampaignEventDispatcher.Instance.GetType().GetMethod("OnHeroesMarried", BindingFlags.NonPublic | BindingFlags.Instance);

			if (methodInfo == null)
				return "OnHeroesMarried not found";

			methodInfo.Invoke(CampaignEventDispatcher.Instance, new object[] { firstHero, secondHero, true });

			return "Success";
		}

		static void EndAllCourtships(Hero forHero)
		{
			foreach (RomanticState romanticState in RomanticStateList)
			{
				if (romanticState.Person1 == forHero || romanticState.Person2 == forHero)
				{
					romanticState.Level = RomanceLevelEnum.Ended;
				}
			}
		}
	}

}
