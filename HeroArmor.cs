using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using System.Reflection;

namespace MB2CustomCommands
{
    class HeroArmor
    {
        public static void Get(Hero hero)
        {
            var _itemSlotsInfo = hero.BattleEquipment.GetType().GetField("_itemSlots", BindingFlags.Instance | BindingFlags.NonPublic);
            EquipmentElement[] equipments = (EquipmentElement[])_itemSlotsInfo.GetValue(hero.BattleEquipment);

            var heroHead = equipments[(int)EquipmentIndex.Head];
            var heroCape = equipments[(int)EquipmentIndex.Cape];
            var heroBody = equipments[(int)EquipmentIndex.Body];
            var heroGloves = equipments[(int)EquipmentIndex.Gloves];
            var heroLeg = equipments[(int)EquipmentIndex.Leg];

            var items = MobileParty.MainParty.ItemRoster;

            var HeadArmors = items.Where(i => !i.IsEmpty && i.EquipmentElement.Item.Type == ItemObject.ItemTypeEnum.HeadArmor);
            var Capes = items.Where(i => !i.IsEmpty && i.EquipmentElement.Item.Type == ItemObject.ItemTypeEnum.Cape);
            var BodyArmors = items.Where(i => !i.IsEmpty && i.EquipmentElement.Item.Type == ItemObject.ItemTypeEnum.BodyArmor);
            var HandArmors = items.Where(i => !i.IsEmpty && i.EquipmentElement.Item.Type == ItemObject.ItemTypeEnum.HandArmor);
            var LegArmors = items.Where(i => !i.IsEmpty && i.EquipmentElement.Item.Type == ItemObject.ItemTypeEnum.LegArmor);

            if (HeadArmors.Count() != 0)
            {
                var bestHeadArmor = HeadArmors.MaxBy(a => a.EquipmentElement.GetModifiedHeadArmor()).EquipmentElement;
                if (heroHead.IsEmpty)
                {
                    heroHead = bestHeadArmor;
                    items.AddToCounts(bestHeadArmor, -1);
                }
                else if (heroHead.GetModifiedHeadArmor() < bestHeadArmor.GetModifiedHeadArmor()
                    || (heroHead.GetModifiedHeadArmor() == bestHeadArmor.GetModifiedHeadArmor() && heroHead.Weight > bestHeadArmor.Weight))
                {
                    heroHead = bestHeadArmor;
                    items.AddToCounts(bestHeadArmor, -1);
                    items.AddToCounts(equipments[(int)EquipmentIndex.Head], 1);
                }
            }

            if (Capes.Count() != 0)
            {
                var bestCape = Capes.ElementAt(0).EquipmentElement;
                for (int i = 1; i < Capes.Count(); i++)
                    bestCape = GetBetterCape(bestCape, Capes.ElementAt(i).EquipmentElement);

                if (heroCape.IsEmpty)
                {
                    heroCape = bestCape;
                    items.AddToCounts(bestCape, -1);
                }
                else if (bestCape.IsEqualTo(GetBetterCape(bestCape, heroCape)))
                {
                    heroCape = bestCape;
                    items.AddToCounts(bestCape, -1);
                    items.AddToCounts(equipments[(int)EquipmentIndex.Cape], 1);
                }
            }

            if (HandArmors.Count() != 0)
            {
                var bestHandArmor = HandArmors.MaxBy(a => a.EquipmentElement.GetModifiedArmArmor()).EquipmentElement;
                if (heroGloves.IsEmpty)
                {
                    heroGloves = bestHandArmor;
                    items.AddToCounts(bestHandArmor, -1);
                }
                else if (heroGloves.GetModifiedArmArmor() < bestHandArmor.GetModifiedArmArmor()
                    || (heroGloves.GetModifiedArmArmor() == bestHandArmor.GetModifiedArmArmor() && heroGloves.Weight > bestHandArmor.Weight))
                {
                    heroGloves = bestHandArmor;
                    items.AddToCounts(bestHandArmor, -1);
                    items.AddToCounts(equipments[(int)EquipmentIndex.Gloves], 1);
                }
            }

            if (LegArmors.Count() != 0)
            {
                var bestLegArmor = LegArmors.MaxBy(a => a.EquipmentElement.GetModifiedLegArmor()).EquipmentElement;
                if (heroLeg.IsEmpty)
                {
                    heroLeg = bestLegArmor;
                    items.AddToCounts(bestLegArmor, -1);
                }
                else if (heroLeg.GetModifiedLegArmor() < bestLegArmor.GetModifiedLegArmor()
                    || (heroLeg.GetModifiedLegArmor() == bestLegArmor.GetModifiedLegArmor() && heroLeg.Weight > bestLegArmor.Weight))
                {
                    heroLeg = bestLegArmor;
                    items.AddToCounts(bestLegArmor, -1);
                    items.AddToCounts(equipments[(int)EquipmentIndex.Leg], 1);
                }
            }

            if (BodyArmors.Count() != 0)
            {
                var bestBodyArmor = BodyArmors.ElementAt(0).EquipmentElement;
                for (int i = 1; i < BodyArmors.Count(); i++)
                    bestBodyArmor = GetBetterBodyArmor(bestBodyArmor, BodyArmors.ElementAt(i).EquipmentElement);

                if (heroBody.IsEmpty)
                {
                    heroBody = bestBodyArmor;
                    items.AddToCounts(bestBodyArmor, -1);
                }
                else if (bestBodyArmor.IsEqualTo(GetBetterBodyArmor(heroBody, bestBodyArmor)))
                {
                    heroBody = bestBodyArmor;
                    items.AddToCounts(bestBodyArmor, -1);
                    items.AddToCounts(equipments[(int)EquipmentIndex.Body], 1);
                }
            }

            equipments[(int)EquipmentIndex.Head] = heroHead;
            equipments[(int)EquipmentIndex.Cape] = heroCape;
            equipments[(int)EquipmentIndex.Body] = heroBody;
            equipments[(int)EquipmentIndex.Gloves] = heroGloves;
            equipments[(int)EquipmentIndex.Leg] = heroLeg;
            _itemSlotsInfo.SetValue(hero.BattleEquipment, equipments);
        }

        static EquipmentElement GetBetterBodyArmor(EquipmentElement a, EquipmentElement b)
        {
            var a_rank = a.GetModifiedBodyArmor() * 2 + a.GetModifiedArmArmor() + a.GetModifiedLegArmor();
            var b_rank = b.GetModifiedBodyArmor() * 2 + b.GetModifiedArmArmor() + b.GetModifiedLegArmor();

            if (a_rank > b_rank)
                return a;
            if (a_rank == b_rank && a.GetModifiedBodyArmor() > b.GetModifiedBodyArmor())
                return a;
            if (a_rank == b_rank && a.GetModifiedBodyArmor() == b.GetModifiedBodyArmor() && a.Weight < b.Weight)
                return a;

            return b;
        }

        static EquipmentElement GetBetterCape(EquipmentElement a, EquipmentElement b)
        {
            var a_rank = a.GetModifiedBodyArmor() * 2 + a.GetModifiedArmArmor();
            var b_rank = b.GetModifiedBodyArmor() * 2 + b.GetModifiedArmArmor();

            if (a_rank > b_rank)
                return a;
            if (a_rank == b_rank && a.GetModifiedBodyArmor() > b.GetModifiedBodyArmor())
                return a;
            if (a_rank == b_rank && a.GetModifiedBodyArmor() == b.GetModifiedBodyArmor() && a.Weight < b.Weight)
                return a;

            return b;
        }
    }
}
