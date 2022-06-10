using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal static class Deck
    {
        public static List<Card> GameDeck { get; set; }
        public static List<Card> NaturalOrder { get; set; }
        public static List<Card> PointOrder { get; set; }
        public static List<Card> TrumpPointOrder { get; set; }

        public static void Initialize()
        {
            Card[] gd = new Card[32];
            for(int i = 0; i < 4; i++)
                for(int j = 0; j < 8; j++)
                    gd[i * 8 + j] = new Card((SuitEnum)Enum.GetValues(typeof(SuitEnum)).GetValue(i), 7 + j);
 
            GameDeck = gd.ToList();

            NaturalOrder =  new List<Card>(GameDeck);

            PointOrder = new List<Card>(GameDeck);
            PointOrder = PointOrder
                .OrderBy(x => x.Suit.ToString())
                .ThenBy(x => x.Value)
                .ThenBy(x => x.Name)
                .ToList();

            TrumpPointOrder = new List<Card>();

            foreach (Card c in GameDeck)
                TrumpPointOrder.Add(new Card(c.Suit, c.InitialValue));
                

            TrumpPointOrder
                .ForEach(x => x.SetTrump());

            TrumpPointOrder = TrumpPointOrder
                .OrderBy(x => x.Suit.ToString())
                .ThenBy(x => x.Value)
                .ThenBy(x => x.Name)
                .ToList();
        }

        public static void Shuffle()
        {
            if(GameDeck.Count != 32)
                Console.WriteLine("The deck must be initialized!");
            else
            {
                Deck.Reset();
                Random rnd = new Random();
                GameDeck = GameDeck.OrderBy(x => rnd.Next()).ToList();
            }
        }

        public static void Print()
        {
            for(int i = 0; i < GameDeck.Count; i++)
            {
                if(i % 8 == 0)
                    Console.WriteLine();
                Console.Write(GameDeck[i].Name + " ");
            }
            Console.WriteLine();
        }

        public static void Deal(List<Player> players)
        {
            int i = 0;
            foreach (Player p in players)
            {
                p.Hand = new Hand(GameDeck.GetRange(i, 8));
                i += 8;
            }
        }
        public static void SetTrumps(SuitEnum trump)
        {
            GameDeck.Where(c => c.Suit.Equals(trump))
                .ToList()
                .ForEach(c => c.SetTrump());
        }
        public static void Reset()
        {
            Deck.GameDeck.ForEach(x => x.PlayedBy = null);
        }

    }
}
