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
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            testCase1();
        }

        public void testCase1()
        {
            List<XMIComponent> architecture = new List<XMIComponent> { };
            for (int c = 0; c < 10; c++)
            {
                XMIComponent com = new XMIComponent();
                com.Interfaces = new List<XMIInterface> { };
                com.DependedInterfaces = new List<XMIInterface> { };
                // init interface
                for (int i = 0; i < 5; i++)
                {
                    XMIInterface intfce = new XMIInterface();
                    intfce.Operators = new List<XMIOperator> { };
                    // init operator
                    for (int o = 0; o < 10; o++)
                    {
                        XMIOperator op = new XMIOperator();
                        op.Arguments = new List<object> { };
                        op.Arguments.Add(new int());
                        op.Name = "operator" + o;
                        op.Id = o;
                        op.OwnerInterface = intfce;
                        intfce.Operators.Add(op);
                    }
                    intfce.Name = "interface" + i;
                    intfce.Id = i;
                    intfce.OwnerComponent = com;
                    com.Interfaces.Add(intfce);
                }
                // init component
                com.Id = c;
                com.Name = "Component" + c;
                architecture.Add(com);
            }
            NSGAIIOptimizer myOptimization = new NSGAIIOptimizer();
            myOptimization.AlgorithmOutput += showOutput;
            myOptimization.SetArchitecture(architecture);
            myOptimization.Configuration(100, 55000);
            myOptimization.StartAsync();
        }

        private bool showOutput(string info)
        {
            richTextBox1.AppendText(info + "\n");
            return true;
        }
    }
}
