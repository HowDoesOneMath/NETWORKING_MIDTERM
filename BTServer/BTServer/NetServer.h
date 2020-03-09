#pragma once
#include <string>
#include <WinSock2.h>
#include <WS2tcpip.h>
#include <vector>
#include <queue>
#include <iostream>
#pragma comment(lib, "ws2_32.lib")

static const int MAX_PACKET_SIZE = 100;

#define INITIAL_OFFSET 12
#define OK_PACKET_STAMP 123456789
#define STAMP_OFFSET 4;

enum PACKET_TYPE
{
	INIT,
	START,
	POSITION,
	BULLET,
	SCORE
};

class Client
{
public:
	SOCKET clientSocket;
	bool connected = false;
};

class Server
{
public:
	Server();
	~Server();

	WSADATA wsa;
	SOCKET serverSocket;
	Client clients[2];

	char receiveBuffer[MAX_PACKET_SIZE];
	char sendBuffer[MAX_PACKET_SIZE];
	int numConnected = 0;

	void ErrDebug(std::string msg);
	void ErrDebugAndCleanup(std::string msg);
	void ErrDebugCleanupFreePtr(std::string msg, addrinfo* ptr);
	void CleanupFreePtr(addrinfo* ptr);

	bool InitWinsock();

	void RefHints(addrinfo* hints);
	bool GetAddress(PCSTR sockPort, addrinfo* hints, addrinfo** ptr);
	bool CreateSocket(addrinfo* hints);

	bool BindSocket(addrinfo* ptr);
	bool ListenForSocket(int LISTEN_LENGTH, addrinfo* ptr);
	bool ListenForClient();

	void ReceiveUpdates();

	void PackAuxilaryData(char* buffer, int stamp, int length, int type);

	void PackString(char* buffer, int* loc, std::string* str);

	template<class T>
	void PackData(char* buffer, int* loc, T data);

	template<class T>
	void UnpackData(char* buffer, int* loc, T* data);

	void UnpackString(char* buffer, int* loc, std::string* str, int* length);
};

template<class T>
inline void Server::PackData(char* buffer, int* loc, T data)
{
	memcpy(buffer + *loc, &data, sizeof(T));
	*loc += sizeof(T);
}

template<class T>
inline void Server::UnpackData(char* buffer, int* loc, T* data)
{
	memcpy(data, buffer + *loc, sizeof(T));
	*loc += sizeof(T);
}
