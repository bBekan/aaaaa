using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal class Environment
    {
        public static Dictionary<string, double> States { get; set; } = new Dictionary<string, double>();

        private static void SetEnvironment()
        {
            Deck.Initialize();
            Card nullCard = new Card(SuitEnum.S, 0) { Name = "null" };
            var gameDeck = new List<Card>(Deck.GameDeck);
            gameDeck.Add(nullCard);
            foreach (Card winningCard in gameDeck)
            {
                List<bool> boolList = new List<bool> { true, false };
                List<int> playerPosition = new List<int> { 0, 1, 2, 3 };
                List<Card> c1Excluded = new List<Card>(Deck.GameDeck);
                if (!winningCard.Name.Equals("null"))
                {
                    c1Excluded.Remove(winningCard);
                    playerPosition.Remove(0);
                }

                else
                {
                    boolList.Remove(false);
                    playerPosition.RemoveAll(x => x != 0);
                }

                foreach (Card toPlay in c1Excluded)//Karta koju želimo baciti
                {
                    int toPlayIndex = Deck.PointOrder.FindIndex(x => x.Name.Equals(toPlay.Name));
                    int winningCardIndex = Deck.PointOrder.FindIndex(x => x.Name.Equals(winningCard.Name));
                    int maxNumberOfBetterCards = 16 -  toPlayIndex % 8;
                    if (winningCard.Suit.Equals(toPlay.Suit) && winningCardIndex > toPlayIndex)
                        maxNumberOfBetterCards--;
                    foreach (int position in playerPosition)//Pozicija igrača
                    {
                        List<int> strongestCardIndex = new List<int> { 0, 1, 2, 3 };
                        strongestCardIndex = strongestCardIndex.Where(x => x < position || x == 0).ToList();
                        foreach (int SCIndex in strongestCardIndex)//Pozicija najjače karte na kupu
                            foreach (int trumpCaller in new List<int> { 0, 1, 2, 3 }) //Pozicija igrača koji je zvao adut
                                for (int strongerCards = 0; strongerCards < maxNumberOfBetterCards; strongerCards++) //Broj jačih karata
                                {
                                    string key = $"{winningCard.Name}|{toPlay.Name}|{SCIndex}|{position}|{trumpCaller}|{strongerCards}";
                                    States.Add(key, 0.0);
                                }
                    }

                }
            }
        }

        internal static void Initialize(bool train = true)
        {
               // LoadEnvironment();
        }

        private static void LoadEnvironment()
        {
            using (StreamReader sr = new StreamReader("policyRandom.txt"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(",");
                    var key = line[0];
                    var value = int.Parse(line[1]);
                    States.Add(key, value);
                }
            }
        }
    }
}
