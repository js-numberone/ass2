/*
*	FILE			: MainWindow.xaml.cs
*	PROJECT			: PROG2121 - Assignment 5
*	PROGRAMMER		: John Stanley and Aaron Perry
*	FIRST VERSION	: 2019-11-01
*	DESCRIPTION		: This file holds the UI and events handlers for the client interface.  Messages are sent via NetworkStream and TcpCLient streams.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace A5
{
    /* -------------------------------------------------------------
NAME	:	MainWindow
PURPOSE :	This class creates the main window class

------------------------------------------------------------- */
    public partial class MainWindow : Window
    {
        TcpClient clientSocket = new TcpClient();
        NetworkStream serverStream = default(NetworkStream);
        List<String> msgList = new List<String>();
        List<String> userName = new List<String>();
        static string readData = null;
        public MainWindow()
        {
            InitializeComponent();
            connect_btn.IsEnabled = false;
            userListbox.IsEnabled = false;
        }


        /*
                Name	:   Connect_btn_Click
                Purpose :   Handles the events of the connect buttons click.  This sends the connection request via TcpClient stream and listens for server messages via a thread poll
                Inputs	:	object sender, RoutedEventArgs e
                Outputs	:	NONE
                Returns	:	Nothing
        */

        private void Connect_btn_Click(object sender, RoutedEventArgs e)
        {
            clientSocket.Connect(ipTextBox.Text, Int32.Parse(portTextBox.Text));
            nameTextBox.IsEnabled = false;
            connect_btn.IsEnabled = false;
            ipTextBox.IsEnabled = false;
            portTextBox.IsEnabled = false;
            
            userName.Add(nameTextBox.Text);

            //Get StreamWriter Stream
            StreamWriter sW = new StreamWriter(clientSocket.GetStream());
            sW.AutoFlush = true;

            // Send the username
            sW.WriteLine(nameTextBox.Text);

            StreamReader sR = new StreamReader(clientSocket.GetStream());

            //Waits to see SyreamReader has any available data to be read 
            while(sR.Peek() != -1)
            {
                 // Read the username (waiting for the client to use WriteLine())
                string connectedUsername = sR.ReadLine();
                if(userName.Contains(connectedUsername))
                userName.Add(connectedUsername);
                userListbox.Items.Add(connectedUsername);
            }
           


            //Start Thread
            Thread ctThread = new Thread(getMessage);
            ctThread.Start();
            
        }


        /*
                Name	:   getMessage
                Purpose :   This function handles the listening POLL for messages from the server
                Inputs	:	NONE
                Outputs	:	NONE
                Returns	:	Nothing
        */
        private void getMessage()
        {

            string returnData;

            while(true)
            {
                serverStream = clientSocket.GetStream();
                
                byte[] instream = new byte[256];

                serverStream.Read(instream, 0, instream.Length);

                returnData = System.Text.Encoding.ASCII.GetString(instream);

                readData = returnData;

                msgList.Add(readData);

                messageCheck();
                
            }
        }

        /*
                Name	:   messageCheck
                Purpose :   This function checks for thread access and if no access Invokes the thread and else adds message to the message box
                Inputs	:	NONE
                Outputs	:	Message in textbox
                Returns	:	Nothing
        */

        private void messageCheck()
        {
            if(!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(messageCheck);
            }
            else
            {
                inMessageBox.Text = "";
                for (int i=0;i<msgList.Count;i++)
                {
                 inMessageBox.AppendText(msgList[i] + "\n");
                
                }
            }
        }

        /*
               Name	    :   Msg_btn_Click
               Purpose  :   This function encodes the message and prpares it for transmission over TCP/IP.
               Inputs	:	object sender, RoutedEventArgs e
               Outputs	:	NONE
               Returns	:	Nothing
       */

        private void Msg_btn_Click(object sender, RoutedEventArgs e)
        {
            string outGoing = outMessageBox.Text;
        
            byte[] msgStream = Encoding.ASCII.GetBytes(outGoing);
            

            serverStream.Write(msgStream, 0, msgStream.Length);
            serverStream.Flush();
            outMessageBox.Text = "";

        }
        /*
               Name	    :   nameTextBox_TextChanged
               Purpose  :   Handles the event of textbox change state.  If the text box is populated, the connect button is enabled allowing connection to the server
               Inputs	:	object sender, RoutedEventArgs e
               Outputs	:	NONE
               Returns	:	Nothing
       */

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {


                if (nameTextBox.Text.Length > 0 )
                    connect_btn.IsEnabled = true;
                else
                    connect_btn.IsEnabled = false;

        }

        /*
              Name	    :   disconnect_btn_Click
              Purpose   :   Handles the event of the dicsonnct button.  When fired the user name in the text box is send along with a stream flush allowing for a gracefull stream close()
              Inputs	:	object sender, RoutedEventArgs e
              Outputs	:	NONE
              Returns	:	Nothing
      */
        private void disconnect_btn_Click(object sender, RoutedEventArgs e)
        {
            string outGoing = nameTextBox.Text;

            byte[] msgStream = Encoding.ASCII.GetBytes(outGoing);


            serverStream.Write(msgStream, 0, msgStream.Length);
            serverStream.Flush();
            disconnect_btn.IsEnabled = false;
      
        }
    }
}
