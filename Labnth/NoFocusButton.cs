using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labnth
{
    public partial class NoFocusButton : Button
    {
        public NoFocusButton() : base()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.Selectable, false);
            this.MinimumSize = new Size(1, 1);
        }

        protected override bool ShowFocusCues
        {
            get
            {
                return false;
            }
        }
    }
}
