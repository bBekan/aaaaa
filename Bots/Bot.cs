using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal class Bot : Player
    {
        public List<Card> Played { get; set; }
        private SuitEnum Trump { get; set; }
        public Dictionary<SuitEnum, List<Card>> WinningBySuit { get; set; }
        public Dictionary<SuitEnum, int> OrderedSuits { get; set; }
        public int Position { get; set; }
        public int CallerPosition { get; set; }
        private bool TheyHaveTrumps { get; set; }
        private bool HasBetter { get; set; }
        private Card CurrentWinning { get; set; }
        private int BestIsOurs { get; set; }
        private bool TheyCalled { get; set; }
        private bool ICalled { get; set; }
        private List<Card> Playable { get; set; } = new List<Card>();

        public override Card PlayCard(Card currentWinning, SuitEnum trump, int position, int trumpCallerIndex, Card firstCard, List<Card> played)
        {
            CurrentWinning = currentWinning;
            Played = played;
            Trump = trump;
            Position = position;
            CallerPosition = trumpCallerIndex;

            Hand.SetPlayable(currentWinning, trump, firstCard);
            SetParameters();
            Random rnd = new Random();

            if (Playable.Count == 1)
                return Playable[0];


            if (BestIsOurs != 0)
            {
                if (HasBetter)
                {
                    if (WinningBySuit.ContainsKey(CurrentWinning.Suit))
                        return Playable.Last();
                    else
                        return Playable.First();
                }
                else if (BestIsOurs == 1)
                {
                    foreach (Card c in Hand.SoloTen)
                        if(WinningBySuit.ContainsKey(c.Suit))
                        if (WinningBySuit[c.Suit].FirstOrDefault(x => x.Value == 10) != null)
                            return c;
                    Hand.Playable.Reverse();
                    foreach (Card c in Hand.Playable)
                        if (c.Value != 10)
                            return c;
                    return Hand.Playable.Last();
                }
                else
                    return Playable.First();
            }
            else
            {
                if (RemainingByColor[trump] == null) //Nema aduta
                {
                    if (WinningBySuit.Count > 1)
                        return WinningBySuit.Where(x => !x.Key.Equals(trump)).First().Value.Last();
                    else if (WinningBySuit.Count == 1 && WinningBySuit.ContainsKey(trump))
                    {
                        List<Card> possible = Hand.Visible.Where(x => x.Value != 10).ToList(); //Nemoj solo desetke
                        if (possible.Count > 0)
                            return possible[rnd.Next(0, possible.Count)];
                        else
                            return Hand.Visible[rnd.Next(0, Hand.Visible.Count)];
                    }
                    else
                        return WinningBySuit[0].Last();

                }
                else if (WinningBySuit.ContainsKey(trump)) //Pokupi preostale adute
                {
                    if (TheyHaveTrumps)
                        return WinningBySuit[trump].Last();
                    else
                    {
                        foreach (var kvp in OrderedSuits.Reverse())
                            if (kvp.Key != Trump && WinningBySuit.ContainsKey(kvp.Key))
                                return WinningBySuit[kvp.Key].Last();

                        if (Hand.ByColor.Count != 1)
                            return Hand.ByColor.Where(x => x.Key != Trump).First().Value.Last();
                        else
                            return Hand.Playable[rnd.Next(0, Hand.Playable.Count)];
                    }
                }
                else //Ima još aduta u igri, a ti nemas niti jednog, baci bodove ili nista ovisno ko drzi kup
                {
                    if (TheyHaveTrumps)
                        return Hand.Playable.First();
                    return Hand.Playable.Last();
                }
            }
        }

        private void SetParameters()
        {
            TheyHaveTrumps = CheckLastTrumpTrick();
            Playable = Hand.Playable;
            ICalled = Position == CallerPosition ? true : false;
            TheyCalled = Position % 2 != CallerPosition % 2 ? true : false;

            if (CurrentWinning == null)
            {
                BestIsOurs = 0;
                HasBetter = true;
            }
            else
            {
                BestIsOurs = Played.Count % 2 == Played.IndexOf(CurrentWinning) % 2 ? 1 : -1;
                HasBetter = Playable[0].Suit == CurrentWinning.Suit && Hand.Playable[0].Value > CurrentWinning.Value ? true : false;
            }




            SetRemainingSuits();
            SetWinningBySuit();
            SetOrderedSuits();
        }

        private void SetOrderedSuits()
        {
            OrderedSuits = new Dictionary<SuitEnum, int>();
            OrderedSuits = RemainingByColor
                           .OrderBy(x => x.Value.Count)
                           .ToDictionary(x => x.Key, y => y.Value.Count);
        }

        private bool CheckLastTrumpTrick()
        {
            Card lastTrump = Played.FindLast(x => x.Suit.Equals(Trump));
            if (lastTrump == null)
                return true;
            int lastTrumpIndex = Played.IndexOf(lastTrump);
            int baseIndex = lastTrumpIndex % 4;
            if (lastTrumpIndex < 4)
                return true;
            List<Card> trick = Played.GetRange(lastTrumpIndex - baseIndex, Played.Count % 4);
            return trick.Where(x => x.Suit.Equals(Trump) && Played.IndexOf(x) % 4 != baseIndex).Any();
        }

        private void SetWinningBySuit()
        {
            WinningBySuit = new Dictionary<SuitEnum, List<Card>>();
            foreach (var kvp in Hand.ByColor)
            {
                if (!RemainingByColor.ContainsKey(kvp.Key))
                    WinningBySuit.Add(kvp.Key, kvp.Value);
                else
                {
                    Card bestRemaining = RemainingByColor[kvp.Key].LastOrDefault();
                    if(bestRemaining != null)
                    {
                        List<Card> toAdd = kvp.Value.Where(x => x.Value >= bestRemaining.Value && x.Name.CompareTo(bestRemaining.Name) == 1).ToList();
                        if (toAdd.Count > 0)
                            WinningBySuit.Add(kvp.Key, toAdd);
                    }
                }
            }
        }

        private void SetRemainingSuits()
        {
            RemainingByColor = new Dictionary<SuitEnum, List<Card>>();
            List<Card> deckCopy = new List<Card>(Deck.GameDeck);
            Hand.Visible.ForEach(x => deckCopy.RemoveAt(deckCopy.FindIndex(y => y.Name.Equals(x.Name))));
            foreach (SuitEnum suit in Enum.GetValues(typeof(SuitEnum)))
            {
                List<Card> doubleCopy = new List<Card>(deckCopy);
                Played.ForEach(x => doubleCopy.Remove(x));
                List<Card> toAdd = doubleCopy
                    .Where(x => x.Suit == suit)
                    .OrderBy(x => x.Value)
                    .ThenBy(x => x.Name)
                    .ToList();
                RemainingByColor.Add(suit, toAdd);
            }

        }
    }
}
