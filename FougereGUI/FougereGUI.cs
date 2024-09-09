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
using StudioElevenLib.Tools;
using StudioElevenLib.Level5.Animation;
using StudioElevenLib.Level5.Animation.Logic;
using StudioElevenLib.Level5.Resource;
using StudioElevenLib.Level5.Resource.RES;
using StudioElevenLib.Level5.Resource.XRES;

namespace FougereGUI
{
    public partial class FougereGUI : Form
    {
        private AnimationManager AnimationManager;

        private TreeNode SelectedRightClickTreeNode;

        private Dictionary<string, string> ResourcesDict;

        public FougereGUI()
        {
            InitializeComponent();

            ResourcesDict = new Dictionary<string, string>();

            if (File.Exists("ResourcesDict.json"))
            {
                try
                {
                    string jsonString = File.ReadAllText("ResourcesDict.json");
                    ResourcesDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
                }
                catch
                {
                    // Ignore
                }
            }
        }

        private IResource TryOpenResource(string fileName)
        {
            try
            {
                return new RES(new FileStream(fileName, FileMode.Open, FileAccess.Read));
            }
            catch
            {
                return null;
            }
        }

        private IResource TryOpenXResource(string fileName)
        {
            try
            {
                return new XRES(new FileStream(fileName, FileMode.Open, FileAccess.Read));
            }
            catch
            {
                return null;
            }
        }

        public string ToJson()
        {
            var properties = new Dictionary<string, object>
            {
                {"Format", AnimationManager.Format},
                {"Version", AnimationManager.Version},
                {"FrameCount", AnimationManager.FrameCount },
                {"AnimationName", AnimationManager.AnimationName},
                {"Nodes", AnimationManager.Tracks}
            };

            return JsonConvert.SerializeObject(properties, Formatting.Indented);
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
            
            foreach (Track track in AnimationManager.Tracks)
            {
                TreeNode categoryNode = new TreeNode(track.Name);
                categoryNode.Tag = "Category";
                categoryNode.ContextMenuStrip = categoryContextMenuStrip;

                foreach (Node node in track.Nodes)
                {
                    string nodeName = node.Name;

                    if (ResourcesDict != null & ResourcesDict.ContainsKey(nodeName))
                    {
                        nodeName = ResourcesDict[nodeName];
                    }

                    TreeNode itemNode = new TreeNode(nodeName);
                    itemNode.Tag = "Item";
                    itemNode.ContextMenuStrip = itemContextMenuStrip;

                    foreach (int frame in node.Frames.Keys)
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
                    File.WriteAllText(fileName, ToJson());
                }
                else
                {
                    File.WriteAllBytes(fileName, AnimationManager.Save());
                }

                MessageBox.Show(Path.GetFileName(fileName) + " saved!");
            }
        }

        private void CreateResourceDictToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog3.FileName = "";
            openFileDialog3.Multiselect = true;
            openFileDialog3.Filter = "Level 5 Resource files (*.bin)|*.bin";
            openFileDialog3.RestoreDirectory = true;

