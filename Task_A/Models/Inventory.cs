
using System;

namespace Task_A.Models
{
    public class Inventory
    {
        public Guid InventoryId { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public bool IsNew { get; private set; }
        public bool IsEdited { get; private set; }
        public bool IsDeleted { get; private set; }
        public string LatitudeStr => Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public string LongitudeStr => Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

        public Inventory(double latitude, double longitude)
        {
            InventoryId = Guid.NewGuid();
            Latitude = latitude;
            Longitude = longitude;
        }

        public Inventory(Guid id, double latitude, double longitude)
        {
            InventoryId = id;
            Latitude = latitude;
            Longitude = longitude;
        }

        public static Inventory CreateNew(double latitude, double longitude)
        {
            return new Inventory(latitude, longitude)
            {
                InventoryId = Guid.NewGuid(),
                IsNew = true 
            };
        }

        public void SetForDelete()
        {
            this.IsDeleted = true;
            this.IsEdited = false;
        }

        public void SetSaved() => this.IsNew = false;

        public void ChangePosition(double latitude, double longitude)
        {
            if(this.Latitude == latitude && this.Longitude == longitude)
            {
                return;
            }

            this.Latitude = latitude;
            this.Longitude = longitude;
            this.IsEdited = true;
        }
    }
}