using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace PebbleFirmwareBackup
{
    class Program
    {
        public static readonly string[] RELEASE_TYPES = new string[] { "nightly","beta","release","release-v2","release-v3" };
        public static readonly string[] RELEASE_HARDWARE = new string[] { "unknown", "ev1", "ev2", "ev2_3", "ev2_4", "bigboard", "v1_5", "v2_0", "snowy_evt2", "snowy_dvt", "snowy_bb", "snowy_bb2" };

        public static string savePath = @"E:\FullPebbleArchives\6-29-18\firmwares\";

        static void Main(string[] args)
        {
            for (int x = 0; x < RELEASE_HARDWARE.Length; x++)
            {
                for (int y = 0; y < RELEASE_TYPES.Length; y++)
                {
                    Console.Write("(" + x.ToString() + " / " + RELEASE_HARDWARE.Length.ToString() + ") (" + y.ToString() + " / " + RELEASE_TYPES.Length.ToString() + ") ");
                    FetchFirmware(RELEASE_HARDWARE[x], RELEASE_TYPES[y]);
                }
            }
            Console.ReadLine();
        }

        static void FetchFirmware(string hardware, string type)
        {
            //First, fetch the firmware data.
            Console.Write("Fetching firmware data for " + hardware + "/" + type + "......");
            try
            {
                string fw_data_raw = DoGetRequest("http://pebblefw.s3.amazonaws.com/pebble/" + hardware + "/" + type + "/latest.json");
                //Deserialize
                try
                {
                    JsonContents fwData = (JsonContents)DeserializeObject(fw_data_raw, typeof(JsonContents));
                    string path = savePath + hardware + "\\" + type + "\\";
                    Directory.CreateDirectory(path);
                    File.WriteAllText(path + "latest.json", fw_data_raw);
                    //Now, download both of these. 
                    Console.Write("Done!\r\n");
                    
                    if (fwData.normal != null)
                    {
                        DownloadFile(fwData.normal.url, path + "normal.pbz", "normal firmware");
                        if (fwData.normal.layouts!=null)
                        {
                            DownloadFile(fwData.normal.layouts, path + "layouts.json", "layouts");
                        }
                    }
                    else
                    {
                        Console.Write("            Normal firmware didn't exist in the data.\r\n");
                    }
                    if (fwData.recovery!=null)
                    {
                        DownloadFile(fwData.recovery.url, path + "recovery.pbz", "recovery firmware");
                    } else
                    {
                        Console.Write("            Recovery firmware didn't exist in the data.\r\n");
                    }

                    
                    
                } catch (Exception ex)
                {
                    Console.Write("Failed! " + ex.Message + " @ "+ex.StackTrace+ "\r\n");
                }
            } catch
            {
                //There was an error.
                Console.Write("Didn't exist.\r\n");
            }
            Console.Write("\r\n");
        }

        public static void DownloadFile(string url, string filename, string displayName)
        {
            //Download
            Console.Write("            Downloading " + displayName + ".....");
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, filename);
                }
                Console.Write("Done!\r\n");
            } catch (Exception ex)
            {
                Console.Write("Failed! " + ex.Message + "\r\n");
            }
        }

        public static string DoGetRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            string data = "";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                data = reader.ReadToEnd();
            }
            return data;
        }

        public static object DeserializeObject(string value, Type objType)
        {
            try
            {
                //Get a data stream
                MemoryStream mainStream = GenerateStreamFromString(value);

                DataContractJsonSerializer ser = new DataContractJsonSerializer(objType);
                //Load it in.
                mainStream.Position = 0;
                var obj = ser.ReadObject(mainStream);
                return Convert.ChangeType(obj, objType);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        public static string SerializeObject(object obj)
        {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
            ser.WriteObject(stream1, obj);
            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            return sr.ReadToEnd();
        }
    }
}
