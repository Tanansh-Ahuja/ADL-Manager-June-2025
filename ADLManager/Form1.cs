using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;

namespace ADLManager
{
    public partial class Form1 : Form
    {
        string columnZeroName = "Select";
        string columnOneName = "Sno";
        string columnTwoName = "feed";
        string columnThreeName = "adl";
        string columnFourName = "createTab";

        // Declare the API objects
        private TTAPI m_api = null;
        private ManualResetEvent mre = new ManualResetEvent(false);
        private tt_net_sdk.WorkerDispatcher m_disp = null;
        private AlgoTradeSubscription m_algoTradeSubscription = null;
        private AlgoLookupSubscription m_algoLookupSubscription = null;
        private IReadOnlyCollection<Account> m_accounts = null;
        private Algo m_algo = null;
        private object m_Lock = new object();
        private bool m_isDisposed = false;
        private Instrument m_instrument = null;


        private Dictionary<string, List<(string paramName, string paramType)>> adlParameters = new Dictionary<string, List<(string, string)>>()
        {
            { "Scalar 2_0", new List<(string, string)>
                {
                    ("P1", "int"),
                    ("P2", "dropdown"),
                    ("P3", "dropdown"),
                    ("P4", "bool")
                }
            },
            { "ADL #2", new List<(string, string)>
                {
                    ("P1", "int"),
                    ("P2", "bool")
                }
            }
        };

        private List<int> selectedRowIndexList = new List<int>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mainGrid.DefaultCellStyle.SelectionBackColor = mainGrid.DefaultCellStyle.BackColor;
            mainGrid.DefaultCellStyle.SelectionForeColor = mainGrid.DefaultCellStyle.ForeColor;
        }

  
        public void Start(tt_net_sdk.TTAPIOptions apiConfig)
        {
            m_disp = tt_net_sdk.Dispatcher.AttachWorkerDispatcher();
            m_disp.DispatchAction(() =>
            {
                Init(apiConfig);
            });

            m_disp.Run();
        }

        
        public void Init(tt_net_sdk.TTAPIOptions apiConfig)
        {
            ApiInitializeHandler apiInitializeHandler = new ApiInitializeHandler(ttNetApiInitHandler);
            TTAPI.ShutdownCompleted += TTAPI_ShutdownCompleted;

            //For Algo Orders
            apiConfig.AlgoUserDisconnectAction = UserDisconnectAction.Cancel;
            TTAPI.CreateTTAPI(tt_net_sdk.Dispatcher.Current, apiConfig, apiInitializeHandler);
        }

        
        public void ttNetApiInitHandler(TTAPI api, ApiCreationException ex)
        {
            if (ex == null)
            {
                Console.WriteLine("TT.NET SDK INITIALIZED");

                // Authenticate your credentials
                m_api = api;
                m_api.TTAPIStatusUpdate += new EventHandler<TTAPIStatusUpdateEventArgs>(m_api_TTAPIStatusUpdate);
                m_api.Start();
            }
            else if (ex.IsRecoverable)
            {
                // Initialization failed but retry is in progress...
            }
            else
            {
                Console.WriteLine("TT.NET SDK Initialization Failed: {0}", ex.Message);
                Dispose();
            }
        }

        void StartAlgo()
        {
            //    while (!m_price.IsValid || m_algo == null)
            //        mre.WaitOne();

            //    // To retrieve the list of parameters valid for the Algo you can call algo.AlgoParameters;
            //    // Construct a dictionary of the parameters and the values to send out 
            //    Dictionary<string, object> algo_userparams = new Dictionary<string, object>
            //        {
            //            {"Ignore Market State",     true},
            //        };

            //    var lines = algo_userparams.Select(kvp => kvp.Key + ": " + kvp.Value.ToString());
            //    Console.WriteLine(string.Join(Environment.NewLine, lines));

            //    OrderProfile algo_op = m_algo.GetOrderProfile(m_instrument);
            //    algo_op.LimitPrice = m_price;
            //    algo_op.OrderQuantity = Quantity.FromDecimal(m_instrument, 5); ;
            //    algo_op.Side = OrderSide.Buy;
            //    algo_op.OrderType = OrderType.Limit;
            //    algo_op.Account = m_accounts.ElementAt(0);
            //    algo_op.UserParameters = algo_userparams;
            //    m_algoTradeSubscription.SendOrder(algo_op);
        }

        public void m_api_TTAPIStatusUpdate(object sender, TTAPIStatusUpdateEventArgs e)
        {
            Console.WriteLine("TTAPIStatusUpdate: {0}", e);
            if (e.IsReady == false)
            {
                // TODO: Do any connection lost processing here
                return;
            }
            // TODO: Do any connection up processing here
            //       note: can happen multiple times with your application life cycle

            // can get status multiple times - do not create subscription if it exists
            //enter algo name here
            AlgoLookupSubscription algoLookupSubscription = new AlgoLookupSubscription(tt_net_sdk.Dispatcher.Current, "MET_ScalarBias_2_1");
            algoLookupSubscription.OnData += AlgoLookupSubscription_OnData;
            algoLookupSubscription.GetAsync();

            // Get the accounts
            m_accounts = m_api.Accounts;

        }

