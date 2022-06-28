using System.Threading.Tasks;
using System.Windows.Controls;

namespace POC.Internal
{
    /// <summary>
    /// Interaction logic for SampleProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialogUserControl : UserControl
    {
        public ProgressDialogUserControl()
        {
            InitializeComponent();
            Loop();
        }
        private async void Loop()
        {
            lbl.Text = "LOADING...";

            for (int i = 0; i < 1; i++)
            {
                await Task.Delay(100);
                //pb.Value =100;

                //pb.Visibility = System.Windows.Visibility.Visible;
                //lblPercentage.Visibility = System.Windows.Visibility.Visible;
                lbl.Text = "COMPLETED...";

            }

        }
    }
}
