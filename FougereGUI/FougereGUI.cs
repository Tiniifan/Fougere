using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.VisualBasic;
using FougereGUI.Level5.Animation;
using FougereGUI.Tools;

namespace FougereGUI
{
    public partial class FougereGUI : Form
    {
        private AnimationManager AnimationManager;

        private TreeNode SelectedRightClickTreeNode;

        public FougereGUI()
        {
            InitializeComponent();
        }

        private TreeNode CreateTreeNode(KeyValuePair<string, Dictionary<int, object>> item)
        {
            TreeNode itemNode = null;



            return itemNode;
        }

        private void DrawTreeView()
        {
            TreeNode rootNode = new TreeNode(AnimationManager.AnimationName);
            rootNode.Tag = "Root";
            rootNode.ContextMenuStrip = rootContextMenuStrip;

            // Add property node
            TreeNode property = new TreeNode("Property");
            property.Tag = "Property";
            rootNode.Nodes.Add(property);
            
            foreach (KeyValuePair<string, Dictionary<string, Dictionary<int, object>>> node in AnimationManager.Nodes)
            {
                TreeNode categoryNode = new TreeNode(node.Key);
                categoryNode.Tag = "Category";
                categoryNode.ContextMenuStrip = categoryContextMenuStrip;

                foreach (KeyValuePair<string, Dictionary<int, object>> item in node.Value)
                {
                    TreeNode itemNode = new TreeNode(item.Key);
                    itemNode.Tag = "Item";
                    itemNode.ContextMenuStrip = itemContextMenuStrip;

                    foreach (int frame in item.Value.Keys)
                    {
                        TreeNode frameNode = new TreeNode(frame.ToString());
                        frameNode.Tag = "Frame";
                        frameNode.ContextMenuStrip = frameContextMenuStrip;
                        itemNode.Nodes.Add(frameNode);
                    }

                    categoryNode.Nodes.Add(itemNode);
                    categoryNode.Expand();
                }

                rootNode.Nodes.Add(categoryNode);
            }

            mainTreeView.Nodes.Clear();
            mainTreeView.Nodes.Add(rootNode);
            rootNode.Expand();
        }

        private void OpenFile(string fileName)
        {
            if (Path.GetExtension(fileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                AnimationManager = JsonConvert.DeserializeObject<AnimationManager>(string.Join("", File.ReadAllLines(fileName)));
            }
            else
            {
                AnimationManager = new AnimationManager(new FileStream(fileName, FileMode.Open, FileAccess.Read));
            }

            DrawTreeView();
            saveToolStripMenuItem.Enabled = true;
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Level 5 Animation files (*.mtn2;*.imm2;*.mtm2)|*.mtn2;*.imm2;*.mtm2|JSON files (*.json)|*.json";
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OpenFile(openFileDialog1.FileName);
            }
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string frameString = Interaction.InputBox("Enter name:");

            AnimationManager = new AnimationManager();
            AnimationManager.AnimationName = frameString;
            AnimationManager.FrameCount = 0;
            AnimationManager.Version = "V2";
            AnimationManager.Format = "XIMA";

            DrawTreeView();
            saveToolStripMenuItem.Enabled = true;
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Level 5 Bone Animation files (*.mtn2)|*.mtn2|Level 5 UV Animation files (*.imm2)|*.imm2|Level 5 Texture Animation files (*.mtm2)|*.mtm2|JSON files (*.json)|*.json";
            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(openFileDialog1.FileName);
            saveFileDialog1.RestoreDirectory = true;

            switch (AnimationManager.Format)
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
                    File.WriteAllText(fileName, AnimationManager.ToJson());
                }
                else
                {
                    AnimationManager.Save(fileName);
                }

