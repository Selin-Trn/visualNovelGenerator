using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationPromptHelper : MonoBehaviour
{
  public static string novelFormatExplanation = @"
You need to generate a text in JSON format for a visual novel. The text will be parsed for integration into Unity. 
(All keys: _jmp for branch jumps, _number_apr and _number_dspr for npc appearances, _number_bg for background switches, _died and _end for endings, _number for narrative text, _number_player for player speech, _number_npc for npc speech). 
Branches are separated by letters (a, b, c..). Narrative texts are numbered (_0, _1, _2…). 
Player speech is numbered (_0_player, _1_player…). NPC speech (_0_npc, _1_npc…) and npc speech format is like  '_0_npc': {'name': 'Goblin Chief', 'txt': 'The road must continue from here... Follow me!'} with a name and txt key in it. Every single Npc given in the prompt must have some dialogue and must appear on the screen (example: '_number_apr':'Goblin') and should also leave the screen (example: '_number_dspr':'Goblin') at the appropriate times. For example, when the narrative goes, 'The princess walked into the room.',  '_number_apr':'Princess' item should come after that narrative text. Or, if the narrative says 'You shot the guard down!', '_number_dspr':'Guard' and guard should leave the screen because he was taken down. Be careful not to call _number_dspr on characters that are not on the screen or _number_apr on characters that are not in the novel prompt above. _num_bg key switches the background. This key should ALWAYS be the first key of the first branch as it sets the background. Background can be switched a few times through the novel but don’t exceed 3 different backgrounds (let’s say you used 'forest' background at branch a and then used 'castle room' background at branch d. If you want to use the background 'forest' again at branch z, make sure to use the exact same name as the visual novel has to generate a new background if you use a different name).  _jmp key comes at the end of the current branch and gives the user several choices. The choice, jumps the storyline to another branch. _died key means the player has died and _end key means the story has ended but the player is not dead. There must be several endings throughout the novel, some can be _died and some can be _end. They can be used as '_end':'You saved the kingdom and now can enjoy the peace.' and '_died':'The blade slid into your chest and before long, your world was in darkness.'. Jumps can't be made to old branches so make sure the branches do not jump to unrelated branches upon making a choice. Also, make sure there are no loops between branches. Be very careful not to have any duplicate keys in a branch! The keys are all supposed to be different! Or there will be problems during parsing. At total, there must be 26 branches in total as there are 26 letters in the alphabet. WRITE 26 BRANCHES! NO LESS! Write a story in this format with the story setting of =";
  public static string novelExampleJson = @"
    
    Example novel extract for formatting=
{
  ""a"": {
    ""_0_bg"": ""forest"",
    ""_0"": ""You are Thomas Woody, a skilled ranger who roams the dark and mysterious Forest of Eldoria. As you walk through the dense undergrowth, you hear the rustling of leaves and the distant howls of creatures."",
    ""_0_player"": ""What is that sound?"",
    ""_1"": ""Suddenly, the ground shakes beneath your feet, and a horde of goblins emerges from the shadows, their sharp teeth glinting in the dim light."",
    ""_1_apr"": ""Goblin Chief"",
    ""_0_npc"": {
      ""name"": ""Goblin Chief"",
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
        ""txt"": ""Try to reason with the goblins."",
      ""chc_2"": {
        ""br"": ""d"",
        ""txt"": ""Run away!""
      }
    }
  },
  ""b"": {
    ""_0"": ""With swift movements, you notch an arrow and let it fly towards the nearest goblin. The arrow whizzes through the air, striking true."",
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
    ""_1_player"": ""I seek peace between our kinds. Let us stop this needless violence."",
    ""_2"": ""A murmur ripples through the goblin ranks, some nodding in agreement, while others seem suspicious."",
    ""_3"": ""The Goblin Chief steps forward, eyeing you thoughtfully."",
    ""_1_apr"": ""Goblin Chief"",
    ""_1_npc"": {
      ""name"": ""Goblin Chief"",
      ""txt"": ""You speak of peace in times of war. What can you offer?""
    },
    ""_2_player"": ""Let us create a pact. I can ensure the forest provides for us all without conflict."",
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
etc.";
}