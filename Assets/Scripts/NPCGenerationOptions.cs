using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCGenerationOptions : MonoBehaviour
{
    public TMP_InputField npc_title;
    public TMP_InputField npc_name;
    public TMP_InputField npc_surname;
    public TMP_Dropdown gender;
    public TMP_Dropdown age;
    public TMP_Dropdown romance_interest;
    public TMP_InputField physical_details;
    public TMP_InputField personality_details;
    // Gender,Age,Title,physicalDetails
    private string promptNPCImageStringTemplate = "{0} {1} {2}. Physical details: {3}. Personality: {4}" +
    "A full body image of this character from shoulders up, that's all.";

    // Gender,Age,Title,name, surname physicalDetails, personalityDetails, name
    private string promptNPCNovelStringTemplate = "This NPC is {0} {1} {2}. Their name is {3} {4}. Physical details: {5}. Personality: {6}" +
    " Is this character a romance interest for the player: {7}. ";
    public class NPCOptions
    {
        public string NPCTitle { get; set; }
        public string NPCName { get; set; }
        public string NPCSurname { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public bool RomanceInterest { get; set; }
        public string PhysicalDetails { get; set; }
        public string PersonalityDetails { get; set; }
    }

    public class NPCOptionsData
    {
        public static NPCOptions CreateFromUI(TMP_InputField npcTitle,
                                          TMP_InputField npcName,
                                          TMP_InputField npcSurname,
                                          TMP_Dropdown gender,
                                          TMP_Dropdown age,
                                                TMP_Dropdown romanceInterest,
                                          TMP_InputField physicalDetails,
                                          TMP_InputField personalityDetails)
        {
            return new NPCOptions
            {
                NPCTitle = npcTitle.text,
                NPCName = npcName.text,
                NPCSurname = npcSurname.text,
                Gender = gender.options[gender.value].text,
                Age = age.options[age.value].text,
                RomanceInterest = romanceInterest.options[romanceInterest.value].text == "Yes",
                PhysicalDetails = physicalDetails.text,
                PersonalityDetails = personalityDetails.text
            };
        }
    }
    public NPCOptions GetPlayerData()
    {
        NPCOptions npcData = NPCOptionsData.CreateFromUI(npc_title, npc_name, npc_surname, age, gender, romance_interest, physical_details, personality_details);

        return npcData;

    }
    public string GetNPCName()
    {
        NPCOptions npcData = GetPlayerData();
        return npcData.NPCName;

    }
    public string GetNPCNovelPrompt()
    {
        NPCOptions npcData = GetPlayerData();
        string prompt = string.Format(promptNPCNovelStringTemplate,
        npcData.Gender,
        npcData.Age,
        npcData.NPCTitle,
        npcData.NPCName,
        npcData.NPCSurname,
        npcData.PhysicalDetails,
        npcData.PersonalityDetails,
        npcData.RomanceInterest);
        return prompt;

    }
    public string GetNPCImagePrompt()
    {
        NPCOptions npcData = GetPlayerData();
        string prompt = string.Format(promptNPCImageStringTemplate,
        npcData.Gender,
        npcData.Age,
        npcData.NPCTitle,
        npcData.PhysicalDetails,
        npcData.PersonalityDetails);
        return prompt;

    }
    public string GetNPCImageCompletePrompt()
    {
        string playerPrompt = GetNPCImagePrompt();
        string settingDetail = GenerationCommonPrompts.characterPromptSettingDetail;
        string completePlayerPrompt = playerPrompt + settingDetail;
        return completePlayerPrompt;

    }

}
