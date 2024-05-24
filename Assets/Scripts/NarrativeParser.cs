using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NarrativeParser : MonoBehaviour
{
    [Serializable]
    public class Story
    {
        public Branch a { get; set; }
        public Branch b { get; set; }
        public Branch c { get; set; }
        public Branch d { get; set; }
        public Branch e { get; set; }
        public Branch f { get; set; }
        public Branch g { get; set; }
        public Branch h { get; set; }
        public Branch i { get; set; }
        public Branch j { get; set; }
        public Branch k { get; set; }
        public Branch l { get; set; }
        public Branch m { get; set; }
        public Branch n { get; set; }
        public Branch o { get; set; }
        public Branch p { get; set; }
        public Branch q { get; set; }
        public Branch r { get; set; }
        public Branch s { get; set; }
        public Branch t { get; set; }
        public Branch u { get; set; }
        public Branch v { get; set; }
        public Branch w { get; set; }
        public Branch x { get; set; }
        public Branch y { get; set; }
        public Branch z { get; set; }

    }

    [Serializable]
    public class Branch
    {
        public string _0 { get; set; }
        public string _1 { get; set; }
        public string _2 { get; set; }
        public string _3 { get; set; }
        public string _4 { get; set; }
        public string _5 { get; set; }
        public string _6 { get; set; }
        public string _7 { get; set; }
        public string _8 { get; set; }
        public string _9 { get; set; }
        public string _10 { get; set; }
        public string _11 { get; set; }
        public string _12 { get; set; }
        public string _13 { get; set; }
        public string _14 { get; set; }

        public string _0_player { get; set; }
        public string _1_player { get; set; }
        public string _2_player { get; set; }
        public string _3_player { get; set; }
        public string _4_player { get; set; }
        public string _5_player { get; set; }
        public string _6_player { get; set; }
        public string _7_player { get; set; }
        public string _8_player { get; set; }
        public string _9_player { get; set; }
        public string _10_player { get; set; }
        public NpcText _0_npc { get; set; }
        public NpcText _1_npc { get; set; }
        public NpcText _2_npc { get; set; }
        public NpcText _3_npc { get; set; }
        public NpcText _4_npc { get; set; }
        public NpcText _5_npc { get; set; }
        public NpcText _6_npc { get; set; }
        public NpcText _7_npc { get; set; }
        public NpcText _8_npc { get; set; }
        public NpcText _9_npc { get; set; }
        public NpcText _10_npc { get; set; }

        // Functions
        public Jump _jmp { get; set; }
        public string _end { get; set; }
        public string _died { get; set; }

        // Character dialogue and appearances/disappearances
        public string _0_apr { get; set; }
        public string _1_apr { get; set; }
        public string _2_apr { get; set; }
        public string _3_apr { get; set; }
        public string _4_apr { get; set; }
        public string _0_dspr { get; set; }
        public string _1_dspr { get; set; }
        public string _2_dspr { get; set; }
        public string _3_dspr { get; set; }
        public string _4_dspr { get; set; }

        // bacgroudn switches
        public string _0_bg { get; set; }
        public string _1_bg { get; set; }
        public string _2_bg { get; set; }
        public string _3_bg { get; set; }
        public string _4_bg { get; set; }

    }


    [Serializable]
    public class NpcText
    {
        public string name { get; set; }
        public string txt { get; set; }

    }

    [Serializable]
    public class Jump
    {
        public Choice chc_0 { get; set; }
        public Choice chc_1 { get; set; }
        public Choice chc_2 { get; set; }
        public Choice chc_3 { get; set; }
        public Choice chc_4 { get; set; }// Method to get all choices as a list
        public List<Choice> GetAllChoices()
        {

            return new List<Choice> { chc_0, chc_1, chc_2, chc_3, chc_4 }
                .Where(choice => choice != null)
                .ToList();
        }


    }

    [Serializable]
    public class Choice
    {
        public string br { get; set; }
        public string txt { get; set; }

    }

    public List<string> functionsKeys = new()
    { "_0_apr", "_1_apr", "_2_apr", "_3_apr", "_4_apr"
,"_0_dspr", "_1_dspr", "_2_dspr", "_3_dspr", "_4_dspr"  };

    public string jumpKey = "_jmp";

    public string choiceKey = "chc_";
    public string backgroundSwitchKey = "_bg";
    public string endKey = "_end";
    public string diedKey = "_died";

    public string playerSpeechKey = "_player";

    public string npcSpeechKey = "_npc";



}
