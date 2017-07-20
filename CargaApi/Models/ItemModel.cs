using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargaApi.Models
{
    public class Item
    {
        public Item()
        {
            this.Id = 1;
            this.Nome = "Maria da Silva";
            this.Idade = 18;
            this.Peso = 20.0;
            this.Altura = 1.58;
        }
        public int Id { get; set; }
        public string Nome { get; set; }
        public int Idade { get; set; }
        public double Peso { get; set; }
        public double Altura { get; set; }
    }
}