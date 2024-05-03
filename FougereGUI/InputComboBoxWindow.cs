using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FougereGUI
{
    public partial class InputComboBoxWindow : Form
    {
        public string SelectedItem { get; private set; }

        public InputComboBoxWindow(string name, string [] items)
        {
            InitializeComponent();

            this.Text = name;
            listComboBox.Items.AddRange(items);
            listComboBox.SelectedIndex = 0;
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            if (listComboBox.SelectedIndex != -1)
            {
                SelectedItem = listComboBox.SelectedItem.ToString();
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
