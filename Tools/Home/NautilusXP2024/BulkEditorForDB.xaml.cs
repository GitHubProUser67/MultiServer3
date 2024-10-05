using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SevenZip.Compression.LZ;

namespace NautilusXP2024
{
    public partial class BulkEditorForDB : Window
    {
        private MainWindow _mainWindow;
        private string _sqlFilePath;

        public BulkEditorForDB(MainWindow mainWindow, string sqlFilePath)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _sqlFilePath = sqlFilePath;
        }

        private async void ApplyToAllButton_Click(object sender, RoutedEventArgs e)
        {
            string keyName = KeyNameTextBox.Text.Trim();
            string value = ValueTextBox.Text.Trim();

            if (string.IsNullOrEmpty(keyName) || string.IsNullOrEmpty(value))
            {
                MessageBox.Show("KeyName and Value cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await AddMetadataToAllEntries(keyName, value);

            // Reload the first 25 entries in the main window
            _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task AddMetadataToAllEntries(string keyName, string value)
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    string query = "SELECT ObjectIndex FROM Objects";
                    var objectIndexes = new List<int>();

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                objectIndexes.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        foreach (int objectIndex in objectIndexes)
                        {
                            string insertQuery = @"
                                INSERT INTO Metadata (ObjectIndex, KeyName, Value)
                                VALUES (@objectIndex, @keyName, @value)";

                            using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                insertCommand.Parameters.AddWithValue("@keyName", keyName);
                                insertCommand.Parameters.AddWithValue("@value", value);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                    }

                    LogDebugInfo($"Added metadata entry (KeyName: {keyName}, Value: {value}) to all object indexes.");
                    MessageBox.Show("Metadata entry added to all entries successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error adding metadata to all entries: {ex.Message}");
                MessageBox.Show($"Error adding metadata to all entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogDebugInfo(string message)
        {
            // Implement your logging method here
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
            else if (e.ChangedButton == MouseButton.Right || e.ChangedButton == MouseButton.Left)
            {
                this.Close();
            }
        }

        private async void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
        {
            string oldValue = ReplaceTextBox.Text.Trim();
            string newValue = ReplaceWithTextBox.Text.Trim();

            if (string.IsNullOrEmpty(oldValue) || string.IsNullOrEmpty(newValue))
            {
                MessageBox.Show("Both values for replacement cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await ReplaceAllMetadataValues(oldValue, newValue);

            // Reload the first 25 entries in the main window

            _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task ReplaceAllMetadataValues(string oldValue, string newValue)
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string updateQuery = @"
                    UPDATE Metadata
                    SET Value = @newValue
                    WHERE Value = @oldValue";

                        using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@newValue", newValue);
                            command.Parameters.AddWithValue("@oldValue", oldValue);
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            transaction.Commit();

                            LogDebugInfo($"Replaced {rowsAffected} metadata entries from '{oldValue}' to '{newValue}'.");
                            MessageBox.Show($"Replaced {rowsAffected} metadata entries successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error replacing metadata values: {ex.Message}");
                MessageBox.Show($"Error replacing metadata values: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SetAllTimestampstoFFFFFFFFButton_Click(object sender, RoutedEventArgs e)
        {
            await SetAllTimestampsToFFFFFFFF();
            _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task SetAllTimestampsToFFFFFFFF()
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string updateQuery = @"
                    UPDATE Objects
                    SET ArchiveTimeStamp = -1";  // Only update ArchiveTimeStamp

                        using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                        {
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            transaction.Commit();
                            using (SQLiteCommand vacuumCommand = new SQLiteCommand("VACUUM", connection))
                            {
                                await vacuumCommand.ExecuteNonQueryAsync();
                                LogDebugInfo("Database vacuumed to reduce file size.");
                            }

                            LogDebugInfo($"Updated {rowsAffected} ArchiveTimeStamp entries to FFFFFFFF.");
                            MessageBox.Show($"Updated {rowsAffected} ArchiveTimeStamp entries to FFFFFFFF.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error setting ArchiveTimeStamp to FFFFFFFF: {ex.Message}");
                MessageBox.Show($"Error setting ArchiveTimeStamp to FFFFFFFF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void SetAllSHA1stoEmptyButton_Click(object sender, RoutedEventArgs e)
        {
            await SetAllSHA1sToEmpty();
            _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task SetAllSHA1sToEmpty()
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string updateQuery = @"
                    UPDATE Objects
                    SET OdcSha1Digest = @emptyBlob"; // Set OdcSha1Digest to an empty blob

                        using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                        {
                            byte[] emptyBlob = new byte[0]; // Zero-length byte array
                            command.Parameters.AddWithValue("@emptyBlob", emptyBlob);
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            transaction.Commit();
                            using (SQLiteCommand vacuumCommand = new SQLiteCommand("VACUUM", connection))
                            {
                                await vacuumCommand.ExecuteNonQueryAsync();
                                LogDebugInfo("Database vacuumed to reduce file size.");
                            }
                            LogDebugInfo($"Updated {rowsAffected} SHA1 entries to empty.");
                            MessageBox.Show($"Updated {rowsAffected} SHA1 entries to empty.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error setting SHA1s to empty: {ex.Message}");
                MessageBox.Show($"Error setting SHA1s to empty: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void RemoveAllMetaDataEntriesButton_Click(object sender, RoutedEventArgs e)
        {
            await RemoveAllMetadataEntries();
            _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task RemoveAllMetadataEntries()
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string deleteQuery = "DELETE FROM Metadata";

                        using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                        {
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            transaction.Commit();
                            using (SQLiteCommand vacuumCommand = new SQLiteCommand("VACUUM", connection))
                            {
                                await vacuumCommand.ExecuteNonQueryAsync();
                                LogDebugInfo("Database vacuumed to reduce file size.");
                            }
                            LogDebugInfo($"Removed {rowsAffected} metadata entries.");
                            MessageBox.Show($"Removed {rowsAffected} metadata entries.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error removing metadata entries: {ex.Message}");
                MessageBox.Show($"Error removing metadata entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void PatchAllPremiumToFreeButton_Click(object sender, RoutedEventArgs e)
        {
            await PatchPremiumToFree();
             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task PatchPremiumToFree()
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string selectQuery = @"
                    SELECT DISTINCT ObjectIndex
                    FROM Metadata
                    WHERE KeyName = 'ENTITLEMENT_ID'";

                        var objectIndexesToPatch = new List<int>();

                        using (SQLiteCommand selectCommand = new SQLiteCommand(selectQuery, connection))
                        {
                            using (SQLiteDataReader reader = (SQLiteDataReader)await selectCommand.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    int objectIndex = reader.GetInt32(0);

                                    string checkEntitlementQuery = @"
                                SELECT Value
                                FROM Metadata
                                WHERE ObjectIndex = @objectIndex AND KeyName = 'ENTITLEMENT_ID'";

                                    using (SQLiteCommand checkCommand = new SQLiteCommand(checkEntitlementQuery, connection))
                                    {
                                        checkCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                        string value = (string)await checkCommand.ExecuteScalarAsync();

                                        if (value != "FREE" && value != "LUA_REWARD" && value != "AUTOMATIC_REWARD")
                                        {
                                            objectIndexesToPatch.Add(objectIndex);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (int objectIndex in objectIndexesToPatch)
                        {
                            string deleteQuery = @"
                        DELETE FROM Metadata
                        WHERE ObjectIndex = @objectIndex AND (KeyName = 'ENTITLEMENT_ID' OR KeyName = 'CATEGORY_ID' OR KeyName = 'PRODUCT_ID')";

                            using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, connection))
                            {
                                deleteCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                await deleteCommand.ExecuteNonQueryAsync();
                            }

                            string insertQuery = @"
                        INSERT INTO Metadata (ObjectIndex, KeyName, Value)
                        VALUES (@objectIndex, 'ENTITLEMENT_ID', 'FREE')";

                            using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                    }
                    using (SQLiteCommand vacuumCommand = new SQLiteCommand("VACUUM", connection))
                    {
                        await vacuumCommand.ExecuteNonQueryAsync();
                        LogDebugInfo("Database vacuumed to reduce file size.");
                    }
                    LogDebugInfo($"Patched premium entries to FREE successfully.");
                    MessageBox.Show("Patched premium entries to FREE successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error patching premium entries to FREE: {ex.Message}");
                MessageBox.Show($"Error patching premium entries to FREE: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void PatchAllPremiumToLuaRewardButton_Click(object sender, RoutedEventArgs e)
        {
            await PatchPremiumToSpecificReward("LUA_REWARD");

             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async void PatchAllPremiumToAutoRewardButton_Click(object sender, RoutedEventArgs e)
        {
            await PatchPremiumToSpecificReward("AUTOMATIC_REWARD");

             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task PatchPremiumToSpecificReward(string rewardType)
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string selectQuery = @"
                    SELECT DISTINCT ObjectIndex
                    FROM Metadata
                    WHERE KeyName = 'ENTITLEMENT_ID'";

                        var objectIndexesToPatch = new List<int>();

                        using (SQLiteCommand selectCommand = new SQLiteCommand(selectQuery, connection))
                        {
                            using (SQLiteDataReader reader = (SQLiteDataReader)await selectCommand.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    int objectIndex = reader.GetInt32(0);

                                    string checkEntitlementQuery = @"
                                SELECT Value
                                FROM Metadata
                                WHERE ObjectIndex = @objectIndex AND KeyName = 'ENTITLEMENT_ID'";

                                    using (SQLiteCommand checkCommand = new SQLiteCommand(checkEntitlementQuery, connection))
                                    {
                                        checkCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                        string value = (string)await checkCommand.ExecuteScalarAsync();

                                        if (value != "FREE" && value != "LUA_REWARD" && value != "AUTOMATIC_REWARD")
                                        {
                                            objectIndexesToPatch.Add(objectIndex);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (int objectIndex in objectIndexesToPatch)
                        {
                            string deleteQuery = @"
                        DELETE FROM Metadata
                        WHERE ObjectIndex = @objectIndex AND (KeyName = 'ENTITLEMENT_ID' OR KeyName = 'CATEGORY_ID' OR KeyName = 'PRODUCT_ID')";

                            using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, connection))
                            {
                                deleteCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                await deleteCommand.ExecuteNonQueryAsync();
                            }

                            string insertQuery = @"
                        INSERT INTO Metadata (ObjectIndex, KeyName, Value)
                        VALUES (@objectIndex, 'ENTITLEMENT_ID', @rewardType)";

                            using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                insertCommand.Parameters.AddWithValue("@rewardType", rewardType);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                    }
                    using (SQLiteCommand vacuumCommand = new SQLiteCommand("VACUUM", connection))
                    {
                        await vacuumCommand.ExecuteNonQueryAsync();
                        LogDebugInfo("Database vacuumed to reduce file size.");
                    }
                    LogDebugInfo($"Patched premium entries to {rewardType} successfully.");
                    MessageBox.Show($"Patched premium entries to {rewardType} successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error patching premium entries to {rewardType}: {ex.Message}");
                MessageBox.Show($"Error patching premium entries to {rewardType}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void SetAllHeatTo1Button_Click(object sender, RoutedEventArgs e)
        {
            await SetAllHeatValuesTo1();
     
             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task SetAllHeatValuesTo1()
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string[] heatKeys = new[] { "host_heat", "main_heat", "net_heat", "ppu_heat", "vram_heat" };

                        foreach (var heatKey in heatKeys)
                        {
                            string updateQuery = @"
                    UPDATE Metadata
                    SET Value = '1'
                    WHERE KeyName = @heatKey";

                            using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                            {
                                command.Parameters.AddWithValue("@heatKey", heatKey);
                                int rowsAffected = await command.ExecuteNonQueryAsync();
                                LogDebugInfo($"Updated {rowsAffected} entries for {heatKey}.");
                            }
                        }

                        transaction.Commit();
                    }

                    LogDebugInfo("Set all heat values to 1 successfully.");
                    MessageBox.Show("Set all heat values to 1 successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error setting heat values to 1: {ex.Message}");
                MessageBox.Show($"Error setting heat values to 1: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void RemoveAllHeatEntriesButton_Click(object sender, RoutedEventArgs e)
        {
            await RemoveAllHeatEntries();
   
            _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task RemoveAllHeatEntries()
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string[] heatKeys = new[] { "host_heat", "main_heat", "net_heat", "ppu_heat", "vram_heat" };

                        foreach (var heatKey in heatKeys)
                        {
                            string deleteQuery = @"
                    DELETE FROM Metadata
                    WHERE KeyName = @heatKey";

                            using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                            {
                                command.Parameters.AddWithValue("@heatKey", heatKey);
                                int rowsAffected = await command.ExecuteNonQueryAsync();
                                LogDebugInfo($"Deleted {rowsAffected} entries for {heatKey}.");
                            }
                        }

                        transaction.Commit();
                    }
                    using (SQLiteCommand vacuumCommand = new SQLiteCommand("VACUUM", connection))
                    {
                        await vacuumCommand.ExecuteNonQueryAsync();
                        LogDebugInfo("Database vacuumed to reduce file size.");
                    }
                    LogDebugInfo("Removed all heat entries successfully.");
                    MessageBox.Show("Removed all heat entries successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error removing heat entries: {ex.Message}");
                MessageBox.Show($"Error removing heat entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SetActiveHeatTo22Button_Click(object sender, RoutedEventArgs e)
        {
            await SetActiveHeatTo22();

             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task SetActiveHeatTo22()
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        // Create a temporary table
                        string createTempTableQuery = "CREATE TEMPORARY TABLE TempActives (ObjectIndex INTEGER);";
                        using (SQLiteCommand createTempTableCommand = new SQLiteCommand(createTempTableQuery, connection))
                        {
                            await createTempTableCommand.ExecuteNonQueryAsync();
                        }

                        // Insert ObjectIndex values into the temporary table
                        string insertIntoTempTableQuery = @"
                    INSERT INTO TempActives 
                    SELECT ObjectIndex 
                    FROM Metadata 
                    WHERE KeyName = 'ACTIVE' 
                    EXCEPT 
                    SELECT ObjectIndex 
                    FROM Metadata 
                    WHERE KeyName IN ('host_heat', 'main_heat', 'net_heat', 'ppu_heat', 'vram_heat');";
                        using (SQLiteCommand insertIntoTempTableCommand = new SQLiteCommand(insertIntoTempTableQuery, connection))
                        {
                            await insertIntoTempTableCommand.ExecuteNonQueryAsync();
                        }

                        // Insert heat entries into Metadata
                        string[] heatKeys = new[] { "host_heat", "main_heat", "net_heat", "ppu_heat", "vram_heat" };
                        foreach (var heatKey in heatKeys)
                        {
                            string insertHeatQuery = $@"
                        INSERT INTO Metadata (ObjectIndex, KeyName, Value)
                        SELECT ObjectIndex, '{heatKey}', 22
                        FROM TempActives;";
                            using (SQLiteCommand insertHeatCommand = new SQLiteCommand(insertHeatQuery, connection))
                            {
                                await insertHeatCommand.ExecuteNonQueryAsync();
                            }
                        }

                        // Drop the temporary table
                        string dropTempTableQuery = "DROP TABLE TempActives;";
                        using (SQLiteCommand dropTempTableCommand = new SQLiteCommand(dropTempTableQuery, connection))
                        {
                            await dropTempTableCommand.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                    }

                    LogDebugInfo("Heat entries with value 22 added to entries with ACTIVE key.");
                    MessageBox.Show("Heat entries with value 22 added to entries with ACTIVE key.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error setting heat entries to 22: {ex.Message}");
                MessageBox.Show($"Error setting heat entries to 22: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void PatchAllMaleToFemaleButton_Click(object sender, RoutedEventArgs e)
        {
            await PatchAllMetadataValues("00000000-00000000-00000010-00000000", "00000000-00000000-00000010-00000001");
           
             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async void PatchAllFemaleToMaleButton_Click(object sender, RoutedEventArgs e)
        {
            await PatchAllMetadataValues("00000000-00000000-00000010-00000001", "00000000-00000000-00000010-00000000");
           
             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task PatchAllMetadataValues(string oldValue, string newValue)
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string updateQuery = @"
                    UPDATE Metadata
                    SET Value = @newValue
                    WHERE Value = @oldValue";

                        using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@newValue", newValue);
                            command.Parameters.AddWithValue("@oldValue", oldValue);
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            transaction.Commit();

                            LogDebugInfo($"Patched {rowsAffected} metadata entries from '{oldValue}' to '{newValue}'.");
                            MessageBox.Show($"Patched {rowsAffected} metadata entries successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error patching metadata values: {ex.Message}");
                MessageBox.Show($"Error patching metadata values: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddBothRigsToAllButton_Click(object sender, RoutedEventArgs e)
        {
            await AddBothRigsMetadataEntries();
             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task AddBothRigsMetadataEntries()
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string selectQuery = @"
                    SELECT ObjectIndex, Value
                    FROM Metadata
                    WHERE KeyName = 'RIGS'";

                        var objectIndexesToAdd = new List<(int, string)>();

                        using (SQLiteCommand selectCommand = new SQLiteCommand(selectQuery, connection))
                        {
                            using (SQLiteDataReader reader = (SQLiteDataReader)await selectCommand.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    int objectIndex = reader.GetInt32(0);
                                    string value = reader.GetString(1);

                                    if (value == "00000000-00000000-00000010-00000000")
                                    {
                                        objectIndexesToAdd.Add((objectIndex, "00000000-00000000-00000010-00000001"));
                                    }
                                    else if (value == "00000000-00000000-00000010-00000001")
                                    {
                                        objectIndexesToAdd.Add((objectIndex, "00000000-00000000-00000010-00000000"));
                                    }
                                }
                            }
                        }

                        foreach (var (objectIndex, valueToAdd) in objectIndexesToAdd)
                        {
                            string insertQuery = @"
                        INSERT INTO Metadata (ObjectIndex, KeyName, Value)
                        VALUES (@objectIndex, 'RIGS', @value)";

                            using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                insertCommand.Parameters.AddWithValue("@value", valueToAdd);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                    }

                    LogDebugInfo($"Added missing RIGS metadata entries successfully.");
                    MessageBox.Show("Missing RIGS metadata entries added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error adding missing RIGS metadata entries: {ex.Message}");
                MessageBox.Show($"Error adding missing RIGS metadata entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void RemoveAllEntitlementsButton_Click(object sender, RoutedEventArgs e)
        {
            await RemoveMetadataEntries("ENTITLEMENT_ID");
            
             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async void RemoveAllProductIdFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            await RemoveMetadataEntries("PRODUCT_ID");
            
             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async void RemoveAllCategoryIdFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            await RemoveMetadataEntries("CATEGORY_ID");
            
             _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task RemoveMetadataEntries(string keyName)
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string deleteQuery = @"
                    DELETE FROM Metadata
                    WHERE KeyName = @keyName";

                        using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@keyName", keyName);
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            transaction.Commit();
                            using (SQLiteCommand vacuumCommand = new SQLiteCommand("VACUUM", connection))
                            {
                                await vacuumCommand.ExecuteNonQueryAsync();
                                LogDebugInfo("Database vacuumed to reduce file size.");
                            }
                            LogDebugInfo($"Removed {rowsAffected} metadata entries with KeyName '{keyName}'.");
                            MessageBox.Show($"Removed {rowsAffected} metadata entries successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error removing metadata entries: {ex.Message}");
                MessageBox.Show($"Error removing metadata entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SetAllVersionsto0Button_Click(object sender, RoutedEventArgs e)
        {
            await SetAllVersionsTo0();

            _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task SetAllVersionsTo0()
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string updateQuery = @"
                    UPDATE Objects
                    SET Version = 0"; // Set Version to 0

                        using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                        {
                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            transaction.Commit();

                            LogDebugInfo($"Updated {rowsAffected} Version entries to 0.");
                            MessageBox.Show($"Updated {rowsAffected} Version entries to 0.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error setting Versions to 0: {ex.Message}");
                MessageBox.Show($"Error setting Versions to 0: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ApplyMatchAndAddButton_Click(object sender, RoutedEventArgs e)
        {
            string matchText = MatchTextBox.Text.Trim();
            string addKeyName = AddKeyNameTextBox.Text.Trim().ToUpper();
            string addValue = AddValueTextBox.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(matchText) || string.IsNullOrEmpty(addKeyName) || string.IsNullOrEmpty(addValue))
            {
                MessageBox.Show("Match Key/Value, Add KeyName, and Add Value cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await AddMetadataBasedOnMatch(matchText, addKeyName, addValue);

            // Reload the first 25 entries in the main window
            _mainWindow.PopulateGridWithEntriesSafe(0, 25);
        }

        private async Task AddMetadataBasedOnMatch(string matchText, string addKeyName, string addValue)
        {
            try
            {
                string connectionString = $"Data Source={_sqlFilePath};Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogDebugInfo("SQLite connection opened successfully.");

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string selectQuery = @"
                    SELECT DISTINCT ObjectIndex 
                    FROM Metadata 
                    WHERE (LOWER(KeyName) = LOWER(@matchText) OR LOWER(Value) = LOWER(@matchText))
                    AND (LENGTH(KeyName) = LENGTH(@matchText) OR LENGTH(Value) = LENGTH(@matchText))";
                        var objectIndexes = new List<int>();

                        using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@matchText", matchText);
                            using (SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    objectIndexes.Add(reader.GetInt32(0));
                                }
                            }
                        }

                        foreach (int objectIndex in objectIndexes)
                        {
                            string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM Metadata 
                        WHERE ObjectIndex = @objectIndex AND KeyName = @addKeyName AND Value = @addValue";

                            using (SQLiteCommand checkCommand = new SQLiteCommand(checkQuery, connection))
                            {
                                checkCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                checkCommand.Parameters.AddWithValue("@addKeyName", addKeyName);
                                checkCommand.Parameters.AddWithValue("@addValue", addValue);

                                long count = (long)await checkCommand.ExecuteScalarAsync();
                                if (count > 0)
                                {
                                    // Skip adding this entry as it already exists
                                    continue;
                                }
                            }

                            string insertQuery = @"
                        INSERT INTO Metadata (ObjectIndex, KeyName, Value)
                        VALUES (@objectIndex, @addKeyName, @addValue)";

                            using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@objectIndex", objectIndex);
                                insertCommand.Parameters.AddWithValue("@addKeyName", addKeyName);
                                insertCommand.Parameters.AddWithValue("@addValue", addValue);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                    }

                    using (SQLiteCommand vacuumCommand = new SQLiteCommand("VACUUM", connection))
                    {
                        await vacuumCommand.ExecuteNonQueryAsync();
                        LogDebugInfo("Database vacuumed to reduce file size.");
                    }

                    LogDebugInfo($"Added metadata entry (KeyName: {addKeyName}, Value: {addValue}) to object indexes matching '{matchText}'.");
                    MessageBox.Show("Metadata entry added to matching entries successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error adding metadata based on match: {ex.Message}");
                MessageBox.Show($"Error adding metadata based on match: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
