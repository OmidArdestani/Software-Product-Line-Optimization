using read_feature_model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        private string logFileName = "OutputLog.csv";
        NSGAIIOptimizer MyOptimization = null;
        XMLFeatureModel featureModel = new XMLFeatureModel();
        List<FeatureRelationship> featureRelationshipMatrix = new List<FeatureRelationship> { };
        struct PLAEvaluationValue
        {
            //Fitness funtions
            public double PLACohesion;
            public double ConventionalCohesion;
            public double Coupling;
            public double Commonality;
            public double Granularity;
            // Metrics
            public double Completeness;
            public double Reusability;
            public double Configurability;
            // info
            public int NumberOfOptionalInterfaces;
            public int NumberOfMandatoryInterfaces;
            public int NumberOfOptionalOperations;
            public int NumberOfMandatoryOperations;
        }
        private PLAEvaluationValue inputEvaluationValue;
        private PLAEvaluationValue outputEvaluationValue;
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            MyOptimization = new NSGAIIOptimizer();
            MyOptimization.AlgorithmOutput += showOutput;
            MyOptimization.OptimizationFinished += GetFinishedOptimization;
            MyOptimization.ProccessProgressBar = progressBar1;
            CheckLogFile();
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
            outputEvaluationValue.Coupling = MyOptimization.problem.EvalCoupling(MyOptimization.BestPLA);
            outputEvaluationValue.Commonality = MyOptimization.problem.EvalCommonality(MyOptimization.BestPLA);
            outputEvaluationValue.ConventionalCohesion = MyOptimization.problem.EvalConventionalCohesion(MyOptimization.BestPLA);
            outputEvaluationValue.PLACohesion = MyOptimization.problem.EvalPLACohesion(MyOptimization.BestPLA);
            outputEvaluationValue.Reusability = MyOptimization.problem.EvalReusability(MyOptimization.BestPLA);
            outputEvaluationValue.Configurability = MyOptimization.problem.EvalConfigurability(MyOptimization.BestPLA);
            outputEvaluationValue.Completeness = MyOptimization.problem.EvalCompleteness(MyOptimization.BestPLA);
            outputEvaluationValue.Granularity = MyOptimization.problem.EvalGranularityObjective(MyOptimization.BestPLA);
            // show in labels
            lblOutputConCohesion.Text = Math.Round(outputEvaluationValue.ConventionalCohesion * 100, 2).ToString() + "%";
            lblOutputPLACOhesion.Text = Math.Round(outputEvaluationValue.PLACohesion * 100, 2).ToString() + "%";
            lblOutputReusability.Text = Math.Round(outputEvaluationValue.Reusability * 100, 2).ToString() + "%";
            lblOutputConfigurability.Text = Math.Round(outputEvaluationValue.Configurability * 100, 3).ToString() + "%";
            lblOutputCoupling.Text = Math.Round(outputEvaluationValue.Coupling * 100, 2).ToString() + "%";
            lblOutputCommonality.Text = Math.Round( outputEvaluationValue.Commonality * 100, 1).ToString() + "%";
            lblOutputCompleteness.Text = Math.Round(outputEvaluationValue.Completeness * 100, 1) + "%";
            lblOutputGranularity.Text = Math.Round(outputEvaluationValue.Granularity, 2).ToString();
            // PLA info
            lblOutputComponentCount.Text = MyOptimization.BestPLA.ComponentCount.ToString();
            lblOutputInterfaceCount.Text = MyOptimization.BestPLA.InterfaceCount.ToString();
            lblOutputOperationCount.Text = MyOptimization.BestPLA.OperatorCount.ToString();
            // Check mandatory and optional count for interfaces and operations
            outputEvaluationValue.NumberOfOptionalOperations = MyOptimization.BestPLA.Components.Select(
                c => c.Interfaces.Select(
                    i => i.Operations.Where(
                        o => (o.Propertie("isOptional") != null ?
                (bool)o.Propertie("isOptional") : false) == true).Count()).Sum()).Sum();

            outputEvaluationValue.NumberOfOptionalInterfaces = MyOptimization.BestPLA.Components.Select(
                c => c.Interfaces.Where(i => Convert.ToBoolean(i.Propertie("isOptional"))).Count()).Sum();

            outputEvaluationValue.NumberOfMandatoryOperations = MyOptimization.BestPLA.OperatorCount - outputEvaluationValue.NumberOfOptionalOperations;
            outputEvaluationValue.NumberOfMandatoryInterfaces = MyOptimization.BestPLA.InterfaceCount - outputEvaluationValue.NumberOfOptionalInterfaces;

            lblOutputOperationMandOptionalCount.Text = outputEvaluationValue.NumberOfMandatoryOperations + "/" + outputEvaluationValue.NumberOfOptionalOperations;
            lblOutputInterfaceMandOptionalCount.Text = outputEvaluationValue.NumberOfMandatoryInterfaces + "/" + outputEvaluationValue.NumberOfOptionalInterfaces;

            groupBox3.Enabled = true;
            LogResultInFile();
            return true;
        }
        private void btnExportOutput_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Architercture Project (*.XMI)|*.xmi|Architercture Project (*.XML)|*.xml",
                FilterIndex = 2
            };
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Architercture Project (*.XMI)|*.xmi|Architercture Project (*.XML)|*.xml";
            dialog.FilterIndex = 2;
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
                FeaturModelLoaded = true;
                if (FeaturModelLoaded && DiagramLoaded)
                {
                    nudMaximumEvaluation.Enabled = true;
                    btnRunAlgorithm.Enabled = true;
                }
                // Show address in text
                string[] addressParts = dialog.FileName.Split('\\');
                if (addressParts.Length > 2)
                    tbFMFileAddress.Text = addressParts[addressParts.Length - 2] + "/" + addressParts[addressParts.Length - 1];
                else
                    tbFMFileAddress.Text = string.Join("/", addressParts);
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
                // Add tag for PLA elements consider to received matrix
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
                                if (rel.RelatedFeature is SolitaireFeature)
                                {
                                    item.SetPropertie("isOptional", ((SolitaireFeature)rel.RelatedFeature).IsOptional);
                                    if (item.OwnerInterface != null)
                                    {
                                        item.OwnerInterface.SetPropertie("isOptional", ((SolitaireFeature)rel.RelatedFeature).IsOptional);
                                    }
                                }
                                featureRelationshipMatrix.Add(rel);
                            }
                        }
                    }
                }
                // Check mandatory and optional count for interfaces and operations
                inputEvaluationValue.NumberOfOptionalOperations = GotArchitecture.Components.Select(
                    c => c.Interfaces.Select(
                        i => i.Operations.Where(
                            o => (o.Propertie("isOptional") != null ?
                    (bool)o.Propertie("isOptional") : false) == true).Count()).Sum()).Sum();
                inputEvaluationValue.NumberOfOptionalInterfaces = GotArchitecture.Components.Select(
                    c => c.Interfaces.Where(
                        i => (i.Propertie("isOptional") != null ? (bool)i.Propertie("isOptional") : false) == true).Count()).Sum();
                inputEvaluationValue.NumberOfMandatoryOperations = GotArchitecture.OperatorCount - inputEvaluationValue.NumberOfOptionalOperations;
                inputEvaluationValue.NumberOfMandatoryInterfaces = GotArchitecture.InterfaceCount - inputEvaluationValue.NumberOfOptionalInterfaces;
                lblOperationMandOptionalCount.Text = inputEvaluationValue.NumberOfMandatoryOperations + "/" + inputEvaluationValue.NumberOfOptionalOperations;
                lblInterfaceMandOptionalCount.Text = inputEvaluationValue.NumberOfMandatoryInterfaces + "/" + inputEvaluationValue.NumberOfOptionalInterfaces;
                // Show file address in text
                string[] addressParts = dialog.FileName.Split('\\');
                if (addressParts.Length > 2)
                    tbFMRelFileAddress.Text = addressParts[addressParts.Length - 2] + "/" + addressParts[addressParts.Length - 1];
                else
                    tbFMRelFileAddress.Text = string.Join("/", addressParts);
                // config My Optimization
                MyOptimization.Configuration(GotArchitecture, featureModel, featureRelationshipMatrix, (int)nudMaximumEvaluation.Value, GotArchitecture.OperatorCount);
                // calc functions
                inputEvaluationValue.Coupling = MyOptimization.problem.EvalCoupling(GotArchitecture);
                inputEvaluationValue.Commonality = MyOptimization.problem.EvalCommonality(GotArchitecture);
                inputEvaluationValue.ConventionalCohesion = MyOptimization.problem.EvalConventionalCohesion(GotArchitecture);
                inputEvaluationValue.PLACohesion = MyOptimization.problem.EvalPLACohesion(GotArchitecture);
                inputEvaluationValue.Reusability = MyOptimization.problem.EvalReusability(GotArchitecture);
                inputEvaluationValue.Configurability = MyOptimization.problem.EvalConfigurability(GotArchitecture);
                inputEvaluationValue.Granularity = MyOptimization.problem.EvalGranularityObjective(GotArchitecture);
                // info

                // show in labels
                lblInputConCohesion.Text = Math.Round(inputEvaluationValue.ConventionalCohesion * 100, 2).ToString() + "%";
                lblInputPLACOhesion.Text = Math.Round(inputEvaluationValue.PLACohesion * 100, 2).ToString() + "%";
                lblInputReusability.Text = Math.Round(inputEvaluationValue.Reusability * 100, 2).ToString() + "%";
                lblInputConfigurability.Text = Math.Round(inputEvaluationValue.Configurability * 100, 3).ToString() + "%";
                lblInputCoupling.Text = Math.Round(inputEvaluationValue.Coupling * 100, 2).ToString() + "%";
                lblInputCommonality.Text = Math.Round(inputEvaluationValue.Commonality * 100, 1).ToString() + "%";
                lblInputGranularity.Text = Math.Round(inputEvaluationValue.Granularity, 2).ToString();
            }
        }

        private void BtnExportVarFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "VAR.txt";
            dialog.Filter = "TEXT (*.txt)|*.txt";
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                string[] addressParts = dialog.FileName.Split('\\');
                tbExportVarFileAddr.Text = string.Join("/", addressParts);
                MyOptimization.ExportVarData(dialog.FileName);
            }
        }

        private void BtnExportFuncFIle_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "FUNC.txt";
            dialog.Filter = "TEXT (*.txt)|*.txt";
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                string[] addressParts = dialog.FileName.Split('\\');
                tbExportFuncFileAddr.Text = string.Join("/", addressParts);
                MyOptimization.ExportFuncData(dialog.FileName);
            }
        }

        private void BtnOpenFuncFile_Click(object sender, EventArgs e)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + tbExportFuncFileAddr.Text);

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            Process proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
        }
        private void CheckLogFile()
        {
            if (!File.Exists(logFileName))
            {
                List<string> headers = new List<string> {"DateTime", "Input PLA","Input Feature Model","Input Feature Relationship",
                    "Input Conventional Cohesion","Input PLA-Cohesion","Input Coupling","Input Granularity","Input Commonality",
                    "Input Reusability","Input Configurability","Input Completeness","Input Number Of Optional Interfaces",
                    "Input Number Of Mandatory Interfaces","Input Number Of Optional Operations","Input Number Of Mandatory Operations",
                    "Output Conventional Cohesion","Output PLA-Cohesion","Output Coupling","Output Granularity","Output Commonality","Output Reusability",
                    "Output Configurability","Output Completeness","Output Number Of Optional Interfaces","Output Number Of Mandatory Interfaces",
                    "Output Number Of Optional Operations","Output Number Of Mandatory Operations"
                };
                File.WriteAllText(logFileName, string.Join(",", headers.ToArray()) + "\n");
            }
        }
        private void LogResultInFile()
        {
            List<string> parameters = new List<string> { };
            parameters.Add(DateTime.Now.ToString("yy-MM-dd HH:mm:ss"));
            parameters.Add(tbArchFileAddress.Text);
            parameters.Add(tbFMFileAddress.Text);
            parameters.Add(tbFMRelFileAddress.Text);
            //
            parameters.Add(inputEvaluationValue.ConventionalCohesion.ToString());
            parameters.Add(inputEvaluationValue.PLACohesion.ToString());
            parameters.Add(inputEvaluationValue.Coupling.ToString());
            parameters.Add(inputEvaluationValue.Granularity.ToString());
            parameters.Add(inputEvaluationValue.Commonality.ToString());
            parameters.Add(inputEvaluationValue.Reusability.ToString());
            parameters.Add(inputEvaluationValue.Configurability.ToString());
            parameters.Add(inputEvaluationValue.Completeness.ToString());
            parameters.Add(inputEvaluationValue.NumberOfOptionalInterfaces.ToString());
            parameters.Add(inputEvaluationValue.NumberOfMandatoryInterfaces.ToString());
            parameters.Add(inputEvaluationValue.NumberOfOptionalOperations.ToString());
            parameters.Add(inputEvaluationValue.NumberOfMandatoryOperations.ToString());
            //
            parameters.Add(outputEvaluationValue.ConventionalCohesion.ToString());
            parameters.Add(outputEvaluationValue.PLACohesion.ToString());
            parameters.Add(outputEvaluationValue.Coupling.ToString());
            parameters.Add(outputEvaluationValue.Granularity.ToString());
            parameters.Add(outputEvaluationValue.Commonality.ToString());
            parameters.Add(outputEvaluationValue.Reusability.ToString());
            parameters.Add(outputEvaluationValue.Configurability.ToString());
            parameters.Add(outputEvaluationValue.Completeness.ToString());
            parameters.Add(outputEvaluationValue.NumberOfOptionalInterfaces.ToString());
            parameters.Add(outputEvaluationValue.NumberOfMandatoryInterfaces.ToString());
            parameters.Add(outputEvaluationValue.NumberOfOptionalOperations.ToString());
            parameters.Add(outputEvaluationValue.NumberOfMandatoryOperations.ToString());
            using (StreamWriter sw = File.AppendText(logFileName))
            {
                sw.WriteLine(string.Join(",", parameters.ToArray()));
            }
        }
        private void BtnOpenVarFile_Click(object sender, EventArgs e)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + tbExportVarFileAddr.Text);

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            Process proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
        }

    }
}
