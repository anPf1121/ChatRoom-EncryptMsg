using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
namespace ChatRoomClient
{
    class Program
    {
        static byte[] key = new byte[32];
        static byte[] iv = new byte[16];
        static void Main()
        {
            try
            {
                Client client = new Client();
                NetworkStream stream = client.Connect();
                Console.Write("Enter your name: ");
                string? name = Console.ReadLine();

                if (name != null)
                {
                    byte[] nameData = Encoding.UTF8.GetBytes(name);
                    stream.Write(nameData, 0, nameData.Length);

                    int keyBytes = stream.Read(key, 0, key.Length);
                    int iVBytes = stream.Read(iv, 0, iv.Length);
                    Console.WriteLine("Encryption Key: " + Convert.ToBase64String(key));
                    Console.WriteLine("Encryption IV: " + Convert.ToBase64String(iv));

                    Thread receiveThread = new Thread(() => client.ReceiveMessages(stream, key, iv));
                    receiveThread.Start();

                    while (true)
                    {
                        try
                        {
                            client.SendMessage(stream, key, iv);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception: " + ex.Message);
                        }
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
