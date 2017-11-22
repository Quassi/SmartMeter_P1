using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

using System.Text.RegularExpressions;

namespace SmartMeter_P1
{
    class myFunctions
    {
        public string AppPath()
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().FullName);
            return path;
        }


        public DateTime stringToDateTime(string datetime)
        {
            //http://www.codeproject.com/KB/cs/String2DateTime.aspx
            DateTime dt;

            dt = new DateTime();

            try
            {
                if (datetime != "")
                {
                    dt = DateTime.ParseExact(datetime, "yyyy-MM-dd HH:mm:ss", null);
                }
                else
                {
                    dt = DateTime.ParseExact("2000-01-01 00:00:00", "yyyy-MM-dd HH:mm:ss", null);
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                logItem(error + "\n");
            }

            return dt;
        }


        public bool IsNumeric(string strTextEntry)
        {
            Regex objNotWholePattern = new Regex("[^0-9]");
            return !objNotWholePattern.IsMatch(strTextEntry);
        }

        public void sendEmail(String strFromAddress, string strToAddress, string strSubject, string strBody, string strSMTPserver)
        {
            try
            {
                logItem(strToAddress + "\t" + strSubject + "\t" + strBody + "\n");

                if (strSMTPserver == "")
                {
                    strSMTPserver = "mail.kpnmail.nl";
                }

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(strFromAddress);
                mailMessage.To.Add(new MailAddress(strToAddress));
                mailMessage.Subject = strSubject;
                mailMessage.Body = strBody;
                mailMessage.IsBodyHtml = true;

                SmtpClient mailSmtpClient = new SmtpClient(strSMTPserver);

                // mail sent
                mailSmtpClient.Send(mailMessage);

                System.Threading.Thread.Sleep(500);  //do not send emails so rapidly

            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                logItem(error + "\t" + strToAddress + "\t" + strSubject + "\t" + strBody + "\t" + strSubject + "\n");
            }
        }

        public void sendSMS(String strFromNumber, string strToNumber, string strSubject)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                logItem(error + "\n");
            }
        }

        public bool AllCapitals(string inputString)
        {
            return Regex.IsMatch(inputString, @"^[A-Z]+$");
        }


        public void logItem(string msg)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText("log.txt");
            try
            {
                string logLine = System.String.Format("{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            catch (Exception ex)
            {
                string error;
                error = ex.ToString();
            }
            finally
            {
                sw.Close();
            }
        }


        public string removeTelephoneNumber(string message)
        {
            try
            {
                string[] patterns1 = new string[10];
                string[] patterns2 = new string[10];

                patterns1[0] = @"\b\d{10}";       //0612345678 or 0306571929 or 0572331454
                patterns1[1] = @"\b\d{2}-\d{8}";  //06-12345678
                patterns1[2] = @"\b\d{3}-\d{7}";  //030-6571929
                patterns1[3] = @"\b\d{4}-\d{6}";  //0572-331454

                //[1] search word by word, split message up into words
                string[] words = Regex.Split(message, @"\s");
                foreach (string word in words)
                {
                    //pattern for telephone number
                    Regex re;

                    foreach (string patt in patterns1)
                    {
                        if (patt != null)
                        {
                            re = new Regex(patt);
                            Match m = re.Match(word);
                            if (re.IsMatch(word))
                            {
                                //MessageBox.Show("TELEPHONE NUMBER: " + m.Value);
                                string number = m.Value;

                                message = message.Replace(number, "***-*******");
                            }
                        }
                    }
                }


                patterns2[0] = @"\b\d{2} \d{8}";  //06 12345678
                patterns2[1] = @"\b\d{3} \d{7}";  //030 6571929
                patterns2[2] = @"\b\d{4} \d{6}";  //0572 331454
                //patterns2[3] = @"\bbel \d{5}";  //bel 12345

                //[2] search now in whole message
                //pattern for telephone number
                Regex re2;

                foreach (string patt in patterns2)
                {
                    if (patt != null)
                    {
                        re2 = new Regex(patt);
                        Match m = re2.Match(message);
                        if (re2.IsMatch(message))
                        {
                            string number = m.Value;

                            message = message.Replace(number, "***-*******");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                logItem(error + "\n");
            }

            return message;
        }
    }
}
