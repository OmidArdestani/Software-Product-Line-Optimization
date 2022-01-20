using read_feature_model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
        private bool FeaturModelLoaded = false;
        private bool DiagramLoaded = false;
        NSGAIIOptimizer MyOptimization = null;
        XMLFeatureModel featureModel = new XMLFeatureModel();
        List<FeatureRelationship> featureRelationshipMatrix = new List<FeatureRelationship> { };
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
                    intfce.Operations = new List<PLAOperation> { };
                    // init operator
                    for (int o = 0; o < 10; o++)
                    {
                        PLAOperation op = new PLAOperation();
                        op.Arguments = new List<object> { };
                        op.Arguments.Add(new int());
                        op.Name = "operator" + o;
                        op.Id = o.ToString();
                        op.OwnerInterface = intfce;
                        intfce.Operations.Add(op);
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
            //rtbOutput.AppendText(info + "\n");
            return true;
        }


        private void btnRunAlgorithm_Click(object sender, EventArgs e)
        {
            MyOptimization.MaxEvaluation = (int)nudMaximumEvaluation.Value;
            MyOptimization.StartAsync();
            btnRunAlgorithm.Enabled = false;
            nudMaximumEvaluation.Enabled = false;
        }
        private bool GetFinishedOptimization()
        {
            btnExportPLA.Enabled = true;
            btnRunAlgorithm.Enabled = true;
            nudMaximumEvaluation.Enabled = true;
            // calc functions
            double coupling = MyOptimization.problem.EvalCoupling(MyOptimization.BestPLA);
            double convetionalCohesion = MyOptimization.problem.EvalConventionalCohesion(MyOptimization.BestPLA);
            double plaCohesion = MyOptimization.problem.EvalPLACohesion(MyOptimization.BestPLA);
            double reusability = MyOptimization.problem.EvalReusability(MyOptimization.BestPLA);
            double configurability = MyOptimization.problem.EvalConfigurability(MyOptimization.BestPLA);
            // show in labels
            lblOutputConCohesion.Text = Math.Round(convetionalCohesion, 2).ToString();
            lblOutputPLACOhesion.Text = Math.Round(plaCohesion, 2).ToString();
            lblOutputReusability.Text = Math.Round(reusability, 2).ToString();
            lblOutputConfigurability.Text = Math.Round(configurability, 4).ToString();
            lblOutputCoupling.Text = Math.Round(coupling, 2).ToString();
            // PLA info
            lblOutputComponentCount.Text = MyOptimization.BestPLA.ComponentCount.ToString();
            lblOutputInterfaceCount.Text = MyOptimization.BestPLA.InterfaceCount.ToString();
            lblOutputOperationCount.Text = MyOptimization.BestPLA.OperatorCount.ToString();

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
                    tbExportPLAFileAddr.Text = addressParts[addressParts.Length - 2] + "/" + addressParts[addressParts.Length - 1];
                else
                    tbExportPLAFileAddr.Text = string.Join("/", addressParts);
                PLArchitecture optimizedPLA = MyOptimization.BestPLA;
                exportFIle.ExportFile(dialog.FileName, optimizedPLA.Components);
            }
        }
        private void GenerateFeatureModelAndComponentsRelationshipMatrix()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    tbArchFileAddress.Text = addressParts[addressParts.Length - 2] + "/" + addressParts[addressParts.Length - 1];
                else
                    tbArchFileAddress.Text = string.Join("/", addressParts);
                lblComponentCnt.Text = xmiConv.GetComponentCount().ToString();
                lblInterfaceCnt.Text = xmiConv.GetInterfaceCount().ToString();
                lblOperatorCnt.Text = xmiConv.GetOperatorCount().ToString();
                DiagramLoaded = true;
                if (FeaturModelLoaded && DiagramLoaded)
                {
                    nudMaximumEvaluation.Enabled = true;
                    btnRunAlgorithm.Enabled = true;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelectFeatureModel_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "SXFM Format (*.XML)|*.xml";
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                featureModel.LoadFile(dialog.FileName);
                int childCnt = featureModel.Root.ChildCount();
                bool hasComponentFeatureModel = true;
                if (!hasComponentFeatureModel)
                {
                    MessageBox.Show("No Component Mandatory in feature mode.", "The feature model has not Component Diagram part.\nPlease load another feature model.");
                    lblFeatureModelValid.Text = "Not Valid";
                    lblFeatureModelValid.ForeColor = Color.Red;
                }
                else
                {
                    lblFeatureModelValid.Text = "Valid";
                    lblFeatureModelValid.ForeColor = Color.DarkGreen;
                    FeaturModelLoaded = true;
                    GenerateFeatureModelAndComponentsRelationshipMatrix();
                    if (FeaturModelLoaded && DiagramLoaded)
                    {
                        nudMaximumEvaluation.Enabled = true;
                        btnRunAlgorithm.Enabled = true;
                    }
                    string[] addressParts = dialog.FileName.Split('\\');
                    if (addressParts.Length > 2)
                        tbFMFileAddress.Text = addressParts[addressParts.Length - 2] + "/" + addressParts[addressParts.Length - 1];
                    else
                        tbFMFileAddress.Text = string.Join("/", addressParts);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelectFeatureRelationship_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV (*.CSV)|*.csv";
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                string[] fileLines = File.ReadAllLines(dialog.FileName);
                featureRelationshipMatrix.Clear();
                string[] featureNames = fileLines[0].Split(',');
                for (int i = 1; i < fileLines.Count(); i++)
                {
                    string[] values = fileLines[i].Split(',');
                    string interfaceName = values[0];
                    for (int j = 1; j < values.Count(); j++)
                    {
                        string featureName = featureNames[j];
                        if (values[j] == "1")
                        {
                            var allOperations = new List<PLAOperation> { };
                            GotArchitecture.Components.ForEach(c =>
                                c.Interfaces.Where(inter =>
                                inter.Name == interfaceName).ToList().ForEach(inter =>
                                    allOperations.AddRange(inter.Operations)));
                            foreach (var item in allOperations)
                            {
                                FeatureRelationship rel = new FeatureRelationship();
                                rel.RelatedFeature = featureModel.GetNodeByName(featureModel.Root, featureName);
                                rel.RelatedOperation = item;
                                featureRelationshipMatrix.Add(rel);
                            }
                        }
                    }
                }
                string[] addressParts = dialog.FileName.Split('\\');
                if (addressParts.Length > 2)
                    tbFMRelFileAddress.Text = addressParts[addressParts.Length - 2] + "/" + addressParts[addressParts.Length - 1];
                else
                    tbFMRelFileAddress.Text = string.Join("/", addressParts);
                // config My Optimization
                MyOptimization.Configuration(GotArchitecture, featureModel, featureRelationshipMatrix, (int)nudMaximumEvaluation.Value, GotArchitecture.OperatorCount);
                // calc functions
                double coupling = MyOptimization.problem.EvalCoupling(GotArchitecture);
                double convetionalCohesion = MyOptimization.problem.EvalConventionalCohesion(GotArchitecture);
                double plaCohesion = MyOptimization.problem.EvalPLACohesion(GotArchitecture);
                double reusability = MyOptimization.problem.EvalReusability(GotArchitecture);
                double configurability = MyOptimization.problem.EvalConfigurability(GotArchitecture);
                // show in labels
                lblInputConCohesion.Text = Math.Round(convetionalCohesion,2).ToString();
                lblInputPLACOhesion.Text = Math.Round(plaCohesion, 2).ToString();
                lblInputReusability.Text = Math.Round(reusability, 2).ToString();
                lblInputConfigurability.Text = Math.Round(configurability,4).ToString();
                lblInputCoupling.Text = Math.Round(coupling,2).ToString();
            }
        }
    }
}
