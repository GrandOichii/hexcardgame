﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
    }

    [Table("decks")]
    public class DeckData
    {
        [Column("name")]
        [Key]
        public string Name { get; set; }
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

        public ExpansionData Expansion { get; set; }
        
        public CardData Card { get; set; }

        #region Foreign keys

        [ForeignKey("Expansion")]
        [Column("expansionname")]
        public string ExpansionNameKey { get; set; }
        
        [ForeignKey("Card")]
        [Column("cardname")]
        public string CardNameKey { get; set; }
        
        #endregion
    }
}
