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

    internal enum State
    {
        Betting,
        DealerUpCard,
        PlayerTurn,
        DealerHoleCard,
        DealerPlay,
        Settlement
    }

    internal interface IUser
    {
        string Name { get;}
        Role Role { get; }
    }
    internal sealed class ShoeOfDecks
    {
        private int _counter;
        private readonly List<Card> _shoe = new List<Card>();
        private readonly Random _random = new Random(Guid.NewGuid().GetHashCode()); // safe Seed (maybe redundant)

        internal ShoeOfDecks(int quantity)
        {
            InitShoeOfDecks(quantity);
        }

        internal IReadOnlyList<Card> Cards => _shoe.AsReadOnly(); //AsReadOnly() prevents deleting via casts

        private void InitShoeOfDecks(int quantity)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity),quantity, "Quantity must be at least 1.");

            _counter = 0;
            int i = 1;

            _shoe.Clear();

            while (i <= quantity)
            {
                foreach (Suit c in Enum.GetValues<Suit>())
                {
                    foreach (Rank v in Enum.GetValues<Rank>())
                    {
                        _shoe.Add(new Card(c, v));
                    }
                }

                i++;
            }

            Shuffle();
        }

        private void Shuffle()
        {
            //Fisher-Yates-Shuffle reverse
            for (int i = 0; i <= _shoe.Count - 2; i++)
            {
                int flag = _random.Next(i, _shoe.Count);
                Card marked = _shoe[i];
                _shoe[i] = _shoe[flag];
                _shoe[flag] = marked;
            }
        }


        internal Card Draw()
        {
            if (_counter >= _shoe.Count) throw new InvalidOperationException("No cards available.");
            _counter++;
            return _shoe[_counter - 1];

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
        internal Guid HandId { get;}
        private readonly List<Card> _hand;
        internal Hand(List<Card> initialCards)
        {
            if (initialCards.Count <1)
                throw new ArgumentException("Initial cards must be at least one.",nameof(initialCards));
            HandId = Guid.NewGuid();
            _hand = new List<Card>(initialCards);
        }
        internal int OptimalPoints => CalculateOptimalPoints();
        internal IReadOnlyList<Card> Cards => _hand.AsReadOnly();
        internal bool IsBust => OptimalPoints > 21;
        internal bool IsBlackJack => OptimalPoints == 21 && _hand.Count == 2;
        internal bool IsSplittable => _hand.Count == 2 &&_hand[0].Rank == _hand[1].Rank ;
       
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
            if (IsBust) throw new InvalidOperationException("No card allowed after bust.");
            if (IsBlackJack) throw new InvalidOperationException("No card allowed after blackjack.");
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
        internal Guid PlayerId { get; }
        internal Guid ?HandId { get; }
        internal decimal Amount { get; }

        internal Bet(decimal amount, Guid playerId, Guid? handId = null)
        {
            if (amount <= 0m)
                throw new ArgumentOutOfRangeException(nameof(amount),"Bet amount has to be positive.");
            PlayerId = playerId;
            HandId = handId;
            Amount = amount;
        }

    }
    internal sealed class Player : IUser
    {
        public Guid PlayerId { get; }
        public string Name { get; }
        public Role Role { get; }
        internal decimal Balance { get; private set; }

        internal Player(string name, decimal balance, Guid? playerId = null)
        {
            PlayerId = playerId ?? Guid.NewGuid();
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
        public string Name { get; }
        public Role Role { get;}

        internal Operator(string name)
        {
            Name = name;
            Role = Role.Operator;
        }
    }
    

    internal sealed class Box
    {
        internal Guid BoxId { get; }
        private readonly List<Guid> _handIds;
        internal decimal MaxBet { get; private set; }

        internal Box(List<Hand> startHands, decimal maxBet, Guid? boxId = null)
        {
            BoxId = boxId ?? Guid.NewGuid();
            _handIds = new List<Guid>();
            foreach (var h in startHands)
            {
                _handIds.Add(h.HandId);
            }
            MaxBet = maxBet;
        }

        internal void SetMaxBet(IUser user, decimal amount)
        {
            if (user.Role != Role.Operator) throw new UnauthorizedAccessException("Only operator can change MaxBet.");
            if (amount <0m) throw new ArgumentOutOfRangeException(nameof(amount),"Placed MaxBet cannot be negative.");
            MaxBet = amount;
        }

        internal void SplitHand(Guid handId, Round round)
        {
            Hand? hand = round.GetHand(handId);
            if (hand == null) throw new InvalidOperationException("Hand not found in round.");
            
            if (!hand.IsSplittable)
                throw new InvalidOperationException("Hand has to contain exact two cards which have equal rank for split.");
           
            Hand newHand = hand.SplitHandOver();
            round.AddHand(newHand);
            _handIds.Add(newHand.HandId);

        }
        internal IReadOnlyList<Guid> Hands 
        {
            get { return  _handIds.AsReadOnly(); }
        }
    }

    internal sealed class Round
    {
        internal Guid RoundId { get;}
        private readonly ShoeOfDecks _shoeOfDecks;
        private Hand _dealerHand;
        private readonly List<Hand> _hands;
        private readonly List<Box> _boxes;
        private State _state;

        internal Round(Guid? roundId = null)
        {
            RoundId = roundId ?? Guid.NewGuid();
            _shoeOfDecks = new ShoeOfDecks(6);
            _hands = new List<Hand>();
            _boxes = new List<Box>();
        }

        internal Hand CreateHand(List<Card> initialCards)
        {
            Hand hand = new Hand(initialCards);
            _hands.Add(hand);
            return hand;
        }

        internal Hand? GetHand(Guid handId)
        {
            foreach (var hand in _hands)
            {
                if (hand.HandId == handId)
                {
                    return hand;
                }
            }
            return null;
        }

        internal void AddHand(Hand hand)
        {
            if (hand == null) throw new ArgumentNullException(nameof(hand));
            _hands.Add(hand);
        }
    }
    ///////////////////////////////////////////////////////// Oben Refactored, unten alt/unfinished.
   
   internal sealed class PlayerCommands
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

            if (b1.IsBust != true || b2.IsBust != false)
                throw new Exception("IsBust doesn't detect properly.");
            Console.WriteLine("Hand - IsBust test successful.");
            
            if (a3.IsBlackJack != true || a2.IsBlackJack != false || a1.IsBlackJack != false)
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
                Bet a = new Bet(-3, Guid.NewGuid());
                throw new Exception("Expected Exception not thrown.");
            }
            catch ( ArgumentOutOfRangeException ex)
            {
                if (!ex.Message.Contains("Bet amount has to be positive."))
                    throw new Exception("Unexpected exception message.");
            }
            
                Bet b = new Bet(2, Guid.NewGuid());
            Console.WriteLine("Bet - test successful.");
            
            
        }
    }
}

