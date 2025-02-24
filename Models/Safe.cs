using System;
using System.ComponentModel.DataAnnotations;

namespace Kasabanka.Models
{
    public class Safe
    {
        [Required]
        [Key]
        public int Id { get; set; }
        [Required]
        public String Name { get; set; }
        [Required]
        public String Code { get; set; }
        public double TL { get; set; }
        [Required]
        public double USD { get; set; }
        [Required]
        public double EUR { get; set; }
        [Required]
        public double GBP { get; set; }
    }
    public class SafeItem
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