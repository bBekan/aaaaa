namespace BelaAI
{
    internal static class Rules
    {
        private static Dictionary<SuitEnum, List<Card>> ByColor { get; set; }
        private static Hand Hand { get; set; }
        private static Card Played { get; set; }
        public static void GetPlayable(Hand hand, SuitEnum trump, Card currentWinning, Card firstPlayed)
        {
            Played = currentWinning;
            Hand = hand;
            Hand.Sort();
            ByColor = new Dictionary<SuitEnum, List<Card>>();

            foreach (Card card in hand.Visible)
            {
                SuitEnum color = card.Suit;
                if (ByColor.ContainsKey(color))
                    ByColor[color].Add(card);
                else
                    ByColor.Add(color, new List<Card> { card });
            }

            Hand.ByColor = ByColor;
            List<Card> toReturn = new List<Card>();

            if (currentWinning == null)
            {
                Hand.Playable =  Hand.Visible;
                return;
            }
            if (!ByColor.ContainsKey(firstPlayed.Suit)) //Nema boju igrane karte
            {
                if (!ByColor.ContainsKey(trump))
                {
                    toReturn = Hand.Visible;
                    Hand.SoloTen = toReturn.Where(x => ByColor[x.Suit].Count == 1 && x.Value == 10).ToList();
                }

                else if (currentWinning.Suit.Equals(trump))
                {
                    Played = currentWinning;
                    toReturn = Uber(Deck.TrumpPointOrder);
                    if (toReturn.Count == 0)
                        toReturn = ByColor[trump];
                }
                else
                    toReturn = ByColor[trump];
            } else //Ima boju prve igrane karte
            {
                if (currentWinning.Suit.Equals(trump)) //Pobjednička karta je adut
                {
                    if (firstPlayed.Suit.Equals(trump))//Prva bačena karta je također adut
                    {
                        toReturn = Uber(Deck.TrumpPointOrder); //Vraćam uber aduta
                        if (toReturn.Count == 0)
                            toReturn = ByColor[trump];
                    } 
                    else
                        toReturn = ByColor[firstPlayed.Suit]; //Prva bačena nije adut, vraćam sve karte iste boje kao prva bačena
                }
                else //Pobjednička karta nije adut
                {
                    toReturn = Uber(Deck.PointOrder); //Vraćam sve jače karte
                    if (toReturn.Count == 0)
                        toReturn = ByColor[currentWinning.Suit]; //Ako nema jačih vraćam sve ostale
                }

            }
                Hand.Playable = toReturn;
            ByColor.Clear();
        }

        private static List<Card> Uber(List<Card> pointOrder)
        {
            List<Card> toReturn = new List<Card>();

            int index = pointOrder.FindIndex(x => x.Name.Equals(Played.Name));

            foreach (Card card in Hand.Visible)
            {
                var cardIndex = pointOrder.FindIndex(x => x.Name.Equals(card.Name));
                if (card.Suit.Equals(Played.Suit) && cardIndex > index)
                    toReturn.Add(card);
            }
            return toReturn;
        }
    }
}