                MessageBox.Show(Path.GetFileName(fileName) + " saved!");
            }
        }

        private void FougereGUI_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string dragPath = Path.GetFullPath(files[0]);
            string dragExt = Path.GetExtension(files[0]);

            if (files.Length > 1) return;
            if (dragExt != ".mtn2" & dragExt != ".imm2" & dragExt != ".mtm2" & dragExt != ".json") return;

            openFileDialog1.FileName = dragPath;
            OpenFile(openFileDialog1.FileName);
        }

        private void FougereGUI_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void MainTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Clear datagrid
            variablesDataGridView.Rows.Clear();

            if (e.Node.Tag != null && e.Node.Tag.ToString() == "Frame")
            {
                // Get frame, item category depending on where clicked
                int frame = Convert.ToInt32(e.Node.Text);
                string item = e.Node.Parent.Text;
                string category = e.Node.Parent.Parent.Text;

                // Get animation data
                object animationData = AnimationManager.Nodes[category][item][frame];

                // Use reflection to obtain the name and its value
                foreach (PropertyInfo property in animationData.GetType().GetProperties())
                {
                    int rowIndex = variablesDataGridView.Rows.Add(property.Name, property.GetValue(animationData));

                    for (int columnIndex = 0; columnIndex < variablesDataGridView.Columns.Count; columnIndex++)
                    {
                        variablesDataGridView.Rows[rowIndex].Cells[columnIndex].ReadOnly = false;
                    }
                }

                variablesDataGridView.Enabled = true;
            } else if (e.Node.Tag != null && e.Node.Tag.ToString() == "Property")
            {
                variablesDataGridView.Rows.Add("Version", Convert.ToInt32(AnimationManager.Version.Replace("V", "")));
                variablesDataGridView.Rows.Add("Format", AnimationManager.Format);
                variablesDataGridView.Rows.Add("Frames", AnimationManager.FrameCount);
                variablesDataGridView.Rows[0].Cells[1].ReadOnly = false;
                variablesDataGridView.Rows[1].Cells[1].ReadOnly = false;
                variablesDataGridView.Rows[2].Cells[1].ReadOnly = false;
                variablesDataGridView.Enabled = true;
            }
            else
            {
                variablesDataGridView.Enabled = false;
            }
        }

        private void MainTreeView_MouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                SelectedRightClickTreeNode = e.Node;
            }
        }

        private void MainTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                SelectedRightClickTreeNode = e.Node;
            }
        }

        private void VariablesDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (mainTreeView.SelectedNode.Tag != null && mainTreeView.SelectedNode.Tag.ToString() == "Frame")
            {
                // Retrieve information about the modified cell
                int rowIndex = e.RowIndex;
                int columnIndex = e.ColumnIndex;

                // Get frame, item category depending on where clicked
                int frame = Convert.ToInt32(mainTreeView.SelectedNode.Text);
                string item = mainTreeView.SelectedNode.Parent.Text;
                string category = mainTreeView.SelectedNode.Parent.Parent.Text;
                object animationData = AnimationManager.Nodes[category][item][frame];

                // Use reflection to obtain the variable being modified 
                string propertyName = variablesDataGridView.Rows[rowIndex].Cells[0].Value.ToString();
                Type animationDataType = animationData.GetType();
                PropertyInfo property = animationDataType.GetProperty(propertyName);

                // Retrieve the modified value
                object modifiedValue = variablesDataGridView.Rows[rowIndex].Cells[columnIndex].Value;

                try
                {
                    // Convert the modified value to single
                    float newValue = Convert.ToSingle(modifiedValue);

                    // Edit value using reflection
                    property.SetValue(animationData, newValue);

                    // Update the value of the cell
                    variablesDataGridView.Rows[rowIndex].Cells[columnIndex].Value = newValue;
                }
                catch
                {
                    // If conversion fails, cancel the edit
                    variablesDataGridView.CancelEdit();
                    variablesDataGridView.Rows[rowIndex].Cells[columnIndex].Value = property.GetValue(animationData);
                }
            } else if (mainTreeView.SelectedNode.Tag != null && mainTreeView.SelectedNode.Tag.ToString() == "Property")
            {
                // Retrieve information about the modified cell
                int rowIndex = e.RowIndex;
                int columnIndex = e.ColumnIndex;

                // Retrieve the modified value
                string modifiedValue = variablesDataGridView.Rows[rowIndex].Cells[columnIndex].Value.ToString();

                if (rowIndex == 1)
                {
                    if (modifiedValue == "XMTN" || modifiedValue == "XIMA" || modifiedValue == "XMTM")
                    {
                        AnimationManager.Format = modifiedValue;
                    } else
                    {
                        MessageBox.Show("Format should be XMTN or XIMA or XMTM");
                        variablesDataGridView.Rows[1].Cells[1].Value = AnimationManager.Format;
                    }
                } else
                {
                    try
                    {
                        // Convert the modified value to single
                        int newValue = Convert.ToInt32(modifiedValue);

                        if (rowIndex == 0)
                        {
                            if (newValue != 1 && newValue != 2)
                            {
                                MessageBox.Show("Version should be 1 or 2");
                                throw new ArgumentException("Version should be 1 or 2");
                            }

                            AnimationManager.Version = "V" + newValue;
                        }
                        else if (rowIndex == 2)
                        {
                            if (newValue < 0)
                            {
                                MessageBox.Show("Please enter a value greater than or equal to 0");
                                throw new ArgumentException("Please enter a value greater than or equal to 0");
                            }

                            AnimationManager.FrameCount = newValue;
                        }
                    }
                    catch
                    {
                        // If conversion fails, cancel the edit
                        variablesDataGridView.CancelEdit();

                        if (rowIndex == 0)
                        {
                            variablesDataGridView.Rows[0].Cells[1].Value = AnimationManager.Version.Replace("V", "");
                        }
                        else if (rowIndex == 2)
                        {
                            variablesDataGridView.Rows[2].Cells[1].Value = AnimationManager.FrameCount;
                        }
                    }
                }
            }
        }

        private void DeleteFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedRightClickTreeNode != null)
            {
                // Get frame, item category depending on where clicked
                int frame = Convert.ToInt32(SelectedRightClickTreeNode.Text);
                string item = SelectedRightClickTreeNode.Parent.Text;
                string category = SelectedRightClickTreeNode.Parent.Parent.Text;

                // Remove frame
                AnimationManager.Nodes[category][item].Remove(frame);
                SelectedRightClickTreeNode.Remove();
            }

            // Reset
            SelectedRightClickTreeNode = null;
        }

        private void AddFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedRightClickTreeNode != null)
            {
                // Get item category depending on where clicked
                string item = SelectedRightClickTreeNode.Text;
                string category = SelectedRightClickTreeNode.Parent.Text;

                string frameString = Interaction.InputBox("Enter frame:");
                try
                {
                    // Get frame
                    int frameNumber = Convert.ToInt32(frameString);

                    if (frameNumber < 0)
                    {
                        MessageBox.Show("Please enter a value greater than or equal to 0");
                    } else
                    {
                        // Get frameIndex
                        int frameIndex = SelectedRightClickTreeNode.Nodes
                            .Cast<TreeNode>()
                            .Select((node, idx) => new { Node = node, Index = idx })
                            .Where(myItem => myItem.Node.Text == frameString)
                            .Select(myItem => myItem.Index)
                            .DefaultIfEmpty(-1)
                            .First();

                        if (frameIndex != -1)
                        {
                            // The frame already exists so select it
                            mainTreeView.SelectedNode = SelectedRightClickTreeNode.Nodes[frameIndex];
                        }
                        else
                        {
                            AnimationManager.Nodes[category][item].Add(frameNumber, Activator.CreateInstance(Type.GetType("FougereGUI.Level5.Animation.Logic." + category)));

                            // The frame has been added, you need to sort the frames
                            Dictionary<int, object> tempFrameDict = new Dictionary<int, object>(AnimationManager.Nodes[category][item]);
                            int[] sortedFrameIndexes = tempFrameDict.Keys.OrderBy(key => key).ToArray();

                            // Clear
                            AnimationManager.Nodes[category][item].Clear();
                            SelectedRightClickTreeNode.Nodes.Clear();

                            TreeNode newSelectedNode = null;

                            foreach (int frame in sortedFrameIndexes)
                            {
                                // Add frames back to the tree view
                                TreeNode newFrame = new TreeNode(frame.ToString());
                                newFrame.Tag = "Frame";
                                newFrame.ContextMenuStrip = frameContextMenuStrip;
                                SelectedRightClickTreeNode.Nodes.Add(newFrame);

                                if (frame == frameNumber)
                                {
                                    newSelectedNode = newFrame;
                                }

                                // Add frames back to the node
                                AnimationManager.Nodes[category][item].Add(frame, tempFrameDict[frame]);                   
                            }

                            // Select the added frame
                            mainTreeView.SelectedNode = newSelectedNode;
                        }
                    }
                } catch
                {
                    MessageBox.Show("Please enter numeric value");
                }
            }

            // Reset
            SelectedRightClickTreeNode = null;
        }

        private void DeleteItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedRightClickTreeNode != null)
            {
                // Get item category depending on where clicked
                string item = SelectedRightClickTreeNode.Text;
                string category = SelectedRightClickTreeNode.Parent.Text;

                // Remove selected item
                AnimationManager.Nodes[category].Remove(item);
                SelectedRightClickTreeNode.Remove();

                variablesDataGridView.Rows.Clear();
                variablesDataGridView.Enabled = false;
            }

            // Reset
            SelectedRightClickTreeNode = null;
        }

        private void AddItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedRightClickTreeNode != null)
            {
                string itemName = Interaction.InputBox("Enter item name (string or hash):");

                if (itemName.Length == 8)
                {
                    try
                    {
                        int itemNumber = Convert.ToInt32(itemName, 16);

                    }
                    catch
                    {
                        itemName = Crc32.Compute(Encoding.GetEncoding(932).GetBytes(itemName)).ToString("X8");
                    }
                } else
                {
                    itemName = Crc32.Compute(Encoding.GetEncoding(932).GetBytes(itemName)).ToString("X8");
                }

                // Get item category depending on where clicked
                string category = SelectedRightClickTreeNode.Text;

                // Add selected item
                AnimationManager.Nodes[category].Add(itemName, new Dictionary<int, object>());

                // Create new item node
                TreeNode itemNode = new TreeNode(itemName);
                itemNode.Tag = "Item";
                itemNode.ContextMenuStrip = itemContextMenuStrip;
                SelectedRightClickTreeNode.Nodes.Add(itemNode);

                // Select the added item
                mainTreeView.SelectedNode = itemNode;
            }

            // Reset
            SelectedRightClickTreeNode = null;
        }

        private void AddNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedRightClickTreeNode != null)
            {
                if (AnimationManager.Nodes.Count > 4)
                {
                    MessageBox.Show("Cannot add more than 4 nodes");
                } else
                {
                    InputComboBoxWindow inputComboBoxWindow = new InputComboBoxWindow("Select Type", AnimationSupport.TrackDataCount.Keys.ToArray());
                    if (inputComboBoxWindow.ShowDialog() == DialogResult.OK)
                    {
                        string selectedType = inputComboBoxWindow.SelectedItem;

                        if (AnimationManager.Nodes.ContainsKey(selectedType))
                        {
                            // The node already exists
                            mainTreeView.SelectedNode = SelectedRightClickTreeNode.Nodes[AnimationManager.Nodes.Keys.ToList().IndexOf(selectedType)];
                        } else
                        {
                            // Add selected node
                            AnimationManager.Nodes.Add(selectedType, new Dictionary<string, Dictionary<int, object>>());

                            // Create new node
                            TreeNode categoryNode = new TreeNode(selectedType);
                            categoryNode.Tag = "Category";
                            categoryNode.ContextMenuStrip = categoryContextMenuStrip;
                            SelectedRightClickTreeNode.Nodes.Add(categoryNode);

                            // Select the added item
                            mainTreeView.SelectedNode = categoryNode;
                        }
                    }
                }
            }

            // Reset
            SelectedRightClickTreeNode = null;
        }

        private void RenameAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedRightClickTreeNode != null)
            {
                string newAnimationName = Interaction.InputBox("Enter name:");
                AnimationManager.AnimationName = newAnimationName;
                SelectedRightClickTreeNode.Text = newAnimationName;
            }

            // Reset
            SelectedRightClickTreeNode = null;
        }
    }
}
