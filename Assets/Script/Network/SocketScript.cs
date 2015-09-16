using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


public class SocketScript : MonoBehaviour
{
    //variables
    private TCPConnections TcpSession;
    private CircularBuffer RecvBuffer;
    public bool IsReady = false;
    public int MaxHeroNum = 4;
    public List<MapIndex> HeroPositions = new List<MapIndex>();
    MapManager MapManager = null;

    string IdInput = "";
    string PswInput = "";
    string debugMsg = "";

    bool IsLoginSuccess = false;
    bool IsMatchStart = false;
    int MyTurn = 0;


    void Awake()
    {
        //add a copy of TCPConnection to this game object
        TcpSession = gameObject.AddComponent<TCPConnections>();
        RecvBuffer = TcpSession.recvBuffer;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        MapManager = GameObject.FindGameObjectWithTag("Map").GetComponent<MapManager>();
    }

    void Update()
    {
        //keep checking the server for messages, if a message is received from server, it gets logged in the Debug console (see function below)
        if (TcpSession.socketReady == false)
            return;

        SocketResponse();
    }

    void OnGUI()
    {
        //if connection has not been made, display button to connect
        if (false == TcpSession.socketReady)
        {

            if (GUILayout.Button("Connect"))
            {
                //try to connect
                Debug.Log("Attempting to connect..");
                TcpSession.setupSocket();
            }
            return;
        }

        //once connection has been made, display editable text field with a button to send that string to the server (see function below)
        else
        {
            if (false == IsLoginSuccess)
            {
                IdInput = GUILayout.TextField(IdInput);
                PswInput = GUILayout.TextField(PswInput);
                if (GUILayout.Button("Login", GUILayout.Height(30)))
                {
                    Login(IdInput, PswInput);
                }
            }

            float width = 300.0f;
            float height = 50.0f;
            float x = Screen.width - width;
            float y = Screen.height - height;

            Rect rect = new Rect(x, y, width, height);
            GUI.Box(rect, debugMsg);
        }

        if (IsMatchStart && !IsReady)
        {
            if (GUILayout.Button("Get Random Hero"))
            {
                RequestRandomHero();
            }
        }

        if (HeroPositions.Count >= 4 && !IsReady)
        {
            if (GUILayout.Button("Ready"))
            {
                AllocHeros();
                IsReady = true;
                MapManager.Ready();
                debugMsg = "Ready...";
            }
        }
    }

    //socket reading script
    void SocketResponse()
    {
        if (false == TcpSession.readSocket())
            return;

        debugMsg = "Get Data..";
        while (true)
        {
            int packetSize = Marshal.SizeOf(typeof(Packets.Header));
            if (RecvBuffer.GetStoredSize() < packetSize)
            {
                break;
            }

            debugMsg = "Packet Making...";
            byte[] headerBuffer = new byte[packetSize];
            RecvBuffer.Peek(headerBuffer, packetSize);
            Packets.Header header = new Packets.Header();
            Packets.ByteSerializer<Packets.Header>.ByteToObj(ref header, headerBuffer);
            Packets.Type packetType = (Packets.Type)header.type;

            if (false == PacketHandler(packetType))
                return;
        }
    }

    //send message to the server
    #region SendMsgToServer
    public void Login(string id, string psw)
    {
        Packets.LoginRequest packet = new Packets.LoginRequest();
        packet.type = (byte)Packets.Type.LOGIN_REQUEST;
        packet.idLength = (sbyte)id.Length;
        packet.passwordLength = (sbyte)psw.Length;
        packet.id = id.ToCharArray();
        packet.password = psw.ToCharArray();

        byte[] data = Packets.ByteSerializer<Packets.LoginRequest>.GetBytes(packet);
        TcpSession.writeSocket(data);

        debugMsg = "Attempting to login..";
    }

