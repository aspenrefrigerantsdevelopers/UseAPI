using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Specialized;
using System.Configuration;

namespace UseAPI
{
    public class freight_API
    {
        public string shipperZip { get; set; }
        public string  consigneeZip { get; set; }
        public string weight { get; set; }
        public string freightClass { get; set; }
        public string orderNumber { get; set; }


        public string IsLocal()
        {
            string db;

            int localPath = HttpContext.Current.Server.MapPath("~").IndexOf("project");
            if (localPath != -1)
            {
                //dev
                db = "dev";
            }
            else
            {
                //live
                db = "live";
            }
            return db;
        }
        public string callJSONPlaceholder()
        {
            string strURL = string.Format("https://jsonplaceholder.typicode.com/posts");
            WebRequest requestObjPost = WebRequest.Create(strURL);
            requestObjPost.Method = "POST";
            requestObjPost.ContentType = "application/json";

            string postData = "{\"title\":\"testdata\",\"body\":\"testbody\",\"userid\":\"50\"}";
            var result = "None";
            using (var streamWriter = new StreamWriter(requestObjPost.GetRequestStream()))
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

        public static DataSet ExecuteProcedureReturnDataSet(string procName,
            params SqlParameter[] parameters)
        {
            DataSet result = null;

            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["LIVEDB"].ToString()))
            {
                using (var command = sqlConnection.CreateCommand())
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(command))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = procName;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        result = new DataSet();
                        sda.Fill(result);
                    }
                }
            }
            return result;
        }

        public DataTable GetTargetFreight(freight_API freightValues)
        {
            string strURL = string.Format("http://targetfmitms.com/index.php?p=api&r=xml&c=rater&m=quote");
            String pwd = String.Format("{0}:{1}", "d5db5543-af3c-4eb6-8073-fc0e98195f06", "");
            Byte[] authBytes = Encoding.UTF8.GetBytes(pwd.ToCharArray());
            var result = "None";
            string postData = "";

            Dictionary<string, string> postValues = new Dictionary<string, string>();
            postValues.Add("general[code]", "ASPENLIC");
            postValues.Add("general[shipper]", freightValues.shipperZip);
            postValues.Add("general[consignee]", freightValues.consigneeZip);
            postValues.Add("general[shipment_type]", "Outbound/Prepaid");
            postValues.Add("units[0][num_of]", "1");
            postValues.Add("units[0][type]", "Pallet");
            postValues.Add("units[0][stack]", "No");
            postValues.Add("units[0][length]", "3");
            postValues.Add("units[0][width]", "3");
            postValues.Add("units[0][height]", "3");
            postValues.Add("units[0][products][0][pieces]", "1");
            postValues.Add("units[0][products][0][weight]", freightValues.weight);
            postValues.Add("units[0][products][0][class]", freightValues.freightClass);

            foreach (string key in postValues.Keys)
            {
                postData += HttpUtility.UrlEncode(key) + "="
                      + HttpUtility.UrlEncode(postValues[key]) + "&";
            }

            HttpWebRequest requestObjPost = (HttpWebRequest)HttpWebRequest.Create(strURL);
            requestObjPost.Method = "POST";
            byte[] byteArray = Encoding.ASCII.GetBytes(postData);
            requestObjPost.ContentType = "application/x-www-form-urlencoded";
            //requestObjPost.ContentLength = byteArray.Length;
            requestObjPost.Headers[HttpRequestHeader.Authorization] = $"Basic { Convert.ToBase64String(authBytes)}";

            using (var streamWriter = new StreamWriter(requestObjPost.GetRequestStream()))
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

            DataSet ds = new DataSet();
            ds.ReadXml(new MemoryStream(System.Text.ASCIIEncoding.Default.GetBytes(result)));
            result = "";

            //  DataTable dt = new DataTable("Branches");  
            DataTable dt = new DataTable("FrerightQuotes");
            DataTable dtRates = new DataTable("Rates");
            dt.Columns.Add("CarrierID", typeof(string));
            dt.Columns.Add("CarrierName", typeof(string));
            dt.Columns.Add("ShipmentDate", typeof(string));
            dt.Columns.Add("ServiceDays", typeof(string));
            dt.Columns.Add("DeliveryDate", typeof(string));
            dt.Columns.Add("Distance", typeof(string));
            dt.Columns.Add("FreightCost", typeof(string));
            dt.Columns.Add("FuelSurcharge", typeof(string));
            dt.Columns.Add("AccessorialCosts", typeof(string));
            dt.Columns.Add("EstimateCost", typeof(string));
            dt.Columns.Add("TrueCost", typeof(string));
            dt.Columns.Add("ServiceType", typeof(string));
            dt.Columns.Add("ShipmentMethod", typeof(string));

            for (int i = 0; i < ds.Tables["carriers"].Rows.Count; i++)
            {
                dt.Rows.Add();
                if (ds.Tables["carriers"].Columns.Contains("scac"))
                {
                    dt.Rows[i]["CarrierID"] = (ds.Tables["carriers"].Rows[i]["scac"].ToString());
                }
                if (ds.Tables["carriers"].Columns.Contains("name"))
                {
                    dt.Rows[i]["CarrierName"] = (ds.Tables["carriers"].Rows[i]["name"].ToString());
                }
                if (ds.Tables["carriers"].Columns.Contains("move_type"))
                {
                    dt.Rows[i]["ServiceType"] = (ds.Tables["carriers"].Rows[i]["move_type"].ToString());
                }
                if (ds.Tables["carriers"].Columns.Contains("trans_time"))
                {
                    dt.Rows[i]["ServiceDays"] = (ds.Tables["carriers"].Rows[i]["trans_time"].ToString());
                }

                if (ds.Tables["carriers"].Columns.Contains("transit_supported"))
                {
                    dt.Rows[i]["ShipmentMethod"] = (ds.Tables["carriers"].Rows[i]["transit_supported"].ToString());
                }
                if (ds.Tables["carriers"].Columns.Contains("acc_total"))
                {
                    dt.Rows[i]["AccessorialCosts"] = (ds.Tables["carriers"].Rows[i]["acc_total"].ToString());
                }

                // rates 
                if (ds.Tables["rate"].Columns.Contains("freight"))
                {
                    dt.Rows[i]["FreightCost"] = (ds.Tables["rate"].Rows[i]["freight"].ToString());
                }
                if (ds.Tables["rate"].Columns.Contains("fuel"))
                {
                    dt.Rows[i]["FuelSurcharge"] = (ds.Tables["rate"].Rows[i]["fuel"].ToString());
                }
                if (ds.Tables["rate"].Columns.Contains("true_cost"))
                {
                    dt.Rows[i]["TrueCost"] = (ds.Tables["rate"].Rows[i]["true_cost"].ToString());
                }

                if (ds.Tables["rate"].Columns.Contains("fmt_true_cost"))
                {
                    dt.Rows[i]["EstimateCost"] = (ds.Tables["rate"].Rows[i]["fmt_true_cost"].ToString());
                }
            }

            foreach (DataRow dr in dt.Rows)
            {
                result += $" {dr["CarrierID"].ToString()} --- ";
                result += $" {dr["CarrierName"].ToString()} --- ";
                result += $" {dr["ServiceType"].ToString()} --- ";
                result += $" {dr["ServiceDays"].ToString()} --- ";
                result += $" {dr["ShipmentMethod"].ToString()} --- ";
                result += $" {dr["FreightCost"].ToString()} --- ";
                result += $" {dr["FuelSurcharge"].ToString()} --- ";
                result += $" {dr["TrueCost"].ToString()} --- ";
                result += $" {dr["EstimateCost"].ToString()} --- ";
                result += $" {dr["AccessorialCosts"].ToString()} --- ";
            }

            return dt;
        }

        public string Target_Carrier_GetQueryID(string orderNumber)
        {
            string queryId = "0";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LIVEDB"].ToString()))
            {
                conn.Open();
                using (SqlCommand comm = new SqlCommand("Target_Carriers_GetQueryID", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("@orderNumber", SqlDbType.Int).Value = orderNumber;

                    using (SqlDataReader rdr = comm.ExecuteReader())
                    {
                        int count = 0;
                        while (rdr.Read())
                        {
                            queryId = rdr[0].ToString();
                            count++;
                        }
                    }
                }
            }
            return queryId;
        }

        public string ExecuteBillsOfLading(string queryID)
        {
            Dictionary<string, string> postValues = new Dictionary<string, string>();
            SqlParameter[] parameter = {
                new SqlParameter("@queryID",queryID)
                ,new SqlParameter("@LinesPerPage", 1)
                ,new SqlParameter("@NumberOfCopies", 1)
            };
            string orderNumber = "0";

            DataSet dsBOL = ExecuteProcedureReturnDataSet("Target_Carriers_BOL_Data", parameter);
            int count = 0;
            foreach (DataTable dt in dsBOL.Tables)
            {
                orderNumber = dt.Rows[0].ItemArray[7].ToString();
                postValues.Add("carrier[query_id]", queryID);
                postValues.Add("carrier[carrier_scac]", dt.Rows[0].ItemArray[39].ToString());
                postValues.Add("general[pro]", "");
                postValues.Add("general[saved_bol]", "");
                postValues.Add("general[bol]", "");
                postValues.Add("general[ref]", "");
                postValues.Add("general[warehouse]", "ASPENLIC");
                postValues.Add("general[direction]", dt.Rows[0].ItemArray[50].ToString());
                postValues.Add("general[so]", dt.Rows[0].ItemArray[7].ToString());
                postValues.Add("general[po]", dt.Rows[0].ItemArray[10].ToString());
                postValues.Add("general[customer_date]", dt.Rows[0].ItemArray[20].ToString());

                postValues.Add("general[references][0][type]", "RIS#");
                postValues.Add("general[references][0][value]", orderNumber);
                postValues.Add("general[references][1][type]", "Deliver By");
                postValues.Add("general[references][1][value]", dt.Rows[0].ItemArray[20].ToString());

                postValues.Add("location[shipper][name]", dt.Rows[0].ItemArray[23].ToString());
                postValues.Add("location[shipper][address1]", dt.Rows[0].ItemArray[24].ToString());
                postValues.Add("location[shipper][address2]", dt.Rows[0].ItemArray[25].ToString());
                postValues.Add("location[shipper][city]", dt.Rows[0].ItemArray[46].ToString());
                postValues.Add("location[shipper][state]", dt.Rows[0].ItemArray[47].ToString());
                postValues.Add("location[shipper][zip]", dt.Rows[0].ItemArray[43].ToString());
                postValues.Add("location[shipper][country]", "USA");
                postValues.Add("location[shipper][contact_name]", "");
                postValues.Add("location[shipper][contact_phone]", "");
                postValues.Add("location[shipper][contact_fax]", "");
                postValues.Add("location[shipper][contact_email]", "");
                postValues.Add("location[shipper][save]", "False");
                postValues.Add("location[consignee][name]", dt.Rows[0].ItemArray[14].ToString());
                postValues.Add("location[consignee][address1]", dt.Rows[0].ItemArray[15].ToString());
                postValues.Add("location[consignee][address2]", dt.Rows[0].ItemArray[16].ToString());
                postValues.Add("location[consignee][city]", dt.Rows[0].ItemArray[44].ToString());
                postValues.Add("location[consignee][state]", dt.Rows[0].ItemArray[45].ToString());
                postValues.Add("location[consignee][zip]", dt.Rows[0].ItemArray[42].ToString());
                postValues.Add("location[consignee][country]", "USA");
                postValues.Add("location[consignee][contact_name]", "");
                postValues.Add("location[consignee][contact_phone]", "");
                postValues.Add("location[consignee][contact_fax]", "");
                postValues.Add("location[consignee][contact_email]", "");
                postValues.Add("location[consignee][save]", "False");
                postValues.Add("location[billing][name]", "ASPEN Refrigerants, Inc. C/O TFM");
                postValues.Add("location[billing][address1]", "5905 Brownsville Rd.");
                postValues.Add("location[billing][address2]", "");
                postValues.Add("location[billing][city]", "Pittsburgh");
                postValues.Add("location[billing][state]", "PA");
                postValues.Add("location[billing][zip]", "15236");
                postValues.Add("location[billing][country]", "USA");
                postValues.Add("location[billing][contact_name]", "Customer Service");
                postValues.Add("location[billing][contact_phone]", "412-653-1323");
                postValues.Add("location[billing][contact_fax]", "412-653-1908");
                postValues.Add("location[billing][contact_email]", "customerservice@targetfmi.com");
                postValues.Add("location[billing][save]", "False");
                postValues.Add("special[notes]", dt.Rows[0].ItemArray[21].ToString());

                foreach (DataRow dr in dt.Rows)
                {
                    postValues.Add($"units[{count}][details][pieces]", "1");
                    postValues.Add($"units[{count}][details][stack]", "False");
                    postValues.Add($"units[{count}][details][type]", "Pallet");
                    postValues.Add($"units[{count}][details][length]", "1");
                    postValues.Add($"units[{count}][details][width]", "1");
                    postValues.Add($"units[{count}][details][height]", "1");
                    postValues.Add($"units[{count}][products][0][product]", $"{dr.ItemArray[34].ToString()} {dr.ItemArray[32].ToString()}");
                    postValues.Add($"units[{count}][products][0][pieces]", dr.ItemArray[29].ToString());
                    postValues.Add($"units[{count}][products][0][uom]", "Cylinder");
                    postValues.Add($"units[{count}][products][0][nmfc]", dr.ItemArray[40].ToString());
                    postValues.Add($"units[{count}][products][0][sub_nmfc]", "");
                    postValues.Add($"units[{count}][products][0][class]", dr.ItemArray[41].ToString());
                    postValues.Add($"units[{count}][products][0][weight]", dr.ItemArray[31].ToString());
                    postValues.Add($"units[{count}][products][0][hazmat]", "");
                    postValues.Add($"units[{count}][products][0][hazmat][class]", "");
                    postValues.Add($"units[{count}][products][0][hazmat][un_num]", "");
                    postValues.Add($"units[{count}][products][0][hazmat][group]", "");
                    postValues.Add($"units[{count}][products][0][hazmat][emergency]", "Chemtrec 1-800-424-9300 CCN829305");
                    count++;
                }
            }

            string strURL = string.Format("Http://targetfmitms.com/index.php?p=api&r=xml&c=billoflading&m=execute");
            String pwd = String.Format("{0}:{1}", "d5db5543-af3c-4eb6-8073-fc0e98195f06", "");
            Byte[] authBytes = Encoding.UTF8.GetBytes(pwd.ToCharArray());
            var result = "None";
            string postData = "";

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
                    result = streamReader.ReadToEnd();
                }
            }

            int okContinue = result.IndexOf("<type>error</type>"); // if -1 returned no errors found
            if (okContinue != -1)
            {
                return result;
            }
            else
            {
                DataSet ds = new DataSet();
                ds.ReadXml(new MemoryStream(System.Text.ASCIIEncoding.Default.GetBytes(result)));

                string bolId = "0";
                string bolDate = "20200101";

                for (int i = 0; i < ds.Tables["body"].Rows.Count; i++)
                {

                    if (ds.Tables["body"].Columns.Contains("bolId"))
                    {
                        bolId = (ds.Tables["body"].Rows[i]["bolId"].ToString());
                    }
                    if (ds.Tables["body"].Columns.Contains("date"))
                    {
                        bolDate = (ds.Tables["body"].Rows[i]["date"].ToString());
                    }
                }
                //bolId = $"{bolDate}{bolId}";
                string pickupStatus = PickupRequest(queryID, $"{bolDate}{bolId}");
                string bolPDF = PrintBillOfLading(bolId, bolDate);
                SendEmailWithBOLAttachment(orderNumber, bolPDF, $"QueryID-{queryID}|{pickupStatus}|Filepath - {bolPDF}");
                return $"Pickup Response - {pickupStatus} - Print PDF Response - {bolPDF}";
            }
        }

        public string PrintBillOfLading(string BolId, string BolDate)
        {
            freight_API freight = new freight_API();
            string local = freight.IsLocal();
            //string auth = System.Configuration.ConfigurationManager.AppSettings["TargetAuth"];
            string url = $"Http://targetfmitms.com/index.php?p=api&r=text&c=billoflading&m=pdf&d={BolId}/{BolDate}";
            string strURL = string.Format($"{url}");
            String pwd = String.Format("{0}:{1}", "d5db5543-af3c-4eb6-8073-fc0e98195f06", "");
            string fp;
            string fn = $"BillOfLading_{BolDate}{BolId}.pdf";
            string fileDirectory;
            Byte[] authBytes = Encoding.UTF8.GetBytes(pwd.ToCharArray());

            DateTime datetimmstamp = DateTime.Now;
            if (local=="dev")
            {   
                //fileDirectory = @"\\10.186.130.3\C-Print_Files\";
                fileDirectory = @"C:\_TargetBOL";
            }
            else
            {
                fileDirectory = @"\\172.20.92.150\Print_Files\";
            }
            fileDirectory = @"\\172.20.92.150\Print_Files\";
            fp = $@"{fileDirectory}\{fn}";

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(strURL);
            req.ContentType = "application/pdf";
            req.Method = "Post";
            req.Headers[HttpRequestHeader.Authorization] = $"Basic {Convert.ToBase64String(authBytes)}";

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (Stream str = resp.GetResponseStream())
                {
                    using (var mm = new MemoryStream())
                    {
                        str.CopyTo(mm);
                        byte[] b = mm.ToArray();
                        FileStream fs = new FileStream(fp, FileMode.Create, FileAccess.Write);

                        if (fs.CanWrite)
                        {
                            fs.Write(b, 0, b.Length);
                        }

                        fs.Flush();
                        fs.Close();
                    }
                }
            }
            return $"{fp}";

        }

        public string SendEmailWithBOLAttachment(string orderNumber, string filePath, string bodyText)
        {
            SqlParameter[] parameter = {
                new SqlParameter("@orderNumber",orderNumber)
                , new SqlParameter("@filePath", filePath)
                , new SqlParameter("@bodyText", bodyText)
            };

            DataSet dsBOL = ExecuteProcedureReturnDataSet("Target_Carriers_SendEmailWithBOLAttachment", parameter);
            return "done";
        }

        public DataTable GetTargetFreightwithBOL(freight_API freightValues)
        {
            string strURL = string.Format("http://www.targetfmitms.com/?p=api&r=xml&c=rater&m=lcc");
            String pwd = String.Format("{0}:{1}", "d5db5543-af3c-4eb6-8073-fc0e98195f06", "");
            Byte[] authBytes = Encoding.UTF8.GetBytes(pwd.ToCharArray());
            var result = "None";
            string postData = "";
            string queryID = "0";
            int count = 0;
            string shipmentType = "Outbound/Prepaid";

            Dictionary<string, string> postValues = new Dictionary<string, string>();

            SqlParameter[] parameter = {
                new SqlParameter("@orderNumber",freightValues.orderNumber)
                ,new SqlParameter("@LinesPerPage", 1)
                ,new SqlParameter("@NumberOfCopies", 1)
            };
            DataSet dsBOL = ExecuteProcedureReturnDataSet("Target_Carriers_GetOrderDetails", parameter);
            foreach (DataTable tbl in dsBOL.Tables)
            {
                foreach (DataRow dr in tbl.Rows)
                {
                    string numberOfunits = dr.ItemArray[2].ToString();
                    shipmentType = dr.ItemArray[7].ToString();
                    postValues.Add($"units[{count}][type]", "Cylinder");
                    postValues.Add($"units[{count}][stack]", "No");
                    postValues.Add($"units[{count}][length]", "1");
                    postValues.Add($"units[{count}][width]", "1");
                    postValues.Add($"units[{count}][height]", "1");
                    postValues.Add($"units[{count}][products][0][pieces]", numberOfunits);
                    postValues.Add($"units[{count}][products][0][weight]", dr.ItemArray[4].ToString());
                    postValues.Add($"units[{count}][products][0][class]", dr.ItemArray[6].ToString());
                    postValues.Add($"units[{count}][num_of]", "1");
                    count++;
                }
            }

            postValues.Add("general[code]", "ASPENLIC");
            postValues.Add("general[shipper]", freightValues.shipperZip);
            postValues.Add("general[consignee]", freightValues.consigneeZip);
            postValues.Add("general[shipment_type]", shipmentType);

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
                    result = streamReader.ReadToEnd();
                }
            }

            DataSet ds = new DataSet();
            ds.ReadXml(new MemoryStream(System.Text.ASCIIEncoding.Default.GetBytes(result)));
            result = "";
  
            DataTable dt = new DataTable("FrerightQuotes");
            DataTable dtRates = new DataTable("Rates");
            dt.Columns.Add("CarrierID", typeof(string));
            dt.Columns.Add("CarrierName", typeof(string));
            dt.Columns.Add("ShipmentDate", typeof(string));
            dt.Columns.Add("ServiceDays", typeof(string));
            dt.Columns.Add("DeliveryDate", typeof(string));
            dt.Columns.Add("Distance", typeof(string));
            dt.Columns.Add("FreightCost", typeof(string));
            dt.Columns.Add("FuelSurcharge", typeof(string));
            dt.Columns.Add("AccessorialCosts", typeof(string));
            dt.Columns.Add("EstimateCost", typeof(string));
            dt.Columns.Add("TrueCost", typeof(string));
            dt.Columns.Add("ServiceType", typeof(string));
            dt.Columns.Add("ShipmentMethod", typeof(string));
            dt.Columns.Add("QueryID", typeof(string));            

            for (int i = 0; i < ds.Tables["body"].Rows.Count; i++)
            {

                if (ds.Tables["body"].Columns.Contains("query_id"))
                {
                    queryID  = (ds.Tables["body"].Rows[i]["query_id"].ToString());
                }
            }

            for (int i = 0; i < ds.Tables["carriers"].Rows.Count; i++)
            {
                dt.Rows.Add();
                if (ds.Tables["carriers"].Columns.Contains("scac"))
                {
                    dt.Rows[i]["CarrierID"] = (ds.Tables["carriers"].Rows[i]["scac"].ToString());
                }
                if (ds.Tables["carriers"].Columns.Contains("name"))
                {
                    dt.Rows[i]["CarrierName"] = (ds.Tables["carriers"].Rows[i]["name"].ToString());
                }
                if (ds.Tables["carriers"].Columns.Contains("move_type"))
                {
                    dt.Rows[i]["ServiceType"] = (ds.Tables["carriers"].Rows[i]["move_type"].ToString());
                }
                if (ds.Tables["carriers"].Columns.Contains("trans_time"))
                {
                    dt.Rows[i]["ServiceDays"] = (ds.Tables["carriers"].Rows[i]["trans_time"].ToString());
                }

                if (ds.Tables["carriers"].Columns.Contains("transit_supported"))
                {
                    dt.Rows[i]["ShipmentMethod"] = (ds.Tables["carriers"].Rows[i]["transit_supported"].ToString());
                }
                if (ds.Tables["carriers"].Columns.Contains("acc_total"))
                {
                    dt.Rows[i]["AccessorialCosts"] = (ds.Tables["carriers"].Rows[i]["acc_total"].ToString());
                }

                // rates 
                if (ds.Tables["rate"].Columns.Contains("freight"))
                {
                    dt.Rows[i]["FreightCost"] = (ds.Tables["rate"].Rows[i]["freight"].ToString());
                }
                if (ds.Tables["rate"].Columns.Contains("fuel"))
                {
                    dt.Rows[i]["FuelSurcharge"] = (ds.Tables["rate"].Rows[i]["fuel"].ToString());
                }
                if (ds.Tables["rate"].Columns.Contains("true_cost"))
                {
                    dt.Rows[i]["TrueCost"] = (ds.Tables["rate"].Rows[i]["true_cost"].ToString());
                }

                if (ds.Tables["rate"].Columns.Contains("fmt_true_cost"))
                {
                    dt.Rows[i]["EstimateCost"] = (ds.Tables["rate"].Rows[i]["fmt_true_cost"].ToString());
                }
                dt.Rows[i]["QueryID"] = queryID;
            }

            foreach (DataRow dr in dt.Rows)
            {
                result += $" {dr["CarrierID"].ToString()} --- ";
                result += $" {dr["CarrierName"].ToString()} --- ";
                result += $" {dr["ServiceType"].ToString()} --- ";
                result += $" {dr["ServiceDays"].ToString()} --- ";
                result += $" {dr["ShipmentMethod"].ToString()} --- ";
                result += $" {dr["FreightCost"].ToString()} --- ";
                result += $" {dr["FuelSurcharge"].ToString()} --- ";
                result += $" {dr["TrueCost"].ToString()} --- ";
                result += $" {dr["EstimateCost"].ToString()} --- ";
                result += $" {dr["AccessorialCosts"].ToString()} --- ";
                result += $" {dr["QueryID"].ToString()} --- ";
            }

            return dt;
        }

        public string PickupRequest(string queryID, string bolId)
        {
            Dictionary<string, string> postValues = new Dictionary<string, string>();

            SqlParameter[] parameter = {
                new SqlParameter("@queryID",queryID)
                ,new SqlParameter("@LinesPerPage", 1)
                ,new SqlParameter("@NumberOfCopies", 1)
            };
            string orderNumber = "0";
            string pronumber = "0";
            string confirmationnumber = "0";
            string transactionID = "0";
            string pickupdate = "0";
            string expectedDeliveryDate = "0";

            DataSet dsBOL = ExecuteProcedureReturnDataSet("Target_Carriers_BOL_Data", parameter);
            int count = 0;
            foreach (DataTable dt in dsBOL.Tables)
            {
                orderNumber = dt.Rows[0].ItemArray[7].ToString();
                postValues.Add("carrier[query_id]", queryID);
                postValues.Add("carrier[carrier_scac]", dt.Rows[0].ItemArray[39].ToString());
                postValues.Add("general[pro]", "");
                postValues.Add("general[saved_bol]", bolId);
                postValues.Add("general[bol]", "");
                postValues.Add("general[ref]", "");
                postValues.Add("general[warehouse]", "ASPENLIC");
                postValues.Add("general[direction]", dt.Rows[0].ItemArray[50].ToString());
                postValues.Add("general[so]", dt.Rows[0].ItemArray[7].ToString());
                postValues.Add("general[po]", dt.Rows[0].ItemArray[10].ToString());
                postValues.Add("general[customer_date]", dt.Rows[0].ItemArray[20].ToString());

                postValues.Add("general[references][0][type]", "RIS#");
                postValues.Add("general[references][0][value]", orderNumber);
                postValues.Add("general[references][1][type]", "Deliver By");
                postValues.Add("general[references][1][value]", dt.Rows[0].ItemArray[20].ToString());

                postValues.Add("location[shipper][name]", dt.Rows[0].ItemArray[23].ToString());
                postValues.Add("location[shipper][address1]", dt.Rows[0].ItemArray[24].ToString());
                postValues.Add("location[shipper][address2]", dt.Rows[0].ItemArray[25].ToString());
                postValues.Add("location[shipper][city]", dt.Rows[0].ItemArray[44].ToString());
                postValues.Add("location[shipper][state]", dt.Rows[0].ItemArray[45].ToString());
                postValues.Add("location[shipper][zip]", dt.Rows[0].ItemArray[42].ToString());
                postValues.Add("location[shipper][country]", "USA");
                postValues.Add("location[shipper][contact_name]", "");
                postValues.Add("location[shipper][contact_phone]", "");
                postValues.Add("location[shipper][contact_fax]", "");
                postValues.Add("location[shipper][contact_email]", "");
                postValues.Add("location[shipper][save]", "False");
                postValues.Add("location[consignee][name]", dt.Rows[0].ItemArray[14].ToString());
                postValues.Add("location[consignee][address1]", dt.Rows[0].ItemArray[15].ToString());
                postValues.Add("location[consignee][address2]", dt.Rows[0].ItemArray[16].ToString());
                postValues.Add("location[consignee][city]", dt.Rows[0].ItemArray[44].ToString());
                postValues.Add("location[consignee][state]", dt.Rows[0].ItemArray[45].ToString());
                postValues.Add("location[consignee][zip]", dt.Rows[0].ItemArray[42].ToString());
                postValues.Add("location[consignee][country]", "USA");
                postValues.Add("location[consignee][contact_name]", "");
                postValues.Add("location[consignee][contact_phone]", "");
                postValues.Add("location[consignee][contact_fax]", "");
                postValues.Add("location[consignee][contact_email]", "");
                postValues.Add("location[consignee][save]", "False");
                postValues.Add("location[billing][name]", "ASPEN Refrigerants, Inc. C/O TFM");
                postValues.Add("location[billing][address1]", "5905 Brownsville Rd.");
                postValues.Add("location[billing][address2]", "");
                postValues.Add("location[billing][city]", "Pittsburgh");
                postValues.Add("location[billing][state]", "PA");
                postValues.Add("location[billing][zip]", "15236");
                postValues.Add("location[billing][country]", "USA");
                postValues.Add("location[billing][contact_name]", "Customer Service");
                postValues.Add("location[billing][contact_phone]", "412-653-1323");
                postValues.Add("location[billing][contact_fax]", "412-653-1908");
                postValues.Add("location[billing][contact_email]", "customerservice@targetfmi.com");
                postValues.Add("location[billing][save]", "False");
                postValues.Add("special[notes]", dt.Rows[0].ItemArray[21].ToString());

                foreach (DataRow dr in dt.Rows)
                {
                    postValues.Add($"units[{count}][details][pieces]", "1");
                    postValues.Add($"units[{count}][details][stack]", "False");
                    postValues.Add($"units[{count}][details][type]", "Pallet");
                    postValues.Add($"units[{count}][details][length]", "1");
                    postValues.Add($"units[{count}][details][width]", "1");
                    postValues.Add($"units[{count}][details][height]", "1");
                    postValues.Add($"units[{count}][products][0][product]", $"{dr.ItemArray[34].ToString()} {dr.ItemArray[32].ToString()}");
                    postValues.Add($"units[{count}][products][0][pieces]", dr.ItemArray[29].ToString());
                    postValues.Add($"units[{count}][products][0][uom]", "Cylinder");
                    postValues.Add($"units[{count}][products][0][nmfc]", dr.ItemArray[40].ToString());
                    postValues.Add($"units[{count}][products][0][sub_nmfc]", "");
                    postValues.Add($"units[{count}][products][0][class]", dr.ItemArray[41].ToString());
                    postValues.Add($"units[{count}][products][0][weight]", dr.ItemArray[31].ToString());
                    postValues.Add($"units[{count}][products][0][hazmat]", "");
                    postValues.Add($"units[{count}][products][0][hazmat][class]", "");
                    postValues.Add($"units[{count}][products][0][hazmat][un_num]", "");
                    postValues.Add($"units[{count}][products][0][hazmat][group]", "");
                    postValues.Add($"units[{count}][products][0][hazmat][emergency]", "Chemtrec 1-800-424-9300 CCN829305");
                    count++;
                }

                postValues.Add("schedulePickup[pickupDateTime]", dt.Rows[0].ItemArray[48].ToString());
                postValues.Add("schedulePickup[dockCloseTime]", dt.Rows[0].ItemArray[49].ToString());
            }

            string strURL = string.Format("Http://targetfmitms.com/index.php?p=api&r=xml&c=pickupRequest&m=schedulePickup");
            String pwd = String.Format("{0}:{1}", "d5db5543-af3c-4eb6-8073-fc0e98195f06", "");
            Byte[] authBytes = Encoding.UTF8.GetBytes(pwd.ToCharArray());
            var result = "None";
            string postData = "";

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
                    result = streamReader.ReadToEnd();
                }
            }

            int okContinue = result.IndexOf("<type>error</type>"); // if -1 returned no errors found
            if (okContinue != -1)
            {
                return $"FREIGHT_API.CS ERROR ALERT - CONTACT IT - {result}";
            }
            else
            {
                DataSet ds = new DataSet();
                ds.ReadXml(new MemoryStream(System.Text.ASCIIEncoding.Default.GetBytes(result)));
                
                for (int i = 0; i < ds.Tables["body"].Rows.Count; i++)
                {

                    if (ds.Tables["body"].Columns.Contains("ProNumber"))
                    {
                        pronumber = (ds.Tables["body"].Rows[i]["ProNumber"].ToString());
                    }
                    if (ds.Tables["body"].Columns.Contains("ConfirmationNumber"))
                    {
                        confirmationnumber = (ds.Tables["body"].Rows[i]["ConfirmationNumber"].ToString());
                    }
                    if (ds.Tables["body"].Columns.Contains("TransactionID"))
                    {
                        transactionID = (ds.Tables["body"].Rows[i]["TransactionID"].ToString());
                    }
                    if (ds.Tables["body"].Columns.Contains("PickupDate"))
                    {
                        pickupdate = (ds.Tables["body"].Rows[i]["PickupDate"].ToString());
                    }
                    if (ds.Tables["body"].Columns.Contains("ExpectedDeliveryDate"))
                    {
                        expectedDeliveryDate = (ds.Tables["body"].Rows[i]["ExpectedDeliveryDate"].ToString());
                    }
                    result = "Success";
                }
                return $"Pronumber-{pronumber}|Confirmation Number-{confirmationnumber}|Transaction ID-{transactionID}|Pickup Date-{pickupdate}|Expected Delivery Date-{expectedDeliveryDate}|PickupRequest Result - {result}";
            }
        }
    }
}