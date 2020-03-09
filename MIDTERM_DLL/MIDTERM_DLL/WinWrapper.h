#pragma once

#include <iostream>
#include <Windows.h>
#include <string>
#include <WinSock2.h>
#include <WS2tcpip.h>
#pragma comment(lib, "ws2_32.lib")

#define INITIAL_OFFSET 12
#define OK_PACKET_STAMP 123456789
#define STAMP_OFFSET 4;

static const int MAX_PACKET_SIZE = 100;

#ifndef PLUGIN_OUT
#define PLUGIN_OUT __declspec(dllexport)

#endif

extern "C"
{
	PLUGIN_OUT int WSAGetErr();
	PLUGIN_OUT bool StartWINSOCK();
	//PLUGIN_OUT void CreateClientSocket(const char* IP, const char* PORT);
	PLUGIN_OUT bool ConnectToServer(const char* IP, const char* PORT);
	PLUGIN_OUT bool CleanUpWINSOCK();

	PLUGIN_OUT int CheckForData();
	PLUGIN_OUT char* GetData();
	PLUGIN_OUT bool SendData(char* buffer, int length);
}

class ClientSock
{
public:
	bool initialized = false;
	bool connected = false;

	SOCKET s;
	addrinfo* ptr = NULL, hints;
	WSADATA wsa;

	char receiveBuffer[MAX_PACKET_SIZE];
	char sendBuffer[MAX_PACKET_SIZE];

	void RefHints();
	bool GetAddress(PCSTR sockName, PCSTR sockPort);
	bool CreateSocket();
	bool ConnectSocket();
};