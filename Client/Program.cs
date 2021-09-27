using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static readonly HashAlgorithm hash = new SHA1CryptoServiceProvider();
        static void Main(string[] args)
        {
            TcpClient clientSocket = new TcpClient("localhost", 6789);

            Stream ns = clientSocket.GetStream();  //provides a NetworkStream
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);
            sw.AutoFlush = true; // enable automatic flushing
            //read user inputs
            Dictionary<string, string> userInformation = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadLine());
            List<string> dictionaryWords = JsonConvert.DeserializeObject<List<string>>(sr.ReadLine());

            string match = dictionaryWords.Where((w) => CheckWithVariations(userInformation, w)).FirstOrDefault();
            if (string.IsNullOrEmpty(match))
            {
                Console.WriteLine("There was no match!");
                Console.ReadLine();
            } else
            {
                Console.WriteLine("The match is " + match);
                Console.ReadLine();
            }
            ns.Close();
            clientSocket.Close();
        }

        private static bool CheckWithVariations(Dictionary<string, string> userInformation, string w)
        {
            List<string> words = new List<string>();
            words.Add(w);
            words.Add(w.ToUpper());
            words.Add(w.ToLower());
            words.Add(StringUtilities.Capitalize(w));
            words.Add(StringUtilities.Reverse(w));

            for (int i = 0; i < 100; i++)
            {
                words.Add(w + i);
                words.Add(i + w);
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    words.Add(i + w + j);
                }
            }
            return words.Any((f) => CheckWord(f, userInformation));
        }

        private static bool CheckWord(string f, Dictionary<string, string> userInformation)
        {
            byte[] wordBytes = Encoding.ASCII.GetBytes(f);
            wordBytes = hash.ComputeHash(wordBytes);
            foreach (var item in userInformation)
            {
                byte[] passwordBytes = Encoding.ASCII.GetBytes(item.Value);
                if (CompareBytes(wordBytes,passwordBytes))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool CompareBytes(IList<byte> firstArray, IList<byte> secondArray)
        {
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("firstArray");
            //}
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("secondArray");
            //}
            if (firstArray.Count != secondArray.Count)
            {
                return false;
            }
            for (int i = 0; i < firstArray.Count; i++)
            {
                if (firstArray[i] != secondArray[i])
                    return false;
            }
            return true;
        }
    }
}
