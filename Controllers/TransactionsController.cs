using Kasabanka.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
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

        // TransactionsController.cs
        public JsonResult GetAmounts(string currency, int safeOrBankId, bool isBank)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DbHelper.connection))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
                SELECT COALESCE(SUM(CAST(Amount AS DECIMAL(18,2))), 0) as Total
                FROM KASABANKA_TRANSACTION 
                WHERE SafeOrBankID = @safeOrBankId 
                AND IsBank = @isBank 
                AND Currency = @currency", connection))
                    {
                        cmd.Parameters.AddWithValue("@safeOrBankId", safeOrBankId);
                        cmd.Parameters.AddWithValue("@isBank", isBank);
                        cmd.Parameters.AddWithValue("@currency", currency);

                        decimal amount = Convert.ToDecimal(cmd.ExecuteScalar());
                        return Json(new { success = true, data = amount }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        public JsonResult GetTransactions(string safeOrBank, string safeOrBankSelection, string type, string currency, string startDate, string endDate)
        {
            try
            {
                List<Transaction> transactions = new List<Transaction>();

                using (SqlConnection connection = new SqlConnection(DbHelper.connection))
                {
                    connection.Open();
                    string query = @"
                    SELECT 
                        T.Id,
                        T.Type,
                        T.No,
                        T.Description,
                        T.Currency,
                        T.Amount,
                        T.Date,
                        T.IsBank,
                        T.SafeOrBankID,
                        CASE 
                            WHEN T.IsBank = 1 THEN B.BankCode + ' - ' + B.BankName
                            ELSE S.SafeCode + ' - ' + S.SafeName
                        END as SafeOrBank
                    FROM KASABANKA_TRANSACTION T
                    LEFT JOIN KASABANKA_BANK B ON T.SafeOrBankID = B.ID AND T.IsBank = 1
                    LEFT JOIN KASABANKA_SAFE S ON T.SafeOrBankID = S.ID AND T.IsBank = 0
                    WHERE 1=1";

                    List<SqlParameter> parameters = new List<SqlParameter>();

                    if (!string.IsNullOrEmpty(safeOrBank))
                    {
                        if (safeOrBank == "Banka")
                        {
                            query += " AND T.IsBank = 1";
                        }
                        else if (safeOrBank == "Kasa")
                        {
                            query += " AND T.IsBank = 0";
                        }
                    }

                    if (!string.IsNullOrEmpty(safeOrBankSelection))
                    {
                        query += @" AND (
                    (T.IsBank = 1 AND B.BankCode = @safeOrBankSelection) OR
                    (T.IsBank = 0 AND S.SafeCode = @safeOrBankSelection)
                )";
                        parameters.Add(new SqlParameter("@safeOrBankSelection", safeOrBankSelection));
                    }

                    if (!string.IsNullOrEmpty(type))
                    {
                        query += " AND T.Type = @type";
                        parameters.Add(new SqlParameter("@type", type));
                    }

                    if (!string.IsNullOrEmpty(currency))
                    {
                        query += " AND T.Currency = @currency";
                        parameters.Add(new SqlParameter("@currency", currency));
                    }

                    if (!string.IsNullOrEmpty(startDate))
                    {
                        query += " AND T.Date >= @startDate";
                        parameters.Add(new SqlParameter("@startDate", DateTime.Parse(startDate)));
                    }

                    if (!string.IsNullOrEmpty(endDate))
                    {
                        query += " AND T.Date <= @endDate";
                        parameters.Add(new SqlParameter("@endDate", DateTime.Parse(endDate).AddDays(1).AddSeconds(-1)));
                    }

                    query += " ORDER BY T.Date DESC";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                transactions.Add(new Transaction
                                {
                                    Id = dr.GetInt32(dr.GetOrdinal("Id")),
                                    Type = dr.GetString(dr.GetOrdinal("Type")),
                                    No = dr.GetString(dr.GetOrdinal("No")),
                                    Description = dr.GetString(dr.GetOrdinal("Description")),
                                    Currency = dr.GetString(dr.GetOrdinal("Currency")),
                                    Amount = dr.GetDecimal(dr.GetOrdinal("Amount")).ToString(CultureInfo.InvariantCulture),
                                    SafeOrBank = dr.GetString(dr.GetOrdinal("SafeOrBank")),
                                    Date = dr.GetDateTime(dr.GetOrdinal("Date")),
                                    IsBank = dr.GetBoolean(dr.GetOrdinal("IsBank")),
                                    SafeOrBankID = dr.GetInt32(dr.GetOrdinal("SafeOrBankID"))
                                });
                            }
                        }
                    }
                }

                return Json(new
                {
                    draw = Request.QueryString["draw"],
                    recordsTotal = transactions.Count,
                    recordsFiltered = transactions.Count,
                    data = transactions
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // NOT USING
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
                var amount = float.Parse(transaction.Amount.ToString(), CultureInfo.InvariantCulture);

                // SafeOrBank değerinden ID ve IsBank değerlerini çıkar
                string[] safeOrBankParts = transaction.SafeOrBank.Split(new[] { " - " }, StringSplitOptions.None);
                string code = safeOrBankParts[0];
                bool isBank = transaction.Type == "Gelen Havale" || transaction.Type == "Giden Havale";

                int safeOrBankId;
                using (SqlConnection connection = new SqlConnection(DbHelper.connection))
                {
                    connection.Open();

                    // ID'yi bul
                    string idQuery = isBank ?
                "SELECT ID FROM KASABANKA_BANK WHERE BankCode = @code" :
                "SELECT ID FROM KASABANKA_SAFE WHERE SafeCode = @code";

                    using (SqlCommand idCmd = new SqlCommand(idQuery, connection))
                    {
                        idCmd.Parameters.AddWithValue("@code", code);
                        safeOrBankId = Convert.ToInt32(idCmd.ExecuteScalar());
                    }

                    // İşlemi kaydet
                    string query = @"INSERT INTO KASABANKA_TRANSACTION 
                (Type, No, Description, Currency, Amount, SafeOrBankID, IsBank, Date) 
                VALUES 
                (@Type, @No, @Description, @Currency, @Amount, @SafeOrBankID, @IsBank, @Date)";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Type", transaction.Type);
                        cmd.Parameters.AddWithValue("@No", transaction.No);
                        cmd.Parameters.AddWithValue("@Description", transaction.Description);
                        cmd.Parameters.AddWithValue("@Currency", transaction.Currency);
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@SafeOrBankID", safeOrBankId);
                        cmd.Parameters.AddWithValue("@IsBank", isBank);
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