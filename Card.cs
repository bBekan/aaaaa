using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal class Card
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public SuitEnum Suit { get; set; }
        public bool IsTrump {get;set;} = false;
        public int InitialValue { get; set; }
        public int MaxNumberOfBetterCards { get; set; }
        public Player PlayedBy { get; set; } = null;

        public Card(SuitEnum _suit, int _value)
        {
            Suit = _suit;
            InitialValue = _value;

            if (_value > 10 && _value < 14)
                Value = _value % 10 + 1;

            if (Value == 2)
                Name = "J" + Suit;
            else if (Value == 3)
                Name = "Q" + Suit;
            else if (Value == 4)
                Name = "K" + Suit;
            else if (_value == 14)
            {
                Name = "A" + Suit;
                Value = 11;
            }
            else if(_value < 10)
            {
                Name = _value + Suit.ToString();
                Value = 0;
            }
            else
            {
                Name = _value + Suit.ToString();
                Value = 10;
            }
        }

        public void SetTrump()
        {
            if (IsTrump)
            {
                if (Name.Contains("J"))
                    Value = 2;
                else if (Name.Contains("9"))
                    Value = 0;
                IsTrump = false;
                return;
            }

            if (Name.Contains("J"))
                Value = 20;
            else if (Name.Contains("9"))
                Value = 14;
            IsTrump = true;
        }

        //Implementiraj
        public int BetterCardsRemaining(List<Card> played, List<Card> hand)
        {
            return 0;
        }
    }
}
