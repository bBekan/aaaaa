using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal class Player
    {
        public Hand Hand { get; set; }
        public string Name { get; set; }
        public Dictionary<SuitEnum, List<Card>> RemainingByColor { get; set; }

        public SuitEnum Muss()
        {
            return (SuitEnum)CallTrump(0, true, true);
        }

        public Dictionary<List<Card>, int> GetTricks()
        {
            return Hand.CurrentTricks;
        }

        public virtual Card PlayCard(Card currentWinning, SuitEnum trump, int playerIndex, int trumpCallerIndex, Card firstCard, List<Card> played)
        {
            Random rnd = new Random();
            Hand.SetPlayable(currentWinning, trump, firstCard);

            int index = rnd.Next(0, Hand.Playable.Count);
            return Hand.Playable[index];
        }

        public virtual SuitEnum? CallTrump(int scoreDifference, bool muss, bool last = false)
        {
            Declarations.GeneratePreTrumpSelection(Hand);
            if (!last)
            {
                int score = scoreDifference / 10;
                if (muss)
                    score -= 10;
                else
                    score += 10;
                if (Hand.CurrentTricks.Count != 0)
                    score += Hand.CurrentTricks.Sum(x => x.Value);

                if (score >= 100 && !muss)
                    return null;
            }

            string maxSuit = null;
            int maxValue = 0;
            foreach (SuitEnum suit in Enum.GetValues(typeof(SuitEnum)))
            {
                int suitValue = 0;
                if (Hand.PotentialBela.Contains(suit))
                    suitValue += 20;
                List<Card> ofSuit = Hand.Visible.Where(x => x.Suit.Equals(suit))
                     .ToList();
                if (ofSuit.Count != 0)
                {
                    suitValue += (int)Math.Pow(2, ofSuit.Count);
                    ofSuit.ForEach(x => x.SetTrump());
                    suitValue += ofSuit.Sum(x => x.Value);
                    ofSuit.ForEach(x => x.SetTrump());
                }
                if (suitValue > maxValue)
                {
                    maxValue = suitValue;
                    maxSuit = suit.ToString();
                }
            }
            if (last)
                return (SuitEnum)Enum.Parse(typeof(SuitEnum), maxSuit);


            Random rnd = new Random();
            if (maxValue > rnd.Next(34, 50))
                return (SuitEnum)Enum.Parse(typeof(SuitEnum), maxSuit);
            return null;
        }
    }
}
