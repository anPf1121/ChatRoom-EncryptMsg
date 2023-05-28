using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

namespace ChatRoomClient
{
    public class Client
    {
        static object _lock = new object();
        public NetworkStream Connect()
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 9999;

            TcpClient client = new TcpClient();
            client.Connect(ipAddress, port);

            NetworkStream stream = client.GetStream();
            return stream;
        }
        public void SendMessage(NetworkStream stream, byte[] key, byte[] iv)
        {
            string? message = Console.ReadLine();
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            if (message != null)
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Padding = PaddingMode.PKCS7;
                    ICryptoTransform encryptor = aes.CreateEncryptor();
                    byte[] encryptedData = encryptor.TransformFinalBlock(messageBytes, 0, messageBytes.Length);
                    stream.Write(encryptedData, 0, encryptedData.Length);
                }
            }
        }
        public void ReceiveMessages(NetworkStream stream, byte[] key, byte[] iv)
        {
            try
            {
                byte[] data = new byte[1024];
                while (true)
                {
                    int bytesRead = stream.Read(data, 0, data.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(data);
                        Console.WriteLine(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }
    }
}
