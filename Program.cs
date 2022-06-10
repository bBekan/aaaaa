using BelaAI;




using (FileStream sw = new FileStream("bots.txt", FileMode.OpenOrCreate));
var learn = false;
BelaAI.Environment.Initialize(learn);
Player player1 = new QBot(learn) { Name = "QBot1" };
Player player2 = new Bot { Name = "Alg1" };
Player player3 = new Bot { Name = "QbotPartner2" };
Player player4 = new Bot { Name = "Alg2" };


for (int i = 0; i < 30000; i++)
{
    ((QBot)player1).SetGameNumber(i);
    //((QBot)player3).SetGameNumber(i);
    Game Game = new Game(player1, player2, player3, player4);
    Game.Start();
    ((QBot)player1).Reset();
    //((QBot)player3).Reset();
}

if(learn)
    using(FileStream fs = new FileStream("policy.txt", FileMode.OpenOrCreate))
    {
        using (StreamWriter sw = new StreamWriter(fs))
        {
        foreach (var kvp in BelaAI.Environment.States)
            sw.Write(kvp.Key + "," + kvp.Value + "\n");
        }
    }





    