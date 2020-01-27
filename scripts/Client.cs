using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

public class Client {

    private static Stream stream;
    private static TcpClient client = new TcpClient();

    public static void init() {
        try {
            client.Connect("81.36.136.84", 2512);
            stream = client.GetStream();

            var str = "join&74567854697b'm'540697b509'569&stupid human";

            Console.WriteLine("Transmitting...");
            send(str);
            Console.WriteLine("Transmitted");

            Console.WriteLine("Press ENTER to stop");
            Console.ReadKey();

        } catch (Exception e) { Console.WriteLine("error: " + e.StackTrace); }
    }

    public static void clean(string packet) {
        stream.Close();
        client.Close();
    }

    public static void send(string packet) {
        byte[] ba = Encoding.ASCII.GetBytes(packet);
        stream.Write(ba, 0, ba.Length);
        stream.WriteByte(0x0A);
        stream.Flush();
    }
}