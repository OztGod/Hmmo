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
    string IdInput = "";
    string PswInput = "";
    bool IsLoginSuccess = false;
	bool IsMatchStart = false;
	public bool IsReady = false;
    string debugMsg = "";
	public MapIndex[] heros = null;

    void Awake()
    {
        //add a copy of TCPConnection to this game object
        TcpSession = gameObject.AddComponent<TCPConnections>();
        RecvBuffer = TcpSession.recvBuffer;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

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
                Camera.main.GetComponent<CameraScript>().OnCameraMove();
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

		if (heros != null && !IsReady)
		{
			if (GUILayout.Button("Ready"))
			{
				AllocHeros(heros);
				IsReady = true;
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
		heros = null;
    }

    public void AllocHeros( MapIndex[] allocIndexes )
    {
        Packets.AllocHero packet = new Packets.AllocHero();
        packet.allocNum = (sbyte)allocIndexes.Length;
        packet.type = (byte)Packets.Type.ALLOC_HERO;
        packet.x = new sbyte[4];
        packet.y = new sbyte[4];

        for(int i= 0 ; i < allocIndexes.Length ; ++i)
        {
            packet.x[i] = (sbyte)(allocIndexes[i].x);
            packet.y[i] = (sbyte)(allocIndexes[i].y);
        }

        byte[] data = Packets.ByteSerializer<Packets.AllocHero>.GetBytes(packet);
        TcpSession.writeSocket(data);

        debugMsg = "Alloc Hero...";
    }

    void OnLoginResponse(Packets.LoginResult result)
    {
        switch(result)
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
    }

    void OnGameData(Packets.GameData result)
    {

    }

    void OnRandomHeroResponse(byte[] heros)
    {
        int[] classArray = new int[heros.Length];
        for (int i = 0; i < heros.Length; ++i)
        {
            classArray[i] = (int)heros[i];
        }

        GameObject map = GameObject.FindGameObjectWithTag("Map");
        map.GetComponent<MapScript>().GetRandomCharacters(classArray);
        debugMsg = "Character Setting...";
    }

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

            default:
                break;
        }

        return true;
    }

}

public class MapIndex
{
    private int maxX = 3;
    private int maxY = 3;

    public int x = 1;
    public int y = 1;
}

namespace Packets
{

    public enum HeroClass
    {
        FIGHTER = 0,
        MAGICIAN = 1,
        ARCHER = 2,
        THIEF = 3,
        PRIEST = 4,
        MONK = 5,
    }
    public enum Type
    {
        LOGIN_REQUEST = 0,
        LOGIN_RESPONSE = 1,
        ALLOC_HERO = 2,
        RANDOM_HERO_REQUEST = 3,
        RANDOM_HERO_RESPONSE = 4,
        MATCH_START = 5,
        GAME_DATA = 6,
        TYPE_NUM = 7,
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
        public string buffer;
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
        public sbyte[] x;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] y;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RandomHeroRequest : Header
    {
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RandomHeroResponse : Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] heroClass;
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
        public sbyte[] skillNum = new sbyte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] skillIdx = new sbyte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] skillLevel = new sbyte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] x = new sbyte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sbyte[] y = new sbyte[4];
    }

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
            catch(Exception e)
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
}