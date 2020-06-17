using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
namespace DevTracker.Classes
{
    public static class ErrorLog
    {
       const string eol = "\r\n";
       public static void LogError(Exception ex, bool showMsgBox = false)
        {
            try
            {
                string msg = "Error------" + eol +
                            ex.Message + eol +
                           "Error Type-----" + eol + ex.GetType().ToString() + eol +
                           "Error Details-----" + eol + ex.ToString();
                if (showMsgBox)
                    MessageBox.Show(msg, "Program Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            

                try
                {
                    StackTrace st = new StackTrace(true);
                    msg += "Stack Trace------" + eol + st.ToString(); 
                }
                catch (Exception) 
                { 
                }

                WriteErrorLog(new StringBuilder(msg));
            }
            catch (Exception)
            {
            }
        }

        public static void LogError(string err, bool showMsgBox = false)
        {
            string msg = "Error------" + eol + err + eol;
            if (showMsgBox)
                MessageBox.Show(msg, "Program Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            WriteErrorLog(new StringBuilder(msg));
        }

        private static void WriteErrorLog(StringBuilder sb)
        {
            string fn = GetErrorFilename();
            if (fn == null) return;
            using (var sw = new StringWriter(sb))
            {
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
            }
        }
        private static string GetErrorFilename()
        {
            try
            {
                string AppPath = Path.Combine(GetExePath(), "ErrLogs");
                var di = new DirectoryInfo(AppPath);
                if (!di.Exists)
                    di.Create();
                return AppPath + @"\ErrLog_" + DateTime.Now.ToString("MMddyyyyHHmmss") + ".txt";
            }
            catch(Exception)
            {
                return null;
            }
        }
        private static string GetExePath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }
    }
}
