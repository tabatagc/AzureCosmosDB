using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace CargaServiceTest
{
    public partial class ServicePOC : ServiceBase
    {
        public ServicePOC()
        {
            InitializeComponent();
        }
        Timer timer;
        protected override void OnStart(string[] args)
        {
            timer = new Timer(new TimerCallback(timer_Tick), null, 15000, 60000);
        }

        protected override void OnStop()
        {
            StreamWriter vWriter = new StreamWriter(@"c:\testeServico.txt", true);

            vWriter.WriteLine("Servico Parado: " + DateTime.Now.ToString());
            vWriter.Flush();
            vWriter.Close();
        }

        private void timer_Tick(object sender)
        {
            StreamWriter vWriter = new StreamWriter(@"c:\testeServico.txt", true);
            vWriter.WriteLine("Servico Rodando: " + DateTime.Now.ToString());
            vWriter.Flush();
            vWriter.Close();
        }



    }
}
