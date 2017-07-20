using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POC.Cache
{
    #region Cache<T> class
    /// Este é um subsistema de cache genérico baseado em pares de chave/valor, onde a chave também é genérica. A chave deve ser única.
    /// Todo cache entra com um timeout.
    /// O Cache é thread safe e ele vai deletar as entradas por conta própria usando 'using System.Threading.Timers' (é utilizado com <see cref="ThreadPool"/> threads).
    public class Cache<K, T> : IDisposable
    {
        #region Constructor e Class
        public Cache() { }

        private Dictionary<K, T> cache = new Dictionary<K, T>();
        private Dictionary<K, Timer> timers = new Dictionary<K, Timer>();
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        #endregion

        #region IDisposable e Clean
        private bool disposed = false;

        /// Realiza taks definidas pelo aplicativo associadas à liberação, releasing ou resetting de recursos não gerenciados.
		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// Releases não gerenciandos e opcionais 
        /// <param name="disposing">
        /// <c>true</c> para liberar recursos gerenciados e não gerenciados; <c> false </c> para liberar apenas recursos não gerenciados. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                    // Dispose recursos gerenciados
                    Clear();
                    locker.Dispose();
                }
                // Dispose recursos não gerenciados
            }
        }

        /// Limpa todos os caches abertos e temporizadores ativos
        public void Clear()
        {
            locker.EnterWriteLock();
            try
            {
                try
                {
                    foreach (Timer t in timers.Values)
                        t.Dispose();
                }
                catch
                { }

                timers.Clear();
                cache.Clear();
            }
            finally { locker.ExitWriteLock(); }
        }
        #endregion

        #region Verifica Timer
        // Verifica se existe um temporarizador específico, se não existir ele cria um novo.
        private void CheckTimer(K key, int cacheTimeout, bool restartTimerIfExists)
        {
            Timer timer;

            if (timers.TryGetValue(key, out timer))
            {
                if (restartTimerIfExists)
                {
                    timer.Change(
                        (cacheTimeout == Timeout.Infinite ? Timeout.Infinite : cacheTimeout * 1000),
                        Timeout.Infinite);
                }
            }
            else
                timers.Add(
                    key,
                    new Timer(
                        new TimerCallback(RemoveByTimer),
                        key,
                        (cacheTimeout == Timeout.Infinite ? Timeout.Infinite : cacheTimeout * 1000),
                        Timeout.Infinite));
        }

        private void RemoveByTimer(object state)
        {
            Remove((K)state);
        }
        #endregion

        #region AddOrUpdate, Get, Remove, Exists, Clear
        /// Adiciona ou atualiza da cache-key especificada com o cacheObject específico e aplica um timeout específicado (em segundos) a chave.
        /// <param name="key">A cache-key para adicionar ou atualizar.</param>
        /// <param name="cacheObject">CacheObject para armazenar.</param>
        /// <param name="cacheTimeout">O tempo limite de cache (tempo de vida/ lifespan) desse objeto. Deve ser 1 ou maior que zero.
        /// Especificar Timeout.Infinite para manter a entrada para sempre. </ Param> 
        /// <param name="restartTimerIfExists">(Opcional).Se configurado para <c> true </ c>, o temporizador para este cacheObject será reiniciado se o objeto já estiver
                 /// existe no cache. (Padrão = false). </param>
        public void AddOrUpdate(K key, T cacheObject, int cacheTimeout, bool restartTimerIfExists = false)
        {
            if (disposed) return;

            if (cacheTimeout != Timeout.Infinite && cacheTimeout < 1)
                throw new ArgumentOutOfRangeException("cacheTimeout deve ser maior que zero.");

            locker.EnterWriteLock();
            try
            {
                CheckTimer(key, cacheTimeout, restartTimerIfExists);

                if (!cache.ContainsKey(key))
                    cache.Add(key, cacheObject);
                else
                    cache[key] = cacheObject;
            }
            finally { locker.ExitWriteLock(); }
        }

        /// Adiciona ou atualiza a chave de cache especificada com o cacheObject especificado e aplica <c> Timeout.Infinite </ c> a esta chave.
        /// <param name = "key"> A chave do cache para adicionar ou atualizar. </ Param>
        /// <param name = "cacheObject"> O cacheObject para armazenar. </ Param>
        public void AddOrUpdate(K key, T cacheObject)
        {
            AddOrUpdate(key, cacheObject, Timeout.Infinite);
        }

        public T this[K key] => Get(key);

        /// Obtém a entrada de cache com a chave especificada ou retorna <c>default(T)</c> se não for encontrada.
        /// <param name = "key"> A chave de cache para recuperar. </param>
        /// <returns> O objeto do cache ou <c>default(T)</c>, se não for encontrado. </returns>
        public T Get(K key)
        {
            if (disposed) return default(T);

            locker.EnterReadLock();
            try
            {
                T rv;
                return (cache.TryGetValue(key, out rv) ? rv : default(T));
            }
            finally { locker.ExitReadLock(); }
        }

        /// Tenta obter a entrada do cache com a chave especificada.
        /// <param name = "key"> A chave. </ Param>
        /// <param name = "value"> (out) O valor, se encontar, ou <c> default(T)</c> se não. </param>
        /// <retorns> <c>true</c>, se <c>key</c> exitir, se não <c>false</c>. </returns>
        public bool TryGet(K key, out T value)
        {
            if (disposed)
            {
                value = default(T);
                return false;
            }

            locker.EnterReadLock();
            try
            {
                return cache.TryGetValue(key, out value);
            }
            finally { locker.ExitReadLock(); }
        }

        /// Remove uma série de entradas de cache em uma única chamada para todas as chaves que correspondem ao padrão de chave especificado (key pattern).
        /// <param name = "keyPattern"> O padrão de chave para remover. O Predicado deve retornar true para remover a chave. </param>
        public void Remove(Predicate<K> keyPattern)
        {
            if (disposed) return;

            locker.EnterWriteLock();
            try
            {
                var removers = (from k in cache.Keys.Cast<K>()
                                where keyPattern(k)
                                select k).ToList();

                foreach (K workKey in removers)
                {
                    try { timers[workKey].Dispose(); }
                    catch { }
                    timers.Remove(workKey);
                    cache.Remove(workKey);
                }
            }
            finally { locker.ExitWriteLock(); }
        }

        /// Remove o cache de uma chave específica.
        /// <param name="key">A cache-key para remoção.</param>
        public void Remove(K key)
        {
            if (disposed) return;

            locker.EnterWriteLock();
            try
            {
                if (cache.ContainsKey(key))
                {
                    try { timers[key].Dispose(); }
                    catch { }
                    timers.Remove(key);
                    cache.Remove(key);
                }
            }
            finally { locker.ExitWriteLock(); }
        }

        /// Verifica se existe Cache
        public bool Exists(K key)
        {
            if (disposed) return false;

            locker.EnterReadLock();
            try
            {
                return cache.ContainsKey(key);
            }
            finally { locker.ExitReadLock(); }
        }
        #endregion
    }
    #endregion

    #region Outras classes do Cache (derivadas)
    public class Cache<T> : Cache<string, T>
    {
    }
    public class Cache : Cache<string, object>
    {
        #region Static Global Cache instance 
        private static Lazy<Cache> global = new Lazy<Cache>();
        /// Obtém a instância de cache compartilhada global válida para todo o processo.
        /// <value> A instância global do cache compartilhado. </value>
        public static Cache Global => global.Value;
        #endregion
    }
    #endregion
}
