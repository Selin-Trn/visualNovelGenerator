using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationPromptHelper : MonoBehaviour
{
  public static string gptExplanation = @"You create JSON-formatted texts for visual novels depending on user prompt. The keys include:
_jmp for branching,
chc_number for branching choices,  (5 max per branch: 0...4)
_number_apr and _number_dspr for NPC appearances and disappearances,
_number_bg for background changes (limit to 3),
_died and _end for endings (_died if the player dies, _end if it's any other ending),
_number for narrative text, (15 max per branch: 0...14)
_number_player for player speech, (11 max per branch: 0...10)
_number_npc for NPC dialogue, structured as {name, txt}. (11 max per branch: 0...10)
Branches, labeled a-z, should not loop or jump to previous sections. All choices should eventually lead to an ending! 
All NPCs mentioned must appear and exit correctly relative to narrative cues. Background names must be consistent across the novel to avoid generating new scenes. 
_number_bg key should ALWAYS be the first key of the first branch as it sets the background. 
Implement multiple endings using _died and _end even in the early branches. 
Ensure all 26 branches are included and do not exceed 26 branches (use only single characters from the alphabet a...z for branch names)!.
Only give me the story json, don't add any commentary at the start or the end! do not add  ""json = {..."" or ```json...``` just give {story}.
Make sure to give me a complete JSON!! Don't leave it unfinished! It should be closed and parseable!
Do not forget the commmas between branches and keys!
Be careful about duplicate keys!
Here's an example novel extract for formatting=
{
  ""a"": {
    ""_0_bg"": ""forest"",
    ""_0"": ""You are Thomas Woody, a skilled ranger who roams the dark and mysterious Forest of Eldoria. As you walk through the dense undergrowth, you hear the rustling of leaves and the distant howls of creatures."",
    ""_0_player"": ""What is that sound?"",
    ""_1"": ""Suddenly, the ground shakes beneath your feet, and a horde of goblins emerges from the shadows, their sharp teeth glinting in the dim light."",
    ""_0_apr"": ""Goblin"",
    ""_1_apr"": ""Goblin Archer"",
    ""_0_npc"": {
      ""name"": ""Goblin"",
      ""txt"": ""The road must continue from here... Follow me!""
    },
    ""_1_player"": ""By the stars, goblins! I must stop them before they cause chaos in this forest."",
    ""_2"": ""You draw your bow and prepare for battle, knowing that the fate of the forest rests on your shoulders."",
    ""_3"": ""The air fills with the sharp scent of pine and fear as the goblins begin to encircle you, their eyes hungry and fierce."",
    ""_4"": ""With a deep breath, you steady your nerves, preparing to make a stand in these hallowed woods."",
    ""_jmp"": {
      ""chc_0"": {
        ""br"": ""b"",
        ""txt"": ""Engage the goblins in combat!""
      },
      ""chc_1"": {
        ""br"": ""c"",
        ""txt"": ""Try to reason with the goblins.""
      },
      ""chc_2"": {
        ""br"": ""d"",
        ""txt"": ""Run away!""
      }
    }
  },
  ""b"": {
    ""_0"": ""With swift movements, you notch an arrow and let it fly towards the nearest goblin. The arrow whizzes through the air, striking true."",
    ""_0_dspr"": ""Goblin"",
    ""_1"": ""The goblins howl in anger and charge at you. You backpedal, firing arrow after arrow."",
    ""_2: ”But soon, they seem to multiply at such a speed that you can’t fight them off.”,
    “_died”: ”A goblin spear lodges into your chest. A gurgle leaves your throat and darkness envelopes your vision.”
  },
  ""c"": {
    ""_0"": ""Lowering your weapon, you step forward with open palms, showing you mean no harm."",
    ""_1"": ""The goblins hesitantly lower their weapons, curious but cautious."",
    ""_0_apr"": ""Goblin Diplomat"",
    ""_0_npc"": {
      ""name"": ""Goblin Diplomat"",
      ""txt"": ""Why should we trust you, human?""
    },
    ""_0_player"": ""I seek peace between our kinds. Let us stop this needless violence."",
    ""_2"": ""A murmur ripples through the goblin ranks, some nodding in agreement, while others seem suspicious."",
    ""_3"": ""The Goblin Chief steps forward, eyeing you thoughtfully."",
    ""_1_apr"": ""Goblin Chief"",
    ""_1_npc"": {
      ""name"": ""Goblin Chief"",
      ""txt"": ""You speak of peace in times of war. What can you offer?""
    },
    ""_1_player"": ""Let us create a pact. I can ensure the forest provides for us all without conflict."",
    ""_3"": ""The chief ponders your proposal, the crowd of goblins waiting for his decision."",
    ""_jmp"": {
      ""chc_0"": {
        ""br"": ""f"",
        ""txt"": ""Wait for the Goblin Chief's decision.""
      },
      ""chc_1"": {
        ""br"": ""g"",
        ""txt"": ""Offer more terms to sweeten the deal.""
      }
    }
  },
  ... (other branches),
  ""j"": {
    ""_0"": ""You travel to nearby villages, rallying the locals to your cause."",
    ""_0_bg"": ""village"",
    ""_1"": ""With a group of brave volunteers, you prepare to take back the forest from the goblins."",
    ""_0_apr"": ""Village Leader"",
    ""_0_npc"": {
      ""name"": ""Village Leader"",
      ""txt"": ""We stand with you, Thomas. Let's reclaim our forest!""
    },
    ""_0_player"": ""Together, we are stronger. Thank you for your courage."",
    ""_2"": ""Armed with local knowledge and reinforced by the villagers, you plan a large-scale assault on the goblin strongholds."",
    ""_jmp"": {
      ""chc_0"": {
        ""br"": ""r"",
        ""txt"": ""Launch the assault on the goblin strongholds.""
      },
      ""chc_1"": {
        ""br"": ""s"",
        ""txt"": ""Use the cover of night to launch a surprise attack.""
      }
    }
  },
  ... (other branches),
  ""z"": {
    ""_0_bg"": ""party_room"",
    ""_0"": ""With your efforts, the forest is fully secured, and the goblin threat is a thing of the past. Eldoria is now a beacon of peace and cooperation."",
    ""_1"": ""As you walk through the rejuvenated forest, you take pride in knowing your actions have helped create a sanctuary for all its inhabitants."",
    ""_0_apr"": ""Forest Guardian"",
    ""_1_apr"": ""Adventurous Spirit"",
    ""_0_npc"": {
      ""name"": ""Forest Guardian"",
      ""txt"": ""Thank you, Thomas. Your courage and wisdom have saved us all.""
    },
    ""_0_player"": ""It was my duty. Seeing the forest thrive is my greatest reward."",
    ""_2"": ""With peace established, you decide to explore new lands, seeking new adventures and ways to spread the peace you've fostered in Eldoria."",
    ""_end"": ""As you set off on new adventures, Eldoria remains a testament to your legacy, a land of peace and natural beauty.""
  }
}";

}