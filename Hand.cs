using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal class Hand
    {
        public List<Card> Visible { get; set; } = new List<Card>();
        public List<Card> Talon { get; set; } = new List<Card>();
        public SuitEnum Trump { get; set; }
        public Dictionary<List<Card>, int> CurrentTricks { get; set; }
        public List<Card> Playable { get; set; } = new List<Card>();
        public Dictionary<SuitEnum, List<Card>> ByColor { get; set; }
        public List<SuitEnum> PotentialBela { get; set; } = new List<SuitEnum>();
        public List<Card> SoloTen { get; set; } = new List<Card>();
        public int HandScore { get; set; } 

        public Hand(List<Card> cards)
        {
            Visible = cards.GetRange(0, 6);
            Talon = cards.GetRange(6, 2);
            SetHandScore();
            SetTrickScore();
        }

        internal void SetPlayable(Card currentWinning, SuitEnum trump, Card firstPlayed)
        {
            Rules.GetPlayable(this, trump, currentWinning, firstPlayed);
        }

        public void Print()
        {
            foreach (Card card in Visible)
                Console.Write(card.Name + " ");

            if (Talon.Count != 0)
                foreach (Card talonCard in Talon)
                    Console.Write("X ");

            Console.WriteLine();
        }
        internal void UncoverTalon()
        {
            Visible.AddRange(Talon);
            Talon.Clear();
        }

        private void SetTrickScore()
        {
            ByColor = new Dictionary<SuitEnum, List<Card>>();
            foreach (Card card in Visible)
            {
                SuitEnum color = card.Suit;
                if (ByColor.ContainsKey(color))
                    ByColor[color].Add(card);
                else
                    ByColor.Add(color, new List<Card> { card });
            }
        }

        private void SetHandScore()
        {
            int score = 0;
            foreach (Card card in Visible)
                score += card.Value;
            HandScore = score;
        }

        public void Sort()
        {
            Visible = Visible.OrderBy(x => x.Suit)
                .ThenBy(x => x.Value)
                .ThenBy(x => x.Name)
                .ToList();
        }
    }
}
