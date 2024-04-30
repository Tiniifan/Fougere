using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using FougereGUI.Level5.Animation;

namespace FougereGUI
{
    public partial class FougereGUI : Form
    {
        public FougereGUI()
        {
            InitializeComponent();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "All Supported Formats|*.mtn2;*.imm2;*.mtm2;*.json|Level 5 Animation files (*.mtn2;*.imm2;*.mtm2)|*.mtn2;*.imm2;*.mtm2|JSON files (*.json)|*.json";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                AnimationManager animationManager;
                string fileName = openFileDialog1.FileName;

                if (Path.GetExtension(fileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    animationManager = JsonConvert.DeserializeObject<AnimationManager>(string.Join("", File.ReadAllLines(fileName)));
                }
                else
                {
                    animationManager = new AnimationManager(new FileStream(fileName, FileMode.Open, FileAccess.Read));
                }

                // Display the file in the output
                previewTextBox.Text = animationManager.ToJson();
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimationManager animationManager = null;

            try
            {
                animationManager = JsonConvert.DeserializeObject<AnimationManager>(string.Join("", previewTextBox.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while creating json: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (animationManager != null)
            {
                saveFileDialog1.Filter = "Level 5 Bone Animation files (*.mtn2)|*.mtn2|Level 5 UV Animation files (*.imm2)|*.imm2|Level 5 Texture Animation files (*.mtm2)|*.mtm2|JSON files (*.json)|*.json";
                saveFileDialog1.RestoreDirectory = true;

                switch (animationManager.Format)
                {
                    case "XMTN":
                        saveFileDialog1.FilterIndex = 1;
                        break;
                    case "XIMA":
                        saveFileDialog1.FilterIndex = 2;
                        break;
                    case "XMTM":
                        saveFileDialog1.FilterIndex = 3;
                        break;
                }

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string fileName = saveFileDialog1.FileName;

                    if (Path.GetExtension(fileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        File.WriteAllText(fileName, previewTextBox.Text);
                    }
                    else
                    {
                        animationManager.Save(fileName);
                    }

                    MessageBox.Show(Path.GetFileName(fileName) + " saved!");
                }
            }
        }
    }
}
