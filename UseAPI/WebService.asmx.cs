using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Services;
using System.Net;
using System.Text;
using System.Collections.Specialized;
using System.Data;
using System.Web.Script.Serialization;

namespace UseAPI
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }


        [WebMethod]
        public void TargetBOL_ExecuteBillsOfLading(int OrderNumber)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            //try
            //{
            //    Context.Response.Write(js.Serialize(letter.BuyBackLetters_GetPO_Information(RHSId)));
            //}
            //catch (Exception e)
            //{
            //    string err = $"Error {e.Message} Error Data {e.Data.ToString()} E - {e.ToString()} ";
            //    Context.Response.Write(err);
            //}
        }


        [WebMethod]
        public string getAPI_Post()
        {
            string strURL = string.Format("https://jsonplaceholder.typicode.com/posts");
            WebRequest requestObjPost = WebRequest.Create(strURL);
            requestObjPost.Method = "POST";
            requestObjPost.ContentType = "application/json";

            string postData = "{\"title\":\"testdata\",\"body\":\"testbody\",\"userid\":\"50\"}";
            var result = "None";
            using(var streamWriter = new StreamWriter(requestObjPost.GetRequestStream()))
            {
                streamWriter.Write(postData);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)requestObjPost.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
            }
            return result;
        }

        [WebMethod]
        public string getFreight()
        {
            freight_API freight = new freight_API();
            string result = freight.callJSONPlaceholder();
            return result;
        }

        [WebMethod]
        public string getTargetFreight()
        {
            freight_API freight = new freight_API();
            //string result = freight.GetTargetFreight();
            return "result";
        }

        [WebMethod]
        public string TargetCarrier_PrintAndEmailDocuments(string orderNumber, string BolId, string BolDate, string copies)
        {
            freight_API freight = new freight_API();
            string bolResult = freight.PrintBillOfLading(BolId, BolDate);
            string bolLabelResult = freight.PrintLabel(BolId, BolDate, copies);
            string filePath = $"{bolResult};{bolLabelResult}";
            string emailResult = freight.SendEmailWithBOLAttachment(orderNumber, filePath, "This is a test. Please ignore. This is a test " );
            return $"BOL Filepath - {bolResult} Label Filepath - {bolResult} email - {emailResult}";
        }

        [WebMethod]
        public string TargetCarrier_GetTargetFreightWithBOLwithOrderNumber(string orderNumber)
        {
            string queryID = "0";
            freight_API freight = new freight_API();
            queryID = freight.Target_Carrier_GetQueryID(orderNumber);
            string bol = freight.ExecuteBillsOfLading(queryID);
            return bol;
        }

        [WebMethod]
        public string TargetCarrier_PrintLabel(string BolId, string BolDate, string copies)
        {
            freight_API freight = new freight_API();
            string result = freight.PrintLabel(BolId, BolDate, copies);
            return result;
        }
        [WebMethod]
        public string TargetCarrier_PrintBillOfLading(string BolId, string BolDate)
        {
            freight_API freight = new freight_API();
            string result = freight.PrintBillOfLading(BolId, BolDate);
            return result;
        }

        [WebMethod]
        public string TargetCarrier_PickupRequest(string queryID, string bolID)
        {
            freight_API freight = new freight_API();
            string result = freight.PickupRequest(queryID, bolID);
            return result;
        }

        [WebMethod]
        public string getTargetFreightWithBOL()
        {
            freight_API freight = new freight_API();
            freight.shipperZip = "53207";
            freight.consigneeZip = "59411";
            freight.weight = "2716";
            freight.freightClass = "110";
            freight.orderNumber = "2223270";
            DataTable dt = freight.GetTargetFreightwithBOL(freight);

            string queryID = "0";

            foreach (DataRow dr in dt.Rows)
            {
                queryID= $" {dr["QueryID"].ToString()}";
            }

            //string bol = freight.ExecuteBillsOfLading(queryID);
            return queryID;
        }

        [WebMethod]
        public string getFreightAPI_Post()
        {
            string strURL = string.Format("http://targetfmitms.com/index.php?p=api&r=xml&c=rater&m=quote");
            String pwd = String.Format("{0}:{1}", "d5db5543-af3c-4eb6-8073-fc0e98195f06", "");
            Byte[] authBytes = Encoding.UTF8.GetBytes(pwd.ToCharArray());
            var result = "None";
            var resultV = "None";
            string postData = "";

            //NameValueCollection postValues = new NameValueCollection();
            Dictionary<string, string> postValues = new Dictionary<string, string>();
            postValues.Add("general[code]", "ASPENLIC");
            postValues.Add("general[shipper]", "32254");
            postValues.Add("general[consignee]", "30339");
            postValues.Add("general[shipment_type]", "Outbound/Prepaid");
            postValues.Add("units[0][num_of]", "1");
            postValues.Add("units[0][type]", "Pallet");
            postValues.Add("units[0][stack]", "No");
            postValues.Add("units[0][length]", "3");
            postValues.Add("units[0][width]", "3");
            postValues.Add("units[0][height]", "3");
            postValues.Add("units[0][products][0][pieces]", "1");
            postValues.Add("units[0][products][0][weight]", "1019");
            postValues.Add("units[0][products][0][class]", "110");

            foreach (string key in postValues.Keys)
            {
                postData += HttpUtility.UrlEncode(key) + "="
                      + HttpUtility.UrlEncode(postValues[key]) + "&";
            }
            
            HttpWebRequest requestObjPost = (HttpWebRequest )HttpWebRequest.Create(strURL);
            requestObjPost.Method = "POST";
            byte[] byteArray = Encoding.ASCII.GetBytes(postData);
            requestObjPost.ContentType = "application/x-www-form-urlencoded";
            //requestObjPost.ContentLength = byteArray.Length;
            requestObjPost.Headers[HttpRequestHeader.Authorization] = $"Basic { Convert.ToBase64String(authBytes)}";

            //Stream requestStream = requestObjPost.GetRequestStream();
            //requestStream.Write(byteArray, 0, byteArray.Length);
            //requestStream.Close();

            //HttpWebResponse myHttpWebResponse = (HttpWebResponse)requestObjPost.GetResponse();

            //Stream responseStream = myHttpWebResponse.GetResponseStream();

            //StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
            //result = myStreamReader.ReadToEnd();
            //myStreamReader.Close();
            //responseStream.Close();
            //myHttpWebResponse.Close();
            //string postData = "{\"title\":\"testdata\",\"body\":\"testbody\",\"userid\":\"50\"}";
            using (var streamWriter = new StreamWriter(requestObjPost.GetRequestStream()))
            {
                streamWriter.Write(postData);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)requestObjPost.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    resultV = streamReader.ReadToEnd();
                }
            }

            WebClient wc = new WebClient();
            wc.Headers[HttpRequestHeader.Authorization] = $"Basic { Convert.ToBase64String(authBytes)}";

            NameValueCollection postValue = new NameValueCollection();

            postValue.Add("general[code]", "HUDSONIL");
            postValue.Add("general[shipper]", "15236");
            postValue.Add("general[consignee]", "15236");
            postValue.Add("general[shipment_type]", "Outbound/Prepaid");
            postValue.Add("units[0][num_of]", "1");
            postValue.Add("units[0][type]", "Pallet");
            postValue.Add("units[0][stack]", "No");
            postValue.Add("units[0][length]", "3");
            postValue.Add("units[0][width]", "3");
            postValue.Add("units[0][height]", "3");
            postValue.Add("units[0][products][0][pieces]", "1");
            postValue.Add("units[0][products][0][weight]", "500");
            postValue.Add("units[0][products][0][class]", "50");

            foreach (string key in postValue.Keys)
            {
                postData += HttpUtility.UrlEncode(key) + "="
                      + HttpUtility.UrlEncode(postValue[key]) + "&";
            }

            Byte[] responseBytes = wc.UploadValues(strURL, "POST", postValue);
            UTF8Encoding utf8 = new UTF8Encoding();
            string responseBody = utf8.GetString(responseBytes);
            return $"Post Value {postData} Response Body {resultV}";
        }

        [WebMethod]
        public string getFreighwithBOL_API_Post()
        {
            string strURL = string.Format("http://www.targetfmitms.com/?p=api&r=xml&c=rater&m=lcc");
            String pwd = String.Format("{0}:{1}", "d5db5543-af3c-4eb6-8073-fc0e98195f06", "");
            Byte[] authBytes = Encoding.UTF8.GetBytes(pwd.ToCharArray());
            string queryID = "0";
            var resultV = "None";
            string postData = "";

            //NameValueCollection postValues = new NameValueCollection();
            Dictionary<string, string> postValues = new Dictionary<string, string>();
            postValues.Add("general[code]", "ASPENLIC");
            postValues.Add("general[shipper]", "32254");
            postValues.Add("general[consignee]", "30339");
            postValues.Add("general[shipment_type]", "Outbound/Prepaid");
            postValues.Add("units[0][num_of]", "1");
            postValues.Add("units[0][type]", "Pallet");
            postValues.Add("units[0][stack]", "No");
            postValues.Add("units[0][length]", "3");
            postValues.Add("units[0][width]", "3");
            postValues.Add("units[0][height]", "3");
            postValues.Add("units[0][products][0][pieces]", "1");
            postValues.Add("units[0][products][0][weight]", "1019");
            postValues.Add("units[0][products][0][class]", "110");

            foreach (string key in postValues.Keys)
            {
                postData += HttpUtility.UrlEncode(key) + "="
                      + HttpUtility.UrlEncode(postValues[key]) + "&";
            }

            HttpWebRequest requestObjPost = (HttpWebRequest)HttpWebRequest.Create(strURL);
            requestObjPost.Method = "POST";
            byte[] byteArray = Encoding.ASCII.GetBytes(postData);
            requestObjPost.ContentType = "application/x-www-form-urlencoded";
            requestObjPost.Headers[HttpRequestHeader.Authorization] = $"Basic { Convert.ToBase64String(authBytes)}";
            
            using (var streamWriter = new StreamWriter(requestObjPost.GetRequestStream()))
            {
                streamWriter.Write(postData);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)requestObjPost.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    resultV = streamReader.ReadToEnd();
                }
            }

            DataSet ds = new DataSet();
            ds.ReadXml(new MemoryStream(System.Text.ASCIIEncoding.Default.GetBytes(resultV)));
            DataTable dt = new DataTable("BOLData");

            for (int i = 0; i < ds.Tables["body"].Rows.Count; i++)
            {

                if (ds.Tables["body"].Columns.Contains("query_id"))
                {
                    queryID = (ds.Tables["body"].Rows[i]["query_id"].ToString());
                }
            }

            return $"QueryID {queryID} Post Value {postData} Response Body {resultV}";
        }
    }
}
