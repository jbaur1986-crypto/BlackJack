using System.ComponentModel;
using System.Linq;

namespace BlackJack
{

    internal enum Suit
    {
        Hearts,
        Diamonds,
        Spades,
        Clubs
    }

    internal enum Rank
    {
        Two ,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King, 
        Ace
    }

    internal enum Role
    {
        Player,
        Operator
    }

    internal interface IUser
    {
        string Name { get;}
        Role Role { get; }
    }
    internal sealed class ShoeOfDecks
    {
        private int counter;
        private List<Card> shoe = new List<Card>();
        private readonly Random zufall = new Random(Guid.NewGuid().GetHashCode()); // sicherer Seed (ggf. redundant)

        internal ShoeOfDecks(int quantity)
        {
            InitShoeOfDecks(quantity);
        }

        internal IReadOnlyList<Card> Cards => shoe.AsReadOnly(); //AsReadOnly() prevents deleting via casts

        private void InitShoeOfDecks(int quantity)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity),"Quantity must be at least 1.");

            counter = 0;
            int i = 1;

            shoe.Clear();

            while (i <= quantity)
            {
                foreach (Suit c in Enum.GetValues<Suit>())
                {
                    foreach (Rank v in Enum.GetValues<Rank>())
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
            //Fisher-Yates-Shuffle reverse
            for (int i = 0; i <= shoe.Count - 2; i++)
            {
                int flag = zufall.Next(i, shoe.Count);
                Card marked = shoe[i];
                shoe[i] = shoe[flag];
                shoe[flag] = marked;
            }
        }


        internal Card Draw()
        {
            if (counter >= shoe.Count) throw new InvalidOperationException("No cards available.");
            counter++;
            return shoe[counter - 1];

        }

    }

    internal readonly record struct Card(Suit Suit, Rank Rank)
    {
        internal int GetBlackJackValue()
        {
            switch (Rank)
            {
                case Rank.Two: return 2;
                case Rank.Three: return 3;
                case Rank.Four: return 4;
                case Rank.Five: return 5;
                case Rank.Six: return 6;
                case Rank.Seven: return 7;
                case Rank.Eight: return 8;
                case Rank.Nine: return 9;
                case Rank.Ten:
                case Rank.Jack:
                case Rank.Queen:
                case Rank.King: return 10;
                case Rank.Ace: return 11;
                default: throw new ArgumentOutOfRangeException(nameof(Rank),"Card rank not valid. Probably illegal new card added in enum");
            }
        }

       public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }

    internal sealed class Hand
    {
        private List<Card> _hand;
        internal int OptimalPoints => CalculateOptimalPoints();
        internal IReadOnlyList<Card> Cards => _hand.AsReadOnly();
        internal bool IsBust => OptimalPoints > 21;
        internal bool IsBlackJack => OptimalPoints == 21 && _hand.Count == 2;
        internal bool IsSplittable => _hand.Count == 2 &&_hand[0].Rank == _hand[1].Rank ;

        internal Hand(List<Card> initialCards)
        {
            if (initialCards == null)
                throw new ArgumentNullException(nameof(initialCards),"Initial cards must not be null.");

            _hand = new List<Card>(initialCards);
        }

        private int CalculateOptimalPoints()
        {
            int aceCount = 0;
            int points = 0;

            foreach (Card card in _hand)
            {
                if (card.Rank == Rank.Ace)
                {
                        points += 11;
                        aceCount++;
                }
                else
                {
                     points += card.GetBlackJackValue();
                }
            }

            while (aceCount > 0 && points > 21)
            {
                points -= 10;
                aceCount--;
            }

            return points;
        }

        internal void AddCard(Card card)
        {
            _hand.Add(card);
        }

        internal Hand SplitHandOver()
        {
            Card handOverCard;
            if (!IsSplittable) throw new InvalidOperationException("No split due to either amount or match of cards.");
            handOverCard = _hand[1];
            _hand.RemoveAt(1);
            List<Card> splitList = new List<Card>();
            splitList.Add(handOverCard);
            Hand splitHand = new Hand(splitList);
            return splitHand;
        }
        
    }

    internal sealed class Bet
    {
        internal decimal Amount { get; init; }
        internal Hand? BettedHand { get; init; }

        internal Bet(decimal amount, Hand? hand = null)
        {
            if (amount <= 0m)
                throw new ArgumentOutOfRangeException(nameof(amount),"Bet amount has to be positive.");
            Amount = amount;
            BettedHand = hand;
        }

    }
    internal sealed class Player : IUser
    {
        public string Name { get; init; }
        public Role Role { get; init; }
        internal decimal Balance { get; private set; }

        internal Player(string name, decimal balance)
        {
            Name = name;
            Role = Role.Player;
            Balance = balance;
        }

        internal void AddBalance(decimal amount)
        {
            if (amount < 0m) throw new ArgumentOutOfRangeException(nameof(amount),"Added amount cannot be negative.");
            Balance += amount;
        }

        internal void PlaceBet(decimal amount)
        {
            if (amount < 0m) throw new ArgumentOutOfRangeException(nameof(amount),"Placed bet cannot be negative.");
            if (amount > Balance) throw new InvalidOperationException("Not enough balance for bet.");
            Balance -= amount;
        }
    }

    internal sealed class Operator : IUser
    {
        public string Name { get; init; }
        public Role Role { get; init; }

        internal Operator(string name)
        {
            Name = name;
            Role = Role.Operator;
        }
    }
    

    internal sealed class Box
    {
        private readonly List<Hand> _hands = new();
        internal decimal MaxBet { get; private set; }

        internal void SetMaxBet(IUser user, decimal amount)
        {
            if (user.Role != Role.Operator) throw new UnauthorizedAccessException("Only operator can change maxbet.");
            if (amount <0m) throw new ArgumentOutOfRangeException(nameof(amount),"Placed MaxBet cannot be negative.");
            MaxBet = amount;
        }

        internal void SplitHand(Hand hand)
        {
            if (!hand.IsSplittable)
                throw new InvalidOperationException("Hand has to contain exact two cards which have equal rank for split.");
            if (!_hands.Contains(hand)) throw new InvalidOperationException("Box has to contain target hand for split.");
            Hand newHand = hand.SplitHandOver();
            _hands.Add(newHand);

        }
        internal IReadOnlyList<Hand> Hands 
        {
            get { return _hands.AsReadOnly(); }
        }
    }
    
    ///////////////////////////////////////////////////////// Oben Refactored, unten alt/unfinished.
   internal interface IGameRules
   {
       internal bool CanSurrenderLate(Hand hand, Card upcard);
   }

   internal static class StandardBjRules
   {
      
   }
   internal sealed class Decisions
   {
       /*
       public bool PlayerWantsCard(Hand hand)
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

       public bool ComputerWantsCard(Hand hand)
       {
           return hand.Points < 16;
       }

       public bool WinLossDecision()
       {
           return true; //fehlt
       }
        */

   }
   



   internal sealed class Game
   {
       //UpCardDealer, PlayPlayerRound, HoleCardDealer, EvaluateWinLoss
   }
 /////////////////////////////////////////////////////////////////////////////////////////////////////////////
 class Program
    {
        static void Main(string[] args)
        {

            Tests.RunAllTests();
           
            
        }
    }
