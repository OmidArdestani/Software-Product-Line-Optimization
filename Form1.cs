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
            //testCase1();
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
                        op.Id = o.ToString();
                        op.OwnerInterface = intfce;
                        intfce.Operators.Add(op);
                    }
                    intfce.Name = "interface" + i;
                    intfce.Id = i.ToString();
                    intfce.OwnerComponent = com;
                    com.Interfaces.Add(intfce);
                }
                // init component
                com.Id = c.ToString();
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

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Architercture Project (*.XMI)|*.xmi|Architercture Project (*.XML)|*.xml";
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                FileConvertor xmiConv = null;
                // Input file is XMI format
                if (dialog.FilterIndex == 1)
                {
                    xmiConv = new XMIConvertor();
                }
                // Input file is XML format
                else if (dialog.FilterIndex == 2)
                {
                    xmiConv = new XMLConvertor();
                }
                PLArchitecture architecture = xmiConv.ReadFile(dialog.FileName);
                NSGAIIOptimizer myOptimization = new NSGAIIOptimizer();
                myOptimization.AlgorithmOutput += showOutput;
                myOptimization.Configuration(architecture, 55000);
                myOptimization.StartAsync();
            }
        }
    }
}
