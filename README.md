# Ryujinx LDN Multiplayer Server

This server drives multiplayer "matchmaking" in Ryujinx's local wireless implementation. Players can create "networks" on the server, which can then be scanned and found by other players as if they were within that network's range. The server entirely manages the network information and player join/leave lifecycle for all games created.

## Proxy

Ryujinx uses an application layer proxy to simulate communications in a network, which directs traffic created by the C# socket to a proxy server, which routes and sends it to the relevant parties. By default, Ryujinx attempts to create a proxy server on the network owner's system, expose a port to the internet, and tell joiners to connect to it. If this does not work, the proxy is done through the master server itself, using a lot of the same mechanisms.

## External Stats via Asp.NET

Coming soon

## Management

(not implemented yet)

The console is listening for commands while the server is running. You can do any of the following:

`list` - Lists all games and connected users. You will also be able to see passphrase games.
`close` - Closes the server.