using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace KarlsonMP
{
    class DataSave
    {
        public static string Load()
        {
            if (PlayerPrefs.HasKey("KarlsonMP_ipHistory"))
            {
                IpHistory = Deserialize<string[]>(PlayerPrefs.GetString("KarlsonMP_ipHistory")).ToList();
                return PlayerPrefs.GetString("KarlsonMP_username");
            }
            return "";
        }
        public static List<string> IpHistory { get; private set; } = new List<string>();

        public static void Save(string username)
        {
            PlayerPrefs.SetString("KarlsonMP_ipHistory", Serialize(IpHistory.ToArray()));
            PlayerPrefs.SetString("KarlsonMP_username", username);
        }

        public static void AddToList(string ip)
        {
            if (IpHistory.Contains(ip))
                IpHistory.Remove(ip); // makes sure that this ip is on top
            IpHistory.Add(ip);
        }

        /*
         * The ip's are display from last to first, to ensure that the last added ip to the
         * list, is the first show in the ip history popup
         */

        public static void MoveUp(int index) // this moves the ip one index higher
        {
            if (index >= IpHistory.Count - 1)
                return; // top most
            string toMove = IpHistory[index];
            IpHistory.Remove(toMove);
            IpHistory.Insert(index + 1, toMove);
        }

        public static void MoveDown(int index) // this moves the ip one index lower
        {
            if (index <= 0)
                return; // lower most
            string toMove = IpHistory[index - 1];
            IpHistory.Remove(toMove);
            IpHistory.Insert(index, toMove); // move the ip below one slot higher
        }

        public static void Remove(int index)
        {
            IpHistory.Remove(IpHistory[index]);
        }

        private static T Deserialize<T>(string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader textReader = new StringReader(toDeserialize);
            return (T)((object)xmlSerializer.Deserialize(textReader));
        }

        private static string Serialize<T>(T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringWriter stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, toSerialize);
            return stringWriter.ToString();
        }
    }
}
