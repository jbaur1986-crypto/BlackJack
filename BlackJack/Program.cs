using System.ComponentModel;

namespace BlackJack
{
                     
    public enum Suit
    {
        Hearts,
        Diamonds,
        Spades,
        Clubs
    }
                     
    public enum Value
    {
        Two = 2,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Ace,
        Jack,
        Queen,
        King,
                     
    }

                        
    public class ShoeOfDecks
    {
        private int counter;
        private List<Card> shoe = new List<Card>();
        private readonly Random zufall = new Random(Guid.NewGuid().GetHashCode());// sicherer Seed
                             
        public ShoeOfDecks(int quantity)
        {
            InitShoeOfDecks(quantity);
        }
                     
        private void InitShoeOfDecks(int quantity)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException("Quantity must be at least 1.");
            
            counter = 0;
            int i = 1;
            
            shoe.Clear();      
            
            while (i <= quantity)
            {
                foreach (Suit c in Enum.GetValues<Suit>())
                {
                    foreach (Value v in Enum.GetValues<Value>())
                    {
                        shoe.Add(new Card(c, v));
                    }
                }

                i++;
            }

            Shuffle();
        }
                     
        private void Shuffle()
        {
            //Fisher-Yates-Shuffle rauf
            for (int i = 0; i <= shoe.Count-2; i++)
            {
                int flag = zufall.Next(i, shoe.Count);
                Card marked = shoe[i];
                shoe[i] = shoe[flag];
                shoe[flag] = marked;
            }
        }
                             

        public Card Draw()
        {
            if (counter >= shoe.Count) throw new InvalidOperationException("No cards available.");
            counter++;
            return shoe[counter-1];
           
        }
                             
    }
                     
    public class Card
    {
        public Suit Suit { get; }
        public Value Value { get; }
                     
        public Card (Suit suit, Value value)
        {
            Suit = suit;
            Value = value;
        }
                     
        public override string ToString()
        {
            return $"{Value} of {Suit}";
        }
    }
     ///////////////////////////////////////////////////////// Oben Refactored, unten alt.
     /* 
    public class Hand
    {
        public int Points { get; private set; }

        public Hand()
        {
            Points = 0;
        }

        public void AddValue(int card)
        {
            if (card < 0)
                throw new ArgumentException("card value cannot be negative.");
            Points += card;
        }
    }


    public class Player
    {
        //player beitzt Hand, Lesbarkeit
    }


    public static class Decisions
    {
        public static bool PlayerWantsCard(Hand hand)
        {
            if (hand.Points >= 22) return false;

            Console.WriteLine($"Punktestand: {hand.Points}");
            Console.WriteLine("Möchten Sie noch eine Karte ziehen? (j/n)");
            char input = Char.ToLower(Console.ReadKey().KeyChar);
            while (input != 'j' && input != 'n')
            {
                Console.WriteLine("Bitte richtig eingeben - 'j' für Karte 'n' für keine Karte ziehen! (j/n)");
                input = Char.ToLower(Console.ReadKey().KeyChar);
            }

            return input == 'j';
        }

        public static bool ComputerWantsCard(Hand hand)
        {
            return hand.Points < 16;
        }

        public static bool WinLossDecision()
        {
            return true; //fehlt
        }


    }


    public static class Rules
    {
        public static bool CanDraw(Hand hand)
        {
            return true; //Logik fehlt noch!
        }

        public static bool IsBust(Hand hand)
        {
            return true; //fehlt noch
        }
    }




    public static class Game
    {
        //PlayPlayerRound, ComputerRound, EvaluateWinLoss
    }
    */
     
    class Program
    {
        static void Main(string[] args)
        {
            ShoeOfDecks shoe1 = new ShoeOfDecks(1);
            foreach (var _ in Enumerable.Range(0, 52))
            {
                shoe1.Draw();
            }
        }
    }
}

