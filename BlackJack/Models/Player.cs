using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack.Models
{

    public class Player
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public int RiskAversion { get; set; }
        public bool IsDealer { get; set; }
        public List<Card> Cards { get; set; }
        public int Status { get; set; }

        public Player()
        {
            Cards = new List<Card>();
        }
    }
}
