using System;
using System.ComponentModel.DataAnnotations;

namespace Kasabanka.Models
{
    public class Transaction
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public String Type { get; set; }
        [Required]
        public String No { get; set; }
        [Required]
        public String Description { get; set; }
        [Required]
        public String Currency { get; set; }
        [Required]
        public String Amount { get; set; }
        [Required]
        public String SafeOrBank { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public bool IsBank { get; set; }
        [Required]
        public int SafeOrBankID { get; set; }

    }
}