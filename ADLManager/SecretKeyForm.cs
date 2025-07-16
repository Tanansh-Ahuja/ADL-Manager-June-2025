using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ADLManager
{
    public partial class SecretKeyForm : Form
    {
        public string SecretKey { get; private set; }
        private readonly string keyFilePath = "key.txt";
        public SecretKeyForm()
        {
            InitializeComponent();
            Load += SecretKeyForm_Load;
        }
        private void SecretKeyForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(keyFilePath))
            {
                string existingKey = File.ReadAllText(keyFilePath);
                txtKey.Text = existingKey;
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string enteredKey = txtKey.Text.Trim();
            if (string.IsNullOrEmpty(enteredKey))
            {
                MessageBox.Show("Please enter a valid key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SecretKey = enteredKey;
            
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
