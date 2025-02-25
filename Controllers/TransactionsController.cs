using Kasabanka.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Kasabanka.Controllers
{
    public class TransactionsController : Controller
    {
        // GET: Transactions
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetTransactions()
        {
            List<Transaction> transactions = new List<Transaction>();


            using (SqlConnection connection = new SqlConnection(DbHelper.connection))
            {
                connection.Open();
                using (SqlCommand bringTransactions = new SqlCommand("SELECT * from KASABANKA_TRANSACTION", connection))
                {
                    using (SqlDataReader dr = bringTransactions.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Transaction transaction = new Transaction()
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Type = Convert.ToString(dr["Type"]),
                                No = Convert.ToString(dr["No"]),
                                Description = Convert.ToString(dr["Description"]),
                                Currency = Convert.ToString(dr["Currency"]),
                                Amount = Convert.ToString(dr["Amount"]),
                                SafeOrBank = Convert.ToString(dr["SafeOrBank"]),
                                Date = Convert.ToDateTime(dr["Date"]),
                            };
                            transactions.Add(transaction);
                        }
                    }
                }
                transactions = transactions.OrderBy(x => x.No).ToList();
                return Json(new { data = transactions }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetTransactionById(String id)
        {
            Transaction transaction = new Transaction();

            using (SqlConnection connection = new SqlConnection(DbHelper.connection))
            {
                connection.Open();
                using (SqlCommand bringTransaction = new SqlCommand("SELECT t.Id, t.Type, t.No, t.Description, t.Currency, t.Amount, t.SafeOrBank, t.Date FROM KASABANKA_TRANSACTION t WHERE ID = @id", connection))
                {
                    bringTransaction.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader dr = bringTransaction.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            transaction = new Transaction()
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Type = Convert.ToString(dr["Type"]),
                                No = Convert.ToString(dr["No"]),
                                Description = Convert.ToString(dr["Description"]),
                                Currency = Convert.ToString(dr["Currency"]),
                                Amount = Convert.ToString(dr["Amount"]),
                                SafeOrBank = Convert.ToString(dr["SafeOrBank"]),
                                Date = Convert.ToDateTime(dr["Date"]),
                            }
                            ;
                        }
                    }
                }
            }

            return Json(transaction, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateTransaction(Transaction transaction)
        {
            try
            {
                var amount = float.Parse(transaction.Amount.ToString(),CultureInfo.InvariantCulture);
                using (SqlConnection connection = new SqlConnection(DbHelper.connection))
                {

                    connection.Open();
                    string query = "INSERT INTO KASABANKA_TRANSACTION (Type, No, Description, Currency, Amount, SafeOrBank, Date) VALUES (@Type, @No, @Description, @Currency, @Amount, @SafeOrBank, @Date)";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Type", transaction.Type);
                        cmd.Parameters.AddWithValue("@No", transaction.No);
                        cmd.Parameters.AddWithValue("@Description", transaction.Description);
                        cmd.Parameters.AddWithValue("@Currency", transaction.Currency);
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@SafeOrBank", transaction.SafeOrBank);
                        cmd.Parameters.AddWithValue("@Date", transaction.Date);

                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { Success = true, Message = "Yeni işlem oluşturuldu." });

            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Beklenmeyen bir hata oluştu. Hata: {ex.Message}" });
            }
        }
    }
}