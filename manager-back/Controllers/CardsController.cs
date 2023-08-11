using Microsoft.AspNetCore.Mvc;
using core.cards;

namespace manager_back.Controllers;

public class CardQuery {
    public string Name { get; set; }="";
    public string Expansion { get; set; }="";
}

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<CardsController> _logger;

    public CardsController(ILogger<CardsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<Card> GetAll(string name="", string expansion="")
    {
        System.Console.WriteLine(name, " ", expansion);
        foreach (var card in Global.CMaster.GetAll()) 
            if (CardMatches(card, name, expansion))
                yield return card;

    }

    public bool CardMatches(Card card, string name, string expansion) {
        if (expansion.Length > 0 && card.Expansion.ToLower() != expansion.ToLower()) return false;
        if (name.Length > 0 && card.Name.ToLower() != name.ToLower()) return false;
        return true;
    }
}
