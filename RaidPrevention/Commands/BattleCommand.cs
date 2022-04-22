using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Unturned.Chat;
using Rocket.Core.Logging;

namespace RaidPrevention.Commands
{
    class BattleCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "Battle";

        public string Help => "";

        public string Syntax => "<Start || Stop>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                Logger.LogError($"RaidPrevention >> {RaidPrevention.Instance.Translate("BattleInvalid")}");
                UnturnedChat.Say(caller, RaidPrevention.Instance.Translate("BattleInvalid"), RaidPrevention.Instance.MessageColour);
                return;
            }
            
            if (command[0].ToLower() == "start")
            {
                RaidPrevention.Instance.isBattle = true;
                Logger.Log(RaidPrevention.Instance.Translate("BattleSuccessStart"));
                UnturnedChat.Say(caller, RaidPrevention.Instance.Translate("BattleSuccessStart"), RaidPrevention.Instance.MessageColour);
                return;
            } else if (command[0].ToLower() == "stop")
            {
                RaidPrevention.Instance.isBattle = false;
                Logger.Log(RaidPrevention.Instance.Translate("BattleSuccessStop"));
                UnturnedChat.Say(caller, RaidPrevention.Instance.Translate("BattleSuccessStop"), RaidPrevention.Instance.MessageColour);
                return;
            } else
            {
                UnturnedChat.Say(caller, RaidPrevention.Instance.Translate("BattleInvalid"), RaidPrevention.Instance.MessageColour);
                return;
            }
        }
    }
}
