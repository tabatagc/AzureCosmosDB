using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CargaApi.Models;

namespace CargaApi.Controllers
{
    public class ItemController : Controller
    {
        
        // GET: Item
        public ActionResult Index()
        {
            Item item = new Item();
            return View(item);
        }

        [HttpPost]
        public ActionResult Index(Item item)
        {
            if(item.Altura > 0.0)
                ViewBag.IMC = (item.Peso / (item.Altura * item.Altura)).ToString();
            
            return View(item);
        }
    }
}
