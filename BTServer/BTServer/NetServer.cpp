#include "NetServer.h"

Server::Server()
{
}

Server::~Server()
{
}

void Server::ErrDebug(std::string msg)
{
	printf((msg + ": %d").c_str(), WSAGetLastError());
}

void Server::ErrDebugAndCleanup(std::string msg)
{
	ErrDebug(msg);
	WSACleanup();
}

void Server::ErrDebugCleanupFreePtr(std::string msg, addrinfo* ptr)
{
	ErrDebug(msg);
	freeaddrinfo(ptr);
	WSACleanup();
}

void Server::CleanupFreePtr(addrinfo* ptr)
{
	freeaddrinfo(ptr);
	WSACleanup();
}

bool Server::InitWinsock()
{
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
	{
		ErrDebug("Winsock died on startup");
		return false;
	}

	return true;
}

void Server::RefHints(addrinfo* hints)
{
	memset(hints, 0, sizeof(*hints));
	hints->ai_family = AF_INET;
	hints->ai_socktype = SOCK_STREAM;
	hints->ai_protocol = IPPROTO_TCP;
	hints->ai_flags = AI_PASSIVE;
}

bool Server::GetAddress(PCSTR sockPort, addrinfo* hints, addrinfo** ptr)
{
	if (getaddrinfo(INADDR_ANY, sockPort, hints, ptr) != 0)
	{
		ErrDebugAndCleanup("Failed to get address info");
		return false;
	}

	return true;
}

bool Server::CreateSocket(addrinfo* hints)
{
	serverSocket = socket(hints->ai_family, hints->ai_socktype, hints->ai_protocol);

	if (serverSocket == INVALID_SOCKET)
	{
		ErrDebugAndCleanup("Failed to create a socket");
		return false;
	}

	return true;
}

bool Server::BindSocket(addrinfo* ptr)
{
	if (bind(serverSocket, ptr->ai_addr, ptr->ai_addrlen) == SOCKET_ERROR)
	{
		ErrDebugCleanupFreePtr("Socket did not bind", ptr);
		return false;
	}

	return true;
}

bool Server::ListenForSocket(int LISTEN_LENGTH, addrinfo* ptr)
{
	if (listen(serverSocket, LISTEN_LENGTH) == SOCKET_ERROR)
	{
		ErrDebugCleanupFreePtr("Listener Broke", ptr);
		return false;
	}

	u_long mode = 1;
	ioctlsocket(serverSocket, FIONBIO, &mode);

	return true;
}

bool Server::ListenForClient()
{
	if (clients[0].connected && clients[1].connected)
		return true;
	
	for (int i = 0; i < 2; ++i)
	{
		if (!clients[i].connected)
		{
			clients[i].clientSocket = accept(serverSocket, NULL, NULL);
			if (clients[i].clientSocket == INVALID_SOCKET)
			{
				if (WSAGetLastError() == WSAEWOULDBLOCK)
					return false;

				ErrDebug("Couldn't accept client!");
				return false;
			}

			clients[i].connected = true;
			numConnected++;
			printf("CLIENT CONNECTED");
			break;
		}
	}

	if (numConnected == 2)
	{
		PACKET_TYPE pt = PACKET_TYPE::START;


		for (int i = 0; i < 2; ++i)
		{
			int loc = INITIAL_OFFSET;

			PackData<int>(&receiveBuffer[0], &loc, i);
			PackAuxilaryData(&receiveBuffer[0], OK_PACKET_STAMP, loc, (int)pt);

			while (send(clients[i].clientSocket, &receiveBuffer[0], MAX_PACKET_SIZE, 0) == SOCKET_ERROR)
			{
				ErrDebug("Failed to send start, retrying...");
			}
		}
		return true;
	}
	else
		return false;
}

