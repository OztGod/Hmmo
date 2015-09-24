using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TotalWindow : MonoBehaviour
{
    public PickWindow MyWindow;
    public PickWindow EnemyWindow;
    public HeroWindow HWindow;
    public UIButton PickButton;
    List<int> pickedIndexes = new List<int>();
    SocketScript Network;
    UserScript User;
    int PickNum = 0;

    void Start()
    {
        PickButton.isEnabled = false;
        Network = GameObject.FindGameObjectWithTag("Network").GetComponent<SocketScript>();
        User = GameObject.FindGameObjectWithTag("Network").GetComponent<UserScript>();
    }

    void Update()
    {
        if (!User.IsMyPick)
        {
            return;
        }

        OnPickTurn(User.PickNum);
        User.IsMyPick = false;
        User.PickNum = 0;
    }

    public void OnPickData(bool isMine, HeroClass heroType)
    {
        if (isMine)
            return;

        PickWindow pickedWindow = isMine ? MyWindow : EnemyWindow;
        pickedWindow.PickHero(heroType);
    }

    public void OnPickTurn(int pickNum)
    {
        pickedIndexes.Clear();
        PickNum = pickNum;
        PickButton.isEnabled = true;
    }

    public void OnPickClicked()
    {
        if (HWindow.SelectedIdx == -1)
            return;

        pickedIndexes.Add(HWindow.SelectedIdx);
        HWindow.DisableButton(HWindow.SelectedIdx);
        MyWindow.PickHero(User.OwnHeroInfos[HWindow.SelectedIdx].heroType);
        HWindow.SelectedIdx = -1;
        PickNum--;
        if (PickNum <= 0)
        {
            //button enabled false;
            //sendPickes
            if (pickedIndexes.Count > 1)
            {
                Network.SendPick(pickedIndexes[0], pickedIndexes[1]);
            }
            else
            {
                Network.SendPick(pickedIndexes[0], -1);
            }

            PickButton.isEnabled = false;
        }
    }
}
