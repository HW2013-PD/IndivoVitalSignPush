using System;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace TCPLibrary
{
	public delegate void LineReceiveEventHandler(UserConnection sender, string Data);
	
	// The UserConnection class encapsulates the functionality of a TcpClient connection
	// with streaming for a single user.
	
	public class UserConnection
	{
	    private const int READ_BUFFER_SIZE = 4096;
	
	    private byte[] _readBuffer = new byte[READ_BUFFER_SIZE];
		private string _message = string.Empty;
	
		private TcpClient _client;
	    public event LineReceiveEventHandler LineReceived;
	
		//public TcpClient Client { get { return _client; } set { _client = value; } }
		public string Name { get; set; }
		
		public UserConnection(TcpClient client)
	    {
	        _client = client;
	        // This starts the asynchronous read thread.  The data will be saved into
	        // readBuffer.
	        _client.GetStream().BeginRead(_readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(StreamReceiver), null);
	    }

        public bool isConnected()
        {
            if (_client.Connected) return true;
            else return false;
        }
	    // This subroutine uses a StreamWriter to send a message to the user.
	    public bool SendData(string Data)
	    {
            if (!_client.Connected)return false;
            try{
                //lock ensure that no other threads try to use the stream at the same time.
                lock (_client.GetStream())
                {
                    
                    StreamWriter writer = new StreamWriter(_client.GetStream());
                    writer.Write(Data + (char)13);//13
                    // Make sure all data is sent now.
                    writer.Flush();
                }
                return true;
            }catch(Exception e) {
                System.Console.WriteLine(e.Message);
                return false;
            }
         
            
	    }
	    // This is the callback function for TcpClient.GetStream.Begin. It begins an 
	    // asynchronous read from a stream.

        // Finish asynchronous read into readBuffer and get number of bytes read.
        private void StreamReceiver(IAsyncResult ar)
	    {
            NetworkStream stream = _client.GetStream();

            int bytesRead;

            try
            {
                // Ensure that no other threads try to use the stream at the same time.
                lock (stream)
                {
                    // Finish asynchronous read into readBuffer and get number of bytes read.
                    bytesRead = stream.EndRead(ar);
                }
                    string ss = Encoding.ASCII.GetString(_readBuffer, 0, bytesRead);

                    string end = "" + (char)13;
                    if (ss.Contains(end))
                    {
                        string[] sss = ss.Split((char)13);
                        _message += sss[0];
                        string aux = _message;
                        _message = string.Empty;
                        LineReceived(this, aux);
                        
                        for (int count = 1; count < sss.Length; count++)
                        {
                            if (count < sss.Length - 1)
                            {
                                LineReceived(this, sss[count]);
                            }
                            else
                            {
                                _message = sss[count];
                            }
                        }


                    }
                    else
                    {
                        _message += ss;
                    }

                
                // We are not finished reading.
                // Asynchronously read more message data from  the server.
                stream.BeginRead(_readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(StreamReceiver), null);

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                stream.BeginRead(_readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(StreamReceiver), null);
            }
        }
	}

}
