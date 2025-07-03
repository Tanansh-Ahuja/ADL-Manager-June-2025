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
            mainGrid = new DataGridView();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            del_btn = new Button();
            add_btn = new Button();
            Select = new DataGridViewCheckBoxColumn();
            Sno = new DataGridViewTextBoxColumn();
            feed = new DataGridViewComboBoxColumn();
            adl = new DataGridViewComboBoxColumn();
            createTab = new DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)mainGrid).BeginInit();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            SuspendLayout();
            // 
            // mainGrid
            // 
            mainGrid.AllowUserToAddRows = false;
            mainGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            mainGrid.Columns.AddRange(new DataGridViewColumn[] { Select, Sno, feed, adl, createTab });
            mainGrid.Location = new Point(3, 2);
            mainGrid.Margin = new Padding(3, 2, 3, 2);
            mainGrid.Name = "mainGrid";
            mainGrid.RowHeadersVisible = false;
            mainGrid.RowHeadersWidth = 51;
            mainGrid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            mainGrid.Size = new Size(520, 351);
            mainGrid.TabIndex = 0;
            mainGrid.CellValueChanged += mainGrid_CellValueChanged;
            mainGrid.CurrentCellDirtyStateChanged += mainGrid_CurrentCellDirtyStateChanged;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Location = new Point(10, 9);
            tabControl1.Margin = new Padding(3, 2, 3, 2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(623, 380);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(del_btn);
            tabPage1.Controls.Add(add_btn);
            tabPage1.Controls.Add(mainGrid);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Margin = new Padding(3, 2, 3, 2);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3, 2, 3, 2);
            tabPage1.Size = new Size(615, 352);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Main";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // del_btn
            // 
            del_btn.Location = new Point(528, 31);
            del_btn.Margin = new Padding(3, 2, 3, 2);
            del_btn.Name = "del_btn";
            del_btn.Size = new Size(82, 22);
            del_btn.TabIndex = 2;
            del_btn.Text = "Delete Row";
            del_btn.UseVisualStyleBackColor = true;
            del_btn.Click += del_btn_Click;
            // 
            // add_btn
            // 
            add_btn.Location = new Point(528, 4);
            add_btn.Margin = new Padding(3, 2, 3, 2);
            add_btn.Name = "add_btn";
            add_btn.Size = new Size(82, 22);
            add_btn.TabIndex = 1;
            add_btn.Text = "Add Row";
            add_btn.UseVisualStyleBackColor = true;
            add_btn.Click += add_btn_Click;
            // 
            // Select
            // 
            Select.Frozen = true;
            Select.HeaderText = "";
            Select.MinimumWidth = 6;
            Select.Name = "Select";
            Select.Width = 50;
            // 
            // Sno
            // 
            Sno.Frozen = true;
            Sno.HeaderText = "S No";
            Sno.MinimumWidth = 6;
            Sno.Name = "Sno";
            Sno.ReadOnly = true;
            Sno.Width = 125;
            // 
            // feed
            // 
            feed.HeaderText = "Feed";
            feed.Items.AddRange(new object[] { "a", "EURUSD", "c", "d", "e" });
            feed.MinimumWidth = 6;
            feed.Name = "feed";
            feed.Width = 125;
            // 
            // adl
            // 
            adl.HeaderText = "ADL";
            adl.Items.AddRange(new object[] { "ADL #1", "Scalar 2_0", "ADL #3", "ADL #4" });
            adl.MinimumWidth = 6;
            adl.Name = "adl";
            adl.Width = 125;
            // 
            // createTab
            // 
            createTab.HeaderText = "Create Tab";
            createTab.MinimumWidth = 6;
            createTab.Name = "createTab";
            createTab.Width = 125;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(642, 406);
            Controls.Add(tabControl1);
            Margin = new Padding(3, 2, 3, 2);
            MaximumSize = new Size(658, 445);
            MinimumSize = new Size(658, 445);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)mainGrid).EndInit();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DataGridView mainGrid;
        private TabControl tabControl1;
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

