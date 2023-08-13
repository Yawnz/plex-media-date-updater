using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using PlexDateAddedUpdater.Helpers;

namespace PlexDateAddedUpdater.PlexDB
{
    public class PlexClient
    {
        public PlexClient()
        {
            sqlConn = new SqliteConnection();
        }

        public PlexClient(string filePath)
        {
            try
            {
                sqlConn = new SqliteConnection($"Data Source={filePath};");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Could not connect to Plex DB: {ex.Message}");
            }
        }

        public PlexClient(SqliteConnection sqlConn)
        {
            this.sqlConn = sqlConn;
        }

        SqliteConnection sqlConn;

        public async Task<List<string>> GetLibraryLocationsAsync()
        {
            List<string> returnPaths = new List<string>();

            try
            {
                var sqlCmd = await CreateCommandAsync($"SELECT root_path FROM section_locations WHERE available = 1");
                var sqlReader = await sqlCmd.ExecuteReaderAsync();

                while (sqlReader.Read())
                {
                    returnPaths.Add($"{sqlReader[0]}");
                }

                await DisposeCommandAsync(sqlCmd);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not retrieve library locations: {ex}");
            }

            return returnPaths;
        }

        public async Task<List<string>> GetMediaItemIdsAsync(string fileName)
        {
            List<string> returnIds = new List<string>();

            try
            {
                var sqlCmd = await CreateCommandAsync($"SELECT media_item_id FROM media_parts WHERE file LIKE \"{fileName}\"");
                var sqlReader = await sqlCmd.ExecuteReaderAsync();

                while (sqlReader.Read())
                {
                    returnIds.Add($"{sqlReader[0]}");
                }

                await DisposeCommandAsync(sqlCmd);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not find matching media item: {ex}");
            }

            return returnIds;
        }

        public async Task<List<string>> GetMetaDataItemIdsAsync(string mediaItemId)
        {
            if (mediaItemId == null) return null;

            List<string> returnIds = new List<string>();

            try
            {
                var sqlCmd = await CreateCommandAsync($"SELECT metadata_item_id FROM media_items WHERE id = {mediaItemId}");
                var sqlReader = await sqlCmd.ExecuteReaderAsync();

                while (sqlReader.Read())
                {
                    returnIds.Add($"{sqlReader[0]}");
                }

                await DisposeCommandAsync(sqlCmd);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Could not find matching meta data Id: {ex}");
            }

            return returnIds;
        }

        public async Task<DateTime?> GetDateAddedByMetaDataItemIdAsync(string metaDataItemId)
        {
            DateTime? returnDT = null;

            try
            {
                var sqlCmd = await CreateCommandAsync($"SELECT added_at FROM metadata_items WHERE id = {metaDataItemId}");
                var sqlReader = await sqlCmd.ExecuteReaderAsync();

                while (sqlReader.Read())
                {
                    if (int.TryParse($"{sqlReader[0]}", out int result))
                    {
                        returnDT = DateTimeHelpers.FromEpoch(result);
                    }
                }

                await DisposeCommandAsync(sqlCmd);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Could not get date added value: {ex}");
            }

            return returnDT;
        }

        public async Task<bool> UpdateDateAddedByMetaDataIdAsync(string mediaItemId, string metaDataItemId, string newDateAdded)
        {
            bool returnData = false;

            try
            {
                var sqlCmd = await CreateCommandAsync($"UPDATE metadata_items SET added_at = {newDateAdded}, created_at = {newDateAdded}, updated_at = {newDateAdded}, refreshed_at = {newDateAdded} WHERE id = {metaDataItemId}");
                var updateCount = await sqlCmd.ExecuteNonQueryAsync();

                if (updateCount > 0)
                {
                    returnData = true;
                }

                sqlCmd.CommandText = $"UPDATE media_parts SET created_at = {newDateAdded}, updated_at = {newDateAdded} WHERE media_item_id = {mediaItemId}";
                updateCount = await sqlCmd.ExecuteNonQueryAsync();

                if (updateCount > 0)
                {
                    returnData = true;
                }

                sqlCmd.CommandText = $"UPDATE media_items SET created_at = {newDateAdded}, updated_at = {newDateAdded} WHERE id = {mediaItemId}";
                updateCount = await sqlCmd.ExecuteNonQueryAsync();

                if (updateCount > 0)
                {
                    returnData = true;
                }

                await DisposeCommandAsync(sqlCmd);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Could not update date added values: {ex}");
            }
            
            return returnData;
        }

        private async Task<SqliteCommand> CreateCommandAsync(string sqlCommand)
        {
            try
            {
                await sqlConn.OpenAsync();
                var sqlCmd = sqlConn.CreateCommand();

                sqlCmd.CommandText = sqlCommand;

                return sqlCmd;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create SQL Command: {ex}");
            }
            return null;
        }

        private async Task DisposeCommandAsync(SqliteCommand sqlCmd)
        {
            try
            {
                await sqlCmd.DisposeAsync();
                await sqlConn.CloseAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Could not dispose SQL command: {ex}");
            }
        }

        public async Task<bool> CheckTriggers()
        {
            try
            {
                var sqlCmd = await CreateCommandAsync($"SELECT name, sql FROM sqlite_master WHERE type='trigger' AND tbl_name='metadata_items';");
                var reader = await sqlCmd.ExecuteReaderAsync();

                var read = string.Empty;
                while (reader.Read() )
                {
                    if (string.IsNullOrEmpty(read))
                    {
                        read = $"{reader[0]};{reader[1]}";
                    }
                    else
                    {
                        read = $"{read}|{reader[0]};{reader[1]}";
                    }
                }
                
                await DisposeCommandAsync(sqlCmd);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to disable triggers: {ex}");
            }
            return false;
        }

        public async Task<bool> DisableTriggersAsync()
        {
            try
            {
                var sqlCmd = await CreateCommandAsync($"DROP TRIGGER fts4_metadata_titles_before_update_icu;");
                var updateCount = await sqlCmd.ExecuteNonQueryAsync();

                sqlCmd.CommandText = $"DROP TRIGGER fts4_metadata_titles_after_update_icu;";
                updateCount = await sqlCmd.ExecuteNonQueryAsync();

                await DisposeCommandAsync(sqlCmd);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to disable triggers: {ex}");
            }
            return false;
        }

        public async Task<bool> EnableTriggersAsync()
        {
            try
            {
                var sqlCmd = await CreateCommandAsync($"CREATE TRIGGER fts4_metadata_titles_before_update_icu BEFORE UPDATE ON metadata_items BEGIN DELETE FROM fts4_metadata_titles_icu WHERE docid=old.rowid; END");
                await sqlCmd.ExecuteNonQueryAsync();

                sqlCmd.CommandText = $"CREATE TRIGGER fts4_metadata_titles_after_update_icu AFTER UPDATE ON metadata_items BEGIN INSERT INTO fts4_metadata_titles_icu(docid, title, title_sort, original_title) VALUES(new.rowid, new.title, new.title_sort, new.original_title); END";
                await sqlCmd.ExecuteNonQueryAsync();

                await DisposeCommandAsync(sqlCmd);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enable triggers: {ex}");
            }
            return false;
        }
    }
}
