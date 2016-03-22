using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsService2.Classes
{
    internal class ServiceManager
    {
        private IEnumerable<Task> mReaderTasks;
        private DatabaseWriter mDatabaseWriter;
        private CancellationTokenSource mCancellationToken;

        public ServiceManager()
        {
            this.mReaderTasks = null;
            this.mCancellationToken = null;
            this.mDatabaseWriter = new DatabaseWriter();
        }

        public void Start(ServiceConfig configuration)
        {
            this.mDatabaseWriter.Configure(configuration.SqlConnectionString);
            this.mCancellationToken = new CancellationTokenSource();

            this.mReaderTasks = configuration.Devices
                .Select(device =>
                    Task.Factory.StartNew(
                        (object ct) => this.Read((CancellationToken)ct, device),
                        this.mCancellationToken.Token,
                        this.mCancellationToken.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default))
                .ToArray();
            Task.WaitAll(this.mReaderTasks.ToArray());
        }

        public void Stop()
        {
            try
            {
                this.mCancellationToken.Cancel();
                Task.WaitAll(this.mReaderTasks.ToArray());
            }
            catch(AggregateException)
            {
            }
            catch(Exception)
            {
            }
            finally
            {
                this.mReaderTasks = null;
            }
        }

        private void Read(CancellationToken token, DeviceConfig device)
        {
            Reader reader = new Reader(device.Id, device.PortName);
            reader.Connect();
            reader.ValueReaded += this.Reader_ValueReaded;

            while (!token.IsCancellationRequested)
            {
                reader.Query(token);
            }

            reader.Disconnect();
        }

        private void Reader_ValueReaded(object sender, Reader.ValueReadedEventArgs e)
        {
            this.mDatabaseWriter.SaveValue(e.DeviceId, e.WeightValue);
        }
    }
}
