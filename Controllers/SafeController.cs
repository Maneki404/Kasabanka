using Kasabanka.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Kasabanka.Controllers
{
    public class SafeController : Controller
    {
        // Display all safes
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetSafes()
        {
            List<SafeItem> safes = new List<SafeItem>();

            using (SqlConnection connection = new SqlConnection(DbHelper.connection))
            {
                connection.Open();
                using (SqlCommand bringSafes = new SqlCommand("SELECT s.Id, s.SafeName, s.SafeCode from KASABANKA_SAFE s", connection))
                {
                    using (SqlDataReader dr = bringSafes.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            SafeItem safe = new SafeItem()
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Name = Convert.ToString(dr["SafeName"]),
                                Code = Convert.ToString(dr["SafeCode"]),
                            };
                            safes.Add(safe);
                        }
                    }
                }
            }

            safes = safes.OrderBy(x => x.Name).ToList();

            return Json(new { data = safes }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSafeById(String id)
        {
            SafeItem safe = new SafeItem();

            using (SqlConnection connection = new SqlConnection(DbHelper.connection))
            {
                connection.Open();
                using (SqlCommand bringSafe = new SqlCommand("SELECT s.Id, s.SafeName, s.SafeCode from KASABANKA_SAFE s WHERE ID = @id", connection))
                {
                    bringSafe.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader dr = bringSafe.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            safe = new SafeItem()
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Name = Convert.ToString(dr["SafeName"]),
                                Code = Convert.ToString(dr["SafeCode"]),
                            };
                        }
                    }
                }
            }

            return Json(safe, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateSafe(SafeItem safe)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DbHelper.connection))
                {
                    connection.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter("SELECT TOP 1 * FROM KASABANKA_SAFE", connection))
                    {
                        using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
                        {
                            DataSet ds1 = new DataSet();
                            da.Fill(ds1, "KASABANKA_SAFE");

                            DataRow dt = ds1.Tables["KASABANKA_SAFE"].NewRow();

                            dt["SafeName"] = safe.Name;
                            dt["SafeCode"] = safe.Code;

                            ds1.Tables["KASABANKA_SAFE"].Rows.Add(dt);
                            da.Update(ds1, "KASABANKA_SAFE");
                        }
                    }
                }
                return Json(new { Success = true, Message = "Yeni kasa oluşturuldu." });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Beklenmeyen bir hata oluştu. Hata: {ex.Message}" });
            }
        }

        public JsonResult DeleteSafe(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DbHelper.connection))
                {
                    connection.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM KASABANKA_SAFE WHERE ID = @id", connection))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@id", id);
                        using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds, "KASABANKA_SAFE");

                            if (ds.Tables["KASABANKA_SAFE"].Rows.Count > 0)
                            {
                                ds.Tables["KASABANKA_SAFE"].Rows[0].Delete();
                                da.Update(ds, "KASABANKA_SAFE");

                                return Json(new { Success = true, Message = "Kasa kaydı silindi." });
                            }
                            else
                            {
                                return Json(new { Success = false, Message = "Silinecek kasa bulunamadı." });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Hata oluştu: {ex.Message}" });
            }
        }

        public JsonResult UpdateSafe(SafeItem safe)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DbHelper.connection))
                {
                    connection.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM KASABANKA_SAFE WHERE ID = @id", connection))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@id", safe.Id);
                        using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds, "KASABANKA_SAFE");

                            if (ds.Tables["KASABANKA_SAFE"].Rows.Count > 0)
                            {
                                DataRow row = ds.Tables["KASABANKA_SAFE"].Rows[0];
                                row["SafeName"] = safe.Name;
                                row["SafeCode"] = safe.Code;

                                da.Update(ds, "KASABANKA_SAFE");

                                return Json(new { Success = true, Message = "Kasa bilgileri güncellendi." });
                            }
                            else
                            {
                                return Json(new { Success = false, Message = "Belirtilen kasa bulunamadı." });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Hata oluştu: {ex.Message}" });
            }
        }
    }
}
