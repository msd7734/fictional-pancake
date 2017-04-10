using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicKL
{
    public partial class Form1 : Form
    {

        private Logger log;

        public Form1(Logger log)
        {
            this.log = log;
            InitializeComponent();
        }
    }
}
