using CachesGenericoCompleto.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CachesGenericoCompleto.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            Response.Write(":::: Cache<T> Teste :::: <br /><br />");

            #region Teste de Threaded Simples

                Response.Write("1: Teste de Threaded Simples<br />");
                
                #region Entradas
                Response.Write("1.1: Adicionando 1 Milhão de entradas.<br />");
                Cache<long, string> c = new Cache<long, string>();

                //Adicionando entraadas
                Stopwatch w = new Stopwatch();
                w.Start();
                for (long i = 0; i < 1000000; i++)
                    c.AddOrUpdate(i, i.ToString("D32"), 100);
                w.Stop();

                Response.Write($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms <br /><br />");
                #endregion
               
                #region Entradas Random
                Response.Write("1.2: 1 Milhão chamadas (Get) Random");
                Random r = new Random();
                string val = "";

                //Gerando entradas Random
                w.Restart();
                for (long i = 0; i < 1000000; i++)
                    val = c.Get(r.Next(0, 1000000));
                w.Stop();

                Response.Write($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms <br /><br />");

                #endregion
                
                #region Removendo Entradas
                Response.Write("1.3: Removendo 1 Milhão de entradas (verifica se existe)<br />");

                //Removendo entradas se existe
                w.Restart();
                for (long i = 0; i < 1000000; i++)
                    if (c.Exists(i)) c.Remove(i);
                w.Stop();

                Response.Write($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms <br /><br />");

                #endregion

            #endregion

            #region Teste com Multi-Threaded

                Response.Write("2: Multi threaded Teste<br />");
                Response.Write("2.1: Adicionando 1 Milhão de entrada.<br />");
                Response.Write("     Threads: A cada 100 add 10k<br />");

                ManualResetEvent phase1 = new ManualResetEvent(false);
                ManualResetEvent phase2 = new ManualResetEvent(false);
                ManualResetEvent phase3 = new ManualResetEvent(false);

                Thread[] t = new Thread[100];//Criação das threads
                int finished = 0;

                #region Entradas Multi-Threaded
                //Add cache com 100 thread parelelo
                for (long threadid = 0; threadid < 100; threadid++)
                {
                    t[threadid] = new Thread((num) =>
                    {
                        long basenum = (long)num * 10000;
                        phase1.WaitOne();
                        for (long i = basenum; i < basenum + 10000; i++)
                            c.AddOrUpdate(i, i.ToString("D32"), 100);//Adiciona ou edita passando chave e time

                        Interlocked.Increment(ref finished);

                        phase2.WaitOne();

                        Random rnd = new Random(); //Gera valor aleatório para o teste
                        for (long i = 0; i < 10000; i++)
                            val = c.Get(rnd.Next(0, 1000000));

                        Interlocked.Increment(ref finished);

                        phase3.WaitOne();
                        for (long i = basenum; i < basenum + 10000; i++)
                            if (c.Exists(i)) c.Remove(i);

                        Interlocked.Increment(ref finished);

                    });
                    t[threadid].Start(threadid);
                }

                w.Restart();
                phase1.Set();
                while (finished < 100) Thread.Sleep(0);
                    w.Stop();
                Response.Write($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms<br /><br />");
                #endregion

                #region Entradas Multi-Threaded Random
                Response.Write("2.2: Adicionando 1 Milhão de entrada Random<br />");
                Response.Write("     Threads: A cada 100 add 10k<br />");

                finished = 0;
                w.Restart();
                phase2.Set();
                while (finished < 100) Thread.Sleep(0);
                w.Stop();

                Response.Write($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms<br /><br />");
                #endregion

                #region Removendo Multi-Threaded 
                Response.Write("2.3: Removendo 1 Milhão de entradas (verifica se existe)");
                Response.Write("     Threads: A cada 100 add 10k");

                finished = 0;
                w.Restart();
                phase3.Set();
                while (finished < 100) Thread.Sleep(0);
                       w.Stop();

                Response.Write($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms<br /><br />");
                #endregion

            #endregion

            Response.Write("<br /> :::: Fim da Poc (Testando Cache) ::::<br /> ");
            Response.Write("<br />:::: 3 milisegundos por adição num cenário de 100 thread por segundo ::::<br />");

            return View();
        }

    }
}