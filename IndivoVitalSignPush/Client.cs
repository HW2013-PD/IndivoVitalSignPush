using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using TCPLibrary;
using IndivoClient;
using System.Configuration;
using System.Threading;
using System.IO;

namespace IndivoVitalSignPush
{
    public class Client
    {
        public static bool demoPurpose = false;
        public DataRequestor dr;

        private List<string> viewers = new List<string>();


        private long lastUpdate = DateTime.Now.Ticks;
        private long lastWoketsTime = DateTime.Now.Ticks;


        public static TCPServer _server;
        
        public Client()
        {
            var dr = InitializeIndivo();
        }

        private DataRequestor InitializeIndivo()
        {
            Console.WriteLine("Initializing");

            dr = new DataRequestor();
            string accountId = "";
            accountId = dr.Login("pd-jplum-patient", "example-patient");

            Demographics dgh = dr.GetDemographics();
            return dr;
        }

        public string SaveVitals(VitalSign vs)
        {
            string documentId = "";
            dr.SaveResults(vs);
            return documentId;
        }

        public void OnDataReceived(string sendername, string data)
        {
        }
    }
}
