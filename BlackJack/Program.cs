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

    public enum Rank
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
        private readonly Random zufall = new Random(Guid.NewGuid().GetHashCode()); // sicherer Seed (ggf. redundant)

        public ShoeOfDecks(int quantity)
        {
            InitShoeOfDecks(quantity);
        }

        public IReadOnlyList<Card> Cards //public Property für zb Tests
        {
            get { return shoe; }
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
            //Fisher-Yates-Shuffle rauf
            for (int i = 0; i <= shoe.Count - 2; i++)
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
            return shoe[counter - 1];

        }

    }

    public class Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
    
   public class Hand
   {
       private readonly List<Card> hand;
       public int OptimalPoints => CalculateOptimalPoints();
       public bool WasCreatedBySplit { get; private set; }
       public bool IsBust => OptimalPoints > 21;
       public bool IsBlackJack => OptimalPoints == 21 && hand.Count == 2;
       public bool CanSurrenderLate => (hand.Count == 2 && WasCreatedBySplit == false /* && Dealerhand kein BlackJack */);
       public bool CanSplitOnce => (hand.Count == 2 && hand[0].Rank == hand[1].Rank && WasCreatedBySplit == false);
       
       public Hand(List<Card> initialCards, bool wasCreatedBySplit = false)
       {
           if (initialCards == null)
               throw new ArgumentNullException("initialCards mustn't be null.");
           
           hand = new List<Card>(initialCards);
           WasCreatedBySplit = wasCreatedBySplit;
       }
       
       public IReadOnlyList<Card> Cards //public Property für zb Tests
       {
           get { return hand; }
       }

       private int CalculateOptimalPoints()
       {
           int aceCount = 0;
           int points = 0;

           foreach (Card card in hand)
           {
               switch (card.Rank)
               {
                   case Rank.Jack:
                   case Rank.Queen:
                   case Rank.King:
                       points += 10;
                       break;
                   case Rank.Ace:
                       points += 11;
                       aceCount++;
                       break;
                   default:
                       points += (int)card.Rank;
                       break;
               }
           }

           while (aceCount > 0 && points > 21)
           {
               points -= 10;
               aceCount--;
           }

           return points;
       }
      
       public void AddCard(Card card)
        {
            if (card == null)
                throw new ArgumentNullException("Card added cannot be null."); 
            hand.Add(card);
        }
       }

   public class Bet
   {
       public int Amount { get; }
       // public Hand BettedHand { get; } //SideBets

       public Bet(int amount /*, Hand hand*/)
       {
           if (amount <= 0)
               throw new ArgumentOutOfRangeException("Bet amount has to be positive.");
           Amount = amount;
           // Hand = hand;
       }
       
       
   }
   
   ///////////////////////////////////////////////////////// Oben Refactored, unten alt.

   public class Player
   {
       //player beitzt Hand, Lesbarkeit
   }


   public static class Decisions
   {
       /*
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
        */

   }
   



   public static class Game
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

    public static class Tests
    {
        public static void RunAllTests()
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
            }, true);
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
            }, true);
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
            }, true);

            if (a1.OptimalPoints != 14 && a2.OptimalPoints != 21 && a3.OptimalPoints != 21 && a4.OptimalPoints != 21 &&
                a5.OptimalPoints != 21 && a6.OptimalPoints != 30)
                throw new Exception("OptimalPoints not calculated properly.");
            Console.WriteLine("Hand - OptimalPoints test successful.");
            
            // Specific hand tests
            
            Hand b1 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Ten), 
                new Card(Suit.Diamonds, Rank.Ace),
                new Card(Suit.Clubs, Rank.Ace)
            }, true); 
            Hand b2 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Ace),
                new Card(Suit.Hearts, Rank.Three), 
                new Card(Suit.Spades, Rank.Six),
            }, true);

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
            },true);

            if (a3.CanSurrenderLate != true && s1.CanSurrenderLate != false && a6.CanSurrenderLate != false)
                throw new Exception("CanSurrenderLate1 doesn't detect properly.");
            Console.WriteLine("Hand - CanSurrenderLate1 test successful.");
            
            
            Hand s2 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Queen),
                new Card(Suit.Diamonds, Rank.Queen) 
            });
            Hand s3 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Queen),
                new Card(Suit.Diamonds, Rank.King) 
            },true);
            
            if (s2.CanSplitOnce != true && a3.CanSplitOnce != false && s1.CanSplitOnce != false && s3.CanSplitOnce != false)
                throw new Exception("CanSplitOnce doesn't detect properly.");
            Console.WriteLine("Hand - CanSplitOnce test successful.");
            
            Hand s4 = new Hand(new List<Card>
            {
                new Card(Suit.Hearts, Rank.Queen),
                new Card(Suit.Diamonds, Rank.King),
                new Card(Suit.Diamonds, Rank.Ace) 
            },true);

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

