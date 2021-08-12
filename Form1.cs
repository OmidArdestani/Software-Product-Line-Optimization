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
            List<PLAComponent> components = new List<PLAComponent> { };
            for (int c = 0; c < 10; c++)
            {
                PLAComponent com = new PLAComponent();
                com.Interfaces = new List<PLAInterface> { };
                com.DependedInterfaces = new List<PLAInterface> { };
                // init interface
                for (int i = 0; i < 5; i++)
                {
                    PLAInterface intfce = new PLAInterface();
                    intfce.Operators = new List<PLAOperator> { };
                    // init operator
                    for (int o = 0; o < 10; o++)
                    {
                        PLAOperator op = new PLAOperator();
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
                components.Add(com);
            }
            NSGAIIOptimizer myOptimization = new NSGAIIOptimizer();
            myOptimization.AlgorithmOutput += showOutput;
            myOptimization.Configuration(new PLArchitecture(components), 55000);
            myOptimization.StartAsync();
        }

        private bool showOutput(string info)
        {
            richTextBox1.AppendText(info + "\n");
            return true;
        }
    }
}
