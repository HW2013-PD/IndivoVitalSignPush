
using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using TCPLibrary;
using IndivoClient;
using System.Configuration;


namespace IndivoVitalSignPush
{
    class MainClass
    {

        public static bool demo = false;
        public static TCPServer serv;
        public static Hashtable Clients;
        
        public static bool exit = false;

        public static void Main(string[] args)
        {
            Console.WriteLine("Vital Sign Push Server");
            Clients = new Hashtable();
            StartServer();

            do
            {

            } while (!exit);

            CloseAll();

        }

        static public void OnDataReceived(string sendername, string data)
        {
            //	System.Console.WriteLine(sendername);
            string[] dataArray = data.Split(new char[] { '|' });
            Client client = new Client();

            VitalSign vs = new VitalSign();

            vs.dateMeasuredEnd = DateTime.Now;

            vs.result.value = Convert.ToDouble(dataArray[0],null);

            client.SaveVitals(vs);
        }


        static private void StartServer()
        {
            serv = new TCPServer();
            serv.DataManager += new DataManager(MainClass.OnDataReceived);
            serv.StartServer();
            Console.WriteLine("TCP Server Started::Listening to port 10000");
        }

        static private void CloseAll()
        {
            serv.Close();
        }

    }

}