        void m_instrLookupRequest_OnData(object sender, InstrumentLookupEventArgs e)
        {
            if (e.Event == ProductDataEvent.Found)
            {
                // Instrument was found
                m_instrument = e.InstrumentLookup.Instrument;
                Console.WriteLine("Found: {0}", m_instrument);

                

            }
            else if (e.Event == ProductDataEvent.NotAllowed)
            {
                Console.WriteLine("Not Allowed : Please check your Token access");
            }
            else
            {
                // Instrument was not found and TT API has given up looking for it
                Console.WriteLine("Cannot find instrument: {0}", e.Message);
                Dispose();
            }
        }

        private void AlgoLookupSubscription_OnData(object sender, AlgoLookupEventArgs e)
        {
            if (e.Event == ProductDataEvent.Found)
            {
                Console.WriteLine("Algo Instrument Found: {0}", e.AlgoLookup.Algo.Alias);
                m_algo = e.AlgoLookup.Algo;

                var algo_userparams = m_algo.AlgoParameters;


                    foreach (var item in m_algo.AlgoParameters)
                {
                    Console.WriteLine($"{item.Name} : {item.Type}");
                }

                // Create an Algo TradeSubscription to listen for order / fill events only for orders submitted through it
                m_algoTradeSubscription = new AlgoTradeSubscription(tt_net_sdk.Dispatcher.Current, m_algo);

                m_algoTradeSubscription.OrderUpdated += new EventHandler<OrderUpdatedEventArgs>(m_algoTradeSubscription_OrderUpdated);
                m_algoTradeSubscription.OrderAdded += new EventHandler<OrderAddedEventArgs>(m_algoTradeSubscription_OrderAdded);
                m_algoTradeSubscription.OrderDeleted += new EventHandler<OrderDeletedEventArgs>(m_algoTradeSubscription_OrderDeleted);
                m_algoTradeSubscription.OrderFilled += new EventHandler<OrderFilledEventArgs>(m_algoTradeSubscription_OrderFilled);
                m_algoTradeSubscription.OrderRejected += new EventHandler<OrderRejectedEventArgs>(m_algoTradeSubscription_OrderRejected);
                m_algoTradeSubscription.OrderBookDownload += new EventHandler<OrderBookDownloadEventArgs>(m_algoTradeSubscription_OrderBookDownload);
                m_algoTradeSubscription.ExportValuesUpdated += new EventHandler<ExportValuesUpdatedEventArgs>(m_algoTradeSubscription_ExportValuesUpdated);
                m_algoTradeSubscription.AlertsFired += new EventHandler<AlertsFiredEventArgs>(m_algoTradeSubscription_AlertsUpdated);
                m_algoTradeSubscription.Start();

                mre.Set();
            }
            else if (e.Event == ProductDataEvent.NotAllowed)
            {
                Console.WriteLine("Not Allowed : Please check your Token access");
            }
            else
            {
                // Algo Instrument was not found and TT API has given up looking for it
                Console.WriteLine("Cannot find Algo instrument: {0}", e.Message);
                Dispose();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event notification for order book download complete. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        void m_algoTradeSubscription_OrderBookDownload(object sender, OrderBookDownloadEventArgs e)
        {
            Console.WriteLine("Orderbook downloaded...");

            // Start Algo Trading thread.
            Thread algoThread = new Thread(() => this.StartAlgo());
            algoThread.Name = "Algo Trading Thread";
            algoThread.Start();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event notification for order rejection. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        void m_algoTradeSubscription_OrderRejected(object sender, OrderRejectedEventArgs e)
        {
            Console.WriteLine("\nOrderRejected for : [{0}]", e.Order.SiteOrderKey);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event notification for order fills. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        void m_algoTradeSubscription_OrderFilled(object sender, OrderFilledEventArgs e)
        {
            if (e.FillType == tt_net_sdk.FillType.Full)
            {
                Console.WriteLine("\nOrderFullyFilled [{0}]: {1}@{2}", e.Fill.SiteOrderKey, e.Fill.Quantity, e.Fill.MatchPrice);
            }
            else
            {
                Console.WriteLine("\nOrderPartiallyFilled [{0}]: {1}@{2}", e.Fill.SiteOrderKey, e.Fill.Quantity, e.Fill.MatchPrice);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event notification for order deletion. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        void m_algoTradeSubscription_OrderDeleted(object sender, OrderDeletedEventArgs e)
        {
            Console.WriteLine("\nOrderDeleted [{0}] , Message : {1}", e.OldOrder.SiteOrderKey, e.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event notification for order addition. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        void m_algoTradeSubscription_OrderAdded(object sender, OrderAddedEventArgs e)
        {
            if (e.Order.IsSynthetic)
                Console.WriteLine("\nPARENT Algo OrderAdded [{0}] for Algo : {1} with Synthetic Status : {2} ", e.Order.SiteOrderKey, e.Order.Algo.Alias, e.Order.SyntheticStatus.ToString());
            else
                Console.WriteLine("\nCHILD OrderAdded [{0}] {1}: {2}", e.Order.SiteOrderKey, e.Order.BuySell, e.Order.ToString());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event notification for order update. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        void m_algoTradeSubscription_OrderUpdated(object sender, OrderUpdatedEventArgs e)
        {
            if (e.NewOrder.ExecutionType == ExecType.Restated)
                Console.WriteLine("\nAlgo Order Restated [{0}] for Algo : {1} with Synthetic Status : {2} ", e.NewOrder.SiteOrderKey, e.NewOrder.Algo.Alias, e.NewOrder.SyntheticStatus.ToString());
            else
                Console.WriteLine("\nOrderUpdated [{0}] {1}: {2}", e.NewOrder.SiteOrderKey, e.NewOrder.BuySell, e.NewOrder.ToString());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event notification for Algo ExportedValue update. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        void m_algoTradeSubscription_ExportValuesUpdated(object sender, ExportValuesUpdatedEventArgs e)
        {
            foreach (string key in e.ExportValues.Keys)
            {
                Console.WriteLine("Algo EVU: Parameter Name = {0} and Parameter Value = {1}", key, e.ExportValues[key]);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event notification for Algo Alert update. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        void m_algoTradeSubscription_AlertsUpdated(object sender, AlertsFiredEventArgs e)
        {
            foreach (string key in e.Alerts.Keys)
            {
                Console.WriteLine("Algo ALERTs Fired: Name = {0} and Alert Value = {1}", key, e.Alerts[key]);
            }
        }


        public void TTAPI_ShutdownCompleted(object sender, EventArgs e)
        {
            // Dispose of any other objects / resources
            Console.WriteLine("TTAPI shutdown completed");
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void add_btn_Click(object sender, EventArgs e)
        {
            int serialNumber = mainGrid.Rows.Count + 1;
            mainGrid.Rows.Add(false, serialNumber);
        }

        private void del_btn_Click(object sender, EventArgs e)
        {
            if (selectedRowIndexList.Count == 0) return;

            selectedRowIndexList.Sort();

            for (int i = 0; i < selectedRowIndexList.Count; i++)
            {
                int index_to_delete = selectedRowIndexList[i];
                DataGridViewRow rowToRemove = mainGrid.Rows[index_to_delete - i];
                mainGrid.Rows[index_to_delete - i].Cells[columnFourName].Value = false;
                mainGrid.Rows.Remove(rowToRemove);
            }
            selectedRowIndexList.Clear();


            Dictionary<int, int> map = new Dictionary<int, int>();
            //         old,new

            for (int i = mainGrid.Rows.Count - 1; i >= 0; i--)
            {
                map[(int)mainGrid.Rows[i].Cells[columnOneName].Value] = i + 1;
                mainGrid.Rows[i].Cells[columnOneName].Value = i + 1;
            }
            var curr_index = 0;
            for (int i = tabControl1.TabPages.Count - 1; i > 0; i--)
            {
                curr_index = int.Parse(tabControl1.TabPages[i].Text);
                if (map.ContainsKey(curr_index))
                {
                    tabControl1.TabPages[i].Text = map[curr_index].ToString();
                }

            }


        }

        private void mainGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && mainGrid.Columns[e.ColumnIndex].Name == columnZeroName)
            {
                //e.RowIndex
                var row = mainGrid.Rows[e.RowIndex];
                bool isChecked = Convert.ToBoolean(row.Cells[columnZeroName].Value);

                if (isChecked)
                {
                    if (!selectedRowIndexList.Contains(e.RowIndex))
                        selectedRowIndexList.Add(e.RowIndex);
                }
                else
                {
                    selectedRowIndexList.Remove(e.RowIndex);
                }
            }


            if (e.RowIndex >= 0 && mainGrid.Columns[e.ColumnIndex].Name == columnFourName) // "createTab"
            {
                var row = mainGrid.Rows[e.RowIndex];
                var activateCell = row.Cells[columnFourName];
                bool isChecked = Convert.ToBoolean(activateCell.Value);
                int sno = Convert.ToInt32(mainGrid.Rows[e.RowIndex].Cells[columnOneName].Value);

                // Only validate if user is trying to activate
                if (isChecked)
                {
                    var feedValue = row.Cells[columnTwoName].Value?.ToString();
                    var adlValue = row.Cells[columnThreeName].Value?.ToString();

                    if (string.IsNullOrWhiteSpace(feedValue) || string.IsNullOrWhiteSpace(adlValue))
                    {
                        MessageBox.Show("Please select both Feed and ADL before activating.", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // Reset the checkbox (temporarily disable event to avoid loop)
                        mainGrid.CellValueChanged -= mainGrid_CellValueChanged;
                        row.Cells[columnFourName].Value = false;
                        mainGrid.CellValueChanged += mainGrid_CellValueChanged;

                        return;
                    }

                    string serial = row.Cells[columnOneName].Value.ToString();
                    if (!TabExists(serial))
                    {
                        CreateTabWithLabels(serial, feedValue, adlValue);
                    }
                }
                else
                {
                    string serial = row.Cells[columnOneName].Value.ToString();
                    for (int i = tabControl1.TabPages.Count - 1; i > 0; i--)
                    {
                        if (tabControl1.TabPages[i].Text == serial)
                        {
                            tabControl1.TabPages.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        private bool TabExists(string serial)
        {
            return tabControl1.TabPages.Cast<TabPage>().Any(tab => tab.Text == serial);
        }

        private void mainGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (mainGrid.IsCurrentCellDirty)
            {
                // Get current column
                int colIndex = mainGrid.CurrentCell.ColumnIndex;
                if (mainGrid.Columns[colIndex].Name == columnFourName || mainGrid.Columns[colIndex].Name == columnZeroName)
                {
                    mainGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
        }

        private void CreateTabWithLabels(string serial, string feedValue, string adlValue)
        {
            TabPage newTab = new TabPage(serial);

            // Static label: "Feed Name"
            Label lblFeedTitle = new Label
            {
                Text = "Feed Name:",
                Left = 20,
                Top = 10,
                AutoSize = true
            };

            // Dynamic label: actual feed value
            Label lblFeedValue = new Label
            {
                Text = feedValue,
                Left = 150,
                Top = 10,
                AutoSize = true
            };

            // Static label: "Algorithm Name"
            Label lblAdlTitle = new Label
            {
                Text = "Algorithm Name:",
                Left = 20,
                Top = 30,
                AutoSize = true
            };

            // Dynamic label: actual adl value
            Label lblAdlValue = new Label
            {
                Text = adlValue,
                Left = 150,
                Top = 30,
                AutoSize = true
            };

            newTab.Controls.Add(lblFeedTitle);
            newTab.Controls.Add(lblFeedValue);
            newTab.Controls.Add(lblAdlTitle);
            newTab.Controls.Add(lblAdlValue);

            // Create DataGridView
            DataGridView paramGrid = new DataGridView
            {
                Left = 20,
                Top = 60,
                Width = 400,
                Height = 200,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            paramGrid.DataError += (s, e) =>
            {
                e.ThrowException = false;
            };

            paramGrid.DefaultCellStyle.SelectionBackColor = paramGrid.DefaultCellStyle.BackColor;
            paramGrid.DefaultCellStyle.SelectionForeColor = paramGrid.DefaultCellStyle.ForeColor;

            paramGrid.Columns.Add("ParamName", "Parameter Name");
            paramGrid.Columns.Add("Value", "Value");

            if (adlParameters.ContainsKey(adlValue))
            {
                foreach (var (paramName, paramType) in adlParameters[adlValue])
                {
                    int rowIndex = paramGrid.Rows.Add(paramName, paramType);
                    if (paramType == "dropdown")
                    {
                        var comboCell = new DataGridViewComboBoxCell();
                        comboCell.Items.AddRange("10", "20", "30");
                        paramGrid.Rows[rowIndex].Cells["Value"] = comboCell;
                    }
                    else if (paramType == "bool")
                    {
                        var checkCell = new DataGridViewCheckBoxCell();
                        paramGrid.Rows[rowIndex].Cells["Value"] = checkCell;
                    }
                    else
                    {
                        var textCell = new DataGridViewTextBoxCell();
                        paramGrid.Rows[rowIndex].Cells["Value"] = textCell;
                    }
                }
            }

            newTab.Controls.Add(paramGrid);

            // Add "Start Algo" button
            Button btnStartAlgo = new Button
            {
                Text = "Start Algo",
                Left = 20,
                Top = paramGrid.Bottom + 10,
                Width = 120,
                Height = 30
            };

            btnStartAlgo.Click += (s, e) =>
            {
                MessageBox.Show($"Starting Algo for {serial} using {adlValue}");
                // Add actual start logic here
            };

            newTab.Controls.Add(btnStartAlgo);

            // Add tab to TabControl
            tabControl1.TabPages.Add(newTab);
        }


        

        
    }
}
