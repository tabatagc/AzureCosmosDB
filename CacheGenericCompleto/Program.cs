using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POC.Cache;
using System.Diagnostics;
using System.Threading;

namespace CacheGenericCompleto
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Cache<T> Teste");
            Console.WriteLine("===================\r\n");

            Console.WriteLine("1: Teste de Threaded Simples");
            Console.WriteLine("1.1: Adicionando 1 Milhão de entradas.");
            Console.WriteLine("     <K,T> = <long, string*32>");

            Cache<long, string> c = new Cache<long, string>();

            Stopwatch w = new Stopwatch();
            w.Start();
            for (long i = 0; i < 1000000; i++)
                c.AddOrUpdate(i, i.ToString("D32"), 100);
            w.Stop();

            Console.WriteLine($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms\r\n-");

            Console.WriteLine("1.2: 1 Milhão chamadas (Get) Random");
            Random r = new Random();
            string val = "";
            w.Restart();
            for (long i = 0; i < 1000000; i++)
                val = c.Get(r.Next(0, 1000000));
            w.Stop();

            Console.WriteLine($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms\r\n-");

            Console.WriteLine("1.3: Removendo 1 Milhão de entradas (verifica se existe)");
            w.Restart();
            for (long i = 0; i < 1000000; i++)
                if (c.Exists(i)) c.Remove(i);
            w.Stop();

            Console.WriteLine($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms\r\n-");

            Console.WriteLine("2: Multi threaded Teste");
            Console.WriteLine("2.1: Adicionando 1 Milhão de entrada.");
            Console.WriteLine("     Threads: A cada 100 add 10k");
            Console.WriteLine("     <K,T> = <long, string*32>");

            ManualResetEvent phase1 = new ManualResetEvent(false);
            ManualResetEvent phase2 = new ManualResetEvent(false);
            ManualResetEvent phase3 = new ManualResetEvent(false);
            Thread[] t = new Thread[100];
            int finished = 0;
            for (long threadid = 0; threadid < 100; threadid++)
            {
                t[threadid] = new Thread((num) =>
                {
                    long basenum = (long)num * 10000;
                    phase1.WaitOne();
                    for (long i = basenum; i < basenum + 10000; i++)
                        c.AddOrUpdate(i, i.ToString("D32"), 100);

                    Interlocked.Increment(ref finished);

                    phase2.WaitOne();

                    Random rnd = new Random();
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

            Console.WriteLine($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms\r\n-");

            Console.WriteLine("2.2: Adicionando 1 Milhão de entrada Random");
            Console.WriteLine("     Threads: A cada 100 add 10k");
            Console.WriteLine("     <K,T> = <long, string*32>");

            finished = 0;
            w.Restart();
            phase2.Set();
            while (finished < 100) Thread.Sleep(0);
            w.Stop();

            Console.WriteLine($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms\r\n-");

            Console.WriteLine("2.3: Removendo 1 Milhão de entradas (verifica se existe)");
            Console.WriteLine("     Threads: A cada 100 add 10k");
            Console.WriteLine("     <K,T> = <long, string*32>");

            finished = 0;
            w.Restart();
            phase3.Set();
            while (finished < 100) Thread.Sleep(0);
            w.Stop();

            Console.WriteLine($"     Performance: {1000000 / w.ElapsedMilliseconds}/ms\r\n-");


            Console.WriteLine("\r\n --- Fim da Poc (Testando Cache) ---");
            Console.ReadKey();
        }
    }
}
