﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;
using MountAndBlade.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using System.Reflection;

namespace MB2CustomCommands
{
	public class util
	{
		public static string ErrorType = "";
		static Dictionary<CharacterObject, int> CharacterDict = new Dictionary<CharacterObject, int>();
		static IOrderedEnumerable<KeyValuePair<CharacterObject, int>> OrderedCD;

		[CommandLineFunctionality.CommandLineArgumentFunction("add_hero_in_party", "campaign")]
		public static string AddCompanion(List<string> strings)
		{
			if (!CheckCheatUsage(ref ErrorType))
				return ErrorType;

			if (CheckParameters(strings, 0) || CheckHelp(strings))
				return "Format is \"campaign.add_hero_in_party [heroName]\".";

			var hero = CampaignCheats.GetHero(ConcatenateString(strings));

			if (hero == null)
				return "hero not found";

			if (Clan.PlayerClan.Name.ToString().ToLower() != hero.Clan.Name.ToString().ToLower())
				AddCompanionAction.Apply(Clan.PlayerClan, hero);

			AddHeroToPartyAction.Apply(hero, MobileParty.MainParty);

			return "companion has been added to the party.";
		}

		public static bool CheckCheatUsage(ref string ErrorType)
		{
			if (Campaign.Current == null)
			{
				ErrorType = "Campaign was not started.";
				return false;
			}
			if (!Game.Current.CheatMode)
			{
				ErrorType = "Cheat mode is disabled!";
				return false;
			}
			ErrorType = "";
			return true;
		}

		public static bool CheckParameters(List<string> strings, int ParameterCount)
		{
			if (strings.Count == 0)
				return ParameterCount == 0;

			return strings.Count == ParameterCount;
		}

		public static bool CheckHelp(List<string> strings)
		{
			if (strings.Count == 0)
				return false;

			return strings[0].ToLower() == "help";
		}

		public static string ConcatenateString(List<string> strings)
		{
			if (strings == null || strings.IsEmpty())
			{
				return string.Empty;
			}
			string text = strings[0];
			if (strings.Count > 1)
			{
				for (int i = 1; i < strings.Count; i++)
				{
					text = text + " " + strings[i];
				}
			}
			return text;
		}

		[CommandLineFunctionality.CommandLineArgumentFunction("sort_troop_in_party", "campaign")]
		public static string SortTroop(List<string> strings)
		{
			if (!CheckCheatUsage(ref ErrorType))
				return ErrorType;

			if (!CheckParameters(strings, 0) || CheckHelp(strings))
				return "Format is \"campaign.sort_troop_in_party\".";

			foreach (var rosterElement in MobileParty.MainParty.MemberRoster.GetTroopRoster())
				if (!rosterElement.Character.IsHero)
					CharacterDict.Add(rosterElement.Character, rosterElement.Number);

			if (CharacterDict.Count == 0)
				return "No regular troop found.";

			MobileParty.MainParty.MemberRoster.RemoveIf(m => !m.Character.IsHero);

			OrderedCD = CharacterDict.OrderBy(c => c.Key.DefaultFormationClass).ThenBy(c => c.Key.Tier);
			foreach (var item in OrderedCD)
				MobileParty.MainParty.MemberRoster.AddToCounts(item.Key, item.Value);

			CharacterDict.Clear();
			return "Troop is sorted by type and tier.";
		}

		[CommandLineFunctionality.CommandLineArgumentFunction("marry_me_to", "campaign")]
		public static string MarryPlayerWithHero(List<string> strings)
		{
			if (!CheckCheatUsage(ref ErrorType))
				return ErrorType;

			if (CheckHelp(strings) || CheckParameters(strings, 0))
				return "Format is \"campaign.marry_me_to [HeroName]\".";

			var hero = Hero.FindFirst(h => h.Name.ToString().ToLower() == (ConcatenateString(strings).ToLower()));

			if (hero == null)
				return "Hero is not found.";

			if (!hero.IsNoble)
				return NonNobleMarriage.Apply(Hero.MainHero, hero);

			MarriageAction.Apply(Hero.MainHero, hero);
			return "Success";
		}

		[CommandLineFunctionality.CommandLineArgumentFunction("sort_companion_by_level", "campaign")]
		public static string SortCompanionByLevel(List<string> strings)
		{
			if (!CheckCheatUsage(ref ErrorType))
				return ErrorType;

			if (!CheckParameters(strings, 0) || CheckHelp(strings))
				return "Format is \"campaign.sort_companion_by_level\".";

			var heroCharacters = MobileParty.MainParty.MemberRoster.GetTroopRoster().Where(m => m.Character.IsHero).Select(m => m.Character);

			if (heroCharacters.Count() < 2)
				return "You don't have companions.";

			for (int i = 1; i < heroCharacters.Count(); i++)
				CharacterDict.Add(heroCharacters.ElementAt(i), heroCharacters.ElementAt(i).Level);

			MobileParty.MainParty.MemberRoster.RemoveIf(m => !m.Character.IsPlayerCharacter && m.Character.IsHero);

			foreach (var item in CharacterDict.OrderBy(d => d.Value))
				MobileParty.MainParty.MemberRoster.AddToCounts(item.Key, 1, true);

			var myCharacter = Hero.MainHero.CharacterObject;
			MobileParty.MainParty.MemberRoster.RemoveTroop(myCharacter);
			MobileParty.MainParty.MemberRoster.AddToCounts(myCharacter, 1, true);

			CharacterDict.Clear();
			return "Companions are sorted by level from highest to lowest";
		}

		[CommandLineFunctionality.CommandLineArgumentFunction("auto_equipped_armor_for_hero", "campaign")]
		public static string AutoEquippedArmorsForHero(List<string> strings)
		{
			if (!CheckCheatUsage(ref ErrorType))
				return ErrorType;

			if (CheckParameters(strings, 0) || CheckHelp(strings))
				return "Format is \"campaign.auto_equipped_armor_for_hero [HeroName]\".";

			HeroArmor.Get(CampaignCheats.GetHero(ConcatenateString(strings)));

			return "Done";
		}

		[CommandLineFunctionality.CommandLineArgumentFunction("auto_equipped_armors_in_party", "campaign")]
		public static string AutoEquippedArmors(List<string> strings)
		{
			if (!CheckCheatUsage(ref ErrorType))
				return ErrorType;

			if (!CheckParameters(strings, 0) || CheckHelp(strings))
				return "Format is \"campaign.auto_equipped_armors_in_party\".";

			var heros = MobileParty.MainParty.MemberRoster.GetTroopRoster().Where(m => m.Character.IsHero).Select(m => m.Character.HeroObject);

			foreach (var h in heros)
				HeroArmor.Get(h);

			return "Done";
		}
	}
}
