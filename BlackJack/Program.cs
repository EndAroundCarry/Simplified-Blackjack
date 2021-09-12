using BlackJack.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlackJack
{
    class Program
    {
        enum Status {Playing, Won, Lost, Stopped }
        private static int executionCounter = 1;
        private static int numberOfTimesToRunTheApp = 5;
        private static string solutionFolder = "";

        static void Main(string[] args)
        {
            Program pr = new Program();
            pr.CleanupAndCreateOutpuDirectory();

            for (int i = 1; i <= numberOfTimesToRunTheApp; i++)
            {
                List<Player> players = new List<Player>();
                List<Card> cards = new List<Card>();

                pr.CreatePlayers(players);
                pr.InitializeGame(cards, players);
                executionCounter++;
            }

        }
        private void CreatePlayers(List<Player> players)
        {
            var rnd = new Random();
            for (int i = 1; i < 5; i++)
            {
                players.Add(new Player { Name = "Player" + i, Points = 0, RiskAversion = rnd.Next(0, 4), IsDealer = false, Status = (int)Status.Playing });
                if (i == 4)
                {
                    players.Add(new Player { Name = "Dealer", Points = 0, RiskAversion = 4, IsDealer = true, Status = (int)Status.Playing });
                }
            }
        }

        private void InitializeGame(List<Card> cards, List<Player> players)
        {
            List<string> specialCardNames = new List<string> { "Jack", "Queen", "King" };

            for (int i = 2; i <= 11; i++)
            {
                if (i == 11)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        cards.Add(new Card { Name = "Ace", Value = i });
                    }
                    break;
                }
                if(i == 10)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        cards.Add(new Card { Name = i.ToString(), Value = i });
                    }

                    foreach (var cr in specialCardNames)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            cards.Add(new Card { Name = cr.ToString(), Value = i });
                        }
                    }
                }
                else if(i < 10)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        cards.Add(new Card { Name = i.ToString(), Value = i });
                    }
                }
            }
            ShuffleCardsAndDeal(cards, players);
        }

        private void ShuffleCardsAndDeal(List<Card> cards, List<Player> players)
        {
            var rnd = new Random();
            var idx1 = 0;
            var idx2 = 0;
            Card aux = new Card();

            for (int i = 0; i < cards.Count * 4; i++)
            {
                 idx1 = rnd.Next(0, 51);
                 idx2 = rnd.Next(0, 51);
                 aux = cards[idx1];
                 cards[idx1] = cards[idx2];
                 cards[idx2] = aux;
            }

            var allPlayersHave2CardsOrPlayerWon = false;

            while (!allPlayersHave2CardsOrPlayerWon) {

                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Cards.Add(new Card { Name = cards[cards.Count - 1].Name, Value = cards[cards.Count - 1].Value });
                    cards.RemoveAt(cards.Count - 1);
                    players[i].Points = players[i].Cards.Sum(x => x.Value);
                    if (players[i].Cards.Count == 2)
                    {
                        ChangeStatus(players, players[i]);
                        if(players[i].Status == (int)Status.Won)
                        {
                            break;
                        }
                    }
                }
                allPlayersHave2CardsOrPlayerWon = players.Where(x => x.Cards.Count == 2).ToList().Count == 5 || players.Where(x=>x.Status == (int)Status.Won).ToList().Count == 1;
            }
            BeginGame(cards, players);
        }

        private void ChangeStatus(List<Player> players, Player player)
        {
            if (player.Points == 21)
            {
                player.Status = (int)Status.Won;
                WriteResults(players, player);
            }
            else if (player.Points > 21)
            {
                if (!player.IsDealer && player.Status != (int)Status.Lost)
                {
                    player.Status = (int)Status.Lost;
                }
                else if(player.IsDealer && player.Status != (int)Status.Stopped)
                {
                    player.Status = (int)Status.Stopped;
                }
            }
            else if (player.Points >= 21 - player.RiskAversion && player.Status != (int)Status.Stopped)
            {
                player.Status = (int)Status.Stopped;
            }
        }
        private void BeginGame(List<Card> cards, List<Player> players)
        {
            var continueCondition = players.Where(x => x.Status == (int)Status.Playing).Any() && !players.Where(x => x.Status == (int)Status.Won).Any();

            while (continueCondition)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].Points <= 21 && players[i].Points < 21 - players[i].RiskAversion)
                    {
                        DrawCard(players, players[i], cards);
                    }
                    if (players[i].Status == (int)Status.Won)
                    {
                        break;
                    }
                }
                continueCondition = players.Where(x => x.Status == (int)Status.Playing).ToList().Count() >= 1 && !players.Where(x => x.Status == (int)Status.Won).Any();
            }
            var determineWinnerCondition = !players.Where(x => x.Status == (int)Status.Won).Any() && players.Where(x => x.Status == (int)Status.Lost).ToList().Count() <= players.Count - 1;

            if (determineWinnerCondition)
            {
                DetermineWinner(players);
            }
        }

        private void DetermineWinner(List<Player> players)
        {
            var stoppedPlayers = players.Where(x => x.Status == (int)Status.Stopped).ToList();

            if (stoppedPlayers.Count == 1)
            {
                stoppedPlayers[0].Status = (int)Status.Won;
                WriteResults(players, stoppedPlayers[0]);
            }
            else
            {
                var winners = stoppedPlayers.Where(x=> !x.IsDealer).OrderByDescending(x => x.Points).ToList();
                var equalPointsPlayers = winners.Where(x => x.Points == winners.FirstOrDefault().Points).ToList();
                var dealer = stoppedPlayers.FirstOrDefault(x => x.IsDealer);

                if (equalPointsPlayers.Count > 1)
                {
                    if (winners[0].Points < dealer.Points)
                    {
                        dealer.Status = (int)Status.Won;
                        WriteResults(players, dealer);
                    }
                    else
                    {
                        WriteResults(players, null, true);
                    }
                }
                else
                {
                    if (winners[0].Points <= dealer.Points)
                    {
                        dealer.Status = (int)Status.Won;
                        WriteResults(players, dealer);
                    }
                    else
                    {
                        winners[0].Status = (int)Status.Won;
                        WriteResults(players, winners[0]);
                    }
                }
            }
        }

        private void DrawCard(List<Player> players, Player player, List<Card> cards)
        {
            player.Cards.Add(new Card { Name = cards[cards.Count - 1].Name, Value = cards[cards.Count - 1].Value });
            cards.RemoveAt(cards.Count - 1);
            player.Points = player.Cards.Sum(x => x.Value);
            ChangeStatus(players, player);
        }

        private void WriteResults(List<Player> players, Player winner, bool isDraw = false)
        {
            using StreamWriter file = new($"{solutionFolder}" + $@"/Outputs/Output{executionCounter}.txt");

            foreach (var pl in players)
            {
                file.WriteLine(pl.Name + " has the following cards in his hand:");
                foreach(Card cd in pl.Cards)
                {
                    file.WriteLine(cd.Name);
                }
                file.WriteLine("");
            }
            if(winner != null)
            {
                file.WriteLine(winner.Name + " has won the game");
            }
        }

        private void CleanupAndCreateOutpuDirectory()
        {
            solutionFolder = Directory.GetCurrentDirectory().ToString();
            while (!solutionFolder.EndsWith("BlackJack"))
            {
                solutionFolder = Directory.GetParent(solutionFolder).ToString();
            }

            if (Directory.Exists(solutionFolder + @"/Outputs"))
            {
                var allOutputFiles = Directory.GetFiles(solutionFolder + @"/Outputs", "Output*.txt");
                foreach (var fl in allOutputFiles)
                {
                    if (File.Exists(fl))
                    {
                        File.Delete(fl);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(solutionFolder + @"/Outputs");
            }
        }

    }
}
