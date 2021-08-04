using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyPLAOptimization
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void testCase1()
        {
            XMIOperator op = new XMIOperator();
            XMIInterface intfce = new XMIInterface();
            XMIComponent com = new XMIComponent();
            // init operator
            op.Arguments.Add(new int());
            op.Name = "operator1";
            op.Id = 0;
            op.OwnerInterface = intfce;
            // init interface
            intfce.Operators.Add(op);
            intfce.Name = "interface1";
            intfce.Id = 1;
            intfce.OwnerComponent = com;
            // init component
            com.Interfaces.Add(intfce);
            com.Id = 50;
            com.Name = "Component2";
        }
    }
}
