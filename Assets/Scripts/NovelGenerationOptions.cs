using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class NovelGenerationOptions : MonoBehaviour
{
    public TMP_InputField novel_name;
    public PlayerGenerationOptions playerGenerationOptions;
    public TMP_Dropdown category;
    public TMP_Dropdown theme;
    public TMP_Dropdown darkness_setting;
    public TMP_InputField plot_setting;
    public TMP_InputField plot_problem;
    public TMP_InputField plot_goal;
    public TMP_Dropdown art_style;
    public TMP_Dropdown romance;
    public TMP_InputField details;

    // Category, PlotSetting, PlotProblem, PlotProblem, Theme, DarknessSetting, Romance, Details
    private string promptNovelStringTemplate = "This is a {0} novel. " +
    "The setting is {1}, the main problem of the plot is {2}, and the goal is {3}. " +
    "The theme is {4}, it should shine in the plot. " +
    "The story should be {5} in its grimness/darkness. " +
    "Is there romance in the story={6}. Extra details: {7}. ";

    // DarknessSetting, Category, ArtStyle
    private string promptCharacterSettingDetailStringTemplate = " Character is from a {0} novel. Grimness/darkness of the novel is {1}. The art sytle is {2}. The setting of the novel is in {3}, the main problem of the plot is {4}, and the goal is {5}.";
    public class Novel
    {
        public string NovelName { get; set; }
        public string Category { get; set; }
        public string Theme { get; set; }
        public string DarknessSetting { get; set; }
        public string PlotSetting { get; set; }
        public string PlotProblem { get; set; }
        public string PlotGoal { get; set; }
        public string ArtStyle { get; set; }
        public bool Romance { get; set; }
        public string Details { get; set; }
    }

    public class NovelData
    {
        public static Novel CreateFromUI(
                                                TMP_InputField novel_name,
                                                TMP_Dropdown category,
                                                TMP_Dropdown theme,
                                                TMP_Dropdown darkness_setting,
                                                TMP_InputField plot_setting,
                                                TMP_InputField plot_problem,
                                                TMP_InputField plot_goal,
                                                TMP_Dropdown art_style,
                                                TMP_Dropdown romance,
                                                TMP_InputField details)
        {
            return new Novel
            {
                NovelName = novel_name.text,
                Category = category.options[category.value].text,
                Theme = theme.options[theme.value].text,
                DarknessSetting = darkness_setting.options[darkness_setting.value].text,
                PlotSetting = plot_setting.text,
                PlotProblem = plot_problem.text,
                PlotGoal = plot_goal.text,
                ArtStyle = art_style.options[art_style.value].text,
                Romance = romance.options[romance.value].text == "Yes",
                Details = details.text
            };
        }
    }

    public Novel GetNovelData()
    {
        Novel novelData = NovelData.CreateFromUI(novel_name, category, theme, darkness_setting, plot_setting, plot_problem, plot_goal, art_style, romance, details);

        return novelData;
    }


    public string GetNovelPrompt()
    {
        Novel novelData = GetNovelData();
        string detail = string.Format(promptNovelStringTemplate, novelData.Category, novelData.PlotSetting, novelData.PlotProblem, novelData.PlotGoal, novelData.Theme, novelData.DarknessSetting, novelData.Romance, novelData.Details);

        return detail;
    }

    public string GetCharacterPromptSettingDetail()
    {
        Novel novelData = GetNovelData();
        string detail = string.Format(promptCharacterSettingDetailStringTemplate, novelData.Category, novelData.DarknessSetting, novelData.ArtStyle, novelData.PlotSetting, novelData.PlotProblem, novelData.PlotGoal);

        return detail;
    }

    public string GetNovelCompletePrompt()
    {
        string novelPrompt = GetNovelPrompt();
        string playerPrompt = playerGenerationOptions.GetPlayerNovelPrompt();
        string completeNovelPrompt = novelPrompt + playerPrompt;
        return completeNovelPrompt;
    }
    public string GetNovelName()
    {

        Novel novelData = GetNovelData();
        return novelData.NovelName;
    }
}
