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
        public static RaidPreventionConfiguration Config { get; private set; }
        public Color MessageColour { get; private set; }
        public bool isBattle { get; set; }
        protected override void Load()
        {
            Instance = this;
            Config = Configuration.Instance;
            MessageColour = UnturnedChat.GetColorFromName(Config.MessageColour, Color.green);
            isBattle = false;

            InteractableFarm.OnHarvestRequested_Global += OnDestroyedCrop;
            BarricadeManager.onDamageBarricadeRequested += OnDestroyedBarricade;
            StructureManager.onDamageStructureRequested += OnDestroyedStructure;

            Logger.Log($"{Name} {Assembly.GetName().Version} by Gamingtoday093 has been loaded");
        }

        protected override void Unload()
        {
            InteractableFarm.OnHarvestRequested_Global -= OnDestroyedCrop;
            BarricadeManager.onDamageBarricadeRequested -= OnDestroyedBarricade;
            StructureManager.onDamageStructureRequested -= OnDestroyedStructure;

            Logger.Log($"{Name} has been unloaded");
        }

        public void OnDestroyedStructure(CSteamID instigatorSteamID, Transform structureTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (isBattle) return;
            if (pendingTotalDamage <= 0) return;

            if (!Config.IsBlacklist && Config.BarricadeStructureIDs.Any(x => x.ToString() == structureTransform.name) || Config.IsBlacklist && !Config.BarricadeStructureIDs.Any(x => x.ToString() == structureTransform.name))
            {
                StructureDrop structure = StructureManager.FindStructureByRootTransform(structureTransform);
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);

                if (player.IsAdmin && Config.AdminByPass) return;
                if (((IRocketPlayer)player).HasPermission(Config.ByPassPermission) && !player.IsAdmin) return;
                if ((ulong)instigatorSteamID == structure.GetServersideData().owner && Config.AllowSelfDestruction) return;
                if ((ulong)player.SteamGroupID == structure.GetServersideData().group && player.SteamGroupID != null && Config.AllowGroupDestruction) return;

                if (structure.GetServersideData().structure.health - pendingTotalDamage <= 0)
                {
                    pendingTotalDamage = 0;
                    shouldAllow = false;
                    UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                    if (Config.LogToConsole)
                    {
                        Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Destroy {structure.asset.itemName}!");
                    }
                    if (Config.DiscordWebhookURL.StartsWith("https://discord.com/api/webhooks/"))
                    {
                        var task = DiscordWebhook.SendDiscordWebhook(Config.DiscordWebhookURL, DiscordWebhook.FormatDiscordWebhook("Raid Prevention (Global)", "https://unturnedstore.com/api/images/511", "Prevented Destruction", "7127038", "Raid Prevention (Global) by Gamingtoday093", "https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png", player.DisplayName, instigatorSteamID.m_SteamID, structure.asset.itemName, SteamGameServer.GetPublicIP().ToString()));
                        Task.Run(async () => await task);
                    }
                }
            }
        }

        public void OnDestroyedBarricade(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (isBattle) return;
            if (pendingTotalDamage <= 0) return;
            if (damageOrigin == EDamageOrigin.Plant_Harvested) return;

            if (!Config.IsBlacklist && Config.BarricadeStructureIDs.Any(x => x.ToString() == barricadeTransform.name) || Config.IsBlacklist && !Config.BarricadeStructureIDs.Any(x => x.ToString() == barricadeTransform.name))
            {
                BarricadeDrop barricade = BarricadeManager.FindBarricadeByRootTransform(barricadeTransform);

                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
                if (player.IsAdmin && Config.AdminByPass) return;
                if (((IRocketPlayer)player).HasPermission(Config.ByPassPermission) && !player.IsAdmin) return;

                if (barricade.asset.build == EBuild.FARM)
                {
                    if ((ulong)instigatorSteamID == barricade.GetServersideData().owner && Config.AllowSelfDestruction) return;
                    if ((ulong)player.SteamGroupID == barricade.GetServersideData().group && player.SteamGroupID != null && Config.AllowGroupDestruction) return;

                    pendingTotalDamage = 0;
                    shouldAllow = false;
                    UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                    if (Config.LogToConsole)
                    {
                        Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Destroy {barricade.asset.itemName}!");
                    }
                    if (Config.DiscordWebhookURL.StartsWith("https://discord.com/api/webhooks/"))
                    {
                        var task = DiscordWebhook.SendDiscordWebhook(Config.DiscordWebhookURL, DiscordWebhook.FormatDiscordWebhook("Raid Prevention (Global)", "https://unturnedstore.com/api/images/511", "Prevented Destruction", "7127038", "Raid Prevention (Global) by Gamingtoday093", "https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png", player.DisplayName, instigatorSteamID.m_SteamID, barricade.asset.itemName, SteamGameServer.GetPublicIP().ToString()));
                        Task.Run(async () => await task);
                    }
                    return;
                }

                if ((ulong)instigatorSteamID == barricade.GetServersideData().owner && Config.AllowSelfDestruction) return;
                if ((ulong)player.SteamGroupID == barricade.GetServersideData().group && player.SteamGroupID != null && Config.AllowGroupDestruction) return;

                if (barricade.GetServersideData().barricade.health - pendingTotalDamage <= 0)
                {
                    pendingTotalDamage = 0;
                    shouldAllow = false;
                    UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                    if (Config.LogToConsole)
                    {
                        Logger.LogError($"[RaidPrevention] {player.DisplayName} ({instigatorSteamID}) Tried to Destroy {barricade.asset.itemName}!");
                    }
                    if (Config.DiscordWebhookURL.StartsWith("https://discord.com/api/webhooks/"))
                    {
                        var task = DiscordWebhook.SendDiscordWebhook(Config.DiscordWebhookURL, DiscordWebhook.FormatDiscordWebhook("Raid Prevention (Global)", "https://unturnedstore.com/api/images/511", "Prevented Destruction", "7127038", "Raid Prevention (Global) by Gamingtoday093", "https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png", player.DisplayName, instigatorSteamID.m_SteamID, barricade.asset.itemName, SteamGameServer.GetPublicIP().ToString()));
                        Task.Run(async () => await task);
                    }
                }
            }
        }
        public void OnDestroyedCrop(InteractableFarm harvestable, SteamPlayer instigatorPlayer, ref bool shouldAllow)
        {
            if (isBattle) return;

            UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(instigatorPlayer);
            if (player.IsAdmin && Config.AdminByPass) return;
            if (((IRocketPlayer)player).HasPermission(Config.ByPassPermission) && !player.IsAdmin) return;

            BarricadeDrop crop = BarricadeManager.FindBarricadeByRootTransform(harvestable.transform);
            if (crop == null) return;

            if (!Config.IsBlacklist && Config.BarricadeStructureIDs.Contains(crop.asset.id) || Config.IsBlacklist && !Config.BarricadeStructureIDs.Contains(crop.asset.id))
            {
                if ((ulong)player.CSteamID == crop.GetServersideData().owner && Config.AllowSelfDestruction) return;
                if ((ulong)player.SteamGroupID == crop.GetServersideData().group && player.SteamGroupID != null && Config.AllowGroupDestruction) return;

                shouldAllow = false;
                UnturnedChat.Say(player, Translate("PreventedDestruction"), MessageColour);
                if (Config.LogToConsole)
                {
                    Logger.LogError($"[RaidPrevention] {player.DisplayName} ({player.CSteamID}) Tried to Harvest {crop.asset.itemName}!");
                }
                if (Config.DiscordWebhookURL.StartsWith("https://discord.com/api/webhooks/"))
                {
                    var task = DiscordWebhook.SendDiscordWebhook(Config.DiscordWebhookURL, DiscordWebhook.FormatDiscordWebhook("Raid Prevention (Global)", "https://unturnedstore.com/api/images/511", "Prevented Destruction", "7127038", "Raid Prevention (Global) by Gamingtoday093", "https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png", player.DisplayName, player.CSteamID.m_SteamID, crop.asset.itemName, SteamGameServer.GetPublicIP().ToString()));
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
