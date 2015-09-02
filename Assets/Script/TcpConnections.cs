using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;

public class TCPConnections : MonoBehaviour
{
    //the name of the connection, not required but better for overview if you have more than 1 connections running
    public string conName = "GodHW";

    //ip/address of the server, 127.0.0.1 is for your own computer
    public string conHost = "10.73.44.30";

    //port for the server, make sure to unblock this in your router firewall if you want to allow external connections
    public int conPort = 41010;

    //a true/false variable for connection status
    public bool socketReady = false;

    public CircularBuffer recvBuffer = new CircularBuffer(1024 * 16);

    TcpClient mySocket;
    NetworkStream theStream;
    BinaryWriter theWriter;
    BinaryReader theReader;

    //try to initiate connection
    public void setupSocket()
    {
        try
        {
            mySocket = new TcpClient(conHost, conPort);
            theStream = mySocket.GetStream();
            theWriter = new BinaryWriter(theStream);
            theReader = new BinaryReader(theStream);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error:" + e);
        }
    }

    //send message to server
    public void writeSocket(byte[] sendData)
    {
        if (!socketReady)
            return;
      
        theWriter.Write(sendData);
        theWriter.Flush();
    }

    //read message from server
    public bool readSocket()
    {
        if (false == theStream.DataAvailable)
        {
            return false;
        }

        int recvByte = 0;
        recvByte = theStream.Read(recvBuffer.MBuffer, recvBuffer.GetBuffer(), recvBuffer.GetFreeSpaceSize());
        recvBuffer.Commit(recvByte);
        return true;
    }

    //disconnect from the socket
    public void closeSocket()
    {
        if (!socketReady)
            return;
        theWriter.Close();
        theReader.Close();
        mySocket.Close();
        socketReady = false;
    }

    //keep connection alive, reconnect if connection lost
    public void maintainConnection()
    {
        if (!theStream.CanRead)
        {
            setupSocket();
        }
    }
}

public class CircularBuffer
{

    byte[] mBuffer;

    public byte[] MBuffer
    {
        get { return mBuffer; }
    }
    int mBufferEnd;

    int mARegionPointer;
    int mARegionSize;

    int mBRegionPointer;
    int mBRegionSize;

    int mCapacity;

    const int NULLPTR = -1;

    public CircularBuffer(int capacity)
    {
        mBRegionPointer = NULLPTR;
        mARegionSize = 0;
        mBRegionSize = 0;
        mCapacity = capacity;
        mBuffer = new byte[mCapacity];
        mBufferEnd = mCapacity;
        mARegionPointer = 0;

    }


    public void BufferReset()
    {
        mBRegionPointer = NULLPTR;
        mARegionSize = 0;
        mBRegionSize = 0;

        mBufferEnd = mCapacity;
        mARegionPointer = 0;
    }


    /// 버퍼의 첫부분 len만큼 날리기
    //public void Remove(int len);

    public int GetFreeSpaceSize()
    {
        if (mBRegionPointer != NULLPTR)
            return GetBFreeSpace();
        else
        {
            /// A 버퍼보다 더 많이 존재하면, B 버퍼로 스위치
            if (GetAFreeSpace() < GetSpaceBeforeA())
            {
                AllocateB();
                return GetBFreeSpace();
            }
            else
                return GetAFreeSpace();
        }
    }

    public int GetStoredSize()
    {
        return mARegionSize + mBRegionSize;
    }

    public int GetContiguiousBytes()
    {
        if (mARegionSize > 0)
            return mARegionSize;
        else
            return mBRegionSize;
    }

    /// 쓰기가 가능한 위치 (버퍼의 끝부분) 반환
    public int GetBuffer()
    {
        if (mBRegionPointer != NULLPTR)
            return mBRegionPointer + mBRegionSize;
        else
            return mARegionPointer + mARegionSize;
    }




    /// 커밋(aka. IncrementWritten)
    public void Commit(int len)
    {
        if (mBRegionPointer != NULLPTR)
            mBRegionSize += len;
        else
            mARegionSize += len;
    }

    /// 버퍼의 첫부분 리턴
    public int GetBufferStart()
    {
        if (mARegionSize > 0)
            return mARegionPointer;
        else
            return mBRegionPointer;
    }



    void AllocateB()
    {
        mBRegionPointer = 0;
    }

    int GetAFreeSpace()
    {
        return (mBufferEnd - mARegionPointer - mARegionSize);
    }

    int GetSpaceBeforeA()
    {
        return mARegionPointer;
    }


    int GetBFreeSpace()
    {
        if (mBRegionPointer == NULLPTR)
            return 0;

        return (mARegionPointer - mBRegionPointer - mBRegionSize);
    }



    private void assert(bool expr)
    {
        if (expr)
            return;
        else
            Debug.Log("Assert!");
    }

    public bool Peek(byte[] destbuf, int bytes)
    {
        assert(mBuffer != null);

        if (mARegionSize + mBRegionSize < bytes)
            return false;

        int cnt = bytes;
        int aRead = 0;

        /// A, B 영역 둘다 데이터가 있는 경우는 A먼저 읽는다
        if (mARegionSize > 0)
        {
            aRead = (cnt > mARegionSize) ? mARegionSize : cnt;
            Array.Copy(mBuffer, mARegionPointer, destbuf, 0, aRead);
            //memcpy(destbuf, mARegionPointer, aRead);
            cnt -= aRead;
        }

        /// 읽기 요구한 데이터가 더 있다면 B 영역에서 읽는다
        if (cnt > 0 && mBRegionSize > 0)
        {
            assert(cnt <= mBRegionSize);

            /// 남은거 마저 다 읽기
            int bRead = cnt;
            Array.Copy(mBuffer, mBRegionPointer, destbuf, aRead, bRead);

            //memcpy(destbuf + aRead, mBRegionPointer, bRead);
            cnt -= bRead;
        }

        assert(cnt == 0);

        return true;

    }

