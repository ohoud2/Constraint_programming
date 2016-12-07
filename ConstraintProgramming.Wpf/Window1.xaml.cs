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

        SystemContext db = new SystemContext();
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

                

                Model productModel = _context.CreateModel();
                productModel.Name = "Product Rec";
                Domain productIds = Domain.Set(db.Products.Select(x => x.Id).ToArray());
                Domain companies = Domain.Enum(db.Companies.Select(x=>x.Name).ToArray());
                Domain stores = Domain.Enum(db.Stores.Select(x => x.Name).ToArray());
                Domain categories = Domain.Enum(db.Categories.Select(x => x.Name).ToArray());

                Decision productId = new Decision(productIds, "ProductId");
                Decision company = new Decision(companies, "ProductCompany");
                Decision store = new Decision(stores, "ProductStore");
                Decision cateogry = new Decision(categories, "ProductCategory");
                Decision price = new Decision(Domain.IntegerRange(100, 10000), "ProductPrice");

                productModel.AddDecisions(productId, company, store, cateogry, price);

                Domain[] domains = new Domain[] { productIds, companies, stores, categories, Domain.IntegerRange(100, 10000) };
                Tuples table = new Tuples("ProductsTuples", domains);

                IFormatProvider provider = System.Globalization.CultureInfo.InvariantCulture;
                //Bind the data from Db
                IEnumerable<Row> _tableData = (from eleRow in db.Products
                                               select new Row
                                               {
                                                   ProductId = eleRow.Id,
                                                   Company = eleRow.Company.Name,
                                                   Store = eleRow.Store.Name,
                                                   Category = eleRow.Category.Name,
                                                   Price = eleRow.Price
                                               });
                table.SetBinding(_tableData, new[] { "ProductId",  "Company", "Store", "Category", "Price" });

                productModel.AddTuple(table);
                productModel.AddConstraint("Const", Model.Equal(new Term[] { productId, company, store, cateogry, price }, table));

                // Companies Selection
                var companiesSelected = new string[] { "Apple", "Sony" };
                var orTerm = new List<Term>();
                foreach( var s in companiesSelected) {
                    orTerm.Add(company == s);
                }
                productModel.AddConstraint("companiesSelected", Model.Or(orTerm.ToArray()));

                // Price Selection
                var maxPrice = 700;
                productModel.AddConstraint("maxPrice", price <= maxPrice);

                // Categories Selection
                var categoriesSelected = new string[] { "Phones", "Tablets" };
                orTerm.Clear();
                foreach (var s in categoriesSelected) {
                    orTerm.Add(cateogry == s);
                }
                productModel.AddConstraint("categoriesSelected", Model.Or(orTerm.ToArray()));
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

        private List<string> ToPriceValues(string decisionName) {
            DecisionBinding d = _decisionsByName[decisionName];
            List<string> result = new List<string>();
            int[] values = d.Int32FeasibleValues.ToArray();
            Array.Sort(values);
            foreach (int i in values) {
                result.Add(string.Format("{0}$", i));
            }
            return result;
        }


        private  IEnumerable<int> ToIntValues(string decisionName) {
            DecisionBinding d = _decisionsByName[decisionName];
            return d.Int32FeasibleValues.ToArray();
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
            ProductPrice.ItemsSource = ToPriceValues(ProductPrice.Name);
            
            var ids = ToIntValues("ProductId");
            var products = db.Products.Include("Company").Include("Category").Include("Store").Where(x => ids.Contains(x.Id))
                .Select(p => new { Name = p.Name, Category = p.Category.Name, Price = p.Price, Company = p.Company.Name, Store = p.Store.Name }).ToList();
            listView.ItemsSource = products;


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
        public int ProductId { get; set; }
        public string Company { get; set; }
        public string Store { get; set; }
        public string Category { get; set; }
        public int Price { get; set; }
    }
}