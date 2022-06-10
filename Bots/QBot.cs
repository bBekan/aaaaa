using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal class QBot : Player
    {
        private Dictionary<string, double> States { get; set; }
        private int Position { get; set; }
        private List<Card> Played { get; set; } = new List<Card>();
        private Card CurrentWinning { get; set; }
        private int CallerPosition { get; set; }
        private SuitEnum Trump { get; set; }
        public List<string> ChosenStatesInRound { get; set; } = new List<string>();
        public List<string> ChosenStatesInGame { get; set; } = new List<string>();
        private double Exploitation{ get; set; }
        private string? LastState { get; set; }
        private int GameNumber { get; set; }
        private bool Learn { get; set; }
        private double LearningRate { get; set; } = 0.01;
        private double Discount { get; set; } = 0.15;
        private List<Card> RemainingCard { get; set; } = new List<Card>();

        public QBot(bool learn = true) : base()
        {
            Learn = learn;
            States = Environment.States;
        }

        public override Card PlayCard(Card currentWinning, SuitEnum trump, int playerIndex, int trumpCallerIndex, Card firstCard, List<Card> played)
        {

            CurrentWinning = currentWinning;
            Trump = trump;
            Played = played;
            Exploitation = 0.05;

            Hand.SetPlayable(currentWinning, trump, firstCard);
            SetRemainingCards();
            if (Learn)
            {
                Exploitation += (float)GameNumber / 31000;

                Random rnd = new Random();
                double db = rnd.NextDouble();
                if (Exploitation < db)
                {
                    var randomCard = base.PlayCard(currentWinning, trump, playerIndex, trumpCallerIndex, firstCard, played);
                    var key = KeyFromState(currentWinning, randomCard, played);
                    LastState = key;
                    ChosenStatesInRound.Add(key);
                    if(!States.ContainsKey(key))
                        States.Add(key, 0.0);
                    return randomCard;
                }

            }
            string bestKey = null;
            double bestScore = -1000000000000.0;
            Card best = null;

            Dictionary<Card, string> CardKey = new Dictionary<Card, string>();
            foreach(Card c in Hand.Playable)
                CardKey.Add(c, KeyFromState(currentWinning, c, played));

            foreach(var kvp in CardKey)
            {
                Card toPlay = kvp.Key;
                string key = kvp.Value;
                if(!States.ContainsKey(key))
                    States.Add(key, 0.0);

                double calculatedGain = (1 - LearningRate) * States[key] + LearningRate * (PossibleReward(toPlay) + Discount * BestPossibleNext(toPlay, 3 - Position, RemainingCard, CurrentWinning));
                if(calculatedGain > bestScore)
                {
                    bestKey = key;
                    bestScore = calculatedGain;
                    best = toPlay;
                }
            }

            if (Learn)
            {
                States[bestKey] = bestScore;
                LastState = bestKey;
                ChosenStatesInRound.Add(bestKey);
            }
            return best;
        }

        private bool IsBest(Card c, Card currentWinning)
        {
            if (currentWinning == null)
                return true;
            bool toReturn = c.Suit == currentWinning.Suit && c.Value > currentWinning.Value ? true : false;
            return toReturn;
        }

        private void SetRemainingCards()
        {
            List<Card> remaining = new List<Card>(Deck.GameDeck);
            Played.ForEach(x => remaining.RemoveAt(remaining.FindIndex(y => y.Name.Equals(x.Name))));
            Hand.Playable.ForEach(x => remaining.RemoveAt(remaining.FindIndex(y => y.Name.Equals(x.Name))));

            RemainingCard = remaining;
        }

        private double BestPossibleNext(Card toPlay, int maxCount, List<Card> remaining, Card currentWinning)
        {
            if (maxCount == 0)
            {
                return 0.0;
            }
            if (IsBest(toPlay, currentWinning))
                currentWinning = toPlay;

            SetImpactfulRemaining(remaining, currentWinning);
            Position++;
            remaining.Remove(toPlay);
            Played.Add(toPlay);

            double bestValue = -100000000.0;
            Card bestCard = null;
            for(int i = 0; i < remaining.Count; i++)
            {
                Card next = remaining[i];

                string key = KeyFromState(currentWinning, next, Played);
                if (!States.ContainsKey(key))
                    States.Add(key, 0.0);
                var keyValue = (1 - LearningRate) * States[key] + LearningRate * (PossibleReward(next) + Discount * BestPossibleNext(next, 3 - Position, remaining, currentWinning));
                if (keyValue > bestValue)
                {
                    bestValue = keyValue;
                    bestCard = next;
                }
            }
            Played.Remove(toPlay);
            remaining.Add(toPlay);
            Position--;
            return bestValue;
        }

        private void SetImpactfulRemaining(List<Card> remaining, Card currentWinning)
        {
            remaining.RemoveAll(x => x.Suit != currentWinning.Suit && x.Suit != Trump);
        }

        private double PossibleReward(Card toPlay)
        {
            List<Card> cup = new List<Card>();
            cup = Played.GetRange(((Played.Count) / 4) * 4, Played.Count % 4);
            cup.Add(toPlay);
            return cup.Sum(x => x.Value);
        }

        public void SetPositions(int i, int j)
        {
            Position = i;
            CallerPosition = j;
        }

        public string KeyFromState(Card currentWinning, Card wantToPlay, List<Card> played)
        {
            int betterCardsLeft = 0;
            int currentWinningIndex = 0;
            if (currentWinning == null)
                currentWinning = new Card(SuitEnum.H, 0) { Name = "null" };

            else
                currentWinningIndex = played.FindIndex(x => x.Name.Equals(currentWinning.Name)) % 4;


                List<Card> gameDeckCopy = new List<Card>(Deck.GameDeck);
                played.ForEach(x => gameDeckCopy.Remove(x));
                Hand.Visible.Where(x => !x.Name.Equals(wantToPlay.Name)).ToList().ForEach(x => gameDeckCopy.Remove(x));
                if (wantToPlay.Suit.Equals(Trump))
                {
                gameDeckCopy.RemoveAll(x => !x.Suit.Equals(Trump));
                gameDeckCopy = gameDeckCopy.OrderBy(x => x.Value)
                    .ThenBy(x => x.Name).ToList();
                betterCardsLeft += gameDeckCopy.Where(x => gameDeckCopy.IndexOf(x) > gameDeckCopy.IndexOf(wantToPlay)).Count();

                }
                else
                {
                    betterCardsLeft += gameDeckCopy.Where(x => x.Suit.Equals(Trump)).Count();
                    gameDeckCopy.RemoveAll(x => !x.Suit.Equals(wantToPlay.Suit));

                gameDeckCopy = gameDeckCopy.OrderBy(x => x.Value)
                .ThenBy(x => x.Name).ToList();
                betterCardsLeft += gameDeckCopy.Where(x => gameDeckCopy.IndexOf(x) > gameDeckCopy.IndexOf(wantToPlay)).Count();
                }

            gameDeckCopy = null;
            string key = $"{currentWinning.Name}|{wantToPlay.Name}|{currentWinningIndex}|{Position}|{CallerPosition}|{betterCardsLeft}"; 
            return key;
        }

        public void CalculateReward(bool wonCup, bool teamWonCup, int pointsWon)
        {
            if (Learn)
            {
                double reward = 0.0;
                if (teamWonCup)
                {
                    reward += 3;
                    if (wonCup)
                        reward += 10;
                }
                else
                    reward -= 7.5;

                reward += pointsWon;
                States[LastState] += reward;
            }

        }

        public void RoundEndReward(int reward)
        {
            if (Learn)
            {
                foreach (var key in ChosenStatesInRound)
                    States[key] += reward / 5; //Promijeni rate
            }

        }

        public void GameEndReward(bool won)
        {
            if (Learn)
            {
                foreach (var key in ChosenStatesInGame)
                {
                    if (won)
                        States[key] += 10;
                    else
                        States[key] -= 10;
                }
            }

        }

        public void SetGameNumber(int i)
        {
            GameNumber = i;
        }

        internal void Reset()
        {
            ChosenStatesInGame.Clear();
            Played.Clear();
            RemainingCard.Clear();

        }
    }
}