            if (openFileDialog3.ShowDialog() == DialogResult.OK)
            {
                foreach (string fileName in openFileDialog3.FileNames)
                {
                    IResource resource = TryOpenResource(fileName) ?? TryOpenXResource(fileName);

                    if (resource == null)
                    {
                        continue;
                    }

                    foreach (string itemName in resource.StringTable)
                    {
                        // Compute CRC32 for the full item name
                        int crc32Full = unchecked((int)Crc32.Compute(Encoding.GetEncoding("Shift-JIS").GetBytes(itemName)));
                        string crc32FullHex = crc32Full.ToString("X8");

                        // Add the full item name to the dictionary if it doesn't already exist
                        if (!ResourcesDict.ContainsKey(crc32FullHex))
                        {
                            ResourcesDict.Add(crc32FullHex, itemName);
                        }

                        // Split the item name by dots to get individual parts
                        string[] parts = itemName.Split('.');

                        // Add each part to the dictionary
                        foreach (string part in parts)
                        {
                            // Compute CRC32 for the individual part
                            int crc32Part = unchecked((int)Crc32.Compute(Encoding.GetEncoding("Shift-JIS").GetBytes(part)));
                            string crc32PartHex = crc32Part.ToString("X8");

                            // Add the individual part to the dictionary if it doesn't already exist
                            if (!ResourcesDict.ContainsKey(crc32PartHex))
                            {
                                ResourcesDict.Add(crc32PartHex, part);
                            }
                        }
                    }
                }

                // Serialize the dictionary to JSON and save to a file
                string jsonString = JsonConvert.SerializeObject(ResourcesDict, Formatting.Indented);
                File.WriteAllText("ResourcesDict.json", jsonString);

                MessageBox.Show("ResourceDict.json updated!");
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

            if (e.Node.Tag != null && e.Node.Tag.ToString() == "Category")
            {
                // Get category depending on where clicked
                int categoryIndex = e.Node.Index - 1;

                Track track = AnimationManager.Tracks[categoryIndex];

                variablesDataGridView.Rows.Add("Name", track.Name);
                variablesDataGridView.Rows.Add("Index", track.Index);

                variablesDataGridView.Rows[0].Cells[1].ReadOnly = true;
                variablesDataGridView.Rows[1].Cells[1].ReadOnly = true;

                int i = 0;
                foreach (Node node in track.Nodes)
                {
                    string nodeName = node.Name;

                    if (ResourcesDict != null & ResourcesDict.ContainsKey(node.Name))
                    {
                        nodeName = ResourcesDict[node.Name];
                    }

                    variablesDataGridView.Rows.Add($"Node {i+1}", nodeName);
                    variablesDataGridView.Rows[2 + i].Cells[1].ReadOnly = true;

                    i++;
                }
            }
            else if (e.Node.Tag != null && e.Node.Tag.ToString() == "Item")
            {
                // Get item, category depending on where clicked
                int itemIndex = e.Node.Index;
                int categoryIndex = e.Node.Parent.Index - 1;

                Node node = AnimationManager.Tracks[categoryIndex].Nodes[itemIndex];

                if (ResourcesDict != null & ResourcesDict.ContainsKey(node.Name))
                {
                    variablesDataGridView.Rows.Add("Name", ResourcesDict[node.Name]);
                    variablesDataGridView.Rows.Add("Hex", node.Name);
                    variablesDataGridView.Rows.Add("Main Track", Convert.ToString(node.IsInMainTrack));
                    variablesDataGridView.Rows.Add("Frames", node.Frames.Count());
                    variablesDataGridView.Rows[0].Cells[1].ReadOnly = true;
                    variablesDataGridView.Rows[1].Cells[1].ReadOnly = true;
                    variablesDataGridView.Rows[2].Cells[1].ReadOnly = true;
                    variablesDataGridView.Rows[3].Cells[1].ReadOnly = true;
                } else
                {
                    variablesDataGridView.Rows.Add("Name", node.Name);
                    variablesDataGridView.Rows.Add("Main Track", Convert.ToString(node.IsInMainTrack));
                    variablesDataGridView.Rows.Add("Frames", node.Frames.Count());
                    variablesDataGridView.Rows[0].Cells[1].ReadOnly = true;
                    variablesDataGridView.Rows[1].Cells[1].ReadOnly = true;
                    variablesDataGridView.Rows[2].Cells[1].ReadOnly = true;
                }

                variablesDataGridView.Enabled = true;
            }
            else if (e.Node.Tag != null && e.Node.Tag.ToString() == "Frame")
            {
                // Get frame, item category depending on where clicked
                int frame = Convert.ToInt32(e.Node.Text);
                int itemIndex = e.Node.Parent.Index;
                int categoryIndex = e.Node.Parent.Parent.Index - 1;

                object animationData = AnimationManager.Tracks[categoryIndex].Nodes[itemIndex].Frames[frame];

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
            } 
            else if (e.Node.Tag != null && e.Node.Tag.ToString() == "Property")
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
                int itemIndex = mainTreeView.SelectedNode.Parent.Index;
                int categoryIndex = mainTreeView.SelectedNode.Parent.Parent.Index - 1;
                object animationData = AnimationManager.Tracks[categoryIndex].Nodes[itemIndex].Frames[frame];

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
                int itemIndex = SelectedRightClickTreeNode.Parent.Index;
                int categoryIndex = SelectedRightClickTreeNode.Parent.Parent.Index - 1;

                // Remove frame
                AnimationManager.Tracks[categoryIndex].Nodes[itemIndex].Frames.Remove(frame);
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
                int itemIndex = SelectedRightClickTreeNode.Index;
                int categoryIndex = SelectedRightClickTreeNode.Parent.Index - 1;

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
                            string category = SelectedRightClickTreeNode.Parent.Text;
                            AnimationManager.Tracks[categoryIndex].Nodes[itemIndex].Frames.Add(frameNumber, Activator.CreateInstance(Type.GetType("StudioElevenLib.Level5.Animation.Logic." + category + ", StudioElevenLib")));

                            // The frame has been added, you need to sort the frames
                            Dictionary<int, object> tempFrameDict = new Dictionary<int, object>(AnimationManager.Tracks[categoryIndex].Nodes[itemIndex].Frames);
                            int[] sortedFrameIndexes = tempFrameDict.Keys.OrderBy(key => key).ToArray();
                            Console.WriteLine("3");

                            // Clear
                            AnimationManager.Tracks[categoryIndex].Nodes[itemIndex].Frames.Clear();
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
                                AnimationManager.Tracks[categoryIndex].Nodes[itemIndex].Frames.Add(frame, tempFrameDict[frame]);                   
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
                int itemIndex = SelectedRightClickTreeNode.Index;
                int categoryIndex = SelectedRightClickTreeNode.Parent.Index - 1;

                // Remove selected item
                AnimationManager.Tracks[categoryIndex].Nodes.RemoveAt(itemIndex);
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
                int categoryIndex = SelectedRightClickTreeNode.Index - 1;

                // Add selected node
                AnimationManager.Tracks[categoryIndex].Nodes.Add(new Node(itemName, true));

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
                if (AnimationManager.Tracks.Count > 4)
                {
                    MessageBox.Show("Cannot add more than 4 tracks");
                } else
                {
                    InputComboBoxWindow inputComboBoxWindow = new InputComboBoxWindow("Select Type", AnimationSupport.TrackDataCount.Keys.ToArray());

                    if (inputComboBoxWindow.ShowDialog() == DialogResult.OK)
                    {
                        string selectedType = inputComboBoxWindow.SelectedItem;

                        // Add selected track
                        AnimationManager.Tracks.Add(new Track(selectedType));

                        // Create new track
                        TreeNode categoryNode = new TreeNode(selectedType);
                        categoryNode.Tag = "Category";
                        categoryNode.ContextMenuStrip = categoryContextMenuStrip;
                        SelectedRightClickTreeNode.Nodes.Add(categoryNode);

                        // Select the added track
                        mainTreeView.SelectedNode = categoryNode;
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