    void RequestRandomHero()
    {
        Packets.RandomHeroRequest packet = new Packets.RandomHeroRequest();
        packet.type = (byte)Packets.Type.RANDOM_HERO_REQUEST;
        byte[] data = Packets.ByteSerializer<Packets.RandomHeroRequest>.GetBytes(packet);
        TcpSession.writeSocket(data);

        debugMsg = "Request Random Hero...";
        HeroPositions.Clear() ;
    }

    public void AllocHeros()
    {
        Packets.AllocHero packet = new Packets.AllocHero();
        packet.allocNum = (sbyte)HeroPositions.Count;
        packet.type = (byte)Packets.Type.ALLOC_HERO;
        packet.x = new sbyte[4];
        packet.y = new sbyte[4];

        for (int i = 0; i < 4; ++i)
        {
            packet.x[i] = (sbyte)(HeroPositions[i].posX);
            packet.y[i] = (sbyte)(HeroPositions[i].posY);
        }

        byte[] data = Packets.ByteSerializer<Packets.AllocHero>.GetBytes(packet);
        TcpSession.writeSocket(data);

        debugMsg = "Alloc Hero...";
    }

    public void CharacterSelect(int heroIndex)
    {
        Packets.SelectHero packet = new Packets.SelectHero();
        packet.type = (byte)Packets.Type.SELECT_HERO;
        packet.idx = (sbyte)heroIndex;
        byte[] data = Packets.ByteSerializer<Packets.SelectHero>.GetBytes(packet);
        TcpSession.writeSocket(data);

        debugMsg = "Select Hero...";
    }

    public void RequestMove(int heroIndex, MapIndex position)
    {
        Packets.MoveHero packet = new Packets.MoveHero();
        packet.type = (byte)Packets.Type.MOVE_HERO;
        packet.idx = (sbyte)heroIndex;
        packet.x = (sbyte)position.posX;
        packet.y = (sbyte)position.posY;
        byte[] data = Packets.ByteSerializer<Packets.MoveHero>.GetBytes(packet);
        TcpSession.writeSocket(data);

        debugMsg = "Request Move...";
    }

    public void RequestTurnEnd()
    {
        Packets.TurnEnd packet = new Packets.TurnEnd();
        packet.type = (byte)Packets.Type.TURN_END;
        packet.turn = (sbyte)MyTurn;
        byte[] data = Packets.ByteSerializer<Packets.TurnEnd>.GetBytes(packet);
        TcpSession.writeSocket(data);

        debugMsg = "Request End Turn...";
    }

    public void RequestSkillRange(int heroIdx, int skillIdx)
    {
        Packets.SkillRangeRequest packet = new Packets.SkillRangeRequest();
        packet.type = (byte)Packets.Type.SKILL_RANGE_REQUEST;
        packet.heroIdx = (sbyte)heroIdx;
        packet.skillIdx = (sbyte)skillIdx;
        byte[] data = Packets.ByteSerializer<Packets.SkillRangeRequest>.GetBytes(packet);
        TcpSession.writeSocket(data);

        debugMsg = "Request SkillRange...";
    }

    public void RequestSkillAction(int heroIdx, MapIndex mapPos, int skillIdx)
    {
        Packets.ActHero packet = new Packets.ActHero();
        packet.type = (byte)Packets.Type.ACT_HERO;
        packet.x = (sbyte)mapPos.posX;
        packet.y = (sbyte)mapPos.posY;
        packet.heroIdx = (sbyte)heroIdx;
        packet.skillIdx = (sbyte)skillIdx;

        byte[] data = Packets.ByteSerializer<Packets.ActHero>.GetBytes(packet);
        TcpSession.writeSocket(data);

        debugMsg = "Request SkillAction...";
    }
    #endregion

    #region RecvServerEvent
    void OnLoginResponse(Packets.LoginResult result)
    {
        switch (result)
        {
            case Packets.LoginResult.SUCCESS:
                IsLoginSuccess = true;
                debugMsg = "Login Complete!...";
                break;
            case Packets.LoginResult.FAILED:
                TcpSession.closeSocket();
                debugMsg = "Login Failed!...";
                break;
        }
    }

