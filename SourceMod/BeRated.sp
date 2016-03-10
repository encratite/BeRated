#include <sourcemod>
#include <socket>

#define STRING_LENGTH 2048

ConVar matchmakingHost = null;
ConVar matchmakingPort = null;

public Plugin:myinfo =
{
    name = "BeRated",
    author = "epicvrvs",
    description = "Matchmaking plugin that assigns people to teams based on their TrueSkill rating",
    version = "0.1",
    url = "http://github.com/epicvrvs/BeRated"
};

public void OnPluginStart()
{
    RegAdminCmd("sm_matchmaking", OnCommandMatchmaking, ADMFLAG_GENERIC);
    matchmakingHost = CreateConVar("sm_matchmaking_host", "localhost", "Host of BeRated HTTP server");
    matchmakingPort = CreateConVar("sm_matchmaking_port", "80", "Port of BeRated HTTP server");
}

public Action OnCommandMatchmaking(int client, int args)
{
    decl String:host[STRING_LENGTH];
    GetConVarString(matchmakingHost, host, sizeof(host));
    int port = GetConVarInt(matchmakingPort);
    new Handle:socket = SocketCreate(SOCKET_TCP, OnSocketError);
    SocketConnect(socket, OnSocketConnected, OnSocketReceive, OnSocketDisconnected, host, port);
    LogMessage("Connecting to %s:%d", host, port);
    return Plugin_Handled;
}

public OnSocketConnected(Handle:socket, any:argument)
{
    LogMessage("Connected");
    new bool:first = true;
    decl String:steamIds[STRING_LENGTH];
    for (new i = 1; i <= MAXPLAYERS + 1; i++)
    {
        decl String:steamId[STRING_LENGTH];
        if (!IsClientInGame(i) || IsFakeClient(i) || !GetClientAuthId(i, AuthId_Steam2, steamId, sizeof(steamId), true))
            continue;
        if (first)
            first = false;
        else
            StrCat(steamIds, sizeof(steamIds), ",");
        StrCat(steamIds, sizeof(steamIds), steamId);
    }
    decl String:host[STRING_LENGTH];
    GetConVarString(matchmakingHost, host, sizeof(host));
    decl String:request[STRING_LENGTH];
    Format(request, sizeof(request), "GET /GetMatchmakingTeams?ids=%s HTTP/1.0\r\nHost: %s\r\nConnection: close\r\n\r\n", steamIds, host);
    LogMessage(request);
    SocketSend(socket, request);
}

public OnSocketReceive(Handle:socket, String:receiveData[], const dataSize, any:argument)
{
    LogMessage("Received data: %s", receiveData);
}

public OnSocketDisconnected(Handle:socket, any:argument)
{
    LogMessage("Disconnected");
    CloseHandle(socket);
}

public OnSocketError(Handle:socket, const errorType, const errorNum, any:argument)
{
    LogError("Socket error %d (errno %d)", errorType, errorNum);
    CloseHandle(socket);
}
