/*
*	FILE			: Server.cs
*	PROJECT			: PROG2121 - Assignment 5
*	PROGRAMMER		: John Stanley and Aaron Perry
*	FIRST VERSION	: 2019-11-01
*	DESCRIPTION		: This file holds the class for the which instantiates the server logic.  The server Logic is controlled by a worker function through a thread.  The messages are recieved and sent via
*	                a TCP/IP Listener and TcpClient.  The usernames and discnonnects are sent via StreamReader and StreamWriter
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace A5
{
    /* -------------------------------------------------------------
NAME	:	Server
PURPOSE :	This class creates the main Server object

------------------------------------------------------------- */

    class Server
    {
        NetworkStream stream = new NetworkStream(default);
        static Dictionary<string, NetworkStream> clientStreams = new Dictionary<string, NetworkStream>();
        static List<string> connectedUsers = new List<string>();

        static int userID = 0 ;
        static void Main(string[] args)
        {


            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("10.192.49.5");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();
               

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    StreamReader sR = new StreamReader(client.GetStream());

                    // Read the username (waiting for the client to use WriteLine())
                    String connectedUsername = sR.ReadLine();
                    connectedUsers.Add(connectedUsername);
                    NetworkStream stream = client.GetStream();
                    
                    
                    Console.WriteLine(connectedUsername + " is connected");


                    StreamWriter sW = new StreamWriter(client.GetStream());
                    
                    //Check for existing clients
                    if(clientStreams.Count == 0)
                    {
                        sW.WriteLine(connectedUsers[0]);
                        // Send the username
                        sW.AutoFlush = true;
                        clientStreams.Add(connectedUsername, stream);
                    }
                    //Send username to all clients
                    else
                    {
                        clientStreams.Add(connectedUsername, stream);
                        int i = 0;
                        foreach (KeyValuePair<string, NetworkStream> user in clientStreams)
                        {
                            
                            sW.WriteLine(connectedUsers[i]);
                            i++;
                            // Send the username
                            
                        }
                    }
                    sW.AutoFlush = true;



                    ParameterizedThreadStart ts = new ParameterizedThreadStart(Worker);
                    Thread clientThread = new Thread(ts);
                    clientThread.Start(client);


                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

/*
Name	: Worker
Purpose : Creates the worker thread to handle clients streams
Inputs	:	Object thread
Outputs	:	NONE
Returns	:	Nothing
*/
        public static void Worker(Object o)
        {

            TcpClient client = (TcpClient)o;
            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;

           

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            

            int i;
           // clientStreams.Add(connectedUsers[userID], stream);
            userID++;
            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            { 
           
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                
                //Check for disconnected clients
                if(connectedUsers.Contains(data))
                {

                    Console.WriteLine("{0} is disconnected", data);
                    clientStreams.Remove(data);
                    byte[] Closemsg = System.Text.Encoding.ASCII.GetBytes(data + " is disconnected");
                    foreach (KeyValuePair<string, NetworkStream> user in clientStreams)
                    {

                        // Send back a response.
                        user.Value.Write(Closemsg, 0, Closemsg.Length);
                        
                    }
                }
                else
                {
                    Console.WriteLine("Received: {0}", data);
                    // Process the data sent by the client.
                    data = data.ToUpper();

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);


                    //Sends messages back to all clients connected
                    foreach (KeyValuePair<string, NetworkStream> user in clientStreams)
                    {


                        
                        user.Value.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);

                    }
                }
             
               

            }

          
            // Shutdown and end connection

            client.Close();
            

        }

    }
}

