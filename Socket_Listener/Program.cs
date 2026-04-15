using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketListener
{
    public static int Main(string[] args)
    {
        StartServer();
        return 0;
    }

    /*The listener application is the majority of the work for these kinds of applications. While the client just has to establish a connection
     and then send/receive responses, the listener has to do the same but then also process/parse the message and know what to do.
    This is a very simple example, but it's a peek into how the mechanisms behind things like LLMs work.*/
    public static void StartServer()
    {
        try
        {
            /*Using IPHostEntry is very convenient for automatically retrieving your IP, so even if you are assigned a new one by DHCP
             the program will still work. This is not practical though, as majority of endpoints will be setup at a static addresses.*/
            //IPHostEntry host = Dns.GetHostEntry("localhost");
            //IPAddress ip = host.AddressList[0];

            IPAddress ip = IPAddress.Parse("10.0.0.164");
            IPEndPoint localEP = new IPEndPoint(ip, 10001);
            /*Create a socket object that gets info about the IP address we use, chooses a socket type that allows us to both send and receive
             transmissions continuously, and chooses the network protocol to achieve the transmission.*/
            Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //Bind the socket to the address and port we've specified, and then await connections.
            //Listen(10) means that the listener can maintain 10 connections at a time. Any more will get "Server Busy" error
            listener.Bind(localEP);
            listener.Listen(10);
            Console.WriteLine("Waiting for a connection...");

            //Accept a connection as it comes in. This particular while loop ensures that if the client disconnects, the listener stays active
            //Removing the loop will make the listener shut down with the client.
            while (true)
            {
                Socket handler = listener.Accept();

                //This loop handles a single connection until it's terminated.
                while (true)
                {
                    string data = "";
                    byte[] bytes = null;
                    string response = null;
                    int dogIndex = -1;
                    int bookIndex = -1;

                    //Loop until the <EOF> tag is found, building the message one chunk at a time
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);

                        //If the bytesReceived is 0, then the client disconnected
                        if (bytesRec == 0)
                        {
                            Console.WriteLine("Client disconnected.");
                            handler.Close();
                            break;
                        }

                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    //data will be null if the client disconnected. Break out of the current connection
                    if (data == "")
                    {
                        break;
                    }

                    Console.WriteLine("Text Received : {0}", data);

                    /*Now that we have a message, the bread and butter of the listener can come into play.
                     We need to parse the message for any discernable meaning that we care about. Often times this
                    will just be looking for keywords, but you can use things like substring/split/splice to go through
                    each word at a time.*/

                    //IndexOf() of is really good for finding words, but also where in the message they occur compared to other words
                    //This is useful if you care about the order things show up in
                    if (data.ToLower().IndexOf("dogs") > -1)
                    {
                        dogIndex = data.ToLower().IndexOf("dog");
                        response = "I like dogs, floppy ears are the way to go!";
                    }

                    //Contains is a quicker/cleaner word search but does not provide positioning information
                    if (data.ToLower().Contains("pizzas"))
                    {
                        //If you want to process multiple messages at once, you can check if you've already added something before adding a newline
                        if (response != null)
                        {
                            response += '\n';
                        }

                        response += "Pineapple on pizza is a warcrime";
                    }

                    if (data.ToLower().IndexOf("books") > -1)
                    {
                        bookIndex = data.ToLower().IndexOf("book");

                        if (response != null)
                        {
                            response += '\n';
                        }

                        response += "Malazan Book of the Fallen is S Tier fantasy!";
                    }

                    if (response == null)
                    {
                        response = "I don't know what you're talking about.";
                    }

                    /*As mentioned, sometimes when 2 things are mentioned at once, maybe you want to have different messages
                     based on if one was before the other*/
                    if ((dogIndex > -1) && (bookIndex > -1))
                    {
                        if (dogIndex > bookIndex)
                        {
                            response = "You should read your book to your dog!";
                        }
                        else
                        {
                            response = "Your dog will probably eat your book.";
                        }
                    }

                    //Construct a response and send it back to the client.
                    byte[] msg = Encoding.ASCII.GetBytes(response + "<EOF>");
                    handler.Send(msg);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}