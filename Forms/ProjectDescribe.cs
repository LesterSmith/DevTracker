using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessObjects;
namespace DevTracker
{
    public partial class ProjectDescribe : Form
    {
        public ProjectName ProjNameDesc = new ProjectName();

        public ProjectDescribe()
        {
            InitializeComponent();
        }

        public void Display()
        {
            this.ShowDialog();
            this.Close();
        }

        private void btnCancelProjectDescription_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSaveProjectDescription_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtProjectDescription.Text) && !string.IsNullOrWhiteSpace(txtProjectID.Text))
            {
                var p = ProjNameDesc;//new ProjectName();
                p.ProjName = txtProjectID.Text;
                p.ProjDescription = txtProjectDescription.Text;
                p.AppName = txtAppName.Text;
                p.Keywords = new List<string>();
                foreach (var line in txtKeywords.Lines)
                {
                    if (line == null) break;
                    if (!string.IsNullOrWhiteSpace(line))
                        p.Keywords.Add(line);
                }
                ProjNameDesc = p;
                Close();
                Application.DoEvents();
            }
            else
            {
                MessageBox.Show("You must enter an ID and Description.  The more data you enter, the more likely we are to get the right project.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
