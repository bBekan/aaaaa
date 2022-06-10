using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal class Round
    {
        public int UsScore { get; set; }
        public int ThemScore { get; set; }
        public List<Card> Played { get; set; }
        public List<Player> Players { get; set; }
        public SuitEnum? Trump { get; set; }
        public Player TrumpCaller { get; set; }
        public int TrumpCallerId { get; set; } = -1;
        public int RoundNumber { get; set; }
        public int ScoreDifference { get; set; }
        public int DeclarationsIndex { get; set; } = -1;
        public Player DeclarationsCaller  { get; set; } = null;
        public int DeclarationSum { get; set; } = -1;
        public Player BelaHolder { get; set; } = null;
        public int Fall { get; set; } = 0;
        public int Flush { get; set; } = 0;
        public int Thrown { get; set; } = 0;
        public int Called { get; set; } = 0;
        public int PointsGained { get; set; } = 0;
        private List<Player> team1 { get; set; } = new List<Player>();
        private List<Player> team2 { get; set; } = new List<Player>();
        public Dictionary<QBot, Card> CardsByQBots { get; set; } = new Dictionary<QBot, Card>();

        public Round(List<Player> _players, int _roundNumber, int _scoreDifference)
        {

            Players = _players;
            Played = new List<Card>();
            RoundNumber = _roundNumber;
            ScoreDifference = _scoreDifference;

            team1.Add(Players[0]);
            team1.Add(Players[2]);
            team2.Add(Players[1]);
            team2.Add(Players[3]);

        }

        public void Play()
        {

            for (int i = 0; i < 4 && Trump == null; i++)
            {
                TrumpCallerId = i;
                if (RoundNumber % 2 != 0)
                    ScoreDifference = -ScoreDifference;
                bool muss = i % 2 == 0 ? false : true;
                TrumpCaller = Players[i];
                if (i < 3)
                    Trump = TrumpCaller.CallTrump(ScoreDifference, muss);
                else
                    Trump = TrumpCaller.Muss();

            }


          /*  foreach (Player p in Players)
            {
                foreach (var h in p.Hand.Visible)
                    Console.Write(h.Name + " ");
                Console.WriteLine();
            }

            Console.WriteLine("Trump: " + Trump.ToString() + " Called by: " + Players.IndexOf(TrumpCaller));*/
            Players.ForEach(x => x.Hand.UncoverTalon());
            Deck.SetTrumps((SuitEnum)Trump);
            Players.ForEach(x => Declarations.GenerateDeclarations(x.Hand, (SuitEnum)Trump));


            //Console.WriteLine("Trump set by: " + TrumpCaller.Name + " to " + Trump.ToString());
            foreach (Player p in Players)
            {
                p.Hand.Sort();
                /*foreach (var h in p.Hand.Visible)
                    Console.Write(h.Name + " ");
                Console.WriteLine(" by: " + p.Name);*/
            }
            //Console.WriteLine();

            DeclarationsIndex = WinningTrickIndex();
            if (DeclarationsIndex != -1)
            {
                DeclarationSum = GetTeamTrickSum(DeclarationsIndex);
                DeclarationsCaller = Players[DeclarationsIndex];
            }

            int position = 0;
            while (Played.Count < 32)
            {
                List<Card> cup = new List<Card>(4);
                Card winning = null;
                Player cupWinner = null;
                Card firstPlayed = null;
                bool teamWinning = true;

                for (int i = 0; i < 4; i++)
                {
                    Player current = Players[(i + position) % 4];
                    if (current.GetType().Equals(typeof(QBot)))
                    {
                        QBot bot = (QBot)current;
                        bot.SetPositions(i, Players.IndexOf(TrumpCaller));
                        current = bot;
                    }
                    bool teamCaller = (i + position) % 2 == Players.IndexOf(TrumpCaller) ? true : false;
                    if (winning != null)
                        teamWinning = (i + position) % 2 == cup.IndexOf(winning) ? true : false;
                    bool last = i == 3 ? true : false;
                    Card played = current.PlayCard(winning, (SuitEnum)Trump, i, Players.IndexOf(TrumpCaller), firstPlayed, Played);
                    played.PlayedBy = current;
                    //Card currentWinning, SuitEnum trump, int position, int trumpCallerIndex, Card firstCard, List<Card> played
                    Played.Add(played);
                    if (current.GetType().Equals(typeof(QBot)))
                    {
                        CardsByQBots.TryAdd((QBot)current, played);
                    }
                    if (BelaHolder == null && current.Hand.PotentialBela.Contains((SuitEnum)Trump))
                    {
                        BelaHolder = current;
                    }


                    if (firstPlayed == null)
                        firstPlayed = played;

                    current.Hand.Visible.Remove(played);

                    cup.Add(played);
                    Card temp = FindWinning(cup);
                    if (temp != winning)
                    {
                        winning = temp;
                        cupWinner = current;
                    }
                }
                /* foreach(Card c in cup)
                     Console.Write(c.Name + " ");
                 Console.WriteLine("Won by: " + cupWinner.Name + " with " + winning.Name);*/
                position = Players.IndexOf(cupWinner);

                int cupWorth = cup.Sum(x => x.Value);
                if (Played.Count > 28)
                    cupWorth += 10;
                if (position % 2 == 0)
                    UsScore += cupWorth;
                else
                    ThemScore += cupWorth;

                //Console.WriteLine("Us: " + UsScore + " Them: " + ThemScore);

                foreach (var kvp in CardsByQBots)
                {
                    int wonGame = 0;
                    bool wonCup = false;
                    bool teamWonCup = false;
                    int pointsWon = 0;
                    var indexOfPlayer = Players.IndexOf(kvp.Key);
                    if (indexOfPlayer == position)
                    {
                        wonCup = true;
                        teamWonCup = true;
                        pointsWon = cupWorth;
                    }
                    else if (indexOfPlayer % 2 == position % 2)
                    {
                        teamWonCup = true;
                        pointsWon = cupWorth;
                    }
                    else
                        pointsWon = -cupWorth;
                    kvp.Key.CalculateReward(wonCup, teamWonCup, pointsWon);
                }

                //Console.WriteLine("Us: " + UsScore + " Them: " + ThemScore);
            }
            SetResults();
            /*Console.WriteLine("FINAL: ");
            Console.WriteLine("Us: " + UsScore + " Them: " + ThemScore);
            if(DeclarationsCaller != null)
            Console.WriteLine("Declarations: " + DeclarationsCaller.Name + " Total: " + DeclarationSum);*/
            int reward = UsScore - ThemScore;
            foreach (var kvp in CardsByQBots)
            {
                kvp.Key.RoundEndReward(reward);
                kvp.Key.ChosenStatesInGame.AddRange(kvp.Key.ChosenStatesInRound);
                kvp.Key.ChosenStatesInRound.Clear();
            }
            Deck.SetTrumps((SuitEnum)Trump);
        }

        private int GetTeamTrickSum(int winningTrick)
        {
            int sum = 0;
            for (int i = 0; i < 2; i++)
            {
                int index = (winningTrick + i * 2) % 4;
                var tricks = Players[index].GetTricks().Values;

                if (tricks != null)
                    sum += tricks.Sum();
            }
            return sum;
        }

        private int WinningTrickIndex()
        {
            var teamTricks = new Dictionary<List<Card>, int>();
            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                var bestTrick = Players[i].GetTricks().FirstOrDefault();
                if (bestTrick.Value != 0)
                {
                    count++;
                    teamTricks.Add(bestTrick.Key, bestTrick.Value);
                }
                else
                    teamTricks.Add(new List<Card>(), 0);


            }
            if (count == 0)
                return -1;
            var bestOfAll = Declarations.GetBestDeclaration(teamTricks);
            return bestOfAll;
        }

        public void SetResults()
        {
            int usScoreBfr = UsScore;
            int themScoreBfr = ThemScore;
            bool swapped = false;
            if(BelaHolder != null)
            {
                //Console.WriteLine("Bela: " + BelaHolder.Name);
                if (team1.Contains(BelaHolder))
                {
                    //Console.WriteLine("UsScore");
                    UsScore += 20;
                }

                ThemScore += 20;
            }

            if (TrumpCallerId % 2 == RoundNumber % 2) //TrumpCallerIndex
                Called++;

            if (RoundNumber % 2 == 1)
            {
                int temp = UsScore;
                UsScore = ThemScore;
                ThemScore = temp;
                swapped = true;
            }
            if (UsScore == 0)
            {
                PointsGained = UsScore;
                ThemScore += 90;
                if (DeclarationSum != -1)
                    ThemScore += DeclarationSum;
                return;
            }
            if (ThemScore == 0)
            {
                UsScore += 90;
                PointsGained = UsScore;
                if (DeclarationSum != -1)
                    UsScore += DeclarationSum;
                Flush++;
                return;
            }
            if (DeclarationsCaller != null)
            {
                if (swapped)
                {
                    if (team1.Contains(DeclarationsCaller))
                        ThemScore += DeclarationSum;
                    else 
                        UsScore += DeclarationSum;

                }
                else
                {
                    if (team1.Contains(DeclarationsCaller))
                        UsScore += DeclarationSum;
                    else
                        ThemScore += DeclarationSum;

                }
            }
            if (swapped)
            {
                if (team1.Contains(TrumpCaller) && UsScore > ThemScore)
                {
                    UsScore += ThemScore;
                    ThemScore = 0;
                    Thrown++;
                }
                else if (team2.Contains(TrumpCaller) && ThemScore > UsScore)
                {
                    Fall++;
                    ThemScore += UsScore;
                    UsScore = 0;
                }

            }
            else
            {
                if (team1.Contains(TrumpCaller) && UsScore < ThemScore)
                {
                    ThemScore += UsScore;
                    UsScore = 0;
                    Fall++;
                }
                else if (team2.Contains(TrumpCaller) && ThemScore < UsScore)
                {
                    UsScore += ThemScore;
                    ThemScore = 0;
                    Thrown++;
                }

            }

            if (UsScore != 0)
            {
                PointsGained = UsScore;
                int diff = swapped ? UsScore - themScoreBfr : UsScore - usScoreBfr;
                PointsGained -= diff;
            }
            else
                PointsGained = 0;
        }

        private Card? FindWinning(List<Card> cup)
        {
            if (cup.Count == 0) return null;

            SuitEnum played = cup[0].Suit;

            Card? possibleTrump = cup
                    .Where(x => x.Suit.Equals(Trump))
                    .OrderBy(x => -x.Value)
                    .ThenByDescending(x => x.Name)
                    .FirstOrDefault();
            if (played.Equals(Trump))
                return possibleTrump;

            if (possibleTrump != null)
                return possibleTrump;
            return cup
                .Where(x => x.Suit.Equals(played))
                .OrderBy(x => -x.Value)
                .ThenByDescending(x => x.Name)
                .First();
        }
    }
}
