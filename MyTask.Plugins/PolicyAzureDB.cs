using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace MyTask.Plugins
{
    public class PolicyAzureDB
    {
        public SqlConnection PolicyAzureDb()
        {
            //Server=tcp:bharathgroup.database.windows.net,1433;Initial Catalog=Policy Table;Persist Security Info=False;User ID=sonupaul;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
                SqlConnectionStringBuilder connstring = new SqlConnectionStringBuilder();
                connstring.DataSource = "bharathgroup.database.windows.net";
                connstring.UserID = "sonupaul";
                connstring.Password = "hitachi007#";
                connstring.InitialCatalog = "Policy Table";

                SqlConnection connection = new SqlConnection(connstring.ConnectionString);
                return connection;
           
        }



    } 
}
