using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace CacheGeneric
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /*
            Absolute Expiration
            -> O cache expira após um certo período de tempo a partir do momento da ativação.
            -> Não leva em conta se o pedido de cache foi feito ou não durante esse período de tempo.
            -> Esse tipo de expiração é útil quando não há alterações freqüentes em dados em cache.
            */
            string strCache = CacheGeneric.GetItemToCache<string>("String", "Poc Cache Teste!", DateTime.Now.AddSeconds(20));
            DateTime dtCache = CacheGeneric.GetItemToCache<DateTime>("DT", DateTime.Now, DateTime.Now.AddSeconds(20));

            Response.Write("Absolute Expiration<br/>");
            Response.Write(strCache + dtCache.ToString() + "<br/>");


            /*
            Sliding Expiration
            -> O cache expirou após um certo período de tempo a partir do momento da ativação do cache.
            -> O tempo de expiração aumenta se o cache for acessado. 
            -> Esse tipo de expiração é útil quando não há alterações freqüentes em dados em cache.
            */

            string strCache1 = CacheGeneric.GetItemToCache<string>("String1", "Poc Cache Teste!", TimeSpan.FromSeconds(20));
            DateTime dtCache1 = CacheGeneric.GetItemToCache<DateTime>("DT1", DateTime.Now, TimeSpan.FromSeconds(20));

            Response.Write("Sliding Expiration<br/>");
            Response.Write(strCache1 + dtCache1.ToString() + "<br/>");
        }
    }
}