    void OnMatchStart(Packets.MatchStart result)
    {
        IsMatchStart = true;
        MyTurn = result.turn;
    }

    void OnGameData(Packets.GameData packet)
    {
        HeroModel[] heroDatas = new HeroModel[4];
        for (int i = 0; i < 4; ++i)
        {
            heroDatas[i] = new HeroModel();
            heroDatas[i].heroClass = (HeroClass)packet.classes[i];
            heroDatas[i].position.posX = packet.x[i];
            heroDatas[i].position.posY = packet.y[i];
            heroDatas[i].hp = packet.hp[i];
            heroDatas[i].ap = packet.act[i];
        }

        bool isMyData = (packet.turn == MyTurn);
        MapManager.GetCharacters(heroDatas, isMyData);
        debugMsg = "Get GameData!";
    }

    void OnSkillData(Packets.SkillData packet)
    {
        List<SkillModel> skills = new List<SkillModel>();
        for (int i = 0; i < (int)packet.skillNum; ++i)
        {
            SkillModel newSkill = new SkillModel();
            newSkill.level = (int)packet.skillLevel[i];
            newSkill.type = (SkillType)packet.skillType[i];
            skills.Add(newSkill);
        }
        MapManager.SetHeroSkills(packet.heroIdx, skills);
        debugMsg = "Get SkillData!";
    }

    void OnChangeHeroState(Packets.ChangeHeroState packet)
    {
        HeroStateModel model = new HeroStateModel();
        model.act = packet.act;
        model.hp = packet.hp;
        model.index = packet.idx;
        model.position.posX = packet.x;
        model.position.posY = packet.y;
        model.isForcedMove = (0 == (int)packet.isMove);

        bool isMine = MyTurn == packet.turn;

        MapManager.SynchronizeState(model, isMine);
    }

    void OnRandomHeroResponse(byte[] heroRes)
    {
        int[] classArray = new int[heroRes.Length];
        for (int i = 0; i < heroRes.Length; ++i)
        {
            classArray[i] = (int)heroRes[i];
        }

        MapManager.GetRandomCharacters(classArray);
        debugMsg = "Character Setting...";
    }

    void OnUpdateTurn(Packets.UpdateTurn packet)
    {
        bool isMine = packet.nowTurn == MyTurn;
        MapManager.TurnStart(isMine);
        debugMsg = "Update turn...";
    }

    void OnValidSkills(Packets.ValidSkills packet)
    {
        List<int> validSkills = new List<int>();
        for (int i = 0; i < packet.num; ++i)
        {
            validSkills.Add((int)(packet.idx[i]));
        }
        MapManager.SetValidSkills(validSkills);
    }

    void OnResponseSkillRange(Packets.SkillRangeResponse packet)
    {
        List<MapIndex> rangeIndexes = new List<MapIndex>();
        Debug.Log("OnSkillRange!");
        for (int i = 0; i < packet.rangeNum; i++)
        {
            Debug.Log("range[" + i + "]:(" + packet.rangeX[i] + "," + packet.rangeY[i] + ")");
            rangeIndexes.Add(new MapIndex(packet.rangeX[i], packet.rangeY[i]));
        }
        MapManager.ResponseRange(packet.heroIdx, packet.skillIdx, rangeIndexes, packet.isMyField == 1);
    }

    void OnResponseSkillEffect(Packets.EffectResponse packet)
    {
        List<EffectRange> effectIndexes = new List<EffectRange>();
        for (int i = 0; i < packet.effectNum; i++)
        {
            Debug.Log("effect[" + i + "]:(" + packet.effectX[i] + "," + packet.effectY[i] + ")");
            effectIndexes.Add(new EffectRange(packet.effectX[i], packet.effectY[i]));
        }
        MapManager.ResponseEffect(packet.heroIdx, packet.skillIdx, effectIndexes);
    }

