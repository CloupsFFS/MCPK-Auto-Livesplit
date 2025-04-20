using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.MinecraftAutoSplitter
{
    public class SettingsControl : UserControl
    {
        private TableLayoutPanel _tableLayout;
        private List<NumericUpDown> _heightInputs;
        private Button _addSplitButton;
        private Button _removeSplitButton;

        public SettingsControl()
        {
            _heightInputs = new List<NumericUpDown>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 2,
                RowCount = 1
            };

            _addSplitButton = new Button
            {
                Text = "Add Split Height",
                AutoSize = true
            };
            _addSplitButton.Click += AddSplitButton_Click;

            _removeSplitButton = new Button
            {
                Text = "Remove Last Split",
                AutoSize = true
            };
            _removeSplitButton.Click += RemoveSplitButton_Click;

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            buttonPanel.Controls.Add(_addSplitButton);
            buttonPanel.Controls.Add(_removeSplitButton);

            Controls.Add(_tableLayout);
            Controls.Add(buttonPanel);

            // Add initial split height input
            AddHeightInput();
        }

        private void AddHeightInput()
        {
            var label = new Label
            {
                Text = $"Split {_heightInputs.Count + 1} Height (Y):",
                AutoSize = true
            };

            var input = new NumericUpDown
            {
                Minimum = -64,
                Maximum = 320,
                DecimalPlaces = 2,
                Value = 0,
                Width = 100
            };

            _heightInputs.Add(input);
            _tableLayout.RowCount = _heightInputs.Count;
            _tableLayout.Controls.Add(label, 0, _heightInputs.Count - 1);
            _tableLayout.Controls.Add(input, 1, _heightInputs.Count - 1);
        }

        private void AddSplitButton_Click(object sender, EventArgs e)
        {
            AddHeightInput();
        }

        private void RemoveSplitButton_Click(object sender, EventArgs e)
        {
            if (_heightInputs.Count > 1)
            {
                var lastIndex = _heightInputs.Count - 1;
                _tableLayout.Controls.RemoveAt(lastIndex * 2 + 1);
                _tableLayout.Controls.RemoveAt(lastIndex * 2);
                _heightInputs.RemoveAt(lastIndex);
                _tableLayout.RowCount--;
            }
        }

        public List<double> GetSplitHeights()
        {
            var heights = new List<double>();
            foreach (var input in _heightInputs)
            {
                heights.Add((double)input.Value);
            }
            return heights;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var settings = document.CreateElement("Settings");
            var heights = document.CreateElement("Heights");

            foreach (var input in _heightInputs)
            {
                var height = document.CreateElement("Height");
                height.InnerText = input.Value.ToString();
                heights.AppendChild(height);
            }

            settings.AppendChild(heights);
            return settings;
        }

        public void SetSettings(XmlNode settings)
        {
            if (settings == null) return;

            var heights = settings.SelectNodes(".//Height");
            if (heights == null) return;

            while (_heightInputs.Count > 1)
            {
                RemoveSplitButton_Click(null, null);
            }

            for (int i = 0; i < heights.Count; i++)
            {
                if (i >= _heightInputs.Count)
                {
                    AddHeightInput();
                }

                if (decimal.TryParse(heights[i].InnerText, out decimal value))
                {
                    _heightInputs[i].Value = value;
                }
            }
        }
    }
} 