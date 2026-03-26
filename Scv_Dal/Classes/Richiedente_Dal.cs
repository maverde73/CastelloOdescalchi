using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Model;
using Scv_Model.Common;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Objects;
using System.Data;
using Scv_Entities;
using System.Collections.ObjectModel;



namespace Scv_Dal
{
	public class Richiedente_Dal
	{
        //Scv_Entities.SCV_DEVEntities _context = null;

        public Richiedente_Dal()
        {
            //_context = new SCV_DEVEntities();
            //_context.ContextOptions.LazyLoadingEnabled = false;
        }

        public ObservableCollection<Richiedente> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber,out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				ObservableCollection<Richiedente> ItemsList = new ObservableCollection<Richiedente>();

                ExpressionBuilder<Richiedente> eb = new ExpressionBuilder<Richiedente>();

                ObjectQuery<Richiedente> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.Richiedenti.Include("LK_Titolo").AsQueryable().Where(expQuery) as ObjectQuery<Richiedente>;
                    }
                    else
                    {
						oQItemsList = _context.Richiedenti.Include("LK_Titolo").AsQueryable() as ObjectQuery<Richiedente>;
                    }
                }
                else
					oQItemsList = _context.Richiedenti.Include("LK_Titolo").AsQueryable() as ObjectQuery<Richiedente>;

                filteredRowsCount = 0;
                //filteredRowsCount = oQItemsList.Count();

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<Richiedente>;
                    if (pageSize > 0)
                        ItemsList = new ObservableCollection<Richiedente>(oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList());
                    else
                        ItemsList = new ObservableCollection<Richiedente>(oQItemsList.ToList());
                }
                else
                    ItemsList =  new ObservableCollection<Richiedente>(oQItemsList.ToList());

                return ItemsList;
            }
        }

        public Richiedente GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.Richiedenti.SingleOrDefault(rx => rx.Id_Richiedente == id);
            }
        }

        public V_RichiedenteFull GetVItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.V_RichiedentiFull.FirstOrDefault(rx => rx.Id_Richiedente == id);
            }
        }

        public List<Richiedente> GetSingleItem(int id)
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.Richiedenti
					.Where(rx => rx.Id_Richiedente == id)
					.OrderBy(x => x.Cognome).ThenBy(x => x.Nome)
					.ToList();
            }
		}

        public Richiedente GetSingleItem(string text)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.Richiedenti.FirstOrDefault(rx => rx.Cognome.Equals(text));
			}
		}

		public Richiedente GetItemByText(string text)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.Richiedenti.FirstOrDefault(x => String.Equals(x.Cognome, text));
			}
		}

		public Richiedente GetPetitionerByID(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.Richiedenti.Include("LK_Titolo").FirstOrDefault(rx => rx.Id_Richiedente == id);
			}
		}

        public int InsertOrUpdate(Richiedente objToUpdate)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (objToUpdate.Id_Richiedente != 0)
                {
                    _context.AttachUpdated(objToUpdate);
                }
                else
                {
                    //objToUpdate.DT_INS = objToUpdate.DT_UPD;
                    _context.Richiedenti.AddObject(objToUpdate);
                }

                _context.SaveChanges();

                return objToUpdate.Id_Richiedente;
            }
        }

		public void SaveObject()
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                _context.SaveChanges();
            }
		}

        //public static List<Richiedente> GetPetitioners()
        //{
        //    List<Richiedente> list = new List<Richiedente>();
        //    Richiedente obj = null;

        //    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SCV_DEV_DBConn"].ConnectionString))
        //    {
        //        using (SqlCommand command = new SqlCommand())
        //        {
        //            SqlDataReader reader = null;

        //            command.Connection = connection;
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.CommandText = "GetPetitioners";

        //            try
        //            {
        //                connection.Open();
        //                reader = command.ExecuteReader();

        //                while (reader.Read())
        //                {
        //                    obj = new Petitioner();
        //                    obj.ID = reader.GetValue(reader.GetOrdinal("ID")) != System.DBNull.Value ? (int)reader.GetInt64(reader.GetOrdinal("ID")) : 0;
        //                    obj.Name = reader.GetValue(reader.GetOrdinal("Name")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Name")) : string.Empty;
        //                    obj.Surname = reader.GetValue(reader.GetOrdinal("Surname")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Surname")) : string.Empty;
        //                    obj.Title = reader.GetValue(reader.GetOrdinal("Title")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Title")) : string.Empty;
        //                    obj.MotherLanguageID = reader.GetValue(reader.GetOrdinal("PreferredLanguageID")) != System.DBNull.Value ? reader.GetInt32(reader.GetOrdinal("PreferredLanguageID")) : 0;
        //                    obj.MotherLanguage = reader.GetValue(reader.GetOrdinal("PreferredLanguage")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("PreferredLanguage")) : string.Empty;
        //                    obj.Notes = reader.GetValue(reader.GetOrdinal("Notes")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Notes")) : string.Empty;
        //                    obj.Address.Country = reader.GetValue(reader.GetOrdinal("Country")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Country")) : string.Empty;
        //                    obj.Address.Region = reader.GetValue(reader.GetOrdinal("Region")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Region")) : string.Empty;
        //                    obj.Address.Province = reader.GetValue(reader.GetOrdinal("Province")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Province")) : string.Empty;
        //                    obj.Address.Town = reader.GetValue(reader.GetOrdinal("Town")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Town")) : string.Empty;
        //                    obj.Address.StreetName = reader.GetValue(reader.GetOrdinal("StreetName")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("StreetName")) : string.Empty;
        //                    obj.Address.StreetNumber = reader.GetValue(reader.GetOrdinal("StreetNumber")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("StreetNumber")) : string.Empty;
        //                    obj.Address.Cap = reader.GetValue(reader.GetOrdinal("Cap")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Cap")) : string.Empty;
        //                    obj.Address.PersonalPhone = reader.GetValue(reader.GetOrdinal("PersonalPhone")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("PersonalPhone")) : string.Empty;
        //                    obj.Address.HomePhone = reader.GetValue(reader.GetOrdinal("HomePhone")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("HomePhone")) : string.Empty;
        //                    obj.Address.OfficePhone = reader.GetValue(reader.GetOrdinal("OfficePhone")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("OfficePhone")) : string.Empty;
        //                    obj.Address.Email = reader.GetValue(reader.GetOrdinal("Email")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Email")) : string.Empty;
        //                    list.Add(obj);
        //                }

        //            }
        //            catch (Exception e)
        //            {
        //                throw e;
        //            }
        //            finally
        //            {
        //                connection.Close();
        //            }
        //        }
        //    }

        //    return list;
        //}

        //public static Petitioner GetPetitioner(int petitionerID)
        //{
        //    Petitioner obj = null;

        //    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SCV_DEV_DBConn"].ConnectionString))
        //    {
        //        using (SqlCommand command = new SqlCommand())
        //        {
        //            SqlDataReader reader = null;

        //            command.Connection = connection;
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.CommandText = "GetPetitioner";

        //            command.Parameters.AddWithValue("@P_ID", petitionerID);

        //            try
        //            {
        //                connection.Open();
        //                reader = command.ExecuteReader();

        //                if (reader.Read())
        //                {
        //                    obj = new Petitioner();
        //                    obj.ID = reader.GetValue(reader.GetOrdinal("ID")) != System.DBNull.Value ? (int)reader.GetInt64(reader.GetOrdinal("ID")) : 0;
        //                    obj.Name = reader.GetValue(reader.GetOrdinal("Name")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Name")) : string.Empty;
        //                    obj.Surname = reader.GetValue(reader.GetOrdinal("Surname")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Surname")) : string.Empty;
        //                    obj.Title = reader.GetValue(reader.GetOrdinal("Title")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Title")) : string.Empty;
        //                    obj.MotherLanguageID = reader.GetValue(reader.GetOrdinal("PreferredLanguageID")) != System.DBNull.Value ? reader.GetInt32(reader.GetOrdinal("PreferredLanguageID")) : 0;
        //                    obj.MotherLanguage = reader.GetValue(reader.GetOrdinal("PreferredLanguage")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("PreferredLanguage")) : string.Empty;
        //                    obj.Notes = reader.GetValue(reader.GetOrdinal("Notes")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Notes")) : string.Empty;
        //                    obj.Address.Country = reader.GetValue(reader.GetOrdinal("Country")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Country")) : string.Empty;
        //                    obj.Address.Region = reader.GetValue(reader.GetOrdinal("Region")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Region")) : string.Empty;
        //                    obj.Address.Province = reader.GetValue(reader.GetOrdinal("Province")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Province")) : string.Empty;
        //                    obj.Address.Town = reader.GetValue(reader.GetOrdinal("Town")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Town")) : string.Empty;
        //                    obj.Address.StreetName = reader.GetValue(reader.GetOrdinal("StreetName")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("StreetName")) : string.Empty;
        //                    obj.Address.StreetNumber = reader.GetValue(reader.GetOrdinal("StreetNumber")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("StreetNumber")) : string.Empty;
        //                    obj.Address.Cap = reader.GetValue(reader.GetOrdinal("Cap")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Cap")) : string.Empty;
        //                    obj.Address.PersonalPhone = reader.GetValue(reader.GetOrdinal("PersonalPhone")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("PersonalPhone")) : string.Empty;
        //                    obj.Address.HomePhone = reader.GetValue(reader.GetOrdinal("HomePhone")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("HomePhone")) : string.Empty;
        //                    obj.Address.OfficePhone = reader.GetValue(reader.GetOrdinal("OfficePhone")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("OfficePhone")) : string.Empty;
        //                    obj.Address.Email = reader.GetValue(reader.GetOrdinal("Email")) != System.DBNull.Value ? reader.GetString(reader.GetOrdinal("Email")) : string.Empty;
        //                }

        //            }
        //            catch (Exception e)
        //            {
        //                throw e;
        //            }
        //            finally
        //            {
        //                connection.Close();
        //            }
        //        }
        //    }

        //    return obj;
        //}

        //public static int SavePetitioner(Petitioner obj)
        //{
        //    int id = 0;

        //    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SCV_DEV_DBConn"].ConnectionString))
        //    {
        //        using (SqlCommand command = new SqlCommand())
        //        {
        //            SqlDataReader reader = null;

        //            command.Connection = connection;
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.CommandText = "SavePetitioner";

        //            command.Parameters.AddWithValue("@P_Name", obj.Name);
        //            command.Parameters.AddWithValue("@P_Surname", obj.Surname);
        //            command.Parameters.AddWithValue("@P_Title", obj.Title);
        //            command.Parameters.AddWithValue("@P_PreferredLanguageID", obj.MotherLanguageID);
        //            command.Parameters.AddWithValue("@P_Notes", obj.Notes);
        //            command.Parameters.AddWithValue("@P_Country", obj.Address.Country);
        //            command.Parameters.AddWithValue("@P_Region", obj.Address.Region);
        //            command.Parameters.AddWithValue("@P_Province", obj.Address.Province);
        //            command.Parameters.AddWithValue("@P_Town", obj.Address.Town);
        //            command.Parameters.AddWithValue("@P_StreetName", obj.Address.StreetName);
        //            command.Parameters.AddWithValue("@P_StreetNumber", obj.Address.StreetNumber);
        //            command.Parameters.AddWithValue("@P_Cap", obj.Address.Cap);
        //            command.Parameters.AddWithValue("@P_PersonalPhone", obj.Address.PersonalPhone);
        //            command.Parameters.AddWithValue("@P_HomePhone", obj.Address.HomePhone);
        //            command.Parameters.AddWithValue("@P_OfficePhone", obj.Address.OfficePhone);
        //            command.Parameters.AddWithValue("@P_Email", obj.Address.Email);

        //            try
        //            {
        //                connection.Open();
        //                reader = command.ExecuteReader();

        //                if (reader.Read())
        //                {
        //                    id = (int)reader.GetDecimal(0); //BigInt
        //                }

        //            }
        //            catch (Exception e)
        //            {
        //                throw e;
        //            }
        //            finally
        //            {
        //                connection.Close();
        //            }
        //        }
        //    }

        //    return id;
        //}

        //public static void UpdatePetitioner(Petitioner obj)
        //{
        //    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SCV_DEV_DBConn"].ConnectionString))
        //    {
        //        using (SqlCommand command = new SqlCommand())
        //        {
        //            command.Connection = connection;
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.CommandText = "UpdatePetitioner";

        //            command.Parameters.AddWithValue("@P_ID", obj.ID);
        //            command.Parameters.AddWithValue("@P_Name", obj.Name);
        //            command.Parameters.AddWithValue("@P_Surname", obj.Surname);
        //            command.Parameters.AddWithValue("@P_Title", obj.Title);
        //            command.Parameters.AddWithValue("@P_PreferredLanguageID", obj.MotherLanguageID);
        //            command.Parameters.AddWithValue("@P_Notes", obj.Notes);
        //            command.Parameters.AddWithValue("@P_Country", obj.Address.Country);
        //            command.Parameters.AddWithValue("@P_Region", obj.Address.Region);
        //            command.Parameters.AddWithValue("@P_Province", obj.Address.Province);
        //            command.Parameters.AddWithValue("@P_Town", obj.Address.Town);
        //            command.Parameters.AddWithValue("@P_StreetName", obj.Address.StreetName);
        //            command.Parameters.AddWithValue("@P_StreetNumber", obj.Address.StreetNumber);
        //            command.Parameters.AddWithValue("@P_Cap", obj.Address.Cap);
        //            command.Parameters.AddWithValue("@P_PersonalPhone", obj.Address.PersonalPhone);
        //            command.Parameters.AddWithValue("@P_HomePhone", obj.Address.HomePhone);
        //            command.Parameters.AddWithValue("@P_OfficePhone", obj.Address.OfficePhone);
        //            command.Parameters.AddWithValue("@P_Email", obj.Address.Email);

        //            try
        //            {
        //                connection.Open();
        //                command.ExecuteNonQuery();
        //            }
        //            catch (Exception e)
        //            {
        //                throw e;
        //            }
        //            finally
        //            {
        //                connection.Close();
        //            }
        //        }
        //    }
        //}

        //public static void DeletePetitioner(int petitionerID)
        //{
        //    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SCV_DEV_DBConn"].ConnectionString))
        //    {
        //        using (SqlCommand command = new SqlCommand())
        //        {
        //            command.Connection = connection;
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.CommandText = "DeletePetitioner";

        //            command.Parameters.AddWithValue("@P_ID", petitionerID);

        //            try
        //            {
        //                connection.Open();
        //                command.ExecuteNonQuery();
        //            }
        //            catch (Exception e)
        //            {
        //                throw e;
        //            }
        //            finally
        //            {
        //                connection.Close();
        //            }
        //        }
        //    }
        //}
	}
}