    void OnSkillResponse(Packets.SkillShot packet)
    {
        SkillEffectModel model = new SkillEffectModel();
        model.IsMyTurn = (MyTurn == (int)packet.turn);
        model.SubjectHeroIdx = packet.heroIdx;
        model.CastingSkill = packet.skillIdx;
        model.AffectedPosNum = packet.effectNum;
        for (int i = 0; i < model.AffectedPosNum; ++i)
        {
            model.IsMyField.Add(MyTurn == (int)packet.effectTurn[i]);
            model.AffectedPositions.Add(new MapIndex(packet.effectX[i], packet.effectY[i]));
        }
        MapManager.ResponseSkill(model);
    }

    void OnCharacterDead(Packets.DeadHero packet)
    {
        bool isMyTurn = (MyTurn == (int)packet.turn);
        int deadHeroIdx = (int)packet.heroIdx;
        MapManager.OnChracterDead(isMyTurn, deadHeroIdx);
    }

    void OnHeroState(Packets.HeroState packet)
    {

    }

    void OnReject()
    {
        MapManager.RejectPacket();
        debugMsg = "Reject...";
    }
    #endregion

    #region PacketHandler
    bool PacketHandler(Packets.Type type)
    {
        switch (type)
        {
            case Packets.Type.LOGIN_RESPONSE:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.LoginResponse));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "Processing Login...";
                    Packets.LoginResponse packet = new Packets.LoginResponse();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.LoginResponse>.ByteToObj(ref packet, buffer);
                    Packets.LoginResult result = (Packets.LoginResult)packet.result;
                    OnLoginResponse(result);
                }
                break;

            case Packets.Type.RANDOM_HERO_RESPONSE:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.RandomHeroResponse));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "Random Hero Response...";
                    Packets.RandomHeroResponse packet = new Packets.RandomHeroResponse();
                    packet.heroClass = new byte[4];
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);
                    packet = Packets.ByteSerializer<Packets.RandomHeroResponse>.Deserialize(buffer);
                    OnRandomHeroResponse(packet.heroClass);
                }
                break;

            case Packets.Type.MATCH_START:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.MatchStart));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "Match Start...";
                    Packets.MatchStart packet = new Packets.MatchStart();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.MatchStart>.ByteToObj(ref packet, buffer);
                    OnMatchStart(packet);
                }
                break;

            case Packets.Type.MATCH_END:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.MatchEnd));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "Match End...";
                    Packets.MatchEnd packet = new Packets.MatchEnd();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.MatchEnd>.ByteToObj(ref packet, buffer);
                }
                break;

            case Packets.Type.GAME_DATA:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.GameData));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "GameData...";
                    Packets.GameData packet = new Packets.GameData();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.GameData>.ByteToObj(ref packet, buffer);
                    OnGameData(packet);
                }
                break;

            case Packets.Type.SKILL_DATA:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.SkillData));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "GameData...";
                    Packets.SkillData packet = new Packets.SkillData();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.SkillData>.ByteToObj(ref packet, buffer);
                    OnSkillData(packet);
                }
                break;

            case Packets.Type.CHANGE_HERO_STATE:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.ChangeHeroState));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "ChangeHeroState...";
                    Packets.ChangeHeroState packet = new Packets.ChangeHeroState();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.ChangeHeroState>.ByteToObj(ref packet, buffer);
                    OnChangeHeroState(packet);
                }
                break;

            case Packets.Type.UPDATE_TURN:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.UpdateTurn));
                    debugMsg = "UpdateTurn Try...";

                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "UpdateTurn...";
                    Packets.UpdateTurn packet = new Packets.UpdateTurn();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.UpdateTurn>.ByteToObj(ref packet, buffer);
                    OnUpdateTurn(packet);
                }
                break;

            case Packets.Type.SELECT_HERO:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.SelectHero));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "SelectHero...";
                    Packets.SelectHero packet = new Packets.SelectHero();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.SelectHero>.ByteToObj(ref packet, buffer);
                }
                break;

            case Packets.Type.VALID_SKILLS:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.ValidSkills));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "ValidSkills...";
                    Packets.ValidSkills packet = new Packets.ValidSkills();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.ValidSkills>.ByteToObj(ref packet, buffer);
                    OnValidSkills(packet);
                }
                break;

            case Packets.Type.SKILL_RANGE_REQUEST:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.SkillRangeRequest));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "SkillRangeRequest...";
                    Packets.SkillRangeRequest packet = new Packets.SkillRangeRequest();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.SkillRangeRequest>.ByteToObj(ref packet, buffer);
                }
                break;

            case Packets.Type.SKILL_RANGE_RESPONSE:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.SkillRangeResponse));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "SkillRangeResponse...";
                    Packets.SkillRangeResponse packet = new Packets.SkillRangeResponse();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.SkillRangeResponse>.ByteToObj(ref packet, buffer);
                    OnResponseSkillRange(packet);
                }
                break;

            case Packets.Type.EFFECT_RESPONSE:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.EffectResponse));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "EffectResponse...";
                    Packets.EffectResponse packet = new Packets.EffectResponse();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.EffectResponse>.ByteToObj(ref packet, buffer);
                    OnResponseSkillEffect(packet);
                }
                break;

            case Packets.Type.SKILL_SHOT:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.SkillShot));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "EnemySkillShot...";
                    Packets.SkillShot packet = new Packets.SkillShot();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.SkillShot>.ByteToObj(ref packet, buffer);
                    OnSkillResponse(packet);
                }
                break;
            case Packets.Type.ACT_HERO:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.ActHero));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "ActHero...";
                    Packets.ActHero packet = new Packets.ActHero();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.ActHero>.ByteToObj(ref packet, buffer);
                }
                break;
            case Packets.Type.DEAD_HERO:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.DeadHero));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "DeadHero...";
                    Packets.DeadHero packet = new Packets.DeadHero();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.DeadHero>.ByteToObj(ref packet, buffer);
                    OnCharacterDead(packet);
                }
                break;
            case Packets.Type.REJECT:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.Reject));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "ActHero...";
                    Packets.Reject packet = new Packets.Reject();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.Reject>.ByteToObj(ref packet, buffer);
                    OnReject();
                }
                break;


            case Packets.Type.HERO_STATE:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.HeroState));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "HeroState...";
                    Packets.HeroState packet = new Packets.HeroState();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.HeroState>.ByteToObj(ref packet, buffer);

                }
                break;

            case Packets.Type.HERO_REMOVE_STATE:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.RemoveHeroState));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                    {
                        return false;
                    }

                    debugMsg = "RemoveHeroState...";
                    Packets.RemoveHeroState packet = new Packets.RemoveHeroState();
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.RemoveHeroState>.ByteToObj(ref packet, buffer);

                }
                break;

            default:
                break;
        }

        return true;
    }
    #endregion
}


