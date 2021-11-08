using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Core.Plugins;
using Logger = Rocket.Core.Logging.Logger;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Rocket.Unturned.Chat;
using Rocket.API.Collections;
using Rocket.Unturned.Player;

namespace RaidPrevention
{
    public class RaidPrevention : RocketPlugin<RaidPreventionConfiguration>
    {
        public static RaidPrevention Instance { get; private set; }
        public static RaidPreventionConfiguration config { get; private set; }
        public UnityEngine.Color MessageColour { get; private set; }
        public bool isBattle { get; set; }
        protected override void Load()
        {
            Instance = this;
            config = Configuration.Instance;
            MessageColour = UnturnedChat.GetColorFromName(config.MessageColour, UnityEngine.Color.green);
            isBattle = false;

            BarricadeManager.onHarvestPlantRequested += OnDestroyedCrop;
            BarricadeManager.onDamageBarricadeRequested += OnDestroyedBarricade;
            StructureManager.onDamageStructureRequested += OnDestroyedStructure;

            Logger.Log($"{Name} {Assembly.GetName().Version} by Gamingtoday093 has been loaded");
        }

        protected override void Unload()
        {
            BarricadeManager.onHarvestPlantRequested -= OnDestroyedCrop;
            BarricadeManager.onDamageBarricadeRequested -= OnDestroyedBarricade;
            StructureManager.onDamageStructureRequested -= OnDestroyedStructure;

            Logger.Log($"{Name} has been unloaded");
        }

        public void OnDestroyedStructure(CSteamID instigatorSteamID, Transform structureTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (isBattle) return;
            if (pendingTotalDamage <= 0) return;

            if (config.BarricadeStructureIDs.Any(x => x.ToString() == structureTransform.name))
            {
                StructureManager.tryGetInfo(structureTransform, out byte xr, out byte yr, out ushort index, out StructureRegion structureRegion);
                StructureData structure = StructureManager.regions[xr, yr].structures[index];
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
                if (player.IsAdmin)
                {
                    return;
                }

                if ((ulong)player.SteamGroupID == structure.group | (ulong)instigatorSteamID == structure.owner)
                {
                    return;
                }

                if (structure.structure.health - pendingTotalDamage <= 0)
                {
                    pendingTotalDamage = 0;
                    shouldAllow = false;
                    UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                    Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Destroy {structure.structure.asset.itemName}!");
                }
            }
        }

        public void OnDestroyedBarricade(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (isBattle) return;
            if (pendingTotalDamage <= 0) return;
            if (damageOrigin == (EDamageOrigin)6) return;

            if (config.BarricadeStructureIDs.Any(x => x.ToString() == barricadeTransform.name))
            {
                BarricadeManager.tryGetInfo(barricadeTransform, out byte xr, out byte yr, out ushort plant, out ushort index, out BarricadeRegion barricadeRegion);
                BarricadeData barricade = BarricadeManager.BarricadeRegions[xr, yr].barricades[index];
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
                if (player.IsAdmin) return;

                if (barricade.barricade.asset.build == (EBuild)7)
                {
                    if ((ulong)player.SteamGroupID == barricade.group | (ulong)instigatorSteamID == barricade.owner) return;

                    pendingTotalDamage = 0;
                    shouldAllow = false;
                    UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                    Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Destroy {barricade.barricade.asset.itemName}!");
                    return;
                }

                if ((ulong)player.SteamGroupID == barricade.group | (ulong)instigatorSteamID == barricade.owner) return;

                if (barricade.barricade.health - pendingTotalDamage <= 0)
                {
                    pendingTotalDamage = 0;
                    shouldAllow = false;
                    UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                    Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Destroy {barricade.barricade.asset.itemName}!");
                }
            }
        }
        public void OnDestroyedCrop(CSteamID instigatorSteamID, byte x, byte y, ushort plant, ushort index, ref bool shouldAllow)
        {
            if (isBattle) return;

            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
            if (player.IsAdmin) return;

            BarricadeData crop = BarricadeManager.BarricadeRegions[x, y].barricades[index];
            if (config.BarricadeStructureIDs.Contains(crop.barricade.id))
            {
                if ((ulong)player.SteamGroupID == crop.group | (ulong)instigatorSteamID == crop.owner) return;

                shouldAllow = false;
                UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Harvest {crop.barricade.asset.itemName}!");
            }
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "PreventedDestruction", "Do not Destroy Barricades or Structures outside of a Faction Battle! Your Attempt has been Logged." },
            { "BattleInvalid", "You must specify Start or Stop!" },
            { "BattleSuccessStart", "You successfully started the Battle! Barricades and Structures can now be Destroyed" },
            { "BattleSuccessStop", "You successfully stopped the Battle! Barricades and Structures can no longer be Destroyed" }
        };
    }
}
