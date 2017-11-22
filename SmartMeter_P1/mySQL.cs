using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;
using MySql.Data;
using System.Data;



namespace SmartMeter_P1
{
    class mySQL
    {
        public string server;
        public string uid;
        public string pwd;
        public string database;

        //classes
        myFunctions func = new myFunctions();
        public MySql.Data.MySqlClient.MySqlConnection conn;
        MySql.Data.MySqlClient.MySqlCommand cmd;
        MySql.Data.MySqlClient.MySqlDataAdapter myAdapter;

        int iNrOfConnections = 0;

        public Boolean connect()
        {
            Boolean result = false;
            
            string myConnectionString;
            string casemessage = "";

            myConnectionString = "server=" + server + ";uid=" + uid + ";pwd=" + pwd + ";database=" + database + ";Pooling=false;"; //;Connection Lifetime=60;

            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = myConnectionString;
                conn.Open();

                iNrOfConnections++;

                result = true;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        casemessage = "Cannot connect to server.  Contact administrator";
                        break;
                    case 1045:
                        casemessage = "Invalid username/password, please try again";
                        break;
                    default:
                        casemessage = ex.Message;
                        break;
                }

                string error = ex.Message.ToString();
                func.logItem("mySQL.connect : " + error + "\t" + casemessage + "\n");
            }
            
            return result;
        }

        public void close()
        {
            try
            {
                conn.Close();

                iNrOfConnections--;

                if (iNrOfConnections > 1)
                {
                    iNrOfConnections = iNrOfConnections;
                }

            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                func.logItem("mySQL.close : " + error + "\n");
            }
        }

        public void query()
        {
            try
            {
                cmd = new MySql.Data.MySqlClient.MySqlCommand();
                myAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter();

                DataSet myData = new DataSet();

                cmd.CommandText = "SELECT version.datum AS date, version.versie AS version, version.opmerking AS remark FROM version";
                cmd.Connection = conn;

                myAdapter.SelectCommand = cmd;
                myAdapter.Fill(myData);

                myData.WriteXml("D:\\dataset.xml", XmlWriteMode.WriteSchema);
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                func.logItem("mySQL.query : " + error + "\n");
            }
        }


        //
        public void insertMeasurementLIVE(string p1_meterreading_in_1, string p1_meterreading_in_2, string p1_current_power_in, string p1_current_tariff, string p1_channel_1_meterreading)
        {
            string Query = "";
            Boolean connected = false;

            cmd = new MySql.Data.MySqlClient.MySqlCommand();
            myAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter();

            try
            {
                //open the mySQL connection
                connected = connect();

                if (connected)
                {
                    //update the data
                    Query = "UPDATE p1_live SET p1_meterreading_in_1=" + p1_meterreading_in_1.Replace(",", ".") + ", p1_meterreading_in_2=" + p1_meterreading_in_2.Replace(",", ".") + ", p1_current_power_in=" + p1_current_power_in + ", p1_current_tariff=" + p1_current_tariff + ", p1_channel_1_meterreading=" + p1_channel_1_meterreading.Replace(",", ".") + " WHERE sample_nr=1";

                    MySqlCommand insertMeasurement = new MySqlCommand(Query, conn);
                    try
                    {
                        insertMeasurement.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message.ToString();
                        func.logItem("mySQL.insertMeasurementLIVE(1) : " + error + "\n");
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                func.logItem("mySQL.insertMeasurementLIVE : " + error + "\n");
            }
            finally
            {
                if (connected)
                {
                    //close the mySQL connection
                    close();
                }
            }
        }


        //
        public void insertMeasurement(string p1_meterreading_in_1, string p1_meterreading_in_2, string p1_current_power_in, string p1_current_tariff, string p1_channel_1_meterreading)
        {
            string Query = "";
            Boolean connected = false;

            cmd = new MySql.Data.MySqlClient.MySqlCommand();
            myAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter();

            try
            {
                //open the mySQL connection
                connected = connect();

                if (connected)
                {
                    //INSERT INTO p1_log ('p1_meterreading_in_1', 'p1_meterreading_in_2', 'p1_current_tariff', 'p1_channel_1_meterreading') VALUES ('', '', '', '', '');
                    string values = " VALUES (" + p1_meterreading_in_1.Replace(",", ".") + ", " + p1_meterreading_in_2.Replace(",", ".") + ", " + p1_current_power_in.Replace(",", ".") + ", " + p1_current_tariff + ", " + p1_channel_1_meterreading.Replace(",", ".") + ");";

                    //insert the data
                    Query = "INSERT INTO p1_log (p1_meterreading_in_1, p1_meterreading_in_2, p1_current_power_in, p1_current_tariff, p1_channel_1_meterreading)" + values;

                    MySqlCommand insertMeasurement = new MySqlCommand(Query, conn);
                    try
                    {
                        insertMeasurement.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message.ToString();
                        func.logItem("mySQL.insertMeasurement(1) : " + error + "\n");
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                func.logItem("mySQL.insertMeasurement : " + error + "\n");
            }
            finally
            {
                if (connected)
                {
                    //close the mySQL connection
                    close();
                }
            }
        }





    
    }

    


}
