using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace SmartMeter_P1
{
    class mySettings
    {
        public string settingsFile = "";
        
        string[] words;

        //classes
        myFunctions func = new myFunctions();


        public string getSetting(string name)
        {
            string value = "";

            try
            {
                TextReader reader = new StreamReader(settingsFile);

                while (reader.Peek() >= 0)
                {
                    string Line = reader.ReadLine();

                    if (Line != "")
                    {
                        words = Line.Split('=');

                        if (words.Length > 1)
                        {
                            if (words[0].Trim() == name)
                            {
                                value = words[1].Trim();
                            }
                        }

                    }
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                func.logItem("mySettings.getSetting : " + error + "\n");
            }

            

            return value;
        }


        public void setSetting(string name, string value)
        {
            string Settings="";
            string par = "";
            string val = "";
            string writeString = "";

            try
            {
                TextReader reader = new StreamReader(settingsFile);

                while (reader.Peek() >= 0)
                {
                    Settings = reader.ReadToEnd();
                }

                reader.Close();



                //nog goed maken!
                TextWriter writer = new StreamWriter(settingsFile);

                string sLine = Settings.Replace('\r', '\n');
                string[] lines = System.Text.RegularExpressions.Regex.Split(sLine, "\n");

                foreach (string line in lines)
                {
                    string myLine = line.Replace('\r', ' ');

                    words = myLine.Split('=');

                    if (myLine != "")
                    {
                        if (words.Length > 1)
                        {
                            par = words[0].Trim();
                            val = words[1].Trim();

                            if (par == name)
                            {
                                //new value
                                writeString = par + " = " + value;
                            }
                            else
                            {
                                //old value
                                writeString = par + " = " + val;
                            }
                        }
                        else
                        {
                            writeString = myLine.Trim();
                        }

                        writer.Write(writeString + '\n');
                    }
                }

                writer.Close();
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                func.logItem("mySettings.setSetting : " + error + "\n");
            }
        }
    }
}