namespace Packets
{
    #region Packets
    public enum Type
    {
        LOGIN_REQUEST = 0,
        LOGIN_RESPONSE = 1,
        ALLOC_HERO = 2,
        RANDOM_HERO_REQUEST = 3,
        RANDOM_HERO_RESPONSE = 4,
        MATCH_START = 5,
        MATCH_END = 6,
        GAME_DATA = 7,
        SKILL_DATA = 8,
        CHANGE_HERO_STATE = 9,
        SELECT_HERO = 10,
        VALID_SKILLS = 11,
        SKILL_RANGE_REQUEST = 12,
        SKILL_RANGE_RESPONSE = 13,
        SKILL_SHOT = 14,
        MOVE_HERO = 15,
        ACT_HERO = 16,
        DEAD_HERO = 17,
        TURN_END = 18,
        UPDATE_TURN = 19,
        REJECT = 20,
        HERO_STATE = 21,
        HERO_REMOVE_STATE = 22,
        EFFECT_RESPONSE = 23,
        TYPE_NUM = 24,
    }

    public enum LoginResult
    {
        FAILED = 0,
        SUCCESS = 1,
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
        public string foo;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class LoginRequest : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte idLength;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte passwordLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] password;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class LoginResponse : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte result;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AllocHero : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte allocNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] x = new sbyte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] y = new sbyte[4];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RandomHeroRequest : Header
    {
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RandomHeroResponse : Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] heroClass = new byte[4];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MatchStart : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte turn;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GameData : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte turn;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte classNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] classes = new byte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] hp = new sbyte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] act = new sbyte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] x = new sbyte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] y = new sbyte[4];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SkillData : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte heroIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte skillNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] skillType = new byte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] skillLevel = new sbyte[4];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MoveHero : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte idx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte x;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte y;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ChangeHeroState : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte turn;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte idx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte hp;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte act;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte x;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte y;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte isMove;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TurnEnd : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte turn;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class UpdateTurn : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte nowTurn;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SelectHero : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte idx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ValidSkills : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public sbyte[] idx = new sbyte[6];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SkillRangeRequest : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte heroIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte skillIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SkillRangeResponse : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte heroIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte skillIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte isMyField;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte rangeNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public sbyte[] rangeX = new sbyte[9];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public sbyte[] rangeY = new sbyte[9];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class EffectResponse : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte heroIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte skillIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte effectNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public sbyte[] effectX = new sbyte[9];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public sbyte[] effectY = new sbyte[9];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ActHero : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte heroIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte skillIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte x;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte y;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SkillShot : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte turn;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte heroIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte skillIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte effectNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public sbyte[] effectTurn = new sbyte[8];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public sbyte[] effectX = new sbyte[8];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public sbyte[] effectY = new sbyte[8];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MatchEnd : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte winner;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Reject : Header
    {
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class DeadHero : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte turn;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte heroIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class HeroState : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte stateType;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte targetTurn;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte targetIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte executerTurn;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte executerIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte stateId;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte damaged;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte hp;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte act;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte attack;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte defence;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte duration;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RemoveHeroState : Header
    {
        [MarshalAs(UnmanagedType.U1)]
        public sbyte targetTurn;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte targetIdx;
        [MarshalAs(UnmanagedType.U1)]
        public sbyte stateId;
        [MarshalAs(UnmanagedType.U1)]
        public byte stateType;
    }
    #endregion

    #region Serializer & Parser
    public class ByteSerializer<T>
    {
        public static byte[] GetBytes(T source)
        {
            int cb = Marshal.SizeOf(source);
            IntPtr ptr = Marshal.AllocHGlobal(cb);

            Marshal.StructureToPtr(source, ptr, false);

            byte[] arr = new byte[cb];
            Marshal.Copy(ptr, arr, 0, cb);

            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static void ByteToObj(ref T obj, byte[] data)
        {
            int cb = Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(cb);

            Marshal.Copy(data, 0, ptr, cb);
            try
            {
                Marshal.PtrToStructure(ptr, obj);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            Marshal.FreeHGlobal(ptr);
        }


        public static T Deserialize(byte[] buffer)
        {
            unsafe
            {
                fixed (byte* ptr = &buffer[0])
                {
                    return (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
                }
            }
        }
    }

    public class ByteParser<T>
    {
        private IntPtr ptr;

        public ByteParser(byte[] data, ref int offset, ref T target)
        {
            int cb = Marshal.SizeOf((T)target);
            //this.ptr = Marshal.AllocCoTaskMem(cb);
            this.ptr = Marshal.AllocHGlobal(cb);
            Marshal.Copy(data, offset, this.ptr, cb);
            target = (T)Marshal.PtrToStructure(this.ptr, typeof(T));
            offset += cb;

            //Marshal.FreeCoTaskMem(this.ptr);
            Marshal.FreeHGlobal(this.ptr);
        }

        ~ByteParser()
        {
            //Marshal.FreeCoTaskMem(this.ptr);
        }
    }
    #endregion
}