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
using Rocket.API;
using RaidPrevention.Connections;

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

                if (player.IsAdmin && config.AdminByPass) return;
                if (((IRocketPlayer)player).HasPermission(config.ByPassPermission) && !player.IsAdmin) return;
                if ((ulong)instigatorSteamID == structure.owner && config.AllowSelfDestruction) return;
                if ((ulong)player.SteamGroupID == structure.group && player.SteamGroupID != null && config.AllowGroupDestruction) return;

                if (structure.structure.health - pendingTotalDamage <= 0)
                {
                    pendingTotalDamage = 0;
                    shouldAllow = false;
                    UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                    if (config.LogToConsole)
                    {
                        Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Destroy {structure.structure.asset.itemName}!");
                    }
                    if (config.DiscordWebhookURL != "Webhook")
                    {
                        var task = DiscordWebhook.SendDiscordWebhook(config.DiscordWebhookURL, ("{  \"username\": \"Raid Prevention (Global)\",  \"avatar_url\": \"https://unturnedstore.com/api/images/497\",  \"embeds\": [    {      \"title\": \"Prevented Destruction\",      \"color\": 7127038,      \"fields\": [        {          \"name\": \"!displayname!\",          \"value\": \"!steamid!\",          \"inline\": true        },        {          \"name\": \"Buildable\",          \"value\": \"*!build!*\"        },        {          \"name\": \"Server IP\",          \"value\": \"!ip!\"        }      ],      \"thumbnail\": {        \"url\": \"\"      },      \"image\": {        \"url\": \"\"      },      \"footer\": {        \"text\": \"Raid Prevention (Global) by Gamingtoday093\",        \"icon_url\": \"https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png\"      }    }  ]}").Replace("!displayname!", player.DisplayName).Replace("!steamid!", player.CSteamID.ToString()).Replace("!ip!", SteamGameServer.GetPublicIP().ToString()).Replace("!build!", structure.structure.asset.itemName));
                        Task.Run(async () => await task);
                    }
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
                if (player.IsAdmin && config.AdminByPass) return;
                if (((IRocketPlayer)player).HasPermission(config.ByPassPermission) && !player.IsAdmin) return;

                if (barricade.barricade.asset.build == (EBuild)7)
                {
                    if ((ulong)instigatorSteamID == barricade.owner && config.AllowSelfDestruction) return;
                    if ((ulong)player.SteamGroupID == barricade.group && player.SteamGroupID != null && config.AllowGroupDestruction) return;

                    pendingTotalDamage = 0;
                    shouldAllow = false;
                    UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                    if (config.LogToConsole)
                    {
                        Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Destroy {barricade.barricade.asset.itemName}!");
                    }
                    if (config.DiscordWebhookURL != "Webhook")
                    {
                        var task = DiscordWebhook.SendDiscordWebhook(config.DiscordWebhookURL, ("{  \"username\": \"Raid Prevention (Global)\",  \"avatar_url\": \"https://unturnedstore.com/api/images/511\",  \"embeds\": [    {      \"title\": \"Prevented Destruction\",      \"color\": 7127038,      \"fields\": [        {          \"name\": \"!displayname!\",          \"value\": \"!steamid!\",          \"inline\": true        },        {          \"name\": \"Buildable\",          \"value\": \"*!build!*\"        },        {          \"name\": \"Server IP\",          \"value\": \"!ip!\"        }      ],      \"thumbnail\": {        \"url\": \"\"      },      \"image\": {        \"url\": \"\"      },      \"footer\": {        \"text\": \"Raid Prevention (Global) by Gamingtoday093\",        \"icon_url\": \"https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png\"      }    }  ]}").Replace("!displayname!", player.DisplayName).Replace("!steamid!", player.CSteamID.ToString()).Replace("!ip!", SteamGameServer.GetPublicIP().ToString()).Replace("!build!", barricade.barricade.asset.itemName));
                        Task.Run(async () => await task);
                    }
                    return;
                }

                if ((ulong)instigatorSteamID == barricade.owner && config.AllowSelfDestruction) return;
                if ((ulong)player.SteamGroupID == barricade.group && player.SteamGroupID != null && config.AllowGroupDestruction) return;

                if (barricade.barricade.health - pendingTotalDamage <= 0)
                {
                    pendingTotalDamage = 0;
                    shouldAllow = false;
                    UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                    if (config.LogToConsole)
                    {
                        Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Destroy {barricade.barricade.asset.itemName}!");
                    }
                    if (config.DiscordWebhookURL != "Webhook")
                    {
                        var task = DiscordWebhook.SendDiscordWebhook(config.DiscordWebhookURL, ("{  \"username\": \"Raid Prevention (Global)\",  \"avatar_url\": \"https://unturnedstore.com/api/images/511\",  \"embeds\": [    {      \"title\": \"Prevented Destruction\",      \"color\": 7127038,      \"fields\": [        {          \"name\": \"!displayname!\",          \"value\": \"!steamid!\",          \"inline\": true        },        {          \"name\": \"Buildable\",          \"value\": \"*!build!*\"        },        {          \"name\": \"Server IP\",          \"value\": \"!ip!\"        }      ],      \"thumbnail\": {        \"url\": \"\"      },      \"image\": {        \"url\": \"\"      },      \"footer\": {        \"text\": \"Raid Prevention (Global) by Gamingtoday093\",        \"icon_url\": \"https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png\"      }    }  ]}").Replace("!displayname!", player.DisplayName).Replace("!steamid!", player.CSteamID.ToString()).Replace("!ip!", SteamGameServer.GetPublicIP().ToString()).Replace("!build!", barricade.barricade.asset.itemName));
                        Task.Run(async () => await task);
                    }
                }
            }
        }
        public void OnDestroyedCrop(CSteamID instigatorSteamID, byte x, byte y, ushort plant, ushort index, ref bool shouldAllow)
        {
            if (isBattle) return;

            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
            if (player.IsAdmin && config.AdminByPass) return;
            if (((IRocketPlayer)player).HasPermission(config.ByPassPermission) && !player.IsAdmin) return;

            BarricadeData crop = BarricadeManager.BarricadeRegions[x, y].barricades[index];
            if (config.BarricadeStructureIDs.Contains(crop.barricade.id))
            {
                if ((ulong)instigatorSteamID == crop.owner && config.AllowSelfDestruction) return;
                if ((ulong)player.SteamGroupID == crop.group && player.SteamGroupID != null && config.AllowGroupDestruction) return;

                shouldAllow = false;
                UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                if (config.LogToConsole)
                {
                    Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Harvest {crop.barricade.asset.itemName}!");
                }
                if (config.DiscordWebhookURL != "Webhook")
                {
                    var task = DiscordWebhook.SendDiscordWebhook(config.DiscordWebhookURL, ("{  \"username\": \"Raid Prevention (Global)\",  \"avatar_url\": \"https://unturnedstore.com/api/images/511\",  \"embeds\": [    {      \"title\": \"Prevented Destruction\",      \"color\": 15258703,      \"fields\": [        {          \"name\": \"!displayname!\",          \"value\": \"!steamid!\",          \"inline\": true        },        {          \"name\": \"Buildable\",          \"value\": \"*!build!*\"        },        {          \"name\": \"Server IP\",          \"value\": \"!ip!\"        }      ],      \"thumbnail\": {        \"url\": \"\"      },      \"image\": {        \"url\": \"\"      },      \"footer\": {        \"text\": \"Raid Prevention (Global) by Gamingtoday093\",        \"icon_url\": \"https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png\"      }    }  ]}").Replace("!displayname!", player.DisplayName).Replace("!steamid!", player.CSteamID.ToString()).Replace("!ip!", SteamGameServer.GetPublicIP().ToString()).Replace("!build!", crop.barricade.asset.itemName));
                    Task.Run(async () => await task);
                }
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
