using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServerChatRoom;


public class Program
{
    static void Main(string[] args)
    {
        Server server = new Server();
        TcpListener svStart = server.Start();
        Console.Write("Server Started. Waiting For Client Connection...");
        try
        {
            while (true)
            {
                TcpClient client = svStart.AcceptTcpClient();
                server.AddClient(client);
                Console.WriteLine("Someone Connected!");
                Thread clientThread = new Thread(() => server.HandleClients(client));
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
        }
    }
}