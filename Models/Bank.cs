using System;
using System.ComponentModel.DataAnnotations;

namespace Kasabanka.Models
{
    public class Bank
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public String Code { get; set; }
        [Required]
        public double TL { get; set; }
        [Required]
        public double USD { get; set; }
        [Required]
        public double EUR { get; set; }
        [Required]
        public double GBP { get; set; }
    }

    public class BankItem
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public String Code { get; set; }
    }
}