void Server::ReceiveUpdates()
{
	if (!ListenForClient())
	{
		return;
	}

	for (int i = 0; i < 2; ++i)
	{
		int bytesReceived = recv(clients[i].clientSocket, &receiveBuffer[0], MAX_PACKET_SIZE, 0);

		if (bytesReceived > 0)
		{
			int loc = 0;
			int stamp;
			UnpackData<int>(&receiveBuffer[0], &loc, &stamp);

			if (stamp == OK_PACKET_STAMP)
			{
				if (send(clients[1 - i].clientSocket, &receiveBuffer[0], MAX_PACKET_SIZE, 0) == SOCKET_ERROR)
				{
					if (WSAGetLastError() == WSAEWOULDBLOCK)
					{
						int pType = 0;
						int length = 0;
						UnpackData<int>(&receiveBuffer[0], &loc, &length);
						UnpackData<int>(&receiveBuffer[0], &loc, &pType);

						switch (pType)
						{
						case (int)PACKET_TYPE::POSITION:
							float x, y, r;
							UnpackData<float>(&receiveBuffer[0], &loc, &x);
							UnpackData<float>(&receiveBuffer[0], &loc, &y);
							UnpackData<float>(&receiveBuffer[0], &loc, &r);

							std::cout << "FAILED!!!!!!: GOT POSITION AND ROTATION OF P" << i << ": " << x << ", " << y << ", ROT: " << r << std::endl;
							break;
						case (int)PACKET_TYPE::BULLET:
							float bx, by, vx, vy;
							UnpackData<float>(&receiveBuffer[0], &loc, &bx);
							UnpackData<float>(&receiveBuffer[0], &loc, &by);
							UnpackData<float>(&receiveBuffer[0], &loc, &vx);
							UnpackData<float>(&receiveBuffer[0], &loc, &vy);

							std::cout << "FAILED!!!!!!: BULLET LAUNCHED:" << i << ": " << bx << ", " << by
								<< " IN DIRECTION " << vx << ", " << vy << std::endl;
							break;
						}
					}

					ErrDebug("Failed to relay packet");
				}
				else
				{
					int pType = 0;
					int length = 0;
					UnpackData<int>(&receiveBuffer[0], &loc, &length);
					UnpackData<int>(&receiveBuffer[0], &loc, &pType);

					switch (pType)
					{
					case (int)PACKET_TYPE::POSITION:
						float x, y, r;
						UnpackData<float>(&receiveBuffer[0], &loc, &x);
						UnpackData<float>(&receiveBuffer[0], &loc, &y);
						UnpackData<float>(&receiveBuffer[0], &loc, &r);

						//std::cout << "GOT POSITION AND ROTATION OF P" << i << ": " << x << ", " << y << ", ROT: " << r << std::endl;
						break;
					case (int)PACKET_TYPE::BULLET:
						float bx, by, vx, vy;
						UnpackData<float>(&receiveBuffer[0], &loc, &bx);
						UnpackData<float>(&receiveBuffer[0], &loc, &by);
						UnpackData<float>(&receiveBuffer[0], &loc, &vx);
						UnpackData<float>(&receiveBuffer[0], &loc, &vy);

						std::cout << "BULLET LAUNCHED:" << i << ": " << bx << ", " << by 
							<< " IN DIRECTION " << vx << ", " << vy << std::endl;
						break;
					}
				}
			}
		}
	}
}

void Server::PackAuxilaryData(char* buffer, int stamp, int length, int type)
{
	int loc = 0;
	PackData<int>(buffer, &loc, stamp);
	PackData<int>(buffer, &loc, length);
	PackData<int>(buffer, &loc, type);
}

void Server::PackString(char* buffer, int* loc, std::string* str)
{
	PackData<int>(buffer, loc, (int)str->size());
	memcpy(buffer + *loc, &(*str)[0], (int)str->size());
	*loc += str->size();
}

void Server::UnpackString(char* buffer, int* loc, std::string* str, int* length)
{
	UnpackData<int>(buffer, loc, length);
	str->resize(*length, 0);
	memcpy(&(*str)[0], buffer + *loc, *length);
	*loc += *length;
}
