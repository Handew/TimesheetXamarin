using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;
using XamarinTimesheet.Models;
using Xamarin.Essentials;
using System.Threading;

namespace XamarinTimesheet
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkAssignmentPage : ContentPage
    {
        int eId;
        string lon;
        string lat;

        public WorkAssignmentPage(int idParam)
        {
            InitializeComponent();

            eId = idParam;

            lon_label.Text = "Sijaintia haetaan";

            Työ_lataus.Text = "Ladataan työtehtäviä...";

            LoadDataFromRestAPI();

            _ = GetCurrentLocation();

            //--------------------lokaatio-----------------

            async Task GetCurrentLocation()
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {
                        lat_label.Text = ($"Longitude: { location.Longitude}");
                        lon_label.Text = ($"Latitude: {location.Latitude}");

                        lon = location.Longitude.ToString();
                        lat = location.Latitude.ToString();
                    }
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    // Handle not supported on device exception
                    await DisplayAlert("Virhe", fnsEx.ToString(), "OK");
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    // Handle not enabled on device exception
                    await DisplayAlert("Virhe", fneEx.ToString(), "OK");
                }
                catch (PermissionException pEx)
                {
                    // Handle permission exception
                    await DisplayAlert("Virhe", pEx.ToString(), "OK");
                }
                catch (Exception ex)
                {
                    // Unable to get location
                    await DisplayAlert("Virhe", ex.ToString(), "OK");
                }
            }


        async void LoadDataFromRestAPI()
            {
                try
                {
                    workList.ItemsSource = new List<string> { "Ladataan", "Työtehtäviä", "palvelimelta..." };

                    HttpClient client = new HttpClient();

                    client.BaseAddress = new Uri("https://timesheetbackend.azurewebsites.net/");
                    string json = await client.GetStringAsync("/api/workassignments/");

                    IEnumerable<WorkAssignment> works = JsonConvert.DeserializeObject<WorkAssignment[]>(json);
                    ObservableCollection<WorkAssignment> dataa = new ObservableCollection<WorkAssignment>(works);
                    workList.ItemsSource = dataa;
                    //Tyhjennetään latausilmoitus
                    Työ_lataus.Text = "";

                }
                catch (Exception e)
                {
                    await DisplayAlert("Virhe: ", e.Message.ToString(), "SELVÄ!");
                }
            }
        }

        async void Aloitus_Nappi_Clicked(object sender, EventArgs e)
        {
            WorkAssignment wa = (WorkAssignment)workList.SelectedItem;

            if (wa == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työtehtävä", "OK");
            }

            try
            {

                Operation op = new Operation
                {
                    EmployeeID = eId,
                    WorkAssidnmentID = wa.IdWorkAssingment,
                    CustomerID = wa.IdCustomer,
                    OperationType = "start",
                    Comment = "Aloitettu",
                    Latitude = lat,
                    Longitude = lon
                };

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://timesheetbackend.azurewebsites.net/");

                // Muutetaan em. op objekti Jsoniksi
                string input = JsonConvert.SerializeObject(op);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/workassignments", content);

                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                // Asetetaan vastaus serialisoituna success muuttujaan
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success == false)
                {
                    await DisplayAlert("Ei voida aloittaa", "Työ on jo käynnissä", "OK");
                }
                else if (success == true)
                {
                    await DisplayAlert("Työ aloitettu", "Työ on aloitettu", "OK");
                }
                
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe: ", ex.Message.ToString(), "SELVÄ");
            }
        }

        async void Lopetus_Nappi_Clicked(object sender, EventArgs e)
        {

            WorkAssignment wa = (WorkAssignment)workList.SelectedItem;
            

            if (wa == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työtehtävä", "OK");
                return;
            }
            string result = await DisplayPromptAsync("Kommentti", "Kirjoita kommentti");

            try
            {
                Operation op = new Operation 
                {   
                    EmployeeID = eId, 
                    WorkAssidnmentID = wa.IdWorkAssingment, 
                    CustomerID = wa.IdCustomer, 
                    OperationType = "stop",
                    Comment = result,
                    Latitude = lat,
                    Longitude = lon
                };

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://timesheetbackend.azurewebsites.net/");

                // Muutetaan em. op objekti Jsoniksi
                string input = JsonConvert.SerializeObject(op);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/workassignments", content);

                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                // Asetetaan vastaus serialisoituna success muuttujaan
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success == false)
                {
                    await DisplayAlert("Ei voida lopettaa", "Työtä ole aloitettu", "OK");
                }
                else if (success == true)
                {
                    await DisplayAlert("Työn päättyminen", "Työ on päättynyt", "OK");

                    await Navigation.PushAsync(new WorkAssignmentPage(eId));
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe: ", ex.Message.ToString(), "SELVÄ");
            }
        }
    }
}