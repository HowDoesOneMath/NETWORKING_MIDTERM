#include "pch.h"
#include "WinWrapper.h"

ClientSock cs;

PLUGIN_OUT int WSAGetErr()
{
	return  WSAGetLastError();
}

PLUGIN_OUT bool StartWINSOCK()
{
	if (WSAStartup(MAKEWORD(2, 2), &cs.wsa) != 0)
	{
		return false;
	}

	cs.RefHints();

	if (!cs.CreateSocket())
	{
		WSACleanup();
		return false;
	}

	cs.initialized = true;

	return true;
}

PLUGIN_OUT bool ConnectToServer(const char* IP, const char* PORT)
{
	if (!cs.GetAddress(IP, PORT))
		return false;

	if (!cs.ConnectSocket())
		return false;

	cs.connected = true;

	u_long mode = 1;
	ioctlsocket(cs.s, FIONBIO, &mode);

	return true;
}

PLUGIN_OUT bool CleanUpWINSOCK()
{
	if (cs.connected)
	{
		freeaddrinfo(cs.ptr);
		cs.connected = false;
	}
	if (cs.initialized)
	{
		shutdown(cs.s, SD_BOTH);
		closesocket(cs.s);
		cs.initialized = false;
	}
	WSACleanup();
	return true;
}

PLUGIN_OUT int CheckForData()
{
	int testValidity = 0;
	int length = -1;
	char testChars[8];
	testChars[0] = 0;
	testChars[1] = 0;
	testChars[2] = 0;
	testChars[3] = 0;
	testChars[4] = 0;
	testChars[5] = 0;
	testChars[6] = 0;
	testChars[7] = 0;

	int amntRecv = recv(cs.s, &testChars[0], 8, MSG_PEEK);

	if (amntRecv <= 0)
		return -1;

	memcpy(&testValidity, &testChars[0], 4);

	if (testValidity == OK_PACKET_STAMP)
	{
		memcpy(&length, &testChars[4], 4);
		if (length > 0)
			return length;
	}

	return -1;
}

PLUGIN_OUT char* GetData()
{
	int amntRecv = recv(cs.s, &cs.receiveBuffer[0], MAX_PACKET_SIZE, 0);

	return cs.receiveBuffer;
}

PLUGIN_OUT bool SendData(char* buffer, int length)
{
	memcpy(&cs.sendBuffer[0], buffer, length);

	if (send(cs.s, &cs.sendBuffer[0], length, 0) == SOCKET_ERROR)
	{
		return false;
	}

	return true;
}

void ClientSock::RefHints()
{
	memset(&hints, 0, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;
	hints.ai_flags = AI_PASSIVE;
}

bool ClientSock::GetAddress(PCSTR sockName, PCSTR sockPort)
{
	if (getaddrinfo(sockName, sockPort, &hints, &ptr) != 0)
	{
		return false;
	}

	return true;
}

bool ClientSock::CreateSocket()
{
	s = socket(hints.ai_family, hints.ai_socktype, hints.ai_protocol);

	if (s == INVALID_SOCKET)
	{
		//ErrDebugAndCleanup("Failed to create a socket");
		return false;
	}

	return true;
}

bool ClientSock::ConnectSocket()
{
	if (connect(s, ptr->ai_addr, (int)ptr->ai_addrlen) == SOCKET_ERROR)
	{
		freeaddrinfo(ptr);
		return false;
	}

	return true;
}
