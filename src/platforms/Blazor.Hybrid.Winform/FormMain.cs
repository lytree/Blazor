using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blazor.Hybrid.Winform
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            BlazorWebView blazorWebView1 = new BlazorWebView();
            blazorWebView1.Dock = DockStyle.Fill;
            this.Controls.Add(blazorWebView1);

            blazorWebView1.HostPage = "wwwroot\\index.html";
            blazorWebView1.Services = Program.ServiceCollection.BuildServiceProvider();
            blazorWebView1.RootComponents.Add<Routes>("#app");
        }

    }
}
