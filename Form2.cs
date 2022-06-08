using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyPLAOptimization
{
    public partial class Form2 : Form
    {
        string path = "";
        public Form2()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

        }
        void Automation(string rootPath)
        {
            var subDir = Directory.GetDirectories(rootPath);
            foreach (var item in subDir)
            {
                string[] files = Directory.GetFiles(item);
                Form1 form = new Form1();
                string plaFileName = files.Where(x => x.Contains("pla")).Single();
                string featureModelFileName = files.Where(x => x.Contains("FM.xml")).Single();
                string relFileName = files.Where(x => x.Contains("FMR.csv")).Single();
                string path = relFileName.Replace("FMR.csv", "");
                form.SetPrimaryPLA<XMLConvertor>(plaFileName);
                form.SetFeatureModel(featureModelFileName);
                form.SetRelationshipMetrix(relFileName);
                form.IsAutoMation = true;
                form.RunOptimization(100000);
                Application.Run(form);
                // calc diff
                double diffReusabilityInspace = form.outputEvaluationValue.ReusabilityInspace - form.inputEvaluationValue.ReusabilityInspace;
                double diffReusabilityIntime = form.inputEvaluationValue.ReusabilityIntime - form.outputEvaluationValue.ReusabilityIntime;
                double diffFM = form.inputEvaluationValue.FM - form.outputEvaluationValue.FM;
                double diffCM = form.inputEvaluationValue.CM - form.outputEvaluationValue.CM;
                if (!Directory.Exists(path + "Output"))
                    Directory.CreateDirectory(path + "Output");
                form.ExportFuncFile(path + "Output/func.txt");
                form.ExportVarFile(path + "Output/var.txt");
                form.ExportOptimizedPLA(path + "Output/optimized PL-A.xml", true);
                AddEvaluationResult(plaFileName.Split('\\').Last(), 
                    diffFM,
                    diffCM,
                    diffReusabilityIntime * 100,
                    diffReusabilityInspace * 100,
                    form.EstimatedTime,
                    form.outputEvaluationValue.NumberOfInterfaces,
                    form.outputEvaluationValue.NumberOfComponents);
            }
        }
        public void AddEvaluationResult(string plaName, double fm, double cm, double configurability, double reusability, long estimatedTime,int numberOfComponents,int numberOfInterfaces)
        {
            var index = dataGridView1.Rows.Add();
            var row = dataGridView1.Rows[index];
            row.Cells[0].Value = plaName;
            row.Cells[1].Value = Math.Round(fm, 2);
            row.Cells[2].Value = Math.Round(cm, 2);
            row.Cells[3].Value = Math.Round(configurability, 2);
            row.Cells[4].Value = Math.Round(reusability, 2);
            row.Cells[5].Value = new DateTime().AddMilliseconds(estimatedTime).TimeOfDay.ToString(@"hh\:mm\:ss");
            row.Cells[6].Value = numberOfInterfaces;
            row.Cells[7].Value = numberOfComponents;
            //dataGridView1.Rows.Add(row);
        }

        private void BtnAutomation_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    path = fbd.SelectedPath;
                    lblbPath.Text = path;
                    new Thread(() =>
                    {
                        //Automation(@"D:\Projects\MyPLAOptimization\MyPLAOptimization\MyPLAOptimization\PLAGenerator\bin\Debug\GeneraAe20 PLA_220520 095155");
                        Automation(path);
                    }).Start();
                }
            }
        }
    }
}
