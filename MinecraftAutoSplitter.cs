using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.MinecraftAutoSplitter
{
    public class MinecraftAutoSplitter : IComponent
    {
        public string ComponentName => "Minecraft Parkour Auto Splitter";

        private LiveSplitState _state;
        private Process _gameProcess;
        private bool _isHooked;
        private MinecraftMemory _memory;
        private List<double> _splitHeights;
        private int _currentSplitIndex;
        private bool _hasPassedHeight;
        private SettingsControl _settings;

        public MinecraftAutoSplitter(LiveSplitState state)
        {
            _state = state;
            _isHooked = false;
            _splitHeights = new List<double>();
            _currentSplitIndex = 0;
            _hasPassedHeight = false;
            _settings = new SettingsControl();

            // Subscribe to events
            _state.OnStart += OnStart;
            _state.OnReset += OnReset;
            _state.OnSplit += OnSplit;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (!_isHooked)
            {
                TryHookProcess();
                return;
            }

            if (_gameProcess == null || _gameProcess.HasExited)
            {
                _isHooked = false;
                return;
            }

            CheckGameState();
        }

        private void TryHookProcess()
        {
            Process[] processes = Process.GetProcessesByName("javaw");
            if (processes.Length > 0)
            {
                _gameProcess = processes[0];
                _memory = new MinecraftMemory(_gameProcess);
                _isHooked = true;
            }
        }

        private void CheckGameState()
        {
            try
            {
                if (_currentSplitIndex >= _splitHeights.Count)
                    return;

                double currentY;
                if (_memory.TryGetPlayerY(out currentY))
                {
                    double targetHeight = _splitHeights[_currentSplitIndex];

                    // Check if player has reached or passed the target height
                    if (currentY >= targetHeight && !_hasPassedHeight)
                    {
                        _hasPassedHeight = true;
                        _state.Split();
                    }
                    // Reset the flag if player goes back below the height
                    else if (currentY < targetHeight)
                    {
                        _hasPassedHeight = false;
                    }
                }
            }
            catch (Exception)
            {
                _isHooked = false;
            }
        }

        private void OnStart(object sender, EventArgs e)
        {
            _currentSplitIndex = 0;
            _hasPassedHeight = false;
            _splitHeights = _settings.GetSplitHeights();
        }

        private void OnReset(object sender, EventArgs e)
        {
            _currentSplitIndex = 0;
            _hasPassedHeight = false;
        }

        private void OnSplit(object sender, EventArgs e)
        {
            _currentSplitIndex++;
        }

        public void Dispose()
        {
            // Cleanup
            _state.OnStart -= OnStart;
            _state.OnReset -= OnReset;
            _state.OnSplit -= OnSplit;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return _settings.GetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return _settings;
        }

        public void SetSettings(XmlNode settings)
        {
            _settings.SetSettings(settings);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region region) { }
        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region region) { }
        public float HorizontalWidth => 0;
        public float MinimumHeight => 0;
        public float MinimumWidth => 0;
        public float PaddingBottom => 0;
        public float PaddingLeft => 0;
        public float PaddingRight => 0;
        public float PaddingTop => 0;
        public float VerticalHeight => 0;
    }
} 