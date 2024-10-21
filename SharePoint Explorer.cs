using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;
using System.Net.Http;

namespace Explorer
{
    public partial class SharePoint_Explorer : System.Windows.Forms.Form
    {
        private Dictionary<string, string> siteUrls;
        private Dictionary<string, Dictionary<string, string>> spUrls;
        private Dictionary<string, Dictionary<string, string>> accUrls;
       // private const string configFile = "config.json";
        public SharePoint_Explorer()
        {
            InitializeComponent();
            siteUrls = new Dictionary<string, string>();
            spUrls = new Dictionary<string, Dictionary<string, string>>();
            accUrls = new Dictionary<string, Dictionary<string, string>>();
        }
      /* private void SaveConfig()
        {
            var config = new configcs
            {
                comboBox1Items = comboBox1.Items.Cast<string>().ToList(),
                comboBox2Items = comboBox2.Items.Cast<string>().ToList(),
                comboBox3Items = comboBox3.Items.Cast<string>().ToList()
            };

            string json = JsonSerializer.Serialize(config);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile), json);
        }*/
      /*  private void LoadConfig()
        {
            if(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,configFile)))
            {
                string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile));
                var configFileName = JsonSerializer.Deserialize<configcs>(json);

                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(configFileName.comboBox1Items.ToArray());

                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(configFileName.comboBox2Items.ToArray());

                comboBox3.Items.Clear();
                comboBox3.Items.AddRange(configFileName.comboBox3Items.ToArray());
            }
        }*/
        private async void SharePoint_Explorer_Load(object sender, EventArgs e)
        {
            try
            {
                string excelFileUrl = "https://raw.githubusercontent.com/Abhinav-105784/Explorer/main/Data.xlsx";
                string excelFile = Path.Combine(@"C:\Users\goswamia0490\Downloads", "Data.xlsx"); //"C:\\Users\\goswamia0490\\OneDrive - ARCADIS\\Sharepoint Practice\\Explorer\\Data.xlsx";

                using(HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(excelFileUrl);
                    response.EnsureSuccessStatusCode();

                    /*if (response.Content.Headers.ContentType.MediaType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        MessageBox.Show("The downloaded file is not a valid Excel file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }*/

                    using (FileStream fs = new FileStream(excelFile,FileMode.Create,FileAccess.Write,FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                if( !File.Exists(excelFile))
                {
                    MessageBox.Show("The file was not downloaded", "Error" ,MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                FileInfo fileInfo = new FileInfo(excelFile);
                if(fileInfo.Length==0)
                {
                    MessageBox.Show("The file downloaded is Empty","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage package = new ExcelPackage(new FileInfo(excelFile)))
                {
                        siteUrls.Clear();
                        spUrls.Clear();
                        accUrls.Clear();
                        ExcelWorksheet clientSheet = package.Workbook.Worksheets[0]; // First sheet for clients (index 1, not 0)

                        // Load Client URLs from the first sheet
                        for (int row = 2; row <= clientSheet.Dimension.End.Row; row++)
                        {
                            var clientName = clientSheet.Cells[row, 1].Text;
                            var clientUrl = clientSheet.Cells[row, 2].Text;

                            if (!siteUrls.ContainsKey(clientName))
                            {
                                siteUrls[clientName] = clientUrl;
                                spUrls[clientName] = new Dictionary<string, string>();
                                accUrls[clientName] = new Dictionary<string, string>();
                            }
                        }
                    

                    // Load Project URLs from subsequent sheets
                    for (int sheetIndex = 1; sheetIndex < package.Workbook.Worksheets.Count; sheetIndex++)
                    {
                        var projectSheet = package.Workbook.Worksheets[sheetIndex];
                        var clientName = projectSheet.Name;

                        for (int row = 2; row <= projectSheet.Dimension.End.Row; row++)
                        {
                            var projectName = projectSheet.Cells[row, 1].Text;
                            var projectSPUrl = projectSheet.Cells[row, 2].Text;
                            var projectACCUrl = projectSheet.Cells[row, 3].Text;

                            if (spUrls.ContainsKey(clientName))
                            {
                                spUrls[clientName][projectName] = projectSPUrl;
                            }

                            if (accUrls.ContainsKey(clientName))
                            {
                                accUrls[clientName][projectName] = projectACCUrl;
                            }
                        }

                        // Populate comboBox1 with client names
                        comboBox1.Items.Clear(); // Clear items before adding
                        comboBox1.Items.AddRange(siteUrls.Keys.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }

           
        }
        /* private void MoveUpTheSelection(ComboBox comboBox)
         {
             object selectedItem = comboBox.SelectedItem;
             if(selectedItem!=null)
             {
                 int index = comboBox.SelectedIndex;
                 if(index>0)
                 {
                     comboBox.Items.RemoveAt(index);
                     comboBox.Items.Insert(index - 1, selectedItem);
                     comboBox.SelectedIndex = index - 1;
                 }
             }
         }*/

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                string excelFile = Path.Combine(@"C:\Users\goswamia0490\Downloads", "Data.xlsx");
                if(File.Exists(excelFile))
                {
                    File.Delete(excelFile);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Open_Click(object sender, EventArgs e)
        {
            string selectedSiteName = comboBox1.SelectedItem?.ToString();

            if(!string.IsNullOrEmpty(selectedSiteName) && siteUrls.ContainsKey(selectedSiteName))
            {
                try
                {
                    string siteUrl = siteUrls[selectedSiteName];

                    Process.Start(siteUrl);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in requesting url: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Close();
            }
        }
        private void Close_Click(object sender, EventArgs e)
        {
            //SaveConfig();
            Close();
        }


        private void OpenSharePoint_Click(object sender, EventArgs e)
        {
            string clientUrl = comboBox1.SelectedItem?.ToString();
            string projectsSP = comboBox2.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(clientUrl) && !string.IsNullOrEmpty(projectsSP))
            {
                try
                {
                    string siteUrl = spUrls[clientUrl][projectsSP];

                    Process.Start(new ProcessStartInfo(siteUrl) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in requesting url: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Close();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //MoveUpTheSelection(comboBox1);
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();

            string selectedClient = comboBox1.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedClient) && spUrls.ContainsKey(selectedClient))
            {
                // Get the dictionary of projects for the selected client
                Dictionary<string, string> projects = spUrls[selectedClient];

                // Populate comboBox2 with the projects
                comboBox2.Items.AddRange(projects.Keys.ToArray());
                comboBox3.Items.AddRange(projects.Keys.ToArray());
               
            }        
        }

        private void OpenAcc_Click(object sender, EventArgs e)
        {
            string clientUrl = comboBox1.SelectedItem?.ToString();
            string projectsACC = comboBox3.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(clientUrl) && !string.IsNullOrEmpty(projectsACC))
            {
                try
                {
                    string siteUrl = accUrls[clientUrl][projectsACC];

                    Process.Start(new ProcessStartInfo(siteUrl) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in requesting url: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Close();
            }
        }

       
    }
}
