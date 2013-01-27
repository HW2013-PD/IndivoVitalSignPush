using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndivoClient
{
    public class DataRequestor
    {
        public string AccountID { get; set; }
        public string RecordID { get; set; }
        public string PlanID { get; set; }

        private IndivoConsumer _indivoClient = null;

        string _consumerKey = "collaboRhythmChrome";
        string _consumerSecret = "collaboRhythmChrome_secret93452";
        string _baseUrl = "https://indivo-demo.media.mit.edu:8000";

        public string Login(string userName, string password)
        {
            _indivoClient = new IndivoConsumer(_consumerKey, _consumerSecret, _baseUrl);

            String accountId = null;

            accountId = _indivoClient.OAuthInternalSessionCreatePost(
                                                        userName,
                                                        password,
                                                        new KeyValuePair<string, object>());

            this.AccountID = accountId;
            return accountId;
        }

        public int SaveResults(VitalSign result)
        {
            string resultID = _indivoClient.Records_X_DocumentsPost(RecordID, null, result.Serialize(), "application/xml", new KeyValuePair<string, object>());
            int i = resultID.IndexOf("<Document id=");
            //string auxs = resultID.Substring(i + 14, 36);
            //string OtherDocumentID = "e0fa3294-a5f1-45ee-bfc8-f2679a4e07a1";
            //_indivoClient.Records_X_Documents_X_Rels_X_X_Put(RecordID, PlanID, "unscheduledResult", OtherDocumentID, new KeyValuePair<string, object>());
            return i;
        }


        public Demographics GetDemographics()
        {
            Demographics dgh = null;
            string label = "";

            if (!String.IsNullOrEmpty(this.AccountID) || !String.IsNullOrEmpty(this.AccountID))
            {
                string records = string.Empty;

                try
                {
                    records = _indivoClient.Accounts_X_RecordsGet(this.AccountID, null, new KeyValuePair<string, object>());
                }
                catch { }

                if (!String.IsNullOrEmpty(records))
                {
                    string[] ss = records.Split('\"');
                    if (ss.Length > 1)
                    {
                        RecordID = ss[5];
                        label = ss[7];
                    }
                    string demographics = _indivoClient.Records_X_Get(RecordID, new KeyValuePair<string, object>());
                    Record r = SerializationHelper.DeserializeObject<Record>(demographics);

                    string demographicsId = r.demographics.document_id;

                    if (!String.IsNullOrEmpty(demographicsId))
                    {

                        string demographicResult = _indivoClient.Records_X_Documents_X_Get(r.id, demographicsId, new KeyValuePair<string, object>());
                        dgh = SerializationHelper.DeserializeObject<Demographics>(demographicResult);
                    }

                }
            }

            Console.WriteLine(dgh.dateOfBirth.ToString());
            return dgh;
        }

    }
}
