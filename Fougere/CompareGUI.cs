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

namespace Fougere
{
    public partial class CompareGUI : Form
    {

        public CompareGUI(string fileName = null)
        {
            InitializeComponent();

            if (fileName != null && fileName != "openFileDialog1")
            {
                openFileDialog1.FileName = fileName;
                filePathTextBox1.Text = fileName;
            }
        }

        public string CompareAndPrintDifferences(AnimationManager manager1, AnimationManager manager2)
        {
            var differences = new StringBuilder();

            // Compare high-level properties
            AppendDifferences(differences, "Format", manager1.Format, manager2.Format);
            AppendDifferences(differences, "Version", manager1.Version, manager2.Version);
            AppendDifferences(differences, "AnimationName", manager1.AnimationName, manager2.AnimationName);
            AppendDifferences(differences, "FrameCount", manager1.FrameCount, manager2.FrameCount);

            // Compare Tracks
            CompareTracks(differences, manager1.Tracks, manager2.Tracks);

            if (differences.Length == 0)
            {
                differences.AppendLine($"No difference");
            }

            return differences.ToString();
        }

        private void AppendDifferences<T>(StringBuilder differences, string propertyName, T value1, T value2)
        {
            if (!EqualityComparer<T>.Default.Equals(value1, value2))
            {
                differences.AppendLine($"{propertyName}: '{value1}' != '{value2}'");
            }
        }

        private void CompareTracks(StringBuilder differences, List<Track> tracks1, List<Track> tracks2)
        {
            var trackDict1 = tracks1.ToDictionary(t => t.Name, t => t);
            var trackDict2 = tracks2.ToDictionary(t => t.Name, t => t);

            var allTrackNames = new HashSet<string>(trackDict1.Keys);
            allTrackNames.UnionWith(trackDict2.Keys);

            foreach (var trackName in allTrackNames)
            {
                if (trackDict1.TryGetValue(trackName, out var track1) && trackDict2.TryGetValue(trackName, out var track2))
                {
                    CompareTrack(differences, track1, track2);
                }
                else
                {
                    differences.AppendLine($"Track '{trackName}' is missing in one of the AnimationManagers.");
                }
            }
        }

        private void CompareTrack(StringBuilder differences, Track track1, Track track2)
        {
            // Compare track properties
            AppendDifferences(differences, "Track Name", track1.Name, track2.Name);
            AppendDifferences(differences, "Track Index", track1.Index, track2.Index);

            // Compare Nodes
            CompareNodes(differences, track1.Nodes, track2.Nodes);
        }

        private void CompareNodes(StringBuilder differences, List<Node> nodes1, List<Node> nodes2)
        {
            var nodeDict1 = nodes1.ToDictionary(n => n.Name, n => n);
            var nodeDict2 = nodes2.ToDictionary(n => n.Name, n => n);

            var allNodeNames = new HashSet<string>(nodeDict1.Keys);
            allNodeNames.UnionWith(nodeDict2.Keys);

            foreach (var nodeName in allNodeNames)
            {
                if (nodeDict1.TryGetValue(nodeName, out var node1) && nodeDict2.TryGetValue(nodeName, out var node2))
                {
                    CompareNode(differences, node1, node2);
                }
                else
                {
                    differences.AppendLine($"Node '{nodeName}' is missing in one of the Tracks.");
                }
            }
        }

        private void CompareNode(StringBuilder differences, Node node1, Node node2)
        {
            // Compare Node properties
            AppendDifferences(differences, "Node Name", node1.Name, node2.Name);
            AppendDifferences(differences, "Node IsMainTrack", node1.IsInMainTrack, node2.IsInMainTrack);

            // Compare Frames
            CompareFrames(differences, node1.Frames, node2.Frames);
        }

        private void CompareFrames(StringBuilder differences, List<Frame> frames1, List<Frame> frames2)
        {
            var frameDict1 = frames1.ToDictionary(f => f.Key, f => f);
            var frameDict2 = frames2.ToDictionary(f => f.Key, f => f);

            var allFrameKeys = new HashSet<int>(frameDict1.Keys);
            allFrameKeys.UnionWith(frameDict2.Keys);

            foreach (var key in allFrameKeys)
            {
                if (frameDict1.TryGetValue(key, out var frame1) && frameDict2.TryGetValue(key, out var frame2))
                {
                    AppendDifferences(differences, $"Frame Key {key} Value", frame1.Value, frame2.Value);
                }
                else
                {
                    differences.AppendLine($"Frame with Key {key} is missing in one of the Nodes.");
                }
            }
        }

        private void OpenButton1_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Level 5 Animation files (*.mtn2;*.imm2;*.mtm2)|*.mtn2;*.imm2;*.mtm2|JSON files (*.json)|*.json";
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePathTextBox1.Text = openFileDialog1.FileName;
            }
        }

        private void OpenButton2_Click(object sender, EventArgs e)
        {
            openFileDialog2.FileName = "";
            openFileDialog2.Filter = "Level 5 Animation files (*.mtn2;*.imm2;*.mtm2)|*.mtn2;*.imm2;*.mtm2|JSON files (*.json)|*.json";
            openFileDialog2.RestoreDirectory = true;

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                filePathTextBox2.Text = openFileDialog2.FileName;
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            AnimationManager animationManager1 = new AnimationManager(new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read));
            AnimationManager animationManager2 = new AnimationManager(new FileStream(openFileDialog2.FileName, FileMode.Open, FileAccess.Read));
            outputTextBox.Text = CompareAndPrintDifferences(animationManager1, animationManager2);
        }
    }
}
