using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManager
{
    // Realtime CPU measuring tool. Use it to visually find bottle neck, and know how much
    // you can put more CPU jobs. TimeRuler provide the following features:
    //  * Up to 8 bars (Configurable)
    //  * Change colors for each markers
    //  * Marker logging.
    //  * It supports up to 32 (Configurable) nested BeginMark method calls.
    public class Profiler : DebugScreen
    {
        // Max bar count.
        private const int MaxBars = 8;

        // Maximum sample number for each bar.
        private const int MaxSamples = 256;

        // Maximum nest calls for each bar.
        private const int MaxNestCall = 32;

        // Maximum display frames.
        private const int MaxSampleFrames = 4;

        // Duration (in frame count) for take snap shot of log.
        private const int LogSnapDuration = 120;

        // Height (in pixels) of bar.
        private const int BarHeight = 8;

        // Padding (in pixels) of bar.
        private const int BarPadding = 2;

        // Delay frame count for auto display frame adjustment.
        private const int AutoAdjustDelay = 30;

        private Rectangle rc;
        private Rectangle rc2;

        // Marker structure.
        private struct Marker
        {
            public int MarkerId;
            public float BeginTime;
            public float EndTime;
            public Color Color;
        }

        // Collection of markers.
        private class MarkerCollection
        {
            // Marker collection.
            public Marker[] Markers = new Marker[MaxSamples];
            public int MarkCount;

            // Marker nest information.
            public int[] MarkerNests = new int[MaxNestCall];
            public int NestCount;
        }

        // Frame logging information.
        private class FrameLog
        {
            public MarkerCollection[] Bars;

            public FrameLog()
            {
                // Initialize markers.
                Bars = new MarkerCollection[MaxBars];
                for (int i = 0; i < MaxBars; ++i)
                    Bars[i] = new MarkerCollection();
            }
        }

        // Marker information.
        private class MarkerInfo
        {
            // Name of marker.
            public string Name;

            // Marker log.
            public MarkerLog[] Logs = new MarkerLog[MaxBars];

            public MarkerInfo(string name)
            {
                Name = name;
            }
        }

        // Marker log information.
        private struct MarkerLog
        {
            public float SnapMin;
            public float SnapMax;
            public float SnapAvg;

            public float Min;
            public float Max;
            public float Avg;

            public int Samples;

            public Color Color;

            public bool Initialized;
        }

        // Logs for each frames.
        private FrameLog[] logs;

        // Previous frame log.
        private FrameLog prevLog;

        // Current log.
        private FrameLog curLog;

        // Current frame count.
        private int frameCount;

        // Stopwatch for measure the time.
        private Stopwatch stopwatch;

        // Marker information array.
        private List<MarkerInfo> markers;

        // Dictionary that maps from marker name to marker id.
        private Dictionary<string, int> markerNameToIdMap;

        // Display frame adjust counter.
        private int frameAdjust;

        // Current display frame count.
        private int sampleFrames;

        // Marker log string.
        private StringBuilder logString;

        // The position of the log.
        private Vector2 logPos;

        // You want to call StartFrame at beginning of Game.Update method.
        // But Game.Update gets calls multiple time when game runs slow in fixed time step mode.
        // In this case, we should ignore StartFrame call.
        // To do this, we just keep tracking of number of StartFrame calls until Draw gets called.
        private int updateCount;


        // Gets/Set log display or no.
        public bool ShowLog { get; set; }

        // Gets/Sets target sample frames.
        public int TargetSampleFrames { get; set; }

        // Gets/Sets TimeRuler rendering position.
        Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        // Gets/Sets timer ruler width.
        public int Width { get; set; }


        // Initializes the Profiler.
        public override void Initialize()
        {
            base.Initialize();

            stopwatch = new Stopwatch();
            markers = new List<MarkerInfo>();
            markerNameToIdMap = new Dictionary<string, int>();
            logString = new StringBuilder(512);

            Width = (int)(ScreenManager.Viewport.Width * 0.9f);
            position = new Vector2(ScreenManager.Viewport.Width * 0.05f, ScreenManager.Viewport.Height - 4 * BarHeight);
            rc = Rectangle.Empty;
            rc2 = Rectangle.Empty;
            logPos = Vector2.Zero;
            sampleFrames = TargetSampleFrames = 1;

            logs = new FrameLog[2];
            for (int i = 0; i < logs.Length; ++i)
                logs[i] = new FrameLog();

            // Add "profiler" command if DebugCommandHost is registered.
            IConsoleHost host = ScreenManager.Game.Services.GetService<IConsoleHost>();

            if (host != null)
                host.RegisterCommand("profiler", "Toggle profiler on/off", CommandExecute);
        }


        // The "profiler" command execution.
        protected override void CommandExecute(IConsoleHost host, string command, List<string> arguments)
        {
            bool previousVisible = IsActive;

            if (arguments.Count == 0)
            {
                IsActive = !IsActive;
                ShowLog = !ShowLog;
            }

            char[] subArgSeparator = new[] { ':' };
            for (int i = 0; i < arguments.Count; i++)
            {
                string arg = arguments[i].ToLower();
                string[] subargs = arg.Split(subArgSeparator);

                switch (subargs[0])
                {
                    case "reset":
                        ResetLog();
                        break;
                    case "frame":
                        int a = Int32.Parse(subargs[1]);
                        a = Math.Max(a, 1);
                        a = Math.Min(a, MaxSampleFrames);
                        TargetSampleFrames = a;
                        break;
                    case "help":
                        host.Echo("reset         Reset marker log.");
                        host.Echo("frame:n       Change target sample frame (n: 1-4)");
                        break;
                }
            }

            // Reset update count when Visible state changed.
            if (IsActive != previousVisible)
                Interlocked.Exchange(ref updateCount, 0);
        }


        // Start new frame.
        public void StartFrame()
        {
            lock (this)
            {
                // We skip reset frame when this method gets called multiple times.
                int count = Interlocked.Increment(ref updateCount);
                if (IsActive && (1 < count && count < MaxSampleFrames))
                    return;

                // Update current frame log.
                prevLog = logs[frameCount++ & 0x1];
                curLog = logs[frameCount & 0x1];

                float endFrameTime = (float)stopwatch.Elapsed.TotalMilliseconds;

                // Update marker and create a log.
                for (int barIdx = 0; barIdx < prevLog.Bars.Length; ++barIdx)
                {
                    MarkerCollection prevBar = prevLog.Bars[barIdx];
                    MarkerCollection nextBar = curLog.Bars[barIdx];

                    // Re-open marker that didn't get called EndMark in previous frame.
                    for (int nest = 0; nest < prevBar.NestCount; ++nest)
                    {
                        int markerIdx = prevBar.MarkerNests[nest];

                        prevBar.Markers[markerIdx].EndTime = endFrameTime;

                        nextBar.MarkerNests[nest] = nest;
                        nextBar.Markers[nest].MarkerId =
                            prevBar.Markers[markerIdx].MarkerId;
                        nextBar.Markers[nest].BeginTime = 0;
                        nextBar.Markers[nest].EndTime = -1;
                        nextBar.Markers[nest].Color = prevBar.Markers[markerIdx].Color;
                    }

                    // Update marker log.
                    for (int markerIdx = 0; markerIdx < prevBar.MarkCount; ++markerIdx)
                    {
                        float duration = prevBar.Markers[markerIdx].EndTime - 
                            prevBar.Markers[markerIdx].BeginTime;

                        int markerId = prevBar.Markers[markerIdx].MarkerId;
                        MarkerInfo m = markers[markerId];

                        m.Logs[barIdx].Color = prevBar.Markers[markerIdx].Color;

                        if (!m.Logs[barIdx].Initialized)
                        {
                            // First frame process.
                            m.Logs[barIdx].Min = duration;
                            m.Logs[barIdx].Max = duration;
                            m.Logs[barIdx].Avg = duration;

                            m.Logs[barIdx].Initialized = true;
                        }
                        else
                        {
                            // Process after first frame.
                            m.Logs[barIdx].Min = Math.Min(m.Logs[barIdx].Min, duration);
                            m.Logs[barIdx].Max = Math.Min(m.Logs[barIdx].Max, duration);
                            m.Logs[barIdx].Avg += duration;
                            m.Logs[barIdx].Avg *= 0.5f;

                            if (m.Logs[barIdx].Samples++ >= LogSnapDuration)
                            {
                                m.Logs[barIdx].SnapMin = m.Logs[barIdx].Min;
                                m.Logs[barIdx].SnapMax = m.Logs[barIdx].Max;
                                m.Logs[barIdx].SnapAvg = m.Logs[barIdx].Avg;
                                m.Logs[barIdx].Samples = 0;
                            }
                        }
                    }

                    nextBar.MarkCount = prevBar.NestCount;
                    nextBar.NestCount = prevBar.NestCount;
                }

                // Start measuring.
                stopwatch.Reset();
                stopwatch.Start();
            }
        }


        // Start measure time.
        public void BeginMark(string markerName, Color color)
        {
            if (IsActive)
                BeginMark(0, markerName, color);
        }


        // Start measure time.
        public void BeginMark(int barIndex, string markerName, Color color)
        {
            lock (this)
            {
                if (barIndex < 0 || barIndex >= MaxBars)
                    throw new ArgumentOutOfRangeException("barIndex");

                MarkerCollection bar = curLog.Bars[barIndex];

                if (bar.MarkCount >= MaxSamples)
                {
                    throw new OverflowException("Exceeded sample count.\n" +
                        "Either set larger number to TimeRuler.MaxSmpale or lower sample count.");
                }

                if (bar.NestCount >= MaxNestCall)
                {
                    throw new OverflowException("Exceeded nest count.\n" + 
                        "Either set larget number to TimeRuler.MaxNestCall or lower nest calls.");
                }

                // Gets registered marker.
                int markerId;
                if (!markerNameToIdMap.TryGetValue(markerName, out markerId))
                {
                    // Register this if this marker is not registered.
                    markerId = markers.Count;
                    markerNameToIdMap.Add(markerName, markerId);
                    markers.Add(new MarkerInfo(markerName));
                }

                // Start measuring.
                bar.MarkerNests[bar.NestCount++] = bar.MarkCount;

                // Fill marker parameters.
                bar.Markers[bar.MarkCount].MarkerId = markerId;
                bar.Markers[bar.MarkCount].Color = color;
                bar.Markers[bar.MarkCount].BeginTime = (float)stopwatch.Elapsed.TotalMilliseconds;

                bar.Markers[bar.MarkCount].EndTime = -1;

                bar.MarkCount++;
            }
        }


        // End measuring.
        public void EndMark(string markerName)
        {
            if (IsActive)
                EndMark(0, markerName);
        }


        // End measuring.
        public void EndMark(int barIndex, string markerName)
        {
            lock (this)
            {
                if (barIndex < 0 || barIndex >= MaxBars)
                    throw new ArgumentOutOfRangeException("barIndex");

                MarkerCollection bar = curLog.Bars[barIndex];

                if (bar.NestCount <= 0)
                    throw new InvalidOperationException("Call BeingMark method before call EndMark method.");

                int markerId;
                if (!markerNameToIdMap.TryGetValue(markerName, out markerId))
                {
                    throw new InvalidOperationException(string.Format("Maker '{0}' is not registered." +
                        "Make sure you specifed same name as you used for BeginMark method.", markerName));
                }

                int markerIdx = bar.MarkerNests[--bar.NestCount];
                if (bar.Markers[markerIdx].MarkerId != markerId)
                {
                    throw new InvalidOperationException("Incorrect call order of BeginMark/EndMark method." +
                        "You call it like BeginMark(A), BeginMark(B), EndMark(B), EndMark(A)" +
                        " But you can't call it like BeginMark(A), BeginMark(B), EndMark(A), EndMark(B).");
                }

                bar.Markers[markerIdx].EndTime = (float)stopwatch.Elapsed.TotalMilliseconds;
            }
        }


        // Get average time of given bar index and marker name.
        public float GetAverageTime(int barIndex, string markerName)
        {
            if (barIndex < 0 || barIndex >= MaxBars)
                throw new ArgumentOutOfRangeException("barIndex");

            float result = 0;
            int markerId;

            if (markerNameToIdMap.TryGetValue(markerName, out markerId))
                result = markers[markerId].Logs[barIndex].Avg;

            return result;
        }


        // Reset marker log.
        public void ResetLog()
        {
            lock (this)
            {
                for (int m = 0; m < markers.Count; m++)
                {
                    for (int i = 0; i < markers[m].Logs.Length; ++i)
                    {
                        markers[m].Logs[i].Initialized = false;
                        markers[m].Logs[i].SnapMin = 0;
                        markers[m].Logs[i].SnapMax = 0;
                        markers[m].Logs[i].SnapAvg = 0;

                        markers[m].Logs[i].Min = 0;
                        markers[m].Logs[i].Max = 0;
                        markers[m].Logs[i].Avg = 0;

                        markers[m].Logs[i].Samples = 0;
                    }
                }
            }
        }


        public override void Draw(GameTime gameTime)
        {
            // Reset update count.
            Interlocked.Exchange(ref updateCount, 0);

            // Adjust size and position based of number of bars we should draw.
            int height = 0;
            float maxTime = 0;
            for (int i = 0; i < prevLog.Bars.Length; i++)
            {
                if (prevLog.Bars[i].MarkCount > 0)
                {
                    height += BarHeight + BarPadding * 2;
                    maxTime = Math.Max(maxTime, prevLog.Bars[i].Markers[prevLog.Bars[i].MarkCount - 1].EndTime);
                }
            }

            // Auto display frame adjustment.
            // For example, if the entire process of frame doesn't finish in less than 16.6ms
            // thin it will adjust display frame duration as 33.3ms.
            const float frameSpan = 1.0f / 60.0f * 1000f;
            float sampleSpan = sampleFrames * frameSpan;

            if (maxTime > sampleSpan)
                frameAdjust = Math.Max(0, frameAdjust) + 1;
            else
                frameAdjust = Math.Min(0, frameAdjust) - 1;

            if (Math.Abs(frameAdjust) > AutoAdjustDelay)
            {
                sampleFrames = Math.Min(MaxSampleFrames, sampleFrames);
                sampleFrames = Math.Max(TargetSampleFrames, (int)(maxTime / frameSpan) + 1);
                frameAdjust = 0;
            }

            // Compute factor that converts from ms to pixel.
            float msToPs = Width / sampleSpan;

            // Draw start position.
            int startY = (int)position.Y - (height - BarHeight);

            // Current y position.
            int y = startY;

            SpriteBatch.Begin();

            // Draw transparency background.
            rc.X = (int)position.X;
            rc.Y = startY;
            rc.Width = Width;
            rc.Height = height;
            SpriteBatch.Draw(Texture, rc, AreaColor);

            // Draw markers for each bars.
            rc.Height = BarHeight;
            for (int i = 0; i < prevLog.Bars.Length; i++)
            {
                rc.Y = y + BarPadding;
                if (prevLog.Bars[i].MarkCount > 0)
                {
                    for (int j = 0; j < prevLog.Bars[i].MarkCount; ++j)
                    {
                        float bt = prevLog.Bars[i].Markers[j].BeginTime;
                        float et = prevLog.Bars[i].Markers[j].EndTime;
                        int sx = (int)(position.X + bt * msToPs);
                        int ex = (int)(position.X + et * msToPs);
                        rc.X = sx;
                        rc.Width = Math.Max(ex - sx, 1);

                        SpriteBatch.Draw(Texture, rc, prevLog.Bars[i].Markers[j].Color);
                    }
                }

                y += BarHeight + BarPadding;
            }

            // Draw grid lines. Each grid represents ms.
            rc.X = (int)position.X;
            rc.Y = startY;
            rc.Width = 1;
            rc.Height = height;
            for (float t = 1.0f; t < sampleSpan; t += 1.0f)
            {
                rc.X = (int)(position.X + t * msToPs);
                SpriteBatch.Draw(Texture, rc, AreaColor);
            }

            // Draw frame grid.
            for (int i = 0; i <= sampleFrames; ++i)
            {
                rc.X = (int)(position.X + frameSpan * i * msToPs);
                SpriteBatch.Draw(Texture, rc, Color.White);
            }

            // Draw log.
            if (ShowLog)
            {
                // Generate log string.
                y = startY - Font.LineSpacing;
                logString.Length = 0;
                for (int m = 0; m < markers.Count; m++)
                {
                    for (int i = 0; i < MaxBars; ++i)
                    {
                        if (markers[m].Logs[i].Initialized)
                        {
                            if (logString.Length > 0)
                                logString.Append("\n");

                            logString.Append(" Bar ");
                            logString.AppendNumber(i);
                            logString.Append(" ");
                            logString.Append(markers[m].Name);

                            logString.Append(" Avg.:");
                            logString.AppendNumber(markers[m].Logs[i].SnapAvg);
                            logString.Append("ms ");

                            y -= Font.LineSpacing;
                        }
                    }
                }

                // Compute background size and draw it.
                Vector2 size = Font.MeasureString(logString);
                rc.X = (int)position.X;
                rc.Y = y;
                rc.Width = (int)size.X + 12;
                rc.Height = (int)size.Y;
                SpriteBatch.Draw(Texture, rc, AreaColor);

                // Draw log string.
                logPos.X = position.X + 12;
                logPos.Y = y;
                SpriteBatch.DrawString(Font, logString, logPos, Color.White);

                // Draw log color boxes.
                y += (int)(Font.LineSpacing * 0.3f);
                rc.X = (int)position.X + 4;
                rc.Y = y;
                rc.Width = 10;
                rc.Height = 10;

                rc2.X = (int)position.X + 5;
                rc2.Y = y + 1;
                rc2.Width = 8;
                rc2.Height = 8;

                for (int m = 0; m < markers.Count; m++)
                {
                    for (int i = 0; i < MaxBars; ++i)
                    {
                        if (markers[m].Logs[i].Initialized)
                        {
                            rc.Y = y;
                            rc2.Y = y + 1;
                            SpriteBatch.Draw(Texture, rc, Color.White);
                            SpriteBatch.Draw(Texture, rc2, markers[m].Logs[i].Color);

                            y += Font.LineSpacing;
                        }
                    }
                }
            }

            SpriteBatch.End();
        }
    }
}