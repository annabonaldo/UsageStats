﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeRecorderApplicationContext.cs" company="objo.net">
//   The MIT License (MIT)
//   
//   Copyright (c) 2012 Oystein Bjorke
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary>
//   The time recorder application context.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TimeRecorder
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Forms;

    using global::TimeRecorder.Properties;

    /// <summary>
    /// The time recorder application context.
    /// </summary>
    public class TimeRecorderApplicationContext : ApplicationContext
    {
        /// <summary>
        /// The cancellation token source
        /// </summary>
        private readonly CancellationTokenSource source;

        /// <summary>
        /// The tray icon
        /// </summary>
        private readonly NotifyIcon trayIcon;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeRecorderApplicationContext" /> class.
        /// </summary>
        public TimeRecorderApplicationContext()
        {
            this.trayIcon = new NotifyIcon
                                {
                                    Icon = GetIcon("TimeRecorder.TimeRecorder.ico"),
                                    ContextMenu =
                                        new ContextMenu(
                                        new[]
                                            {
                                                new MenuItem("About Time Recorder...", this.About), 
                                                new MenuItem("-"), new MenuItem("Options...", this.Options), 
                                                new MenuItem("Statistics...", this.Statistics), 
                                                new MenuItem("Explore...", this.Explore), new MenuItem("-"), 
                                                new MenuItem("Exit", this.Exit)
                                            }),
                                    Visible = true
                                };

            this.InitializeTimeRecorder();

            this.source = TimeRecorder.RunAsync();
        }

        /// <summary>
        /// Gets the icon.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The icon.
        /// </returns>
        private static Icon GetIcon(string name)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            return stream != null ? new Icon(stream) : null;
        }

        /// <summary>
        /// Opens the options dialog.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        private void Options(object sender, EventArgs e)
        {
            var dialog = new OptionsDialog
                             {
                                 DatabaseRootFolder = Settings.Default.DatabaseRootFolder,
                                 RecordWindowTitles = Settings.Default.RecordWindowTitles
                             };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.DatabaseRootFolder = dialog.DatabaseRootFolder;
                Settings.Default.RecordWindowTitles = dialog.RecordWindowTitles;
                Settings.Default.Save();

                this.InitializeTimeRecorder();
            }
        }

        private void InitializeTimeRecorder()
        {
            TimeRecorder.RecordWindowTitles = Settings.Default.RecordWindowTitles;
        }

        /// <summary>
        /// Opens the statistics application.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        private void Statistics(object sender, EventArgs e)
        {
            var exe = "TimeRecorderStatistics.exe";
            if (File.Exists(exe))
            {
                Process.Start(exe);
            }
            else
            {
                MessageBox.Show(
                    exe + " must be located in the same folder as TimeRecorder.exe",
                    "TimeRecorder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Opens the statistics folder in Windows Explorer.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        private void Explore(object sender, EventArgs e)
        {
            Process.Start("Explorer.exe", TimeRecorder.Folder);
        }

        /// <summary>
        /// Opens the project web page.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        private void About(object sender, EventArgs e)
        {
            Process.Start("http://usagestats.codeplex.com/");
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        private void Exit(object sender, EventArgs e)
        {
            this.trayIcon.Visible = false;
            this.source.Cancel();
            Application.Exit();
        }
    }
}