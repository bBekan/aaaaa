using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal class Game
    {
        public List<Round> Rounds { get; set; } = new List<Round>();
        public int TotalUsScore { get; set; }
        public int TotalThemScore { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public bool RotateWin { get; set; } = false;


        public Game(params Player[] _players)
        {
            Players.AddRange(_players);
            Deck.Initialize();
        }

        public void Start()
        {
            //Console.WriteLine("Us  |  Them");
            int roundNumber = 0;
            int totalFail = 0;
            int totalFlush = 0;
            int wonRounds = 0;
            int won = 0;
            int total = 0;
            int totalThrown = 0;
            int totalCalled = 0;
            int pointsGained = 0;

            while (TotalUsScore < 1001 && TotalThemScore < 1001)
            {
                Deck.Shuffle();
                Deck.Deal(Players);
               
                Round r = new Round(Players, roundNumber++, TotalUsScore - TotalThemScore);
                r.Play();
                if (r.UsScore > r.ThemScore)
                    wonRounds++;
                total++;
                pointsGained += r.PointsGained;
                totalFail += r.Fall;
                totalFlush += r.Flush;
                TotalUsScore += r.UsScore;
                TotalThemScore += r.ThemScore;
                totalThrown += r.Thrown;
                totalCalled += r.Called;

                //Console.WriteLine(TotalUsScore + " | " + TotalThemScore);

                Rotate();
            }
            using(StreamWriter sw = new StreamWriter("bots.txt", append: true))
            {
                bool wonGame = false;
                if (TotalUsScore > TotalThemScore)
                {
                    won = 1;
                    wonGame = true;
                }

                foreach(Player p in Players)
                    if (p.GetType().Equals(typeof(QBot)))
                    {
                        var qbotp = (QBot)p;
                        qbotp.GameEndReward(wonGame);
                        qbotp.ChosenStatesInGame.Clear();
                    }

                sw.Write(TotalUsScore - TotalThemScore +", " + pointsGained +  ", " + totalCalled + ", " + totalFail + ", " + totalThrown +  ", " + totalFlush + ", " + ((decimal) Math.Round((decimal)pointsGained/total, 3)).ToString(
                CultureInfo.CreateSpecificCulture("en-GB")) + ", " + total + ", "+ won + "\n");
            }
        }

        private void Rotate()
        {
            List<Player> toMove = Players.GetRange(0, 3);
            Players.RemoveRange(0, 3);
            Players.AddRange(toMove);
        }
    }
}
