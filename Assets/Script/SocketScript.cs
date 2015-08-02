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
            }

            float buttonWidth = 200;
            float buttonHeight = 50;
            float buttonX = Screen.width / 2;
            float buttonY = Screen.height / 2 - buttonHeight / 2;

            if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "GameStart"))
            {
                GameObject manager = GameObject.Find("SceneManager");
                SceneManagerScript script = manager.GetComponent(typeof(SceneManagerScript)) as SceneManagerScript;
                script.SceneChange();
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
            else
            {
                float width = 300.0f;
                float height = 50.0f;
                float x = Screen.width / 2 - width / 2;
                float y = Screen.height / 2 - height /2;

                Rect rect = new Rect(x, y, width, height);
                GUI.Box(rect, "LOGIN SUCCESS!!");

                float buttonWidth = 200;
                float buttonHeight = 50;
                float buttonX = Screen.width / 2;
                float buttonY = Screen.height / 2 - buttonHeight/2;
                
                if(GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "GameStart"))
                {
                    GameObject manager = GameObject.Find("SceneManager");
                    SceneManagerScript script = manager.GetComponent(typeof(SceneManagerScript)) as SceneManagerScript;
                    script.SceneChange();
                }
            }
        }
    }

    //socket reading script
    void SocketResponse()
    {
        if (false == TcpSession.readSocket())
            return;

        Debug.Log("Get Data..");
        while (true)
        {
            int packetSize = Marshal.SizeOf(typeof(Packets.Header));
            if (RecvBuffer.GetStoredSize() < packetSize)
            {
                break;
            }

            Debug.Log("Packet Making...");
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

        Debug.Log("Attempting to login..");
    }

    void OnLoginResponse(Packets.LoginResult result)
    {
        switch(result)
        {
            case Packets.LoginResult.SUCCESS:
                IsLoginSuccess = true;
                Debug.Log("Login Complete!...");
                break;
            case Packets.LoginResult.FAILED:
                TcpSession.closeSocket();
                Debug.Log("Login Failed!...");
                break;
        }
    }

    bool PacketHandler(Packets.Type type)
    {
        switch (type)
        {
            case Packets.Type.LOGIN_RESPONSE:
                {
                    int packetSize = Marshal.SizeOf(typeof(Packets.LoginResponse));
                    if (RecvBuffer.GetStoredSize() < packetSize)
                        return false;

                    Debug.Log("Processing Login...");
                    Packets.LoginResponse packet = new Packets.LoginResponse();                    
                    byte[] buffer = new byte[packetSize];
                    RecvBuffer.Read(ref buffer, packetSize);

                    Packets.ByteSerializer<Packets.LoginResponse>.ByteToObj(ref packet, buffer);
                    Packets.LoginResult result = (Packets.LoginResult)packet.result;
                    OnLoginResponse(result);
                }
                break;
            default:
                break;
        }

        return true;
    }

}


namespace Packets
{
    public enum Type
    {
        LOGIN_REQUEST = 0,
        LOGIN_RESPONSE = 1,
        TYPE_NUM = 2,
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

            Marshal.PtrToStructure(ptr, obj);

            Marshal.FreeHGlobal(ptr);
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