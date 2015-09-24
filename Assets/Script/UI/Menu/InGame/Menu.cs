using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour
{
    CharacterScript targetHero = null;
    TurnOverButton turnButton;
    SkillContainer skillsCon;
    BarContainer barCon;
    ClassContainer classCon;
    // Use this for initialization
    bool isClicked = false;
    void Awake()
    {
        skillsCon = transform.FindChild("SkillContainer").GetComponent<SkillContainer>();
        barCon = transform.FindChild("BarContainer").GetComponent<BarContainer>();
        classCon = transform.FindChild("HeroClassContainer").GetComponent<ClassContainer>();
        turnButton = transform.FindChild("TurnButton").GetComponent<TurnOverButton>();
    }

    void Start()
    {
        ResetMenu();
    }

    public void SetTarget(CharacterScript target)
    {
        targetHero = target;
        UpdateMenu();
    }

    public void UpdateMenu()
    {
        if (targetHero == null)
        {
            ResetMenu();
            return;
        }

        skillsCon.SetSkillUIs(targetHero.Skills);
        barCon.SetHp(targetHero.MaxHP, targetHero.CurrentHP);
        barCon.SetAp(targetHero.MaxAp, targetHero.CurrentAp);
        classCon.SetClass(targetHero.heroType, targetHero.Level);
    }

    public void ResetMenu()
    {
        targetHero = null;
        skillsCon.Reset();
        barCon.Reset();
        classCon.Reset();
    }

    public void Turn(bool isOn)
    {
        turnButton.Turn(isOn);
        //ResetMenu();
    }

    public void Surrender()
    {
        var network = GameObject.FindGameObjectWithTag("Network").GetComponent<SocketScript>();
        network.RequestSurrender();
    }
}
