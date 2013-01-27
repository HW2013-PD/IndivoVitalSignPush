using System;
using System.Collections;
using System.Threading;
using System.Net.Sockets;


namespace TCPLibrary
{
	public delegate void DataManager(string sendername, string data);
	
	public class TCPServer
	{
	    private int _serverPort= 10000;
	    private Hashtable clients = new Hashtable();
	    private TcpListener listener;
	    private Thread listenerThread;
		public event DataManager DataManager;
		
		public TCPServer(){
			
		}
	    // This subroutine sends a message to all attached clients
	    public void Broadcast(string strMessage)
	    {
	        UserConnection client;
	        // All entries in the clients Hashtable are UserConnection so it is possible
	        // to assign it safely.
	        foreach(DictionaryEntry entry in clients)
	        {
	            client = (UserConnection) entry.Value;
	            client.SendData(strMessage);
	        }
	    }
		public int ServerPort
		{
			get { return _serverPort; }
			set { _serverPort = value; }
		}
	
	    // This subroutine checks to see if username already exists in the clients
	    // Hashtable.  if it does, send a REFUSE message, otherwise confirm with a JOIN.
	    private void ConnectUser(string userName, UserConnection sender)
	    {
	        if (clients.Contains(userName))
	        {
	            ReplyToSender(EConnectionResponse.ConnectionRefused.ToString(), sender);
	        }
	        else
	        {
				sender.Name=userName;
				clients.Add(userName, sender);
				System.Console.WriteLine("CONNECTED: "+sender.Name);
	
	            ReplyToSender(EConnectionResponse.ConnectionAccepted.ToString(), sender);
                
	        }
	    }
	
	    // This subroutine notifies other clients that sender left the chat, and removes
	    // the name from the clients Hashtable
	    private void DisconnectUser(string userName)
	    {
	        clients.Remove(userName);
			System.Console.WriteLine("DISCONNECTED: " + userName);
	    }
	
	    // This subroutine is used a background listener thread to allow reading incoming
	    // messages without lagging the user interface.
	    private void DoListen()
	    {
	        try {
	            // Listen for new connections.
	            listener = new TcpListener(System.Net.IPAddress.Any, _serverPort);
	            listener.Start();
	
	            do
	            {
	                // Create a new user connection using TcpClient returned by
	                // TcpListener.AcceptTcpClient()
	                UserConnection client = new UserConnection(listener.AcceptTcpClient());
	                // Create an event handler to allow the UserConnection to communicate
	                // with the window.
	                client.LineReceived += new LineReceiveEventHandler(OnLineReceived);
	                //AddHandler client.LineReceived, AddressOf OnLineReceived;
	            }while(true);
	        }
	        catch(Exception ex){
                System.Console.WriteLine(ex.ToString());
	        }
	    }
	
	    // When the window closes, stop the listener.
	    public void Close()
	    {
	        listener.Stop();
	    }
	
	    // Start the background listener thread.
	    public void StartServer()
	    {
	        listenerThread = new Thread(new ThreadStart(DoListen));
	        listenerThread.Start();
			System.Console.WriteLine("Server Started :: Waiting for clients");
	    }
	
	    // This is the event handler for the UserConnection when it receives a full line.
	    // Parse the cammand and parameters and take appropriate action.
	    private void OnLineReceived(UserConnection sender, string data)
	    {
	        string[] dataArray;
	        // Message parts are divided by "|"  Break the string into an array accordingly.
	        // Basically what happens here is that it is possible to get a flood of data during
	        // the lock where we have combined commands and overflow
	        // to simplify this proble, all I do is split the response by char 13 and then look
	        // at the command, if the command is unknown, I consider it a junk message
	        // and dump it, otherwise I act on it
	      //  dataArray = data.Split((char) 13);
	        dataArray = data.Split((char) 124);
			
	        // dataArray(0) is the command.
	        switch( dataArray[0])
	        {
	            case "CONNECT":
	                ConnectUser(dataArray[1], sender);
                    DataManager(dataArray[1], data);
				break;
	            case "DISCONNECT":
                    DisconnectUser(sender.Name);
                    DataManager(sender.Name, data);
	                break;
	            default:
                    DataManager(sender.Name, data);
	                break;
	        }
	    }

        public void disconnect(string username){
            DisconnectUser(username);
            DataManager(username, "DISCONNECT|");
        }
	
	    // This subroutine sends a response to the sender.
	    private void ReplyToSender(string strMessage, UserConnection sender)
	    {
	        sender.SendData(strMessage);
	    }
	
	    // This subroutine sends a message to all attached clients.
	    public void SendToClients(string strMessage)
	    {
	        UserConnection client;
	        // All entries in the clients Hashtable are UserConnection so it is possible
	        // to assign it safely.
	        foreach(DictionaryEntry entry in clients)
	        {
	            client = (UserConnection) entry.Value;
	            client.SendData(strMessage);
	        }
	    }
	
		// This subroutine sends a message to an specified attached client.
	    public bool SendToClient(string strMessage, string clientName)
	    {
            bool bcon = false;
            try
            {
                
                UserConnection client;
                // All entries in the clients Hashtable are UserConnection so it is possible
                // to assign it safely.
                foreach (DictionaryEntry entry in clients)
                {
                    client = (UserConnection)entry.Value;
                    // Exclude the sender.
                    if (client.Name == clientName)
                    {
                       // Console.WriteLine(strMessage);
                        bcon = client.SendData(strMessage);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return false;
            }
            return bcon;
	    }
			
		public bool IsClientConnected(string clientName)
		{
            try
            {
                if (clients == null)
                    return false;
                if (!clients.ContainsKey(clientName))
                    return false;

                UserConnection client = (UserConnection)clients[clientName];

                if (client.isConnected())
                    return true;

                clients.Remove(clientName);
                return false;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return false;
            }
		}
	}
}