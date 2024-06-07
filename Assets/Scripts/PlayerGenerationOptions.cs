using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PlayerGenerationOptions : MonoBehaviour
{
    public NovelGenerationOptions novelGenerationOptions;
    public TMP_InputField player_title;
    public TMP_InputField player_name;
    public TMP_InputField player_surname;
    public TMP_Dropdown face_shape;
    public TMP_Dropdown hair_length;
    public TMP_Dropdown hair_color;
    public TMP_Dropdown hair_texture;
    public TMP_Dropdown eye_color;
    public TMP_Dropdown eye_shape;
    public TMP_Dropdown eyebrow_color;
    public TMP_Dropdown eyebrow_shape;
    public TMP_Dropdown eyebrow_thickness;
    public TMP_Dropdown nose_shape;
    public TMP_Dropdown lips_shape;
    public TMP_Dropdown gender;
    public TMP_Dropdown age;
    public TMP_Dropdown skin;
    public TMP_Dropdown height;
    public TMP_Dropdown weight;
    public TMP_Dropdown muscle;
    public TMP_InputField additional_details;

    // Gender,Age,Title,FaceShape,HairLength,HairColor,HairTexture,EyeColor,EyeShape,EyebrowColor,EyebrowShape,EyebrowThickness,NoseShape,LipsShape,Skin,Height,Weight,Muscle,AdditionalDetails
    private string promptPlayerPortaitStringTemplate = "{0} {1} {2}. {3} face. " +
    "{4} {5} {6} hair. " +
    "{7} {8} eyes. " +
    "{9} {10} {11} eyebrows. " +
    "{12} nose, {13} lips, {14} skin. " +
    "{15} height, {16} weight. " +
    "{17}. " +
    "{18}. " +
    "A portrait of this character from shoulders up. Black only background. No repetitive heads. There should be only one single head!";

    // Gender,Age,Title,name,surname,FaceShape,HairLength,HairColor,HairTexture,EyeColor,EyeShape,EyebrowColor,EyebrowShape,EyebrowThickness,NoseShape,LipsShape,Skin,Height,Weight,Muscle,AdditionalDetails
    private string promptPlayerNovelStringTemplate = "The player is {0} {1} {2}. Their name is {3} {4}. They have; {5} face. " +
        "{6} {7} {8} hair. " +
        "{9} {10} eyes. " +
        "{11} {12} {13} eyebrows. " +
        "{14} nose, {15} lips, {16} skin. " +
        "{17} height, {18} weight. " +
        "{19}. " +
        "{20}. " +
        "This was the description of the player. ";
    public class PlayerOptions
    {
        public string FaceShape { get; set; }
        public string HairLength { get; set; }
        public string HairColor { get; set; }
        public string HairTexture { get; set; }
        public string EyeColor { get; set; }
        public string EyeShape { get; set; }
        public string EyebrowColor { get; set; }
        public string EyebrowShape { get; set; }
        public string EyebrowThickness { get; set; }
        public string NoseShape { get; set; }
        public string LipsShape { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public string Skin { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Muscle { get; set; }
        public string PlayerTitle { get; set; }
        public string PlayerName { get; set; }
        public string PlayerSurname { get; set; }
        public string AdditionalDetails { get; set; }
    }

    public class PlayerOptionsData
    {
        public static PlayerOptions CreateFromUI(TMP_InputField playerTitle,
                                          TMP_InputField playerName,
                                          TMP_InputField playerSurname,
                                          TMP_InputField additionalDetails,
                                          TMP_Dropdown face_shape,
                                          TMP_Dropdown hair_length,
                                          TMP_Dropdown hair_color,
                                          TMP_Dropdown hair_texture,
                                          TMP_Dropdown eye_color,
                                          TMP_Dropdown eye_shape,
                                          TMP_Dropdown eyebrow_color,
                                          TMP_Dropdown eyebrow_shape,
                                          TMP_Dropdown eyebrow_thickness,
                                          TMP_Dropdown nose_shape,
                                          TMP_Dropdown lips_shape,
                                          TMP_Dropdown gender,
                                          TMP_Dropdown age,
                                          TMP_Dropdown skin,
                                          TMP_Dropdown height,
                                          TMP_Dropdown weight,
                                          TMP_Dropdown muscle)
        {
            return new PlayerOptions
            {
                FaceShape = face_shape.options[face_shape.value].text,
                HairLength = hair_length.options[hair_length.value].text,
                HairColor = hair_color.options[hair_color.value].text,
                HairTexture = hair_texture.options[hair_texture.value].text,
                EyeColor = eye_color.options[eye_color.value].text,
                EyeShape = eye_shape.options[eye_shape.value].text,
                EyebrowColor = eyebrow_color.options[eyebrow_color.value].text,
                EyebrowShape = eyebrow_shape.options[eyebrow_shape.value].text,
                EyebrowThickness = eyebrow_thickness.options[eyebrow_thickness.value].text,
                NoseShape = nose_shape.options[nose_shape.value].text,
                LipsShape = lips_shape.options[lips_shape.value].text,
                Gender = gender.options[gender.value].text,
                Age = age.options[age.value].text,
                Skin = skin.options[skin.value].text,
                Height = height.options[height.value].text,
                Weight = weight.options[weight.value].text,
                Muscle = muscle.options[muscle.value].text,
                PlayerTitle = playerTitle.text,
                PlayerName = playerName.text,
                PlayerSurname = playerSurname.text,
                AdditionalDetails = additionalDetails.text
            };
        }
    }
    public PlayerOptions GetPlayerData()
    {
        PlayerOptions playerData = PlayerOptionsData.CreateFromUI(player_title, player_name, player_surname, additional_details, face_shape,
        hair_length, hair_color, hair_texture, eye_color, eye_shape, eyebrow_color, eyebrow_shape, eyebrow_thickness, nose_shape,
        lips_shape, gender, age, skin, height, weight, muscle);

        return playerData;

    }
    public string GetPlayerNovelPrompt()
    {
        PlayerOptions playerData = GetPlayerData();
        string prompt = string.Format(promptPlayerNovelStringTemplate,
        playerData.Gender,
        playerData.Age,
        playerData.PlayerTitle,
        playerData.PlayerName,
        playerData.PlayerSurname,
        playerData.FaceShape,
        playerData.HairLength,
        playerData.HairColor,
        playerData.HairTexture,
        playerData.EyeColor,
        playerData.EyeShape,
        playerData.EyebrowColor,
        playerData.EyebrowShape,
        playerData.EyebrowThickness,
        playerData.NoseShape,
        playerData.LipsShape,
        playerData.Skin,
        playerData.Height,
        playerData.Weight,
        playerData.Muscle,
        playerData.AdditionalDetails);
        return prompt;

    }
    public string GetPlayerPortaitPrompt()
    {
        PlayerOptions playerData = GetPlayerData();
        string prompt = string.Format(promptPlayerPortaitStringTemplate,
        playerData.Gender,
        playerData.Age,
        playerData.PlayerTitle,
        playerData.FaceShape,
        playerData.HairLength,
        playerData.HairColor,
        playerData.HairTexture,
        playerData.EyeColor,
        playerData.EyeShape,
        playerData.EyebrowColor,
        playerData.EyebrowShape,
        playerData.EyebrowThickness,
        playerData.NoseShape,
        playerData.LipsShape,
        playerData.Skin,
        playerData.Height,
        playerData.Weight,
        playerData.Muscle,
        playerData.AdditionalDetails);
        return prompt;

    }
    public string GetPlayerPortraitCompletePrompt()
    {
        string playerPrompt = GetPlayerPortaitPrompt();
        string settingDetail = novelGenerationOptions.GetCharacterPromptSettingDetail();
        string completePlayerPrompt = playerPrompt + settingDetail;
        return completePlayerPrompt;

    }

    public string GetPlayerName()
    {

        PlayerOptions playerData = GetPlayerData();
        return playerData.PlayerName;
    }
}