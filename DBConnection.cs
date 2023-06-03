using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace data_bosch
{
    static class DBConnection
    {
        public static OracleConnection GetDBConnection()
        {
            string host = "10.100.10.80";
            int port = 1521;
            string sid = "OEE";
            string user = "TRACE";
            string password = "trace##2017";

            // Connection String để kết nối trực tiếp tới Oracle.
            string connString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = "
                 + host + ")(PORT = " + port + "))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = "
                 + sid + ")));Password=" + password + ";User ID=" + user;


            OracleConnection conn = new OracleConnection();

            conn.ConnectionString = connString;

            return conn;
        }
    }
}
