using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack.Models
{
    public class Dealer
    {
        public int Points { get; set; }
        public int RiskAversion = 4;
        public List<Card> Cards { get; set; }

        public Dealer()
        {
            Cards = new List<Card>();
        }
    }
}
