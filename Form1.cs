using read_feature_model;
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
        private IFileConvertor xmiConv = null;
        private PLArchitecture GotArchitecture = null;
        NSGAIIOptimizer MyOptimization = null;
        XMLFeatureModel featureModel = new XMLFeatureModel();
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            MyOptimization = new NSGAIIOptimizer();
            MyOptimization.AlgorithmOutput += showOutput;
            MyOptimization.OptimizationFinished += GetFinishedOptimization;
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
                    intfce.Operation = new List<PLAOperator> { };
                    // init operator
                    for (int o = 0; o < 10; o++)
                    {
                        PLAOperator op = new PLAOperator();
                        op.Arguments = new List<object> { };
                        op.Arguments.Add(new int());
                        op.Name = "operator" + o;
                        op.Id = o.ToString();
                        op.OwnerInterface = intfce;
                        intfce.Operation.Add(op);
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
            MyOptimization.AlgorithmOutput += showOutput;
            MyOptimization.OptimizationFinished += GetFinishedOptimization;

            //myOptimization.Configuration(new PLArchitecture(components), 55000);
            MyOptimization.StartAsync();
        }

        private bool showOutput(string info)
        {
            rtbOutput.AppendText(info + "\n");
            return true;
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Architercture Project (*.XMI)|*.xmi|Architercture Project (*.XML)|*.xml";
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
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
                GotArchitecture = xmiConv.ReadFile(dialog.FileName);
                string[] addressParts = dialog.FileName.Split('\\');
                if (addressParts.Length > 2)
                    tbInputFileAddress.Text = addressParts[addressParts.Length - 2] + "/" + addressParts[addressParts.Length - 1];
                else
                    tbInputFileAddress.Text = string.Join("/", addressParts);
                lblComponentCnt.Text = xmiConv.GetComponentCount().ToString();
                lblInterfaceCnt.Text = xmiConv.GetInterfaceCount().ToString();
                lblOperatorCnt.Text = xmiConv.GetOperatorCount().ToString();
                nudMaximumEvaluation.Enabled = true;
                btnRunAlgorithm.Enabled = true;
            }
        }

        private void btnRunAlgorithm_Click(object sender, EventArgs e)
        {
            MyOptimization.Configuration(GotArchitecture, (int)nudMaximumEvaluation.Value, xmiConv.GetOperatorCount());
            MyOptimization.StartAsync();
            btnRunAlgorithm.Enabled = false;
            nudMaximumEvaluation.Enabled = false;
        }
        private bool GetFinishedOptimization()
        {
            btnExportOutput.Enabled = true;
            btnRunAlgorithm.Enabled = true;
            nudMaximumEvaluation.Enabled = true;
            return true;
        }
        private void btnExportOutput_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Architercture Project (*.XMI)|*.xmi|Architercture Project (*.XML)|*.xml";
            dialog.ShowDialog();
            IFileConvertor exportFIle = null;
            if (dialog.FileName != "")
            {
                // Input file is XMI format
                if (dialog.FilterIndex == 1)
                {
                    exportFIle = new XMIConvertor();
                }
                // Input file is XML format
                else if (dialog.FilterIndex == 2)
                {
                    exportFIle = new XMLConvertor();
                }
                string[] addressParts = dialog.FileName.Split('\\');
                if (addressParts.Length > 2)
                    tbExportFileAddress.Text = addressParts[addressParts.Length - 2] + "/" + addressParts[addressParts.Length - 1];
                else
                    tbExportFileAddress.Text = string.Join("/", addressParts);
                PLArchitecture optimizedPLA = MyOptimization.BestPLA;
                exportFIle.ExportFile(dialog.FileName, optimizedPLA.Components);
            }
        }

        private void BtnSelectFeatureModel_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "SXFM Format (*.XML)|*.xml";
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                featureModel.LoadFile(dialog.FileName);
            }
        }
    }
}
