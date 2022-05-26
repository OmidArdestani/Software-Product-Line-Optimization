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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyPLAOptimization
{
    public partial class Form1 : Form
    {
        public bool IsAutoMation { get; set; }
        private IFileConvertor xmiConv = null;
        private PLArchitecture GotArchitecture = null;
        private bool FeaturModelLoaded = false;
        private bool DiagramLoaded = false;
        private string logFileName = "OutputLog.csv";
        NSGAIIOptimizer MyOptimization = null;
        XMLFeatureModel featureModel = new XMLFeatureModel();
        List<FeatureRelationship> featureRelationshipMatrix = new List<FeatureRelationship> { };
        public struct PLAEvaluationValue
        {
            //Fitness funtions
            public double PLACohesion;
            public double ConventionalCohesion;
            public double Coupling;
            public double Commonality;
            public double Granularity;
            public double FM;
            public double CM;
            // Metrics
            public double Completeness;
            public double ReusabilityIntime;
            public double ReusabilityInspace;
            // info
            public int NumberOfOptionalInterfaces;
            public int NumberOfMandatoryInterfaces;
            public int NumberOfOptionalOperations;
            public int NumberOfMandatoryOperations;
        }

        public PLAEvaluationValue inputEvaluationValue;
        public PLAEvaluationValue outputEvaluationValue;
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
            RunOptimization((int)nudMaximumEvaluation.Value);
        }
        public void RunOptimization(int maxEvaluation)
        {
            nudMaximumEvaluation.Value = maxEvaluation;
            MyOptimization.MaxEvaluation = maxEvaluation;
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
            outputEvaluationValue.ReusabilityInspace = MyOptimization.problem.EvalReusabilityInspace(MyOptimization.BestPLA);
            outputEvaluationValue.ReusabilityIntime = MyOptimization.problem.EvalReusabilityIntime(MyOptimization.BestPLA);
            outputEvaluationValue.Completeness = MyOptimization.problem.EvalCompleteness(MyOptimization.BestPLA);
            outputEvaluationValue.Granularity = MyOptimization.problem.EvalGranularityObjective(MyOptimization.BestPLA);
            outputEvaluationValue.FM = MyOptimization.problem.EvalFMObjective(MyOptimization.BestPLA);
            outputEvaluationValue.CM = MyOptimization.problem.EvalCMObjective(MyOptimization.BestPLA);
            // show in labels
            lblOutputPLACOhesion.Text = Math.Round(outputEvaluationValue.PLACohesion * 100, 2).ToString() + "%";
            lblOutputConventionalCohesion.Text = Math.Round(outputEvaluationValue.ConventionalCohesion * 100, 2).ToString() + "%";
            lblOutputReusabilityIntime.Text = Math.Round(outputEvaluationValue.ReusabilityIntime * 100, 2).ToString() + "%";
            lblOutputReusabilityInspace.Text = Math.Round(outputEvaluationValue.ReusabilityInspace * 100, 3).ToString() + "%";
            lblOutputCoupling.Text = Math.Round(outputEvaluationValue.Coupling * 100, 2).ToString() + "%";
            lblOutputCommonality.Text = Math.Round(outputEvaluationValue.Commonality * 100, 1).ToString() + "%";
            lblOutputCompleteness.Text = Math.Round(outputEvaluationValue.Completeness * 100, 1) + "%";
            lblOutputGranularity.Text = Math.Round(outputEvaluationValue.Granularity, 2).ToString();
            lblOutputFM.Text = Math.Round(outputEvaluationValue.FM, 2).ToString();
            lblOutputCM.Text = Math.Round(outputEvaluationValue.CM, 2).ToString();
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
            // calc diff
            double diffReusabilityInspace = outputEvaluationValue.ReusabilityInspace - inputEvaluationValue.ReusabilityInspace;
            double diffReusabilityIntime = inputEvaluationValue.ReusabilityIntime - outputEvaluationValue.ReusabilityIntime;
            double diffFM = inputEvaluationValue.FM - outputEvaluationValue.FM;
            double diffCM = inputEvaluationValue.CM - outputEvaluationValue.CM;
            lblbDiffReusabilityIntime.Text = Math.Round(diffReusabilityIntime * 100, 2).ToString();
            lblDiffReusabilityInspace.Text = Math.Round(diffReusabilityInspace * 100, 2).ToString();
            lblDiffFM.Text = Math.Round(diffFM, 2).ToString();
            lblDiffCM.Text = Math.Round(diffCM, 2).ToString();
            if (diffReusabilityIntime < 0)
                lblbDiffReusabilityIntime.ForeColor = Color.Red;
            else
                lblbDiffReusabilityIntime.ForeColor = Color.Green;

            if (diffReusabilityInspace < 0)
                lblDiffReusabilityInspace.ForeColor = Color.Red;
            else
                lblDiffReusabilityInspace.ForeColor = Color.Green;

            if (diffFM < 0)
                lblDiffFM.ForeColor = Color.Red;
            else
                lblDiffFM.ForeColor = Color.Green;

            if (diffCM < 0)
                lblDiffCM.ForeColor = Color.Red;
            else
                lblDiffCM.ForeColor = Color.Green;
            // finishing
            groupBox3.Enabled = true;
            LogResultInFile();
            // play sound
            if (diffFM > 0 && diffCM > 0 && diffReusabilityIntime > 0 && diffReusabilityInspace > 0)
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"C:\Users\OmidHome\Downloads\beep-01a.wav");
                player.Play();
            }
            else
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"C:\Users\OmidHome\Downloads\beep beep.wav");
                player.Play();
            }
            // check automation condition
            if (IsAutoMation)
            {
                Thread.Sleep(10000);
                this.Close();
            }
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
                    SetPrimaryPLA<XMIConvertor>(dialog.FileName);
                }
                // Input file is XML format
                else if (dialog.FilterIndex == 2)
                {
                    SetPrimaryPLA<XMLConvertor>(dialog.FileName);
                }
            }
        }
        public void SetPrimaryPLA<T>(string filePath)
        {
            IFileConvertor fileConvertor;
            // Input file is XMI format
            if (typeof(T) == typeof(XMIConvertor))
            {
                xmiConv = new XMIConvertor();
            }
            // Input file is XML format
            else if (typeof(T) == typeof(XMLConvertor))
            {
                xmiConv = new XMLConvertor();
            }
            GotArchitecture = xmiConv.ReadFile(filePath);
            string[] addressParts = filePath.Split('\\');
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
                SetFeatureModel(dialog.FileName);
            }
        }
        public void SetFeatureModel(string filePath)
        {
            featureModel = new XMLFeatureModel();
            featureModel.LoadFile(filePath);
            int childCnt = featureModel.Root.ChildCount();
            FeaturModelLoaded = childCnt > 0;
            if (FeaturModelLoaded && DiagramLoaded)
            {
                nudMaximumEvaluation.Enabled = true;
                btnRunAlgorithm.Enabled = true;
            }
            // Show address in text
            string[] addressParts = filePath.Split('\\');
            if (addressParts.Length > 2)
                tbFMFileAddress.Text = addressParts[addressParts.Length - 2] + "/" + addressParts[addressParts.Length - 1];
            else
                tbFMFileAddress.Text = string.Join("/", addressParts);
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
                SetRelationshipMetrix(dialog.FileName);
            }
        }
        public void SetRelationshipMetrix(string filePath)
        {
            string[] fileLines = File.ReadAllLines(filePath);
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
            string[] addressParts = filePath.Split('\\');
            if (addressParts.Length > 2)
                tbFMRelFileAddress.Text = addressParts[addressParts.Length - 2] + "/" + addressParts[addressParts.Length - 1];
            else
                tbFMRelFileAddress.Text = string.Join("/", addressParts);
            // config My Optimization
            MyOptimization.Configuration(GotArchitecture, featureModel, featureRelationshipMatrix, (int)nudMaximumEvaluation.Value, GotArchitecture.OperatorCount);
            // calc functions
            inputEvaluationValue.Coupling = MyOptimization.problem.EvalCoupling(GotArchitecture);
            inputEvaluationValue.Commonality = MyOptimization.problem.EvalCommonality(GotArchitecture);
            //inputEvaluationValue.ConventionalCohesion = MyOptimization.problem.EvalConventionalCohesion(GotArchitecture);
            inputEvaluationValue.PLACohesion = MyOptimization.problem.EvalPLACohesion(GotArchitecture);
            inputEvaluationValue.ConventionalCohesion = MyOptimization.problem.EvalConventionalCohesion(GotArchitecture);
            inputEvaluationValue.ReusabilityInspace = MyOptimization.problem.EvalReusabilityInspace(GotArchitecture);
            inputEvaluationValue.ReusabilityIntime = MyOptimization.problem.EvalReusabilityIntime(GotArchitecture);
            inputEvaluationValue.Granularity = MyOptimization.problem.EvalGranularityObjective(GotArchitecture);
            inputEvaluationValue.FM = MyOptimization.problem.EvalFMObjective(GotArchitecture);
            inputEvaluationValue.CM = MyOptimization.problem.EvalCMObjective(GotArchitecture);
            // info

            // show in labels
            lblInputPLACOhesion.Text = Math.Round(inputEvaluationValue.PLACohesion * 100, 2).ToString() + "%";
            lblInputConventionalCohesion.Text = Math.Round(inputEvaluationValue.ConventionalCohesion * 100, 2).ToString() + "%";
            lblInputReusabilityIntime.Text = Math.Round(inputEvaluationValue.ReusabilityIntime * 100, 2).ToString() + "%";
            lblInputReusabilityInspace.Text = Math.Round(inputEvaluationValue.ReusabilityInspace * 100, 3).ToString() + "%";
            lblInputCoupling.Text = Math.Round(inputEvaluationValue.Coupling * 100, 2).ToString() + "%";
            lblInputCommonality.Text = Math.Round(inputEvaluationValue.Commonality * 100, 1).ToString() + "%";
            lblInputGranularity.Text = Math.Round(inputEvaluationValue.Granularity, 2).ToString();
            lblInputFM.Text = Math.Round(inputEvaluationValue.FM, 2).ToString();
            lblInputCM.Text = Math.Round(inputEvaluationValue.CM, 2).ToString();
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
                    "Input PLA-Cohesion","Input Coupling","Input Granularity","Input Commonality",
                    "Input Reusability Inspace","Input Reusability Intime","Input Completeness","Input Number Of Optional Interfaces",
                    "Input Number Of Mandatory Interfaces","Input Number Of Optional Operations","Input Number Of Mandatory Operations","Input FM","Input CM",
                    "Output PLA-Cohesion","Output Coupling","Output Granularity","Output Commonality","Output Reusability Inspace",
                    "Output Reusability Intime","Output Completeness","Output Number Of Optional Interfaces","Output Number Of Mandatory Interfaces",
                    "Output Number Of Optional Operations","Output Number Of Mandatory Operations","Output FM","Output CM","EstimatedTime[ms]"
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
            parameters.Add(inputEvaluationValue.PLACohesion.ToString());
            parameters.Add(inputEvaluationValue.Coupling.ToString());
            parameters.Add(inputEvaluationValue.Granularity.ToString());
            parameters.Add(inputEvaluationValue.Commonality.ToString());
            parameters.Add(inputEvaluationValue.ReusabilityInspace.ToString());
            parameters.Add(inputEvaluationValue.ReusabilityIntime.ToString());
            parameters.Add(inputEvaluationValue.Completeness.ToString());
            parameters.Add(inputEvaluationValue.NumberOfOptionalInterfaces.ToString());
            parameters.Add(inputEvaluationValue.NumberOfMandatoryInterfaces.ToString());
            parameters.Add(inputEvaluationValue.NumberOfOptionalOperations.ToString());
            parameters.Add(inputEvaluationValue.NumberOfMandatoryOperations.ToString());
            parameters.Add(inputEvaluationValue.FM.ToString());
            parameters.Add(inputEvaluationValue.CM.ToString());
            //
            parameters.Add(outputEvaluationValue.PLACohesion.ToString());
            parameters.Add(outputEvaluationValue.Coupling.ToString());
            parameters.Add(outputEvaluationValue.Granularity.ToString());
            parameters.Add(outputEvaluationValue.Commonality.ToString());
            parameters.Add(outputEvaluationValue.ReusabilityInspace.ToString());
            parameters.Add(outputEvaluationValue.ReusabilityIntime.ToString());
            parameters.Add(outputEvaluationValue.Completeness.ToString());
            parameters.Add(outputEvaluationValue.NumberOfOptionalInterfaces.ToString());
            parameters.Add(outputEvaluationValue.NumberOfMandatoryInterfaces.ToString());
            parameters.Add(outputEvaluationValue.NumberOfOptionalOperations.ToString());
            parameters.Add(outputEvaluationValue.NumberOfMandatoryOperations.ToString());
            parameters.Add(outputEvaluationValue.FM.ToString());
            parameters.Add(outputEvaluationValue.CM.ToString());
            // 
            parameters.Add(MyOptimization.EstimatedTime.ToString());
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
