using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace MB2CustomCommands
{
    public class util
	{
		public static string ErrorType = "";

		[CommandLineFunctionality.CommandLineArgumentFunction("add_hero_in_party", "campaign")]
		public static string AddCompanion(List<string> strings)
		{
			if (!CheckCheatUsage(ref ErrorType))
				return ErrorType;

			if (!CheckParameters(strings, 1) || CheckHelp(strings))
				return "Format is \"campaign.add_hero_in_party [heroName]\".";

			var hero = CampaignCheats.GetHero(strings[0]);

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
	}
}
