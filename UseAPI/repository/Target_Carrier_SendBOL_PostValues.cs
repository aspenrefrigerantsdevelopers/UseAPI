using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;


namespace UseAPI.repository
{
    public class Target_Carrier_SendBOL_PostValues
    {
        public freight_API Target_Carrier_Call(freight_API request)
        {
            Dictionary<string, string> postValues = new Dictionary<string, string>();

            var nmfc = "0";
            var subnmfc = "";
            string queryID = string.Empty;
            string accessorials = string.Empty;
            string orderNumber = request.orderNumber;
            string targetBolId = $"{request.bolDate}{request.bolId}";
            string api = request.apiName;
            string apiURL = request.apiURL;
            var result = "None";
            string postData = string.Empty;
            string addAccessorial = string.Empty;

            SqlParameter[] parameter = {
                new SqlParameter("@orderNumber",orderNumber)
            };

            DataSet dsBOL = freight_API.ExecuteProcedureReturnDataSet("Target_Carriers_BOL_DataV2", parameter);
            int count = 0;
            foreach (DataTable dt in dsBOL.Tables)
            {
                queryID = dt.Rows[0].ItemArray[60].ToString();
                accessorials = dt.Rows[0].ItemArray[57].ToString();

                postValues.Add("carrier[query_id]", queryID);
                postValues.Add("carrier[carrier_scac]", dt.Rows[0].ItemArray[39].ToString());
                postValues.Add("general[pro]", "");
                postValues.Add("general[saved_bol]", targetBolId);
                postValues.Add("general[bol]", dt.Rows[0].ItemArray[18].ToString());
                postValues.Add("general[ref]", "");
                postValues.Add("general[warehouse]", "ASPENLIC");
                postValues.Add("general[direction]", dt.Rows[0].ItemArray[50].ToString());
                postValues.Add("general[so]", dt.Rows[0].ItemArray[7].ToString());
                postValues.Add("general[po]", dt.Rows[0].ItemArray[10].ToString());
                postValues.Add("general[custom_date]", dt.Rows[0].ItemArray[54].ToString());

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
                    nmfc = dr.ItemArray[40].ToString();
                    if (nmfc == "41160")
                    {
                        subnmfc = "02";
                    }
                    else
                    {
                        subnmfc = "";
                    }
                    if (accessorials != "0")
                    {
                        addAccessorial = "add";
                    }
                    postValues.Add($"units[{count}][details][pieces]", dr.ItemArray[56].ToString());
                    postValues.Add($"units[{count}][details][stack]", "False");
                    postValues.Add($"units[{count}][details][type]", "Pallet");
                    postValues.Add($"units[{count}][details][length]", "1");
                    postValues.Add($"units[{count}][details][width]", "1");
                    postValues.Add($"units[{count}][details][height]", "1");
                    postValues.Add($"units[{count}][products][0][product]", $"{dr.ItemArray[34].ToString()}");
                    postValues.Add($"units[{count}][products][0][pieces]", dr.ItemArray[58].ToString());
                    postValues.Add($"units[{count}][products][0][uom]", "Cylinder");
                    postValues.Add($"units[{count}][products][0][nmfc]", nmfc);
                    postValues.Add($"units[{count}][products][0][sub_nmfc]", subnmfc);
                    postValues.Add($"units[{count}][products][0][class]", dr.ItemArray[41].ToString());
                    postValues.Add($"units[{count}][products][0][weight]", dr.ItemArray[61].ToString());
                    postValues.Add($"units[{count}][products][0][hazmat]", "");
                    postValues.Add($"units[{count}][products][0][hazmat][class]", dr.ItemArray[51].ToString()); /* 2.2 */
                    postValues.Add($"units[{count}][products][0][hazmat][un_num]", dr.ItemArray[52].ToString());/* UN */
                    postValues.Add($"units[{count}][products][0][hazmat][group]", "");
                    postValues.Add($"units[{count}][products][0][hazmat][packing_inst]", $"{ dr.ItemArray[32].ToString()}");
                    postValues.Add($"units[{count}][products][0][hazmat][emergency]", "Chemtrec 1-800-424-9300 CCN829305");
                    count++;
                }

                if (api == "PickupRequest")
                {
                    postValues.Add("schedulePickup[pickupDateTime]", dt.Rows[0].ItemArray[48].ToString());
                    postValues.Add("schedulePickup[dockCloseTime]", dt.Rows[0].ItemArray[49].ToString());
                }
                if (addAccessorial == "add")
                {
                    postValues.Add($"accessorials[0]", "420");
                }
            }

            string strURL = string.Format(apiURL);
            String pwd = String.Format("{0}:{1}", "d5db5543-af3c-4eb6-8073-fc0e98195f06", "");
            Byte[] authBytes = Encoding.UTF8.GetBytes(pwd.ToCharArray());

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
            request.comment = result;
            request.bolQueryId = queryID;
            return request;
        }
    }
}