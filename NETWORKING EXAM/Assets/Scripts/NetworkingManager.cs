using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum PACKET_TYPE
{
    INIT,
    START,
    POSITION,
    BULLET,
    SCORE
}

public class NetworkingManager : MonoBehaviour
{
    public static GameObject tank1, tank2;
    public static GameObject bullet;

    public static int INITIAL_OFFSET = 12;
    public static int MAX_PACKET_SIZE = 100;
    public static int STAMP_OFFSET = 4;
    public static int OK_PACKET_STAMP = 123456789;

    public static int MY_ID = -1;

    public Text IP;
    public Text PORT;

    const string DLL_NAME = "MIDTERM_DLL";

    [DllImport(DLL_NAME)]
    internal static extern int WSAGetErr();
    [DllImport(DLL_NAME)]
    internal static extern bool StartWINSOCK();
    [DllImport(DLL_NAME)]
    internal static extern bool ConnectToServer(string IP, string PORT);
    [DllImport(DLL_NAME)]
    internal static extern bool CleanUpWINSOCK();
    [DllImport(DLL_NAME)]
    internal static extern int CheckForData();
    [DllImport(DLL_NAME)]
    internal static extern IntPtr GetData();
    [DllImport(DLL_NAME)]
    internal static extern bool SendData(IntPtr buffer, int length);

    static byte[] sendBuffer;
    static byte[] receiveBuffer;

    // Start is called before the first frame update
    void Start()
    {
        sendBuffer = new byte[MAX_PACKET_SIZE];
        receiveBuffer = new byte[MAX_PACKET_SIZE];
        if (!StartWINSOCK())
        {
            Debug.Log("GU");
            CloseUp();
            return;
        }


    }

    public void LaunchJoin()
    {
        JoinServer(IP.text, PORT.text);
    }

    public static bool JoinServer(string ip, string port)
    {
        if (ConnectToServer(ip, port))
        {
            return true;
        }
        Debug.Log(WSAGetErr());

        return false;
    }

    public static void SendPosition(Vector3 pos, float rot)
    {
        int loc = INITIAL_OFFSET;
        PackData(ref sendBuffer, ref loc, pos.x);
        PackData(ref sendBuffer, ref loc, pos.z);
        PackData(ref sendBuffer, ref loc, rot);

        SendIntPtr(loc, (int)PACKET_TYPE.POSITION);
    }

    public static void SendBullet(Vector3 pos, Vector3 dir)
    {
        int loc = INITIAL_OFFSET;
        PackData(ref sendBuffer, ref loc, pos.x);
        PackData(ref sendBuffer, ref loc, pos.z);
        PackData(ref sendBuffer, ref loc, dir.x);
        PackData(ref sendBuffer, ref loc, dir.z);

        SendIntPtr(loc, (int)PACKET_TYPE.BULLET);
    }

    public static void ProcessPackets(GameObject t1, GameObject t2)
    {
        int test = CheckForData();
        while (test > 0)
        {
            Debug.Log("GOT PACKET");
            IntPtr data = GetData();
            Marshal.Copy(data, receiveBuffer, 0, test);

            SwitchPacket(t1, t2);

            test = CheckForData();
        }
    }

    static void SwitchPacket(GameObject t1, GameObject t2)
    {
        int loc = STAMP_OFFSET;

        int length = 0;
        int type = -1;

        UnpackInt(ref receiveBuffer, ref loc, ref length);
        UnpackInt(ref receiveBuffer, ref loc, ref type);

        switch ((PACKET_TYPE)type)
        {
            case PACKET_TYPE.START:
                if (MY_ID < 0)
                {
                    UnpackInt(ref receiveBuffer, ref loc, ref MY_ID);
                    SceneManager.LoadScene(1);
                }
                break;
            case PACKET_TYPE.POSITION:
                float x = 0, y = 0, r = 0;
                UnpackFloat(ref receiveBuffer, ref loc, ref x);
                UnpackFloat(ref receiveBuffer, ref loc, ref y);
                UnpackFloat(ref receiveBuffer, ref loc, ref r);
                if (MY_ID == 0)
                {
                    t2.transform.SetPositionAndRotation(new Vector3(x, 0, y), Quaternion.Euler(0, r, 0));
                }
                else
                {
                    t1.transform.SetPositionAndRotation(new Vector3(x, 0, y), Quaternion.Euler(0, r, 0));
                }
                break;
            case PACKET_TYPE.BULLET:
                float vx = 0, vy = 0, bx = 0, by = 0;
                UnpackFloat(ref receiveBuffer, ref loc, ref bx);
                UnpackFloat(ref receiveBuffer, ref loc, ref by);
                UnpackFloat(ref receiveBuffer, ref loc, ref vx);
                UnpackFloat(ref receiveBuffer, ref loc, ref vy);
                GameObject newBul = Instantiate(bullet, new Vector3(bx, 0, by), Quaternion.LookRotation(new Vector3(vx, 0, vy)));
                break;
        }
    }

