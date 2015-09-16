using UnityEngine;
using System.Collections;

public class SkillUIScript : MonoBehaviour {
    UIManager           uiManager;
    UI2DSprite          skillSprite;
    SkillLevelContainer skillLevel;
    CostContainer       skillCost;
    CoolDownContainer   skillCoolDown;
    UILabel             description;
    MapManager          mapManager;

    public int          skillIdx = -1;
    bool                isUsing;

	// Use this for initialization
    void Awake() 
    {
        uiManager = GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>();
        skillSprite = transform.FindChild("CoolDownContainer").GetComponent<UI2DSprite>();
        skillLevel = transform.FindChild("LevelContainer").GetComponent<SkillLevelContainer>();
        skillCost = transform.FindChild("CostCell").GetComponent<CostContainer>();
        skillCoolDown = transform.FindChild("CoolDownContainer").GetComponent<CoolDownContainer>();
        description = transform.FindChild("Description").GetComponent<UILabel>();
        mapManager = GameObject.FindGameObjectWithTag("Map").GetComponent<MapManager>();
	}

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SkillModel model = new SkillModel();
            model.type = (SkillType)Random.Range((int)SkillType.FIGHTER_ATTACK, (int)SkillType.MONK_KICK);
            model.level = Random.Range(1, 3);
            model.cost = Random.Range(1, 10);
            model.maxCool = Random.Range(1, 5);
            model.curCool = Random.Range(0, model.maxCool);
            SetSkillUI(model);
        }
        
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            Reset();
        }
    }

    public void SetSkillUI(SkillModel model)
    {
        if (model.type == SkillType.NUM)
        {
            Reset();
            return;
        }

        isUsing = true;
        skillSprite.sprite2D = uiManager.GetSkillSprite(model.type);
        description.text = uiManager.GetSkillDesc(model.type);
        skillLevel.SetLevel(model.level);
        skillCost.SetCost(model.cost);
        if(model.isAvailable)
        {
            skillCoolDown.SetCoolDown(model.curCool, model.maxCool);
        }
        else
        {
            skillCoolDown.CoolOff();
        }
    }

    public void Reset()
    {
        isUsing = true;
        skillSprite.sprite2D = null;
        description.text = "";
        skillLevel.SetLevel(0);
        skillCost.SetCost(0);
        skillCoolDown.SetCoolDown(0, 0);
    }

    void OnHover(bool isOver)
    {
        if (!isUsing)
            return;

        if(isOver)
        {
            if (Input.GetMouseButton(0))
            {
                description.gameObject.SetActive(false);
            }

            description.gameObject.SetActive(true);
        }
        else
        {
            description.gameObject.SetActive(false);
        }
    }

    public void OnButtonClick()
    {
        mapManager.OnSkillClick(skillIdx);
    }
}
