namespace QA.DPC.PDFServer.Services.Interfaces
{
    public class CustomerCodeConfiguration
    {
        public string Name { get; set; }
        public DatabaseType DbType { get; set; }
        public string ConnectionString { get; set; }
    }
}