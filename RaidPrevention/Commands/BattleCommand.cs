using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Unturned.Chat;
using RaidPrevention.Models;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;

namespace RaidPrevention.Commands
{
    class BattleCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "Battle";

        public string Help => "Start and Stop Battles";

        public string Syntax => "<Start | Stop> (Radius)";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, RaidPrevention.Instance.Translate("BattleInvalid"), RaidPrevention.Instance.MessageColour);
                return;
            }
            
            if (command[0].ToLower() == "start")
            {
                if (command.Length < 2)
                {
                    RaidPrevention.Instance.isBattle = true;
                    UnturnedChat.Say(caller, RaidPrevention.Instance.Translate("BattleSuccessStart"), RaidPrevention.Instance.MessageColour);
                }
                else
                {
                    if (caller is ConsolePlayer)
                    {
                        Logger.LogError("[RaidPrevention] A Local Battle Can't be Started through the Console!");
                        return;
                    }

                    if (!float.TryParse(command[1], out float radius))
                    {
                        UnturnedChat.Say(caller, RaidPrevention.Instance.Translate("BattleFailedParse"), RaidPrevention.Instance.MessageColour);
                        return;
                    }

                    RaidPrevention.Instance.isBattle = true;
                    RaidPrevention.Instance.localBattle = new LocalBattle((caller as UnturnedPlayer).Position, radius);
                    UnturnedChat.Say(caller, RaidPrevention.Instance.Translate("BattleSuccessStartLocal", radius), RaidPrevention.Instance.MessageColour);
                }
            } else if (command[0].ToLower() == "stop")
            {
                RaidPrevention.Instance.isBattle = false;
                RaidPrevention.Instance.localBattle = null;
                UnturnedChat.Say(caller, RaidPrevention.Instance.Translate("BattleSuccessStop"), RaidPrevention.Instance.MessageColour);
            } else
            {
                UnturnedChat.Say(caller, RaidPrevention.Instance.Translate("BattleInvalid"), RaidPrevention.Instance.MessageColour);
            }
        }
    }
}
