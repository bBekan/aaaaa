using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal static class Declarations
    {
        public static Hand Hand { get; set; }
        public static SuitEnum Trump { get; set; }
        public static bool HasBela { get; set; } = false;
        public static Dictionary<List<Card>, int> CurrentDeclarations { get; set; }
        public static void GenerateDeclarations(Hand _hand, SuitEnum _trump)
        {
            CurrentDeclarations = new Dictionary<List<Card>, int>();

            _hand.Visible = _hand.Visible.OrderBy(x => Deck.NaturalOrder.IndexOf(x)).ToList();
            Hand = _hand;
            Trump = _trump;

            Belot();
            FourSame();
            SuitStreak();
            Bela();

            Hand.CurrentTricks = CurrentDeclarations
                .OrderBy(x => x.Value)
                .Reverse()
                .ToDictionary(x => x.Key, x => x.Value);

            CurrentDeclarations.Clear();
        }
        public static void GeneratePreTrumpSelection(Hand _hand)
        {
            _hand.Visible = _hand.Visible.OrderBy(x => Deck.NaturalOrder.IndexOf(x)).ToList();
            Hand = _hand;
            CurrentDeclarations = new Dictionary<List<Card>, int>();

            FourSame();
            CheckBela();

            Hand.CurrentTricks = CurrentDeclarations
    .OrderBy(x => x.Value)
    .Reverse()
    .ToDictionary(x => x.Key, x => x.Value);

            CurrentDeclarations.Clear();
        }
        private static void CheckBela()
        {
            List<SuitEnum> bela = new List<SuitEnum>();

            foreach (var suit in Enum.GetValues(typeof(SuitEnum)))
            {
                int belaCount = Hand.Visible.Where(x => x.Suit.Equals(suit) && (x.Name.Contains("J") || x.Name.Contains("Q"))).Count();
                if (belaCount == 2)
                    bela.Add((SuitEnum)suit);
            }
            foreach (var suit in bela)
                Hand.PotentialBela.Add(suit);
        }

        private static void Bela()
        {
            List<Card> bela = new List<Card>();

            foreach (Card card in Hand.Visible)
            {
                if (card.Name.Equals("Q" + Trump.ToString()) || card.Name.Equals("K" + Trump.ToString()))
                    bela.Add(card);
            }
            if (bela.Count == 2)
                HasBela = true;
        }
        private static void SuitStreak()
        {
            int i = 0;
            while (i < 7)
            {
                int streak = 1;
                List<Card> trick = new List<Card>();
                Card current = Hand.Visible[i];
                Card next = Hand.Visible[i + 1];
                int indexOfCurrent = Deck.NaturalOrder.IndexOf(current);
                int indexOfNext = Deck.NaturalOrder.IndexOf(next);
                i++;

                trick.Add(current);
                while (indexOfNext == indexOfCurrent + 1 && current.Suit.Equals(next.Suit) && i < 7)
                {
                    trick.Add(next);
                    streak++;
                    current = next;
                    next = Hand.Visible[i + 1];
                    indexOfCurrent = Deck.NaturalOrder.IndexOf(current) % 8;
                    indexOfNext = Deck.NaturalOrder.IndexOf(next) % 8;
                    i++;
                }

                if (trick.Count > 2)
                {
                    if (trick.Count >= 5)
                        CurrentDeclarations.Add(trick, 100);
                    else if (trick.Count == 4)
                        CurrentDeclarations.Add(trick, 50);
                    else
                        CurrentDeclarations.Add(trick, 20);

                }
            }
        }
        private static void FourSame()
        {
            List<Card> jacks = new List<Card>();
            List<Card> queens = new List<Card>();
            List<Card> kings = new List<Card>();
            List<Card> aces = new List<Card>();
            List<Card> nines = new List<Card>();

            foreach (Card card in Hand.Visible)
            {
                string name = card.Name.ToCharArray()[0].ToString();
                if (name.Equals("J"))
                    jacks.Add(card);
                else if (name.Equals("Q"))
                    queens.Add(card);
                else if (name.Equals("K"))
                    kings.Add(card);
                else if (name.Equals("A"))
                    aces.Add(card);
                else if (name.Equals("9"))
                    nines.Add(card);
            }

            if (jacks.Count == 4)
                CurrentDeclarations.Add(jacks, 200);
            if (queens.Count == 4)
                CurrentDeclarations.Add(queens, 100);
            if (kings.Count == 4)
                CurrentDeclarations.Add(kings, 100);
            if (aces.Count == 4)
                CurrentDeclarations.Add(aces, 100);
            if (nines.Count == 4)
                CurrentDeclarations.Add(nines, 150);
        }

        private static void Belot()
        {
            SuitEnum toMatch = Hand.Visible[0].Suit;
            foreach (Card card in Hand.Visible)
                if (!card.Suit.Equals(toMatch))
                    return;

            CurrentDeclarations.Add(Hand.Visible.ToList(), 1001); //Add end game
        }

        public static int GetBestDeclaration(Dictionary<List<Card>, int> tricks)
        {
            var tricksCopy = new Dictionary<List<Card>, int>(tricks.OrderBy(x => -x.Value).ToDictionary(x => x.Key, y => y.Value));
            if (tricksCopy.First().Value == 0)
                return -1;
            var bestTrick = new List<Card>();
            var bestValue = tricksCopy.Values.First();

            int i = 1;

            bestTrick = tricksCopy.ElementAt(0).Key;
            while(i < tricks.Count && tricksCopy.ElementAt(i).Value == bestValue) 
            {
                var possibleBest = tricksCopy.ElementAt(i).Key;
                int indexOfBest = Deck.NaturalOrder.IndexOf(bestTrick.First()) % 8;
                int indexOfPossibleBest = Deck.NaturalOrder.IndexOf(possibleBest.First()) % 8;

                if (indexOfPossibleBest > indexOfBest)
                    bestTrick = possibleBest;
                i++;
            }
            return tricks.Keys.ToList().IndexOf(bestTrick);
        }
    }
}
