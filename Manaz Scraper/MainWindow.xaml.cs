using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using HtmlAgilityPack;
using System.Net;

namespace Manaz_Scraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataTable table;
        HtmlWeb web;

        public MainWindow()
        {
            InitializeComponent();
            InitTable();
        }

        private void InitTable()
        {
            table = new DataTable("BusinessContacts");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Tele", typeof(string));

            dataGrid.DataContext = table.DefaultView;
        }

        private async Task<List<BusinessContact>> BusinessContactsFromPage(int pageNum)
        {
            string url = "https://www.yellowpages.com/search?search_terms=computer+repair&geo_location_terms=Los+Angeles%2C+CA";
            if (pageNum != 0)
                url = "https://www.yellowpages.com/search?search_terms=computer+repair&geo_location_terms=Los+Angeles%2C+CA&page=" + pageNum.ToString();

            //var doc = await Task.Factory.StartNew(() => web.Load(url));
            HtmlDocument doc = null;

            await Task.Run(() =>
            {
                doc = new HtmlDocument();
                doc.OptionReadEncoding = false;
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        doc.Load(stream, Encoding.UTF8);
                    }
                }
            });

            var nameNodes = doc.DocumentNode.SelectNodes("//a[@class=\"business-name\"]//span");
            var scoreNodes = doc.DocumentNode.SelectNodes("//*[@class=\"result\"]//div//div//div//div[@class=\"phones phone primary\"]");
            //If these are null it means the name/score nodes couldn't be found on the HTML page
            if (nameNodes == null || scoreNodes == null)
                return new List<BusinessContact>();
            var names = nameNodes.Select(node => node.InnerText);
            var scores = scoreNodes.Select(node => node.InnerText);

            return names.Zip(scores, (name, tele) => new BusinessContact() { Name = name, Telephone = tele }).ToList();
        }

        //private async HtmlDocument getHTMLDocument(string url)
        //{
        //    var doc = new HtmlDocument();
        //    doc.OptionReadEncoding = false;
        //    var request = (HttpWebRequest)WebRequest.Create(url);
        //    request.Method = "GET";
        //    using (var response = (HttpWebResponse)request.GetResponse())
        //    {
        //        using (var stream = response.GetResponseStream())
        //        {
        //            doc.Load(stream, Encoding.UTF8);
        //        }
        //    }

        //    return doc;
        //}

        private async void btnScrape_Click(object sender, RoutedEventArgs e)
        {
            int pageNumber = 20;

            var contacts = await BusinessContactsFromPage(pageNumber);
            //foreach (var contact in contacts)
            //    table.Rows.Add(contact.Name, contact.Telephone);
            
            while (contacts.Count > 0)
            {
                foreach (var contact in contacts)
                    table.Rows.Add(contact.Name, contact.Telephone);
                pageNumber++;
                contacts = await BusinessContactsFromPage(pageNumber);

            }

            dataGrid.ItemsSource = table.DefaultView;
        }
    }
}
