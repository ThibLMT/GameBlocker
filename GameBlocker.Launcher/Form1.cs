using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameBlocker.Launcher;

public partial class Form1 : Form
{
    private Microsoft.Web.WebView2.WinForms.WebView2 webView;

    public Form1()
    {
        InitializeComponent();
        InitializeWebView();
    }

    private async void InitializeWebView()
    {
        // 1. Setup the Window
        this.Text = "GameBlocker Dashboard";
        this.Size = new Size(1024, 768);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(17, 24, 39); // Match Tailwind Dark Mode

        // 2. Create the WebView2 Control
        webView = new Microsoft.Web.WebView2.WinForms.WebView2();
        webView.Dock = DockStyle.Fill; // Fill the whole window
        this.Controls.Add(webView);

        // 3. Initialize the Engine
        await webView.EnsureCoreWebView2Async(null);

        // 4. Navigate to your Service
        // Ideally, we check if port 5000 is open. If not, we show an error.
        webView.Source = new Uri("http://localhost:5000");

        // 5. Disable "Browser" features (Right click menu, DevTools, etc)
        webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
    }
}