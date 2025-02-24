using System.Configuration;

namespace Kasabanka.Models
{
    public class DbHelper
    {
        public static readonly string connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
    }
}