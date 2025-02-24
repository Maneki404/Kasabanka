using Kasabanka.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Kasabanka.Controllers
{
    public class BankController : Controller
    {
        // Display all banks
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetBanks()
        {
            List<BankItem> banks = new List<BankItem>();

            using (SqlConnection connection = new SqlConnection(DbHelper.connection))
            {
                connection.Open();
                using (SqlCommand bringBanks = new SqlCommand("SELECT b.Id, b.BankName, b.BankCode from KASABANKA_BANK b", connection))
                {
                    using (SqlDataReader dr = bringBanks.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            BankItem bank = new BankItem()
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Name = Convert.ToString(dr["BankName"]),
                                Code = Convert.ToString(dr["BankCode"]),
                            };
                            banks.Add(bank);
                        }
                    }
                }
            }

            banks = banks.OrderBy(x => x.Name).ToList();

            return Json(new { data = banks }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateBank(BankItem bank)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DbHelper.connection))
                {
                    connection.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter("SELECT TOP 1 * FROM KASABANKA_BANK", connection))
                    {
                        using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
                        {
                            DataSet ds1 = new DataSet();
                            da.Fill(ds1, "KASABANKA_BANK");

                            DataRow dt = ds1.Tables["KASABANKA_BANK"].NewRow();

                            dt["BankName"] = bank.Name;
                            dt["BankCode"] = bank.Code;

                            ds1.Tables["KASABANKA_BANK"].Rows.Add(dt);
                            da.Update(ds1, "KASABANKA_BANK");
                        }
                    }
                }
                return Json(new { Success = true, Message = "Yeni banka oluşturuldu." });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Beklenmeyen bir hata oluştu. Hata: {ex.Message}" });
            }
        }
    }
}