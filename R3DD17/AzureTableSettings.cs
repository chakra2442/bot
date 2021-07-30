namespace R3DD17
{
    public class AzureTableSettings
    {
        public AzureTableSettings(string connectionString, string tableName)
        {
            TableName = tableName;
            ConnectionString = connectionString;
        }
        public string TableName
        {
            get;
        }
        public string ConnectionString
        {
            get;
            set;
        }
    }
}