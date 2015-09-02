using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class PanelScript : MonoBehaviour {

    public GameObject ButtonPrefeb;
    public int ButtonMaxNum;
    public float WidthSpaceRatio;
    public float HeightSpaceRatio;

    UISprite BgSprite = null;
    MapScript MyMap = null;
    
    List<GameObject> Buttons = new List<GameObject>();
    List<ButtonSetting> ActionButtonSets = new List<ButtonSetting>();
    List<ButtonSetting> FirstSetting = new List<ButtonSetting>();
    List<int> ActiveActionIndexes = new List<int>();
    ButtonSetting BackButtonSetting = new ButtonSetting();

    Vector3 OpenPosition = new Vector3();
    Vector3 ClosePosition = new Vector3();
  
    int buttonWidth = 0;
    int buttonHeight = 0;
    bool IsOpen = false;

    // Use this for initialization
    void Start () 
    {
        for(int i = 0; i< ButtonMaxNum; i++)
        {
            GameObject button = Instantiate(ButtonPrefeb) as GameObject;
            button.transform.parent = transform;
            button.transform.localScale = Vector3.one;
            button.transform.localPosition = Vector3.zero;
            button.transform.localEulerAngles = Vector3.zero;
            Buttons.Add(button);
            button.SetActive(false);
        }
        BgSprite = GetComponent<UISprite>();
        buttonWidth = ButtonPrefeb.GetComponent<UISprite>().width;
        buttonHeight = ButtonPrefeb.GetComponent<UISprite>().height;

        FirstSetting.Add(new ButtonSetting());
        FirstSetting.Add(new ButtonSetting());
        FirstSetting[0].Name = "Action";
        FirstSetting[0].Type = ButtonType.ACTION;
        FirstSetting[0].IsEnable = true;
        FirstSetting[1].Name = "Exit";
        FirstSetting[1].Type = ButtonType.EXIT;
        FirstSetting[1].IsEnable = true;

        BackButtonSetting.Type = ButtonType.BACK;
        BackButtonSetting.Name = "Back";
        BackButtonSetting.IsEnable = true;

        ActionButtonSets.Add(BackButtonSetting);

        OpenPosition = new Vector3(-Screen.width / 2 + BgSprite.width / 2, Screen.height / 2 - BgSprite.height / 2, 0);
        ClosePosition = new Vector3(Screen.width, 0, 0);

	}
	
	// Update is called once per frame
	void Update () 
    {

	}

    public void SetMyMap(MapScript myMap)
    {
        MyMap = myMap;
    }


    public void OpenMenu()
    {
        if (IsOpen == true)
            return;
        
        transform.localPosition = OpenPosition;
        IsOpen = true;
        ButtonsSetting(FirstSetting);
    }

    public void CloseMenu()
    {
        if (IsOpen == false)
            return;

        transform.localPosition = ClosePosition;
        IsOpen = false;
        ClearButtons();
    }

    public void ButtonsSetting(List<ButtonSetting> buttonSets)
    {
        int setNum = Math.Min(buttonSets.Count, ButtonMaxNum);
        ClearButtons();
        ExtendBgSprite(setNum);
        float newY = -buttonHeight * 0.5f;
        for(int i = 0; i < setNum; ++i)
        {
            newY -= buttonHeight * HeightSpaceRatio;
            Buttons[i].SetActive(true);
            Buttons[i].GetComponent<ButtonScript>().SetButton(buttonSets[i]);
            Buttons[i].transform.localPosition = new Vector3(0, newY, 0);
            newY -= buttonHeight;
        }
    }

    public void SetActionButtons(List<SkillModel> skills)
    {
        ActionButtonSets.Clear();
        foreach(var skill in skills)
        {
            ActionButtonSets.Add(GetButtonSettingFromSkill(skill));
        }
        ActionButtonSets.Add(BackButtonSetting);

        if(IsOpen)
        {
            CloseMenu();
            OpenMenu();
        }
    }

    public void SetActiveButtons(List<int> validIndexes)
    {
        ActiveActionIndexes.Clear();
        for(int i = 0 ; i < ActionButtonSets.Count ; ++i)
        {
            ActionButtonSets[i].IsEnable = false;
        }
        ActionButtonSets[ActionButtonSets.Count - 1].IsEnable = true;
        for(int i = 0 ;i < validIndexes.Count; ++i)
        {
            ActiveActionIndexes.Add(validIndexes[i]);
            ActionButtonSets[validIndexes[i]].IsEnable = true;
        }

        if (IsOpen)
        {
            CloseMenu();
            OpenMenu();
        }
    }

    void ClearButtons()
    {
        for(int i = 0 ;i < ButtonMaxNum;++i)
        {
            Buttons[i].SetActive(false);
        }
    }

    void ExtendBgSprite(int buttonNum)
    {
        if (buttonNum == 0)
        {
            BgSprite.width = 0;
            BgSprite.height = 0;
        }
        else
        {
            BgSprite.width = buttonWidth + (int)(buttonWidth * WidthSpaceRatio * 2);
            BgSprite.height = buttonHeight * buttonNum + (int)(buttonHeight * HeightSpaceRatio * (buttonNum + 1));
        }
    }


    public void OnButtonClick(ButtonType buttonType)
    {
        switch(buttonType)
        {
            case ButtonType.ACTION:
                OnActionClick();
                break;
            case ButtonType.EXIT:
                OnQuitClick();
                break;
            case ButtonType.BACK:
                OnBackClick();
                break;
            default:
                OnSkillClick(buttonType);
                break;
        }
    }

    void OnActionClick()
    {
        ButtonsSetting(ActionButtonSets);
    }

    void OnQuitClick()
    {
        MyMap.CharacterTurnOver();
        CloseMenu();
    }

    void OnBackClick()
    {
        CloseMenu();
        OpenMenu();
    }

    void OnSkillClick(ButtonType type)
    {
        int selectedIdx = -1;
        foreach(int index in ActiveActionIndexes)
        {
            if (ActionButtonSets[index].Type == type)
            {
                selectedIdx = index;
                break;
            }
        }
        if (selectedIdx == -1)
        {
            Debug.Log("Skill Click Failed...");
            return;
        }

        MyMap.OnSkillClicked(selectedIdx);
    }

    ButtonSetting GetButtonSettingFromSkill(SkillModel skill)
    {
        ButtonSetting result = new ButtonSetting();
        result.Type = GetButtonTypeFromSkill(skill.type);
        result.Name = GetButtonNameFromSkill(skill.type, skill.level);
        return result;
    }


    ButtonType GetButtonTypeFromSkill(SkillType skillType)
    {
        return (ButtonType)((int)ButtonType.SKILL_START + (int)skillType + 1);
    }

    string GetButtonNameFromSkill(SkillType skillType, int level)
    {
        string typeString = skillType.ToString();
        string resultString = "";
        var splits = typeString.Split('_');
        if (splits.Length > 1)
        {
            for (int i = 1; i < splits.Length; ++i)
            {
                resultString += splits[i];
                resultString += " ";
            }
        }
        else
        {
            resultString = splits[0];
        }
        resultString += " " + level;
        return resultString;
    }
}

public enum ButtonType
{
    BACK,
    ACTION,
    EXIT,
    SKILL_START,
    FIGHTER_ATTACK,
    FIGHTER_CHARGE,
    FIGHTER_HARD,
    FIGHTER_IRON,
    MAGICIAN_ICE_ARROW,
    MAGICIAN_FIRE_BLAST,
    MAGICIAN_THUNDER_STORM,
    MAGICIAN_POLYMORPH,
    ARCHER_ATTACK,
    ARCHER_BACK_ATTACK,
    ARCHER_PENETRATE_SHOT,
    ARCHER_SNIPE,
    THIEF_ATTACK,
    THIEF_BACK_STEP,
    THIEF_POISON,
    THIEF_TAUNT,
    PRIEST_HEAL,
    PRIEST_ATTACK,
    PRIEST_BUFF,
    PRIEST_REMOVE_MAGIC,
    MONK_ATTACK,
    MONK_SACRIFICE,
    MONK_PRAY,
    MONK_KICK,
    NUM,
}

public class ButtonSetting
{
    public string Name = "";
    public ButtonType Type = ButtonType.BACK;
    public bool IsEnable = false;
}