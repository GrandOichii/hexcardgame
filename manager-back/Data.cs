﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using core.cards;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace manager_back
{

    [Table("cards")]
    public class CardData
    {
        [Column("name")]
        [Key]
        public string Name { get; set; }

        [Column("cost")]
        public int Cost { get; set; }
        [Column("type")]
        public string Type { get; set; }
        [Column("text")]
        public string Text { get; set; }
        [Column("power")]
        public int Power { get; set; }
        [Column("life")]
        public int Life { get; set; }
        [Column("deckusable")]
        public bool DeckUsable { get; set; }
        [Column("script")]
        public string Script { get; set; }

        [NotMapped]
        public List<ExpansionData>? Expansions { get; set; }
    }

    [Table("decks")]
    public class DeckData
    {
        [Column("name")]
        [Key]
        public string Name { get; set; }
        [Column("description")]
        public string Description { get; set; }

        [NotMapped]
        public List<DeckCardData> Cards { get; set; }
    }

    [Table("expansions")]
    public class ExpansionData
    {
        [Column("name")]
        [Key]
        public string Name { get; set; }
    }

    [Table("expansioncards")]
    public class ExpansionCardData
    {
        [Column("id")]
        [Key]
        public int ID { get; set; }

        public ExpansionData? Expansion { get; set; }
        
        public CardData Card { get; set; }

        #region Foreign keys

        [ForeignKey("Expansion")]
        [Column("expansionname")]
        public string ExpansionNameKey { get; set; }
        
        [ForeignKey("Card")]
        [Column("cardname")]
        public string? CardNameKey { get; set; }
        
        #endregion

        public ExpansionCard ToCard() {
            var result = new ExpansionCard();

            result.Name = Card.Name;
            result.Cost = Card.Cost;
            result.Type = Card.Type;
            result.Text = Card.Text;
            result.Power = Card.Power;
            result.Life = Card.Life;
            result.Expansion = ExpansionNameKey;
            result.Script = Card.Script;

            return result;
        }
    }

    [Table("deckcards")]
    [PrimaryKey("CardIDKey", "Amount")]
    public class DeckCardData
    {
        [Column("amount")]
        public int Amount { get; set; }

        public DeckData? Deck { get; set; }

        public ExpansionCardData Card { get; set; }
        
        #region Foreign keys

        [ForeignKey("Deck")]
        [Column("deckname")]
        public string DeckNameKey { get; set; }

        [ForeignKey("Card")]
        [Column("cardid")]
        public int CardIDKey { get; set; }

        #endregion
    }

    [Table("matchconfigs")]
    public class MatchConfigData
    {
        [Column("name")]
        [Key]
        public string Name { get; set; }

        [Column("turnstartdraw")]
        public int TurnStartDraw { get; set; }
        [Column("seed")]
        public int Seed { get; set; }
        [Column("setupscript")]
        public string SetupScript { get; set; }
        [Column("addons")]
        public string[] Addons { get; set; }
        [Column("map")]
        public string Map { get; set; }
    }

}
