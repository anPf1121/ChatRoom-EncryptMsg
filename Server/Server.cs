using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerChatRoom
{
    public class Server
    {
        const int port = 9999;
        public readonly object _lock = new object();
        public List<TcpClient> connectedClients = new List<TcpClient>();
        public TcpListener Start()
        {
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();
            return server;
        }
        public void AddClient(TcpClient client)
        {
            lock (_lock) connectedClients.Add(client);
        }
        public void RemoveClient(TcpClient client)
        {
            connectedClients.Remove(client);
            client.Close();
        }
        public void HandleClients(TcpClient client)
        {
            byte[] key;
            byte[] iv;
            using (AesManaged aes = new AesManaged())
            {
                aes.GenerateKey();
                aes.GenerateIV();
                key = aes.Key;
                iv = aes.IV;
                Console.WriteLine("Encryption Key: " + Convert.ToBase64String(key));
                Console.WriteLine("Encryption IV: " + Convert.ToBase64String(iv));
                try
                {
                    NetworkStream stream = client.GetStream();
                    byte[] nameData = new byte[1024];
                    int bytesRead = stream.Read(nameData, 0, nameData.Length);
                    string name = Encoding.UTF8.GetString(nameData, 0, bytesRead);
                    Console.WriteLine("Client name: " + name);

                    stream.Write(key, 0, key.Length);
                    stream.Write(iv, 0, iv.Length);

                    byte[] messageBytes = new byte[1024];
                    byte[] encryptedByte = messageBytes;
                    while (true)
                    {
                        bytesRead = stream.Read(messageBytes, 0, messageBytes.Length);
                        if (bytesRead > 0)
                        {
                            aes.Padding = PaddingMode.PKCS7;
                            ICryptoTransform decryptor = aes.CreateDecryptor();
                            byte[] decryptedData = decryptor.TransformFinalBlock(messageBytes, 0, bytesRead);
                            string message = Encoding.UTF8.GetString(decryptedData);
                            Console.WriteLine("Received message from " + name + ": " + message);
                            BroadcastMessage(name, message, client);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
                finally
                {
                    RemoveClient(client);
                }
            }
        }
        public void BroadcastMessage(string name, string message, TcpClient senderClient)
        {
            byte[] messageByte = Encoding.UTF8.GetBytes(name + ": " + message);
            foreach (TcpClient client in connectedClients)
            {
                if (client != senderClient)
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(messageByte, 0, messageByte.Length);
                }
            }
        }
    }
}