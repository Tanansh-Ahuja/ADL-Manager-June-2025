using System.Drawing;
using System.Windows.Forms;

namespace ADLManager
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainGrid = new System.Windows.Forms.DataGridView();
            this.Select = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Sno = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.feed = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.adl = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.createTab = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.MainTab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.del_btn = new System.Windows.Forms.Button();
            this.add_btn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.mainGrid)).BeginInit();
            this.MainTab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainGrid
            // 
            this.mainGrid.AllowUserToAddRows = false;
            this.mainGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.mainGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.mainGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Select,
            this.Sno,
            this.feed,
            this.adl,
            this.createTab});
            this.mainGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainGrid.Location = new System.Drawing.Point(3, 2);
            this.mainGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.mainGrid.Name = "mainGrid";
            this.mainGrid.RowHeadersVisible = false;
            this.mainGrid.RowHeadersWidth = 51;
            this.mainGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.mainGrid.Size = new System.Drawing.Size(850, 765);
            this.mainGrid.TabIndex = 0;
            this.mainGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.mainGrid_CellValueChanged);
            this.mainGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.mainGrid_CurrentCellDirtyStateChanged);
            // 
            // Select
            // 
            this.Select.Frozen = true;
            this.Select.HeaderText = "";
            this.Select.MinimumWidth = 6;
            this.Select.Name = "Select";
            this.Select.Width = 40;
            // 
            // Sno
            // 
            this.Sno.Frozen = true;
            this.Sno.HeaderText = "S No";
            this.Sno.MinimumWidth = 6;
            this.Sno.Name = "Sno";
            this.Sno.ReadOnly = true;
            this.Sno.Width = 70;
            // 
            // feed
            // 
            this.feed.HeaderText = "Feed";
            this.feed.Items.AddRange(new object[] {
            "a",
            "EURUSD",
            "c",
            "d",
            "e"});
            this.feed.MinimumWidth = 6;
            this.feed.Name = "feed";
            this.feed.Width = 125;
            // 
            // adl
            // 
            this.adl.HeaderText = "ADL";
            this.adl.Items.AddRange(new object[] {
            "connecting..."});
            this.adl.MinimumWidth = 6;
            this.adl.Name = "adl";
            this.adl.Width = 125;
            // 
            // createTab
            // 
            this.createTab.HeaderText = "Create Tab";
            this.createTab.MinimumWidth = 6;
            this.createTab.Name = "createTab";
            this.createTab.Width = 125;
            // 
            // MainTab
            // 
            this.MainTab.Controls.Add(this.tabPage1);
            this.MainTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTab.Location = new System.Drawing.Point(0, 0);
            this.MainTab.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MainTab.Name = "MainTab";
            this.MainTab.SelectedIndex = 0;
            this.MainTab.Size = new System.Drawing.Size(864, 798);
            this.MainTab.TabIndex = 1;
            this.MainTab.SelectedIndexChanged += new System.EventHandler(this.MainTab_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.del_btn);
            this.tabPage1.Controls.Add(this.add_btn);
            this.tabPage1.Controls.Add(this.mainGrid);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage1.Size = new System.Drawing.Size(856, 769);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Main";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // del_btn
            // 
            this.del_btn.Location = new System.Drawing.Point(677, 43);
            this.del_btn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.del_btn.Name = "del_btn";
            this.del_btn.Size = new System.Drawing.Size(124, 34);
            this.del_btn.TabIndex = 2;
            this.del_btn.Text = "Delete Row";
            this.del_btn.UseVisualStyleBackColor = true;
            this.del_btn.Click += new System.EventHandler(this.del_btn_Click);
            // 
            // add_btn
            // 
            this.add_btn.Location = new System.Drawing.Point(677, 6);
            this.add_btn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.add_btn.Name = "add_btn";
            this.add_btn.Size = new System.Drawing.Size(124, 31);
            this.add_btn.TabIndex = 1;
            this.add_btn.Text = "Add Row";
            this.add_btn.UseVisualStyleBackColor = true;
            this.add_btn.Click += new System.EventHandler(this.add_btn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 798);
            this.Controls.Add(this.MainTab);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(749, 468);
            this.Name = "Form1";
            this.Text = "Control ADL";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.mainGrid)).EndInit();
            this.MainTab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DataGridView mainGrid;
        private TabControl MainTab;
        private TabPage tabPage1;
        private Button add_btn;
        private Button del_btn;
        private DataGridViewCheckBoxColumn Select;
        private DataGridViewTextBoxColumn Sno;
        private DataGridViewComboBoxColumn feed;
        private DataGridViewComboBoxColumn adl;
        private DataGridViewCheckBoxColumn createTab;
    }
}

