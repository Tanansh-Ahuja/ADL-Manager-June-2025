using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;
using ADLManager;


namespace ADLManager
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (SecretKeyForm keyForm = new SecretKeyForm())
            {
                if (keyForm.ShowDialog() != DialogResult.OK)
                {
                    MessageBox.Show("App cannot continue without a valid secret key.");
                    return;
                }

                string appSecretKey = keyForm.SecretKey;
                tt_net_sdk.ServiceEnvironment environment = tt_net_sdk.ServiceEnvironment.UatCert;
                tt_net_sdk.TTAPIOptions apiConfig = new tt_net_sdk.TTAPIOptions(environment, appSecretKey, 5000);


                using (Dispatcher disp = Dispatcher.AttachUIDispatcher())
                {
                    Application.EnableVisualStyles();

                    // Create an instance of the API
                    Form1 frm = new Form1(appSecretKey);

                    // Add your app secret Key here. It looks like: 00000000-0000-0000-0000-000000000000:00000000-0000-0000-0000-000000000000

                    //Sim
                    //string appSecretKey = "a020b041-fbe2-fdca-d098-a93421779bba:bd3d8014-c55d-fc72-1205-95578f326993"; // own key


                   //apiConfig.EnableAccountFiltering = true;
                    ApiInitializeHandler handler = new ApiInitializeHandler(frm.ttNetApiInitHandler);
                    TTAPI.CreateTTAPI(disp, apiConfig, handler);

                    Application.Run(frm);
                }

            }

            
        }
    }


}