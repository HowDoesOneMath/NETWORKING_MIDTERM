#include "NetServer.h"
#include <stdio.h>
#include <iostream>

std::string GetPort()
{
	std::string str = "";
	printf("Specify a port number:\n");
	std::getline(std::cin, str);
	printf("\n");
	return str;
}

int main()
{
	Server mainServer;

	if (!mainServer.InitWinsock())
		return 1;

	//std::string ipAddr = GetIP();
	std::string port = GetPort();

	addrinfo* ptr = NULL, hints;

	mainServer.RefHints(&hints);

	if (!mainServer.GetAddress(port.c_str(), &hints, &ptr))
		return 1;

	if (!mainServer.CreateSocket(&hints))
		return 1;

	if (!mainServer.BindSocket(ptr))
		return 1;

	if (!mainServer.ListenForSocket(SOMAXCONN, ptr))
		return 1;

	std::cout << "SERVER READY!\n" << std::endl;

	while (true)
	{
		mainServer.ReceiveUpdates();
	}
}