    public static void CloseUp()
    {
        CleanUpWINSOCK();
    }

    #region packingData
    public static void PackData(ref byte[] bytes, ref int loc, bool data)
    {
        BitConverter.GetBytes(data).CopyTo(bytes, loc);
        loc += Marshal.SizeOf(data);
    }
    public static void PackData(ref byte[] bytes, ref int loc, int data)
    {
        BitConverter.GetBytes(data).CopyTo(bytes, loc);
        loc += Marshal.SizeOf(data);
    }
    public static void PackData(ref byte[] bytes, ref int loc, float data)
    {
        BitConverter.GetBytes(data).CopyTo(bytes, loc);
        loc += Marshal.SizeOf(data);
    }
    public static void PackData(ref byte[] bytes, ref int loc, char data)
    {
        //BitConverter.GetBytes(data).CopyTo(bytes, loc);
        bytes[loc] = (byte)data;
        loc += Marshal.SizeOf(data);
    }

    public static void PackData(ref byte[] bytes, ref int loc, string data)
    {
        PackData(ref bytes, ref loc, data.Length);

        for (int i = 0; i < data.Length; ++i)
        {
            PackData(ref bytes, ref loc, data[i]);
        }
    }

    static bool SendIntPtr(int length, int packetType)
    {
        bool returnVal = false;

        BitConverter.GetBytes(OK_PACKET_STAMP).CopyTo(sendBuffer, 0);
        BitConverter.GetBytes(length).CopyTo(sendBuffer, 4);
        BitConverter.GetBytes(packetType).CopyTo(sendBuffer, 8);

        //SendDebugOutput("ID: " + playerID.ToString() + ", Type: " + packetType.ToString() + ", LENGTH: " + length.ToString());

        IntPtr ptr = Marshal.AllocCoTaskMem(length);

        Marshal.Copy(sendBuffer, 0, ptr, length);

        //SendDataFunc

        //SendDebugOutput("C#: SENDING PACKET");
        returnVal = SendData(ptr, length);

        Marshal.FreeCoTaskMem(ptr);

        return returnVal;
    }

    public static void UnpackBool(ref byte[] byteArray, ref int loc, ref bool output)
    {
        output = BitConverter.ToBoolean(byteArray, loc);
        loc += Marshal.SizeOf(output);
    }

    public static void UnpackInt(ref byte[] byteArray, ref int loc, ref int output)
    {
        output = BitConverter.ToInt32(byteArray, loc);
        loc += Marshal.SizeOf(output);
    }

    public static void UnpackFloat(ref byte[] byteArray, ref int loc, ref float output)
    {
        output = BitConverter.ToSingle(byteArray, loc);
        loc += Marshal.SizeOf(output);
    }
    public static void UnpackChar(ref byte[] byteArray, ref int loc, ref char output)
    {
        output = (char)byteArray[loc];
        loc += Marshal.SizeOf(output);
    }

    public static void UnpackString(ref byte[] byteArray, ref int loc, ref string output)
    {
        int strLen = 0;
        UnpackInt(ref byteArray, ref loc, ref strLen);
        strLen += loc;

        while (loc < strLen)
        {
            char c = '0';
            UnpackChar(ref byteArray, ref loc, ref c);
            output += c;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
#endregion
