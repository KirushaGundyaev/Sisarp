using System.ServiceProcess;
using ServiceLibrary;

namespace Lab4
{
    public partial class DataManager : ServiceBase
    {
        readonly DataIO appInsights;

        readonly DataOptions dataOptions;

        public DataManager(DataOptions dataOptions, DataIO appInsights)
        {
            InitializeComponent();

            this.dataOptions = dataOptions;

            this.appInsights = appInsights;
        }

        protected override async void OnStart(string[] args)
        {
            DataIO reader = new DataIO(dataOptions.ConnectionString);

            FileTransfer fileTransfer = new FileTransfer(dataOptions.OutputFolder, dataOptions.SourcePath);

            string customersFileName = "customers";

            await reader.GetCustomersAsync(dataOptions.OutputFolder, appInsights, customersFileName);

            await fileTransfer.SendFileToFtpAsync($"{customersFileName}.xml");
            await fileTransfer.SendFileToFtpAsync($"{customersFileName}.xsd");

            await appInsights.InsertInsightAsync("Files were sent to FTP successfully");

            await appInsights.InsertInsightAsync("Service was successfully stopped");

            await appInsights.WriteInsightsToXmlAsync(dataOptions.OutputFolder);
        }

        protected override void OnStop()
        {

        }
    }
}
