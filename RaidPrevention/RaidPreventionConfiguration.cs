using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;
using System.Xml.Serialization;

namespace RaidPrevention
{
    public class RaidPreventionConfiguration : IRocketPluginConfiguration
    {
        public string MessageColour { get; set; }
        public bool LogToConsole { get; set; }
        public string DiscordWebhookURL { get; set; }
        public string ByPassPermission { get; set; }
        public bool AdminByPass { get; set; }
        public bool AllowSelfDestruction { get; set; }
        public bool AllowGroupDestruction { get; set; }
        public bool OnlyPreventDestruction { get; set; }
        public bool IsBlacklist { get; set; }
        [XmlArrayItem("BarricadeStructureID")]
        public ushort[] BarricadeStructureIDs { get; set; }
        public void LoadDefaults()
        {
            MessageColour = "yellow";
            LogToConsole = true;
            DiscordWebhookURL = "Webhook";
            ByPassPermission = "RaidPrevention.Bypass";
            AdminByPass = true;
            AllowSelfDestruction = true;
            AllowGroupDestruction = true;
            OnlyPreventDestruction = true;
            IsBlacklist = false;
            BarricadeStructureIDs = new ushort[]
            {
                40300,
                40301,
                40302,
                40303,
                40304,
                40305,
                40306,
                40307,
                40308,
                40309,
                40310,
                40311,
                40312,
                40313,
                40400,
                40401,
                40402,
                40403,
                40404,
                40405,
                40406,
                40407,
                40408,
                40409,
                40410,
                40411,
                40412,
                40436,
                40437,
                40438,
                40439,
                40440,
                40441,
                40442,
                40443,
                40444,
                40445,
                40446,
                40315,
                40316,
                40317,
                40318,
                40320,
                40321,
                40326,
                40327,
                40328,
                40329,
                40330,
                40331,
                40332,
                40333,
                40334,
                40335,
                40336,
                284,
                386,
                378,
                286,
                1238,
                1119,
                457,
                1230,
                458,
                1298,
                1299,
                1297,
                1331,
                1332,
                379,
                328,
                1219,
                1470,
                365,
                1229,
                1226,
                1098,
                1227
            };
        }
    }
}
