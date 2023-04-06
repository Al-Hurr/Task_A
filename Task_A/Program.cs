using System;
using System.Windows.Forms;
using Task_A.DbContext;

namespace Task_A
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var gmapDal = GMapDataAccessLayer.Create();
            if (!gmapDal.TryReadDataFromDb())
            {
                DbCreator.Create().TryCreateDb();
                TableCreator.Create().TryCreateTable();
            }
            gmapDal.TryFillDb();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GMapForm(gmapDal));
        }
    }
}
    