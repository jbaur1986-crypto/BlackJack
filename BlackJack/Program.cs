using System.ComponentModel;

namespace BlackJack
{

    public enum Color
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

    public static class CardSource
    {
        private static Random zufall = new Random();
        private static List<Card> deck = new List<Card>();

        private static void InitDeck()
        {
            deck.Clear();
            
            foreach (Color c in Enum.GetValues(typeof(Color)))
            foreach (Value v in Enum.GetValues(typeof(Value)))
                deck.Add(new Card(c, v));

            Shuffle();
        }

        private static void Shuffle()
        {
            //Fisher-Yates-Shuffle
        }
    }

    public class Card
    {
        public Color Color { get; }
        public Value Value { get; }

        public Card (Color color, Value value)
        {
            Color = color;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Value} of {Color}";
        }
    }

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


    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}

