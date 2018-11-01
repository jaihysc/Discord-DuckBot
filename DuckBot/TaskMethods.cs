using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot
{
    public class TaskMethods
    {
        public static List<string> ReadFromFileToList(string fileName)
        {
            List<string> returnFileInfoList = new List<string>();

            try
            {
                //Read root path file
                var fileLocations = File.ReadAllLines(MainProgram.rootLocation + @"\Paths.txt");

                //Check path file for specified name of txt file
                //E.G "UserCredits.txt"
                string returnFileLocation = fileLocations.First(p => p.Contains(fileName)).ToString();
                foreach (var item in File.ReadAllLines(returnFileLocation))
                {
                    returnFileInfoList.Add(item);
                }

            }
            catch (Exception)
            {
            }

            return returnFileInfoList;
        }

        public static List<string> ReadFromFilePathToList(string filePath)
        {
            List<string> returnFileInfoList = new List<string>();

            try
            {
                //Check path file for specified name of txt file
                //E.G "UserCredits.txt"
                foreach (var item in File.ReadAllLines(filePath))
                {
                    returnFileInfoList.Add(item);
                }

            }
            catch (Exception)
            {
            }

            return returnFileInfoList;
        }

        public static void WriteListToFile(List<string> listToWrite, bool overwriteExistingContent, string filePath)
        {
            try
            {
                //Overwrite existing contents if true
                if (overwriteExistingContent == true)
                {
                    File.WriteAllText(filePath, "");
                }

                //Write items from list
                foreach (var item in listToWrite)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
                    {
                        file.WriteLine(item);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
}

        public static void WriteStringToFile(string stringToWrite, bool overwriteExistingContent, string filePath)
        {
            try
            {
                //Overwrite existing contents if true
                if (overwriteExistingContent == true)
                {
                    File.WriteAllText(filePath, "");
                }

                //Write string
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
                {
                    file.WriteLine(stringToWrite);
                }

            }
            catch (Exception)
            {
            }
        }

        public static string GetFileLocation(string fileName)
        {
            string returnFileLocation = "";
            try
            {
                //Read root path file
                var fileLocations = File.ReadAllLines(MainProgram.rootLocation + @"\Paths.txt");

                //Check path file for specified name of txt file
                //E.G "UserCredits.txt"
                returnFileLocation = fileLocations.First(p => p.Contains(fileName)).ToString();
            }
            catch (Exception)
            {
            }

            return returnFileLocation;
        }
    }
}
