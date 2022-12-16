using MalwareScanner;

namespace incident_response_scanner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            scanner scanner = new scanner(displayFiles, displayProcesses, displayFiles, displayIPAddress);
            scanner.scan();
        }
    }
}