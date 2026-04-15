using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class SocketClient
{
    public static int Main(string[] args)
    {
        StartClient();
        return 0;
    }

    /*The Client application is what user's would use to connect to another device being hosted at a particular location.
     This program just needs to establish the connection, and then send a message/receive a response.*/
    private static void StartClient()
    {
        try
        {
            /*Establish an endpoint at the address of the server you're connecting to
            (in this case both programs are on our local machine, so use 'ipconfig' in CMD to find yours.
            Ports over 10000 are generally available. Both the IP and the port need to match the Socket_Listener configuration*/
            IPAddress ip = IPAddress.Parse("10.0.0.164");
            IPEndPoint remoteEP = new IPEndPoint(ip, 10001);

            /*Create a socket object that gets info about the IP address we use, chooses a socket type that allows us to both send and receive
             transmissions continuously, and chooses the network protocol to achieve the transmission.*/
            Socket sender = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                //Connect to the listener and print out confirmation of the address
                sender.Connect(remoteEP);
                Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                //Loop until the kill passphrase is entered
                while (true)
                {
                    byte[] msg = null;
                    string input = null;
                    string data = "";
                    int bytesRec;
                    int bytesSent;

                    //Accept user input for a message to be sent
                    Console.Write("Message to the server : ");
                    input = Console.ReadLine();

                    /*Set msg array equal to each encoded character of the message 1 at a time.
                    We also add a tag to the end of the message that we can use to verify that the full message has been transmitted.
                    This is so that if the message shows up in pieces instead of just 1 transmission, the full message is received.
                    We use byte[1024] as a buffer for how many characters at a time will be sent*/
                    msg = Encoding.ASCII.GetBytes(input + "<EOF>");

                    //Transmit message
                    bytesSent = sender.Send(msg);

                    //Loop until the <EOF> tag is found
                    while (true)
                    {
                        //Receive the response 1024 bytes at a time, decoding the characters and adding them to data
                        msg = new byte[1024];
                        bytesRec = sender.Receive(msg);
                        data += Encoding.ASCII.GetString(msg, 0, bytesRec);

                        //When displaying messages to the user, be sure to trim off the tag you use for message ends, in this case 5 chars
                        if (data.IndexOf("<EOF>") > -1 )
                        {
                            data = data.Substring(0, data.Length - 5);
                            break;
                        }
                    }
                    
                    Console.WriteLine("Server Response = {0}", data);

                    //Use some kind of killphrase for closing the connection beyond just closing the window. This gives you more control.
                    if (input == "die")
                    {
                        Console.WriteLine("Closing connection...");

                        //Shutdown and Close are good to run at the end of each socket connection, it helps close off the connections and de-allocate resources
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                        break;
                    }
                }

                
            }
            //ANEs are used to handle when null values are passed into methods. This helps us diagnose if certain values are not being set correctly
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException: {0}", ane.ToString());
            }
            //SEs are for any socket related issues (can't connect to specified address, port busy, etc.)
            catch (SocketException se)
            {
                Console.WriteLine("SocketException: {0}", se.ToString());
            }
            //Catch all bucket for anything else while sending/receiving messages
            catch (Exception e)
            {
                Console.WriteLine("Unexpected Exception: {0}", e.ToString());
            }
        }
        //Catch all bucket for establishing the connection
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}