using Windows.UI.Xaml.Controls;
using PopMailDemo.MVVM.ViewModel;
using Windows.UI.Xaml.Navigation;
using PopMailDemo.View;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PopMailDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Button1.Click += new Windows.UI.Xaml.RoutedEventHandler(OnButton1Click);
        }
        protected void OnButton1Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            e = new Windows.UI.Xaml.RoutedEventArgs();

            this.Frame.Navigate(typeof(EmailProvider));
        }
   }
}