/////////////////////////////////////////////////////////////////////////////////////////////////////////////

    internal static class Tests
    {
        internal static void RunAllTests()
        {
            ShoeTest();
            HandTest();
            BetTest();
        }

        private static void ShoeTest()
        {
            //Test 52 cards?
            try
            {
                ShoeOfDecks shoe1 = new ShoeOfDecks(1);
                foreach (var _ in Enumerable.Range(0, 53))
                {
                    shoe1.Draw();
                }

                throw new Exception("Expected exception not thrown. Should be thrown because of 52 cards and 53 draws.");
            }
            catch (InvalidOperationException ex)
            {
                if (!ex.Message.Contains("No cards available.")) throw new Exception("Unexpected exception message.");
                Console.WriteLine("Shue - Draw test successful.");
            }
            //Test cards match in correct number?
            
            ShoeOfDecks shoe2 = new ShoeOfDecks(5);
            foreach (Card c in shoe2.Cards)
            {
                int cardCounter = 0;
                foreach (Card d in shoe2.Cards)
                {
                    if (c.Suit == d.Suit && c.Rank == d.Rank)
                    {
                        cardCounter++;
                    }
                }

                if (cardCounter != 5)
                {
                    throw new InvalidOperationException("Card number does not match.");
                }
               
            }
            Console.WriteLine("Shue - Card match test successful. ");
        }

        private static void HandTest()
        {
            //OptimalPoints test first
            
            Hand a1 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Ace),
                new Card(Suit.Diamonds, Rank.Ace), 
                new Card(Suit.Spades, Rank.Ace),
                new Card(Suit.Clubs, Rank.Ace)
            });
            Hand a2 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Ace),
                new Card(Suit.Diamonds, Rank.Ace), 
                new Card(Suit.Spades, Rank.Nine)
            });
            Hand a3 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Queen),
                new Card(Suit.Diamonds, Rank.Ace) 
            });
            Hand a4 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Seven),
                new Card(Suit.Diamonds, Rank.Seven), 
                new Card(Suit.Spades, Rank.Seven)
            });
            Hand a5 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Ace), 
                new Card(Suit.Spades, Rank.Ten)
            });
            Hand a6 = new Hand(new List<Card>
            {
                new Card(Suit.Diamonds, Rank.Ten), 
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Clubs, Rank.Ten)
            });

            if (a1.OptimalPoints != 14 || a2.OptimalPoints != 21 || a3.OptimalPoints != 21 || a4.OptimalPoints != 21 ||
                a5.OptimalPoints != 21 || a6.OptimalPoints != 30)
                throw new Exception("OptimalPoints not calculated properly.");
            Console.WriteLine("Hand - OptimalPoints test successful.");
            
            // Specific hand tests
            
            Hand b1 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Ten), 
                new Card(Suit.Diamonds, Rank.Ace),
                new Card(Suit.Clubs, Rank.Ace)
            }); 
            Hand b2 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Ace),
                new Card(Suit.Hearts, Rank.Three), 
                new Card(Suit.Spades, Rank.Six),
            });

            if (b1.IsBust != true && b2.IsBust != false)
                throw new Exception("IsBust doesn't detect properly.");
            Console.WriteLine("Hand - IsBust test successful.");
            
            if (a3.IsBlackJack != true && a2.IsBlackJack != false && a1.IsBlackJack != false)
                throw new Exception("IsBlackJack doesn't detect properly.");
            Console.WriteLine("Hand - IsBlackJack test successful.");
            
            Hand s1 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Queen),
                new Card(Suit.Diamonds, Rank.Queen) 
            });

            /*if (a3.CanSurrenderLate != true && s1.CanSurrenderLate != false && a6.CanSurrenderLate != false)
                throw new Exception("CanSurrenderLate1 doesn't detect properly.");
            Console.WriteLine("Hand - CanSurrenderLate1 test successful.");
            */
            
            Hand s2 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Queen),
                new Card(Suit.Diamonds, Rank.Queen) 
            });
            Hand s3 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Queen),
                new Card(Suit.Diamonds, Rank.King) 
            });
            
            /*if (s2.CanSplitOnce != true && a3.CanSplitOnce != false && s1.CanSplitOnce != false && s3.CanSplitOnce != false)
                throw new Exception("CanSplitOnce doesn't detect properly.");
            Console.WriteLine("Hand - CanSplitOnce test successful.");
            */
            
            Hand s4 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Queen),
                new Card(Suit.Diamonds, Rank.King),
                new Card(Suit.Diamonds, Rank.Ace) 
            });

            s3.AddCard(new Card(Suit.Diamonds, Rank.Ace));
           if ( s3.Cards.Count != s4.Cards.Count)
                throw new Exception("AddCard() calculates wrong number of cards.");
           if (s3.Cards[2].Suit != s4.Cards[2].Suit && s3.Cards[2].Rank != s4.Cards[2].Rank)
               throw new Exception("AddCard() has problems with card content of the added card. ");
            Console.WriteLine("Hand - AddCard() test successful.");

            
        }

        private static void BetTest()
        {
            try
            {
                Bet a = new Bet(-3);
                throw new Exception("Expected Exception not throwm");
            }
            catch ( ArgumentOutOfRangeException ex)
            {
                if (!ex.Message.Contains("Bet amount has to be positive."))
                    throw new Exception("Unexpected exception message.");
            }
            
                Bet b = new Bet(2);
            Console.WriteLine("Bet - test successful.");
            
            
        }
    }
}

