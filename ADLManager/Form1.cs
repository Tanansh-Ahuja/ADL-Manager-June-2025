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
using System.IO;

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
        private string _appSecretKey;
        private TTAPI m_api = null;
        private ManualResetEvent mre = new ManualResetEvent(false);
        private tt_net_sdk.WorkerDispatcher m_disp = null;
        private AlgoTradeSubscription m_algoTradeSubscription = null;
        //private AlgoLookupSubscription m_algoLookupSubscription = null;
        private IReadOnlyCollection<Account> m_accounts = null;
        private Algo m_algo = null;
        //private object m_Lock = new object();
        //private bool m_isDisposed = false;
        private Instrument m_instrument = null;


        private List<string> adlAlgoNames = new List<string> {
                                                                  "MET_ScalarBias_2_0",
                                                                  //"MET_ScalarBias_2_1",
                                                                  //"alert_test"
                                                              };
        private int ADLAlgosFound = 0;

        private string currentTab = null;
        private string currentADLName = null;
        private Dictionary<string, List<(string paramName, string paramType)>> adlParameters = new Dictionary<string, List<(string, string)>>();
        private List<int> selectedRowIndexList = new List<int>();
        Dictionary<string, object> algo_userparams = new Dictionary<string, object>();
        private Dictionary<string, DataGridView> tabParamGrids = new Dictionary<string, DataGridView>();
        private Dictionary<string, string> tabAdlMapping = new Dictionary<string, string>();


        public Form1(string appSecretKey)
        {
            InitializeComponent();
            MainTab.Hide();
            _appSecretKey = appSecretKey;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mainGrid.DefaultCellStyle.SelectionBackColor = mainGrid.DefaultCellStyle.BackColor;
            mainGrid.DefaultCellStyle.SelectionForeColor = mainGrid.DefaultCellStyle.ForeColor;
            MainTab.SelectedIndexChanged += MainTab_SelectedIndexChanged;
            add_btn.Enabled = false;
            add_btn.Text = "Loading ADL";
        }
        #region API initialize

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
                try
                {
                    File.WriteAllText("key.txt", _appSecretKey);
                }
                catch (Exception fileEx)
                {
                    MessageBox.Show($"Connected, but failed to save key: {fileEx.Message}", "File Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

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
        public void m_api_TTAPIStatusUpdate(object sender, TTAPIStatusUpdateEventArgs e)
        {
            Console.WriteLine("TTAPIStatusUpdate: {0}", e);
            if (e.IsReady == false)
            {
                // TODO: Do any connection lost processing here
                return;
            }

            // Get the accounts
            m_accounts = m_api.Accounts;

            foreach (var algoName in adlAlgoNames)
            {
                Console.WriteLine("Algo name: {0}", algoName);
                var algoLookupSubscription = new AlgoLookupSubscription(tt_net_sdk.Dispatcher.Current, algoName);
                algoLookupSubscription.OnData += AlgoLookupSubscription_OnData;
                algoLookupSubscription.GetAsync();
            }

        }

        #endregion

        #region Instrument and ADL lookup

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
                string algoName = e.AlgoLookup.Algo.Alias;

                var paramList = new List<(string paramName, string paramType)>();


                foreach (var item in e.AlgoLookup.Algo.AlgoParameters)
                {
                    string type;
                    if (item.Type == "Int_t")
                        type = "int";
                    else if (item.Type == "Float_t")
                        type = "float";
                    else if (item.Type == "String_t")
                        type = "string";
                    else if (item.Type == "Boolean_t")
                        type = "bool";
                    else
                        type = "string"; // fallback default

                    paramList.Add((item.Name, type));
                }
                lock (adlParameters) 
                {
                    adlParameters[algoName] = paramList;
                }

                UpdateAdlDropdownSource();
                
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

                ADLAlgosFound++;

                if (ADLAlgosFound == adlAlgoNames.Count)
                {
                    MainTab.Show();
                }
            }
            else if (e.Event == ProductDataEvent.NotAllowed)
            {
                Console.WriteLine("Not Allowed : Please check your Token access");
            }
            else if (e.Event == ProductDataEvent.NotFound)    
            {
                // Algo Instrument was not found and TT API has given up looking for it
                Console.WriteLine("Cannot find Algo instrument: {0}", e.Message);
            }
        }

        #endregion

        void StartAlgo()
        {
            while (m_algo == null)
                mre.WaitOne();

            foreach (KeyValuePair<string, object> kvp in algo_userparams)
            {
                Console.WriteLine($"{kvp.Key} : {kvp.Value}");
            }

            OrderProfile algo_op = m_algo.GetOrderProfile();

            algo_op.OrderQuantity = Quantity.FromDecimal(m_instrument, 5); ;
            algo_op.Side = OrderSide.Buy;
            algo_op.OrderType = OrderType.Limit;
            algo_op.Account = m_accounts.ElementAt(0);
            algo_op.UserParameters = algo_userparams;
            m_algoTradeSubscription.SendOrder(algo_op);
        }

        #region ADL events

        void m_algoTradeSubscription_OrderBookDownload(object sender, OrderBookDownloadEventArgs e)
        {
            Console.WriteLine("Orderbook downloaded...");

            // Start Algo Trading thread.
            Thread algoThread = new Thread(() => this.StartAlgo());
            algoThread.Name = "Algo Trading Thread";
            algoThread.Start();
        }

        void m_algoTradeSubscription_OrderRejected(object sender, OrderRejectedEventArgs e)
        {
            Console.WriteLine("\nOrderRejected for : [{0}]", e.Order.SiteOrderKey);
        }

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

        void m_algoTradeSubscription_OrderDeleted(object sender, OrderDeletedEventArgs e)
        {
            Console.WriteLine("\nOrderDeleted [{0}] , Message : {1}", e.OldOrder.SiteOrderKey, e.Message);
        }

        void m_algoTradeSubscription_OrderAdded(object sender, OrderAddedEventArgs e)
        {
            if (e.Order.IsSynthetic)
                Console.WriteLine("\nPARENT Algo OrderAdded [{0}] for Algo : {1} with Synthetic Status : {2} ", e.Order.SiteOrderKey, e.Order.Algo.Alias, e.Order.SyntheticStatus.ToString());
            else
                Console.WriteLine("\nCHILD OrderAdded [{0}] {1}: {2}", e.Order.SiteOrderKey, e.Order.BuySell, e.Order.ToString());
        }

        void m_algoTradeSubscription_OrderUpdated(object sender, OrderUpdatedEventArgs e)
        {
            if (e.NewOrder.ExecutionType == ExecType.Restated)
                Console.WriteLine("\nAlgo Order Restated [{0}] for Algo : {1} with Synthetic Status : {2} ", e.NewOrder.SiteOrderKey, e.NewOrder.Algo.Alias, e.NewOrder.SyntheticStatus.ToString());
            else
                Console.WriteLine("\nOrderUpdated [{0}] {1}: {2}", e.NewOrder.SiteOrderKey, e.NewOrder.BuySell, e.NewOrder.ToString());
        }

        void m_algoTradeSubscription_ExportValuesUpdated(object sender, ExportValuesUpdatedEventArgs e)
        {
            foreach (string key in e.ExportValues.Keys)
            {
                Console.WriteLine("Algo EVU: Parameter Name = {0} and Parameter Value = {1}", key, e.ExportValues[key]);
            }
        }

        void m_algoTradeSubscription_AlertsUpdated(object sender, AlertsFiredEventArgs e)
        {
            foreach (string key in e.Alerts.Keys)
            {
                Console.WriteLine("Algo ALERTs Fired: Name = {0} and Alert Value = {1}", key, e.Alerts[key]);
            }
        }

        #endregion

        public void TTAPI_ShutdownCompleted(object sender, EventArgs e)
        {
            // Dispose of any other objects / resources
            Console.WriteLine("TTAPI shutdown completed");
        }
        private void UpdateAdlDropdownSource()
        {
            if (mainGrid.InvokeRequired)
            {
                mainGrid.Invoke(new Action(UpdateAdlDropdownSource));
                return;
            }

            var adlColumn = mainGrid.Columns[columnThreeName] as DataGridViewComboBoxColumn;
            if (adlColumn != null)
            {
                adlColumn.Items.Clear();
                adlColumn.Items.AddRange(adlParameters.Keys.ToArray());
            }
            mainGrid.Columns[columnThreeName].ReadOnly = false;
            add_btn.Enabled = true;
            add_btn.Text = "Add Row";
        }

        private void MainTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTab = MainTab.SelectedTab;
            if (selectedTab != null)
            {
                string tabName = selectedTab.Text;

                if (tabName == "main")
                {
                    currentTab = null;
                    currentADLName = null;
                }
                else if (tabAdlMapping.ContainsKey(tabName))
                {
                    currentTab = tabName;
                    currentADLName = tabAdlMapping[tabName];
                }
                else
                {
                    currentTab = null;
                    currentADLName = null;  // Fallback safety
                }

                Console.WriteLine($"Current ADL: {currentADLName ?? "None"}");
            }
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


            Dictionary<string, string> map = new Dictionary<string, string>();
            //         old,new

            for (int i = mainGrid.Rows.Count - 1; i >= 0; i--)
            {
                int x = i + 1;
                map[mainGrid.Rows[i].Cells[columnOneName].Value.ToString()] = x.ToString();
                mainGrid.Rows[i].Cells[columnOneName].Value = x.ToString();


            }
            Dictionary<string, DataGridView> temp_paramGrid = new Dictionary<string, DataGridView>();
            foreach (KeyValuePair<string, DataGridView> entry in tabParamGrids)
            {
                string old_key = entry.Key;
                if(map.ContainsKey(old_key))
                {
                    string new_key = map[old_key];
                    temp_paramGrid[new_key] = entry.Value;
                }
                
            }
            tabParamGrids.Clear();
            tabParamGrids = temp_paramGrid.ToDictionary(
                                entry => entry.Key,
                                entry => entry.Value // still shallow copy of value
                            );
            temp_paramGrid.Clear();

            Dictionary<string, string> temp_ADLmap= new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> entry in tabAdlMapping)
            {
                string old_key = entry.Key;
                if( map.ContainsKey(old_key))
                {
                    string new_key = map[old_key];
                    temp_ADLmap[new_key] = entry.Value;
                }
                
            }
            tabAdlMapping.Clear();
            tabAdlMapping = temp_ADLmap.ToDictionary(
                                entry => entry.Key,
                                entry => entry.Value // still shallow copy of value
                            );
            temp_ADLmap.Clear();

            foreach (KeyValuePair<string, string> entry in tabAdlMapping)
            {
                Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");
            }
            foreach (KeyValuePair<string, DataGridView> entry in tabParamGrids)
            {
                Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");
            }

            string curr_index = null;
            for (int i = MainTab.TabPages.Count - 1; i > 0; i--)
            {
                curr_index = MainTab.TabPages[i].Text.ToString();
                if (map.ContainsKey(curr_index))
                {
                    MainTab.TabPages[i].Text = map[curr_index].ToString();
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
                    tabAdlMapping.Remove(serial);
                    tabParamGrids.Remove(serial);
                    for (int i = MainTab.TabPages.Count - 1; i > 0; i--)
                    {
                        if (MainTab.TabPages[i].Text == serial)
                        {
                            MainTab.TabPages.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        private bool TabExists(string serial)
        {
            return MainTab.TabPages.Cast<TabPage>().Any(tab => tab.Text == serial);
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
            tabAdlMapping[serial] = adlValue;

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
                Height = 500,
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
            paramGrid.CellValidating += ParamGrid_CellValidating;
            paramGrid.Columns.Add("ParamName", "Parameter Name");
            paramGrid.Columns["ParamName"].ReadOnly = true;
            paramGrid.Columns["ParamName"].DefaultCellStyle.BackColor = Color.LightGray;
            paramGrid.Columns.Add("Value", "Value");

            tabParamGrids[serial] = paramGrid;

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
                OnStartbtnClick(paramGrid);
            };

            newTab.Controls.Add(btnStartAlgo);

            // Add tab to TabControl
            MainTab.TabPages.Add(newTab);
        }

        private void OnStartbtnClick(DataGridView paramGrid)
        {
            foreach (DataGridViewRow row in paramGrid.Rows)
            {
                if (row.IsNewRow) continue; // skip any placeholder row

                string paramName = row.Cells["ParamName"].Value?.ToString()?.Trim();
                var valueCell = row.Cells["Value"];

                if (valueCell != null)
                {
                    if (string.IsNullOrEmpty(paramName)) continue;

                    object value = null;

                    // Handle cell type: TextBox, ComboBox, CheckBox
                    if (valueCell is DataGridViewTextBoxCell || valueCell is DataGridViewComboBoxCell)
                    {
                        value = valueCell.Value;
                        if (value == null || (value is string && string.IsNullOrWhiteSpace((string)value)))
                        {
                            MessageBox.Show("Please enter all the parameters before starting the algo.");
                            return;
                        }
                    }
                    else if (valueCell is DataGridViewCheckBoxCell && valueCell.Value != null)
                    {
                        value = Convert.ToBoolean(valueCell.Value);
                    }

                    algo_userparams[paramName] = value;
                }
                else
                {
                    MessageBox.Show("Please enter all the parameters before starting the algo.");
                    return;
                }
            }

            StartAlgo();
        }

        private void add_btn_Click(object sender, EventArgs e)
        {
            int serialNumber = mainGrid.Rows.Count + 1;
            mainGrid.Rows.Add(false, serialNumber);
        }

        private void mainGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
           

            if (dgv.CurrentCell.ColumnIndex == 1) // Assuming "Value" column
            {
                TextBox tb = e.Control as TextBox;

                if (tb != null)
                {
                    tb.KeyPress -= NumericKeyPressHandler; // Prevent double hook
                    tb.KeyPress += NumericKeyPressHandler;

                    tb.Tag = GetCurrentParamType(dgv.CurrentCell.RowIndex, dgv); // Store type in Tag
                }
            }
        }
        private void NumericKeyPressHandler(object sender, KeyPressEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string paramType = tb.Tag?.ToString() ?? "string";

            if (paramType == "int")
            {
                // Allow digits and control keys only
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
            else if (paramType == "float")
            {
                // Allow digits, one '.', and control keys
                if (!char.IsControl(e.KeyChar) &&
                    !char.IsDigit(e.KeyChar) &&
                    e.KeyChar != '.')
                {
                    e.Handled = true;
                }

                // Only one dot allowed
                if (e.KeyChar == '.' && tb.Text.Contains('.'))
                {
                    e.Handled = true;
                }
            }
            else
            {
                // allow anything for string/bool
                e.Handled = false;
            }
        }
        private string GetCurrentParamType(int rowIndex, DataGridView dgv)
        {
            // First column is parameter name, we use that to find the type from dictionary
            string paramName = dgv.Rows[rowIndex].Cells[0].Value?.ToString();

            // Each tab has the ADL name in its header
            string adlName = MainTab.SelectedTab?.Text;
            if (adlParameters.ContainsKey(adlName))
            {
                foreach (var (name, type) in adlParameters[adlName])
                {
                    if (name == paramName)
                        return type;
                }
            }
            return "string"; // fallback
        }

        private bool IsValidInput(string input, string expectedType)
        {
            switch (expectedType.ToLower())
            {
                case "int":
                case "int_t":
                    return int.TryParse(input, out _);

                case "float":
                case "float_t":
                    // Only 1 decimal point allowed
                    if (input.Count(c => c == '.') > 1)
                        return false;
                    return float.TryParse(input, out _);

                case "string":
                case "string_t":
                    return true; // Any input is valid

                case "bool":
                case "boolean_t":
                    return input.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                           input.Equals("false", StringComparison.OrdinalIgnoreCase);

                default:
                    return false;
            }
        }
        private void ParamGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView paramGrid = tabParamGrids[currentTab];
            if (paramGrid.Columns[e.ColumnIndex].Name == "Value")
            {
                string input = e.FormattedValue.ToString();
                string expectedType = paramGrid.Rows[e.RowIndex].Cells["ParamName"].Value?.ToString();

                // If you're storing type in another column (say Tag or ParamType), fetch it that way
                string typeHint = adlParameters[currentADLName][e.RowIndex].paramType; // Example

                if (!IsValidInput(input, typeHint))
                {
                    MessageBox.Show($"Invalid input for type {typeHint}", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true; // Prevent leaving the cell
                }
            }
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            TTAPI.Shutdown();
        }
    }
}