    public bool Read(ref byte[] destbuf, int bytes)
    {
        assert(mBuffer != null);

        //destbuf = null;

        if (mARegionSize + mBRegionSize < bytes)
            return false;

        int cnt = bytes;
        int aRead = 0;


        /// A, B 영역 둘다 데이터가 있는 경우는 A먼저 읽는다
        if (mARegionSize > 0)
        {
            aRead = (cnt > mARegionSize) ? mARegionSize : cnt;
            Array.Copy(mBuffer, mARegionPointer, destbuf, 0, aRead);

            //memcpy(destbuf, mARegionPointer, aRead);
            mARegionSize -= aRead;
            mARegionPointer += aRead;
            cnt -= aRead;
        }

        /// 읽기 요구한 데이터가 더 있다면 B 영역에서 읽는다
        if (cnt > 0 && mBRegionSize > 0)
        {
            assert(cnt <= mBRegionSize);

            /// 남은거 마저 다 읽기
            int bRead = cnt;

            Array.Copy(mBuffer, mBRegionPointer, destbuf, aRead, bRead);

            //memcpy(destbuf + aRead, mBRegionPointer, bRead);
            mBRegionSize -= bRead;
            mBRegionPointer += bRead;
            cnt -= bRead;
        }

        assert(cnt == 0);

        /// A 버퍼가 비었다면 B버퍼를 맨 앞으로 당기고 A 버퍼로 지정 
        if (mARegionSize == 0)
        {
            if (mBRegionSize > 0)
            {
                if (mBRegionPointer != 0)
                {
                    // copy b region to front
                    char[] tempArr = new char[mBRegionSize];
                    Array.Copy(mBuffer, mBRegionPointer, tempArr, 0, mBRegionSize);
                    Array.Copy(tempArr, 0, mBuffer, 0, mBRegionSize);

                    //memmove(mBuffer, mBRegionPointer, mBRegionSize);
                }

                mARegionPointer = 0;
                mARegionSize = mBRegionSize;
                mBRegionPointer = NULLPTR;
                mBRegionSize = 0;
            }
            else
            {
                /// B에 아무것도 없는 경우 그냥 A로 스위치
                mBRegionPointer = NULLPTR;
                mBRegionSize = 0;
                mARegionPointer = 0;
                mARegionSize = 0;
            }
        }

        return true;
    }




    public bool Write(byte[] data, int bytes)
    {
        assert(mBuffer != null);

        /// Read와 반대로 B가 있다면 B영역에 먼저 쓴다
        if (mBRegionPointer != NULLPTR)
        {
            if (GetBFreeSpace() < bytes)
                return false;

            Array.Copy(data, 0, mBuffer, mBRegionPointer + mBRegionSize, bytes);
            //memcpy(mBRegionPointer + mBRegionSize, data, bytes);
            mBRegionSize += bytes;

            return true;
        }

        /// A영역보다 다른 영역의 용량이 더 클 경우 그 영역을 B로 설정하고 기록
        if (GetAFreeSpace() < GetSpaceBeforeA())
        {
            AllocateB();

            if (GetBFreeSpace() < bytes)
                return false;

            Array.Copy(data, 0, mBuffer, mBRegionPointer + mBRegionSize, bytes);

            //memcpy(mBRegionPointer + mBRegionSize, data, bytes);
            mBRegionSize += bytes;

            return true;
        }
        /// A영역이 더 크면 당연히 A에 쓰기
        else
        {
            if (GetAFreeSpace() < bytes)
                return false;

            Array.Copy(data, 0, mBuffer, mARegionPointer + mARegionSize, bytes);

            //memcpy(mARegionPointer + mARegionSize, data, bytes);
            mARegionSize += bytes;

            return true;
        }
    }




    public void Remove(int len)
    {
        int cnt = len;

        /// Read와 마찬가지로 A가 있다면 A영역에서 먼저 삭제

        if (mARegionSize > 0)
        {
            int aRemove = (cnt > mARegionSize) ? mARegionSize : cnt;
            mARegionSize -= aRemove;
            mARegionPointer += aRemove;
            cnt -= aRemove;
        }

        // 제거할 용량이 더 남은경우 B에서 제거 
        if (cnt > 0 && mBRegionSize > 0)
        {
            int bRemove = (cnt > mBRegionSize) ? mBRegionSize : cnt;
            mBRegionSize -= bRemove;
            mBRegionPointer += bRemove;
            cnt -= bRemove;
        }

        /// A영역이 비워지면 B를 A로 스위치 
        if (mARegionSize == 0)
        {
            if (mBRegionSize > 0)
            {
                /// 앞으로 당겨 붙이기
                if (mBRegionPointer != 0)
                {
                    //memmove(mBuffer, mBRegionPointer, mBRegionSize);
                    // copy b region to front
                    char[] tempArr = new char[mBRegionSize];
                    Array.Copy(mBuffer, mBRegionPointer, tempArr, 0, mBRegionSize);
                    Array.Copy(tempArr, 0, mBuffer, 0, mBRegionSize);
                }

                mARegionPointer = 0;
                mARegionSize = mBRegionSize;
                mBRegionPointer = NULLPTR;
                mBRegionSize = 0;
            }
            else
            {
                mBRegionPointer = NULLPTR;
                mBRegionSize = 0;
                mARegionPointer = 0;
                mARegionSize = 0;
            }
        }
    }
}
