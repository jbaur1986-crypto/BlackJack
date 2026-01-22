namespace BlackJack;

public class Hand
{
    private int _points;

    public int Points => _points;

    public void AddValue(int card)
    {
        _points += card;
    }
}
public static class PlayerDecision
{
    public static bool WantsCard(Hand hand)
    {
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
}

public static class ComputerDecision
{
    
}

public static class Rules
{
    //Doppelzug und Einfachzug
}

public class PlayerRound
{
    
}

public class WinLossRound
{
    
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}