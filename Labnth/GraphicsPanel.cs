using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labnth
{
    public partial class GraphicsPanel : Panel
    {
        public GraphicsPanel()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
