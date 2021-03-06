using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Net.Http;
using XamarinTimesheet.Models;
using System.Collections.ObjectModel;

namespace XamarinTimesheet
{
    public partial class EmployeePage : ContentPage
    {
        public EmployeePage()
        {
            InitializeComponent();

            Emp_Lataus.Text = "Ladataan työntekijöitä...";

            LoadDataFromRestAPI();

            async void LoadDataFromRestAPI()
            {
                try
                {
                    HttpClient client = new HttpClient();

                    client.BaseAddress = new Uri("https://timesheetbackend.azurewebsites.net");
                    string json = await client.GetStringAsync("/api/employees/");

                    IEnumerable<Employee> employees = JsonConvert.DeserializeObject<Employee[]>(json);
                    ObservableCollection<Employee> dataa = new ObservableCollection<Employee>(employees);
                    employeeList.ItemsSource = dataa;
                    //Tyhjennetään latausilmoitus label
                    Emp_Lataus.Text = null;

                }
                catch (Exception e)
                {
                    await DisplayAlert("Virhe: ", e.Message.ToString(), "SELVÄ!");
                }
            }
        }

        async void navbutton_Clicked(object sender, EventArgs e)
        {
            Employee emp = (Employee)employeeList.SelectedItem;
            if (emp == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työntekijä", "OK"); // (otsiko, teksti, kuittausnapin teksti)
            }
            else
            {
                //await DisplayAlert("Valinta puuttuu", emp.FirstName, "OK");
                int id = emp.IdEmployee;
                await Navigation.PushAsync(new WorkAssignmentPage(id)); // Navigoidaan uudelle sivulle
            }
        }
    }
}
