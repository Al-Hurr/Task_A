using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using System;
using System.Windows.Forms;
using Task_A.DbContext;
using Task_A.Models;

namespace Task_A
{
    public partial class GMapForm : Form
    {
        private const string _markers = "markers";
        private readonly GMapOverlay _gMapOverlay;
        private bool _isMarkerRemoving;
        private bool _isCursorOnMarker;
        private GMapMarker _currentMarker = null;
        private readonly GMapDataAccessLayer _gmapDAL;

        public GMapForm()
        {
            _gMapOverlay = new GMapOverlay(_markers);

            InitializeComponent();
        }

        public GMapForm(GMapDataAccessLayer gmapDAL) : this()
        {
            if (gmapDAL == null)
            {
                throw new ArgumentNullException();
            }

            _gmapDAL = gmapDAL;
        }

        private void GMapControl_Load(object sender, EventArgs e)
        {
            this.GMapControl.Bearing = 0;
            this.GMapControl.CanDragMap = true;
            this.GMapControl.DragButton = MouseButtons.Left;
            this.GMapControl.GrayScaleMode = true;
            this.GMapControl.MarkersEnabled = true;
            this.GMapControl.MaxZoom = 20;
            this.GMapControl.MinZoom = 2;
            this.GMapControl.MinZoom = 2;
            this.GMapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            this.GMapControl.NegativeMode = false;
            this.GMapControl.PolygonsEnabled = true;
            this.GMapControl.RoutesEnabled = true;
            this.GMapControl.ShowTileGridLines = false;
            this.GMapControl.ShowCenter = false;
            this.GMapControl.Zoom = 15;
            this.GMapControl.MapProvider = GMapProviders.GoogleMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            this.GMapControl.Position = new PointLatLng(55.7838455281082, 49.123747790105696);

            this.GMapControl.Overlays.Add(_gMapOverlay);

            foreach(var inventory in _gmapDAL.Inventories.Values)
            {
                GMarkerGoogle gMarkerGoogle = 
                    new GMarkerGoogle(new PointLatLng(inventory.Latitude, inventory.Longitude), GMarkerGoogleType.red);
                gMarkerGoogle.Tag = inventory.InventoryId;
                gMarkerGoogle.ToolTip = new GMapRoundedToolTip(gMarkerGoogle);
                _gMapOverlay.Markers.Add(gMarkerGoogle);
            }

            this.SaveButton.Enabled = false;
        }

        private void GMapControl_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            AddOrRemoveMarker(item, e);
        }

        private void GMapControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isMarkerRemoving)
            {
                _isMarkerRemoving = false;
                return;
            }

            AddOrRemoveMarker(sender, e);
        }

        private void AddOrRemoveMarker(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            if (sender is GMapMarker item)
            {
                item?.Overlay?.Markers?.Remove(item);
                _currentMarker = null;
                _isMarkerRemoving = true;
                _isCursorOnMarker = false;
                double x_position = item.Position.Lat;
                double y_position = item.Position.Lng;

                _gmapDAL.Remove(item.Tag as Guid?);
            }
            else
            {
                double x_position = GMapControl.FromLocalToLatLng(e.X, e.Y).Lat;
                double y_position = GMapControl.FromLocalToLatLng(e.X, e.Y).Lng;
                var inventory = Inventory.CreateNew(x_position, y_position);
                GMarkerGoogle gMarkerGoogle = new GMarkerGoogle(new PointLatLng(x_position, y_position), GMarkerGoogleType.red);
                gMarkerGoogle.Tag = inventory.InventoryId;
                gMarkerGoogle.ToolTip = new GMapRoundedToolTip(gMarkerGoogle);
                _gMapOverlay.Markers.Add(gMarkerGoogle);

                _gmapDAL.Add(inventory);
            }

            this.Refresh();
        }

        private void GMapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isCursorOnMarker)
            {
                PointLatLng point = GMapControl.FromLocalToLatLng(e.X, e.Y);
                double x_position = _currentMarker.Position.Lat;
                double y_position = _currentMarker.Position.Lng;

                _gmapDAL.Update(_currentMarker.Tag as Guid?, point.Lat, point.Lng);

                _currentMarker.Position = point;

                this.Refresh();
            }
        }

        private void GMapControl_OnMarkerEnter(GMapMarker item)
        {
            // чтобы не цеплялся на другие маркеры при перетаскивании маркера
            if (_isCursorOnMarker)
            {
                return;
            }

            _currentMarker = item;
            _isCursorOnMarker = true;
        }

        private void GMapControl_OnMarkerLeave(GMapMarker item)
        {
            // чтобы не цеплялся на другие маркеры при перетаскивании маркера
            if (!item.Equals(_currentMarker))
            {
                return;
            }

            _isCursorOnMarker = false;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_gmapDAL.SaveChanges())
            {
                MessageBox.Show("Изменения успешно сохранены", "Сохранение", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("Возникла ошибка при сохранении", "Сохранение", MessageBoxButtons.OK);
            }

            this.Refresh();
        }

        public override void Refresh()
        {
            base.Refresh();

            this.SaveButton.Enabled = _gmapDAL.IsChangesExists;
        }

        private void GMapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_gmapDAL.IsChangesExists 
                && MessageBox.Show("Остались несохраненные изменения, вы действительно хотите выйти?", "Выход", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}