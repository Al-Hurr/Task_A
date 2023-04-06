using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Task_A.Models;

namespace Task_A.DbContext
{
    public class GMapDataAccessLayer : ProcessDbConnectionBase
    {
        private readonly HashSet<Guid> _sendToDbInventoriesHashCodes;
        public Dictionary<Guid, Inventory> Inventories => _inventories;
        public bool IsChangesExists => _sendToDbInventoriesHashCodes?.Count > 0;

        public GMapDataAccessLayer() : base()
        {
            _inventories = new Dictionary<Guid, Inventory>();
            _sendToDbInventoriesHashCodes = new HashSet<Guid>();
        }

        public bool TryReadDataFromDb()
        {
            string getInventoriesScript = "select * from dbo.Inventory";
            return TryConnectAndExecute(ReadDataFromDb, getInventoriesScript);
        }

        public void Add(Inventory inventory)
        {
            #region Speсific situation handling
            // если маркер, которую взяли из базы, был помечен на удаление,
            // потом позже туда же поставили новый маркер, убираем этот маркер из списка на отправление в базу
            // т.к. он уже там есть
            if (_inventories.TryGetValue(inventory.InventoryId, out Inventory existingInDbInventory)
                && existingInDbInventory.IsDeleted)
            {
                _sendToDbInventoriesHashCodes.Remove(inventory.InventoryId);
                return;
            }
            #endregion

            _inventories.Add(inventory.InventoryId, inventory);
            _sendToDbInventoriesHashCodes.Add(inventory.InventoryId);
        }

        public void Update(Guid? id, double newLatitude, double newLongitude)
        {
            if (!id.HasValue || id.Equals(default(Guid)))
            {
                return;
            }

            if (_inventories.TryGetValue(id.Value, out Inventory inventory))
            {
                #region Speсific situation handling
                // если маркер, которую взяли из базы, был помечен на удаление,
                // потом позже туда же переместили другой маркер, убираем этот маркер из списка на отправление в базу
                // т.к. он уже там есть

                if (_inventories.TryGetValue(id.Value, out Inventory existingInDbInventory)
                    && existingInDbInventory.IsDeleted)
                {
                    _sendToDbInventoriesHashCodes.Remove(id.Value);
                    return;
                }
                #endregion

                inventory.ChangePosition(newLatitude, newLongitude);
                _sendToDbInventoriesHashCodes.Add(id.Value);
            }
        }

        public void Remove(Guid? id)
        {
            if (!id.HasValue || id.Equals(default(Guid)))
            {
                return;
            }

            if (_inventories.TryGetValue(id.Value, out Inventory inventory))
            {
                if (inventory.IsNew)
                {
                    _inventories.Remove(id.Value);
                    _sendToDbInventoriesHashCodes.Remove(id.Value);
                }
                else
                {
                    inventory.SetForDelete();
                    _sendToDbInventoriesHashCodes.Add(id.Value);
                }
            }
        }

        public bool SaveChanges()
        {
            bool isAllCommandsExecutedSuccessfully = true;

            List<Inventory> inventoriesForCreate = new List<Inventory>();
            List<Inventory> inventoriesForUpdate = new List<Inventory>();
            List<Inventory> inventoriesForDelete = new List<Inventory>();

            foreach (Guid inventoryIds in _sendToDbInventoriesHashCodes)
            {
                if (_inventories.TryGetValue(inventoryIds, out Inventory inventory))
                {
                    if (inventory.IsNew)
                    {
                        inventoriesForCreate.Add(inventory);
                    }
                    else if (inventory.IsEdited)
                    {
                        inventoriesForUpdate.Add(inventory);
                    }
                    else if (inventory.IsDeleted)
                    {
                        inventoriesForDelete.Add(inventory);
                    }
                }
            }

            if (inventoriesForCreate.Count > 0)
            {
                StringBuilder sb = new StringBuilder()
                    .AppendLine("insert into dbo.Inventory (InventoryID, Latitude, Longitude) values");

                int lastInvIdx = inventoriesForCreate.Count - 1;
                for (int i = 0; i < inventoriesForCreate.Count; i++)
                {
                    bool isLast = i == lastInvIdx;
                    sb.AppendLine($" ('{inventoriesForCreate[i].InventoryId}', " +
                        $"{inventoriesForCreate[i].LatitudeStr}, " +
                        $"{inventoriesForCreate[i].LongitudeStr}){(isLast ? "" : ",")}");
                }

                string sqlScript = sb.ToString();

                if (this.TryConnectAndExecute(ExecuteSqlCommand, sqlScript))
                {
                    foreach (var inventory in inventoriesForCreate)
                    {
                        inventory.SetSaved();
                        _sendToDbInventoriesHashCodes.Remove(inventory.InventoryId);
                    }

                    System.Console.WriteLine($"Added {inventoriesForCreate.Count} Inventories");
                }
                else
                {
                    System.Console.WriteLine($"Adding {inventoriesForCreate.Count} Inventories FAILED");
                    isAllCommandsExecutedSuccessfully = false;
                }
            }

            if (inventoriesForUpdate.Count > 0)
            {
                StringBuilder sb = new StringBuilder()
                    .AppendLine("UPDATE dbo.Inventory");
                StringBuilder idsSb = new StringBuilder();
                StringBuilder latitudesSb = new StringBuilder()
                    .AppendLine("SET Latitude")
                    .AppendLine("= CASE InventoryID");
                StringBuilder longitudesSb = new StringBuilder()
                    .AppendLine("Longitude")
                    .AppendLine("= CASE InventoryID");

                for (int i = 0; i < inventoriesForUpdate.Count; i++)
                {
                    latitudesSb.AppendLine($"WHEN '{inventoriesForUpdate[i].InventoryId}' THEN {inventoriesForUpdate[i].LatitudeStr}");
                    longitudesSb.AppendLine($"WHEN '{inventoriesForUpdate[i].InventoryId}' THEN {inventoriesForUpdate[i].LongitudeStr}");
                    bool isLast = i == inventoriesForUpdate.Count - 1;
                    idsSb.Append($"'{inventoriesForUpdate[i].InventoryId}'{(isLast ? "" : ",")} ");
                }

                string sqlScript = $"{sb}" +
                    $"{latitudesSb.AppendLine("ELSE Latitude END, ")}" +
                    $"{longitudesSb.AppendLine("ELSE Longitude END ")}" +
                    $"WHERE InventoryID IN({idsSb})";

                if (this.TryConnectAndExecute(ExecuteSqlCommand, sqlScript))
                {
                    foreach (var inventory in inventoriesForUpdate)
                    {
                        _sendToDbInventoriesHashCodes.Remove(inventory.InventoryId);
                    }

                    System.Console.WriteLine($"Updated {inventoriesForUpdate.Count} Inventories");
                }
                else
                {
                    System.Console.WriteLine($"Updating {inventoriesForUpdate.Count} Inventories FAILED");
                    isAllCommandsExecutedSuccessfully = false;
                }
            }


            if (inventoriesForDelete.Count > 0)
            {
                string sqlScript = $"DELETE FROM dbo.Inventory " +
                    $"WHERE InventoryID IN ({string.Join(", ", inventoriesForDelete.Select(x => $"'{x.InventoryId}'"))})";

                if (this.TryConnectAndExecute(ExecuteSqlCommand, sqlScript))
                {
                    foreach (var inventory in inventoriesForDelete)
                    {
                        _sendToDbInventoriesHashCodes.Remove(inventory.InventoryId);
                        _inventories.Remove(inventory.InventoryId);
                    }

                    System.Console.WriteLine($"Deleted {inventoriesForDelete.Count} Inventories");
                }
                else
                {
                    System.Console.WriteLine($"Deleteting {inventoriesForDelete.Count} Inventories FAILED");
                    isAllCommandsExecutedSuccessfully = false;
                }
            }

            return isAllCommandsExecutedSuccessfully;
        }

        public bool TryFillDb()
        {
            if(_inventories.Count > 0)
            {
                return true;
            }

            List<Inventory> inventories = new List<Inventory>
            {
                new Inventory(55.788301598489838, 49.113907814025879),
                new Inventory(55.784706174591278, 49.109101295471191),
                new Inventory(55.781122485712231, 49.1169548034668),
                new Inventory(55.783656441276918, 49.123778343200684),
                new Inventory(55.792536051393739, 49.129786491394043)
            };

            StringBuilder sb = new StringBuilder()
                    .AppendLine("insert into dbo.Inventory (InventoryID, Latitude, Longitude) values");

            int lastInvIdx = inventories.Count - 1;
            for (int i = 0; i < inventories.Count; i++)
            {
                bool isLast = i == lastInvIdx;
                sb.AppendLine($" ('{inventories[i].InventoryId}', " +
                    $"{inventories[i].LatitudeStr}, " +
                    $"{inventories[i].LongitudeStr}){(isLast ? "" : ",")}");
            }

            string sqlScript = sb.ToString();

            if(this.TryConnectAndExecute(ExecuteSqlCommand, sqlScript))
            {
                _inventories = inventories.ToDictionary(x => x.InventoryId, x => x);

                return true;
            }

            return false;
        }

        public static GMapDataAccessLayer Create()
        {
            return new GMapDataAccessLayer();
        }

        private void ReadDataFromDb(string sqlScripts)
        {
            using (SqlCommand sqlCommand = new SqlCommand(sqlScripts, _sqlConnection))
            {
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Guid id = reader.GetGuid(0);
                        double latitude = (double)reader.GetDecimal(1);
                        double longitude = (double)reader.GetDecimal(2);

                        var inventory = new Inventory(id, latitude, longitude);

                        _inventories[inventory.InventoryId] = inventory;
                    }
                }
            }
        }

        private void ExecuteSqlCommand(string sqlScript)
        {
            using (SqlCommand sqlCommand = new SqlCommand(sqlScript, _sqlConnection))
            {
                sqlCommand.ExecuteNonQuery();
            }
        }
    }
}
