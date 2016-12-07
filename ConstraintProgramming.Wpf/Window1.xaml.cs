using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.SolverFoundation.Services;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ConstraintProgramming.Entities;

namespace ProductRecommender {
    public partial class Window1 {

        private SolverContext _context;
        private DecisionBinding[] _configurationDecisions;
        private Dictionary<string, DecisionBinding> _decisionsByName;

        public Window1() {
            this.InitializeComponent();
            _context = SolverContext.GetContext();
            CreateModelAndBindToDbData();
            InitializeConfigurator();
        }

        private void Window_Initialized(object sender, EventArgs e) {

        }

        private void CreateModelAndBindToDbData() {
            try {

                SystemContext db = new SystemContext();

                Model productModel = _context.CreateModel();
                Domain companies = Domain.Enum(db.Companies.Select(x=>x.Name).ToArray());
                Domain stores = Domain.Enum(db.Companies.Select(x => x.Name).ToArray());
                Domain categories = Domain.Enum(db.Companies.Select(x => x.Name).ToArray());

                Decision company = new Decision(companies, "ProductCompany");
                Decision store = new Decision(stores, "ProductStore");
                Decision cateogry = new Decision(categories, "ProductCategory");
                Decision price = new Decision(Domain.IntegerRange(20, 60), "ProductPrice");
                productModel.AddDecisions(company, store, cateogry, price);

                Domain[] domains = new Domain[] { companies, stores, categories, Domain.IntegerRange(100, 10000) };
                Tuples table = new Tuples("tuples", domains);

                IFormatProvider provider = System.Globalization.CultureInfo.InvariantCulture;
                //Bind the data from Xml
                XDocument doc = XDocument.Load(@"..\..\data.xml");
                IEnumerable<Row> _tableData = from eleRow in db.Products
                                              select new Row
                                              {
                                                  Company = eleRow.Company.Name,
                                                  Store = eleRow.Store.Name,
                                                  Category = eleRow.Category.Name,
                                                  Price = eleRow.Price
                                              };
                table.SetBinding(_tableData, new[] { "Company", "Store", "Category", "Price" });
                productModel.AddTuple(table);
                productModel.AddConstraint("TableConstraint", Model.Equal(new Term[] { company, store, cateogry, price }, table));
            }
            catch (Exception ex) {
                Trace.TraceError(ex.Message);
                _context = null;
            }
        }

        private List<string> ToStringValues(string decisionName) {
            DecisionBinding d = _decisionsByName[decisionName];
            List<string> result = new List<string>();
            result = d.StringFeasibleValues.ToList();
            return result;
        }

        private List<string> ToIntValues(string decisionName) {
            DecisionBinding d = _decisionsByName[decisionName];
            List<string> result = new List<string>();
            int[] values = d.Int32FeasibleValues.ToArray();
            Array.Sort(values);
            foreach (int i in values) {
                result.Add(string.Format("{0}k", i));
            }
            return result;
        }

        private void InitializeConfigurator() {
            if (_context == null)
                return;

            if (_context.CurrentModel.IsEmpty)
                return;

            List<DecisionBinding> decisions = new List<DecisionBinding>();
            _decisionsByName = new Dictionary<string, DecisionBinding>();
            foreach (Decision d in _context.CurrentModel.Decisions) {
                var dbinding = d.CreateBinding();
                decisions.Add(dbinding);
                _decisionsByName.Add(d.Name, dbinding);
            }

            _configurationDecisions = decisions.ToArray();
            _context.FindAllowedValues(_configurationDecisions);
            UpdateListBoxContent();
        }

        private void UpdateListBoxContent() {
            ProductCompany.ItemsSource = ToStringValues(ProductCompany.Name);
            ProductStore.ItemsSource = ToStringValues(ProductStore.Name);
            ProductCategory.ItemsSource = ToStringValues(ProductCategory.Name);
            ProductPrice.ItemsSource = ToIntValues(ProductPrice.Name);
        }

        private void Configurate(Button button, ListBox listBox) {
            string name = listBox.Name;
            if ((string)button.Content == "Select") {
                if (name == "ProductPrice") {
                    string value = listBox.SelectedValue.ToString();
                    _decisionsByName[name].Fix(Convert.ToInt32(value.Substring(0, value.Length - 1)));
                }
                else
                    _decisionsByName[name].Fix((string)listBox.SelectedValue);
                _context.FindAllowedValues(_configurationDecisions);
                UpdateListBoxContent();
                button.Content = "Unselect";
            }
            else {
                _decisionsByName[name].Unfix();
                _context.FindAllowedValues(_configurationDecisions);
                UpdateListBoxContent();
                button.Content = "Select";
            }
        }

        private void EngineButton_Click(object sender, RoutedEventArgs e) {
            Configurate(EngineButton, ProductCompany);
        }

        private void ModelButton_Click(object sender, RoutedEventArgs e) {
            Configurate(ModelButton, ProductStore);
        }

        private void PackageButton_Click(object sender, RoutedEventArgs e) {
            Configurate(PackageButton, ProductCategory);
        }

        private void PriceButton_Click(object sender, RoutedEventArgs e) {
            Configurate(PriceButton, ProductPrice);
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e) {
            Window.Close();
        }
    }

    public class Row {
        public string Company { get; set; }
        public string Store { get; set; }
        public string Category { get; set; }
        public int Price { get; set; }
    }
}