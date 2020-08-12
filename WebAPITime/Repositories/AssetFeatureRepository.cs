using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using WebAPITime.Repositories;
using WebAPITime.HelperTools;
using WebAPITime.Models;

namespace WebAPITime.Repositories
{
    public class AssetFeatureRepository : IAssetFeatureRepository
    {
        string mConnStr = ConfigurationManager.AppSettings["mConnStr"];
        string mProjName = ConfigurationManager.AppSettings["mProjName"];

        public List<AssetFeature> GetAll()
        {
            List<AssetFeature> arrAssetFeature = new List<AssetFeature>();
            AssetFeature currAssetFeature = new AssetFeature();
            string query = string.Format("SELECT * FROM asset_feature");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(mConnStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Prepare();

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if ((reader != null) && (reader.HasRows))
                            {
                                while (reader.Read())
                                {
                                    currAssetFeature = DataMgrTools.BuildAssetFeature(reader);
                                    arrAssetFeature.Add(currAssetFeature);
                                }
                            }
                        }

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogEvent(mProjName, String.Format("AssetFeatureRepository GetAll() Exception: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error);
            }

            return arrAssetFeature;
        }
    }
}
