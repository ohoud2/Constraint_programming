using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ConstraintProgramming.Entities.Solver {
    public class ProductRecommender {
        private SystemContext db = new SystemContext();
        private SolverContext _context;
        private DecisionBinding[] _configurationDecisions;
        private Dictionary<string, DecisionBinding> _decisionsByName;
        private UserFilter _filter;
        public ProductRecommender(UserFilter filter) {
            _filter = filter;
            _context = SolverContext.GetContext();
        }

        private void CreateModelAndBindToDbData() {
            try {
                Model productModel = _context.CreateModel();
                productModel.Name = "Product Rec";
                Domain productIds = Domain.Set(db.Products.Select(x => x.Id).ToArray());
                Domain companies = Domain.Set(db.Companies.Select(x => x.Id).ToArray());
                Domain stores = Domain.Set(db.Stores.Select(x => x.Id).ToArray());
                Domain categories = Domain.Set(db.Categories.Select(x => x.Id).ToArray());

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
                                                   Company = eleRow.Company.Id,
                                                   Store = eleRow.Store.Id,
                                                   Category = eleRow.Category.Id,
                                                   Price = eleRow.Price
                                               });
                table.SetBinding(_tableData, new[] { "ProductId", "Company", "Store", "Category", "Price" });

                productModel.AddTuple(table);
                productModel.AddConstraint("Const", Model.Equal(new Term[] { productId, company, store, cateogry, price }, table));

                if (_filter.selectedCompanies.Any()) {
                    // Companies Selection
                    var orTerm = new List<Term>();
                    foreach (var s in _filter.selectedCompanies) {
                        orTerm.Add(company == s);
                    }
                    productModel.AddConstraint("companiesSelected", Model.Or(orTerm.ToArray()));
                }

                if (_filter.selectedCategories.Any()) {
                    // Categories Selection
                    var orTerm = new List<Term>();
                    foreach (var s in _filter.selectedCategories) {
                        orTerm.Add(cateogry == s);
                    }
                    productModel.AddConstraint("categoriesSelected", Model.Or(orTerm.ToArray()));
                }

                if (_filter.selectedStores.Any()) {
                    if (_filter.selectedStores.Contains(0)) { // Nearest Location
                        var nearestId = getNearestStoreId(_filter.userX, _filter.userY);
                        _filter.selectedStores = new List<int>(_filter.selectedStores.Union(new int[] { nearestId }));
                        _filter.selectedStores.Remove(0);
                    }
                    // Categories Selection
                    var orTerm = new List<Term>();
                    foreach (var s in _filter.selectedStores) {
                        orTerm.Add(store == s);
                    }
                    productModel.AddConstraint("storesSelected", Model.Or(orTerm.ToArray()));
                }

                // Price Selection
                productModel.AddConstraint("maxPrice", price <= _filter.maxPrice);
                productModel.AddConstraint("minPrice", price >= _filter.minPrice);
            }
            catch (Exception ex) {
                Trace.TraceError(ex.Message);
                _context = null;
            }
        }

        public int getNearestStoreId(double userX, double userY) {
            var coord = new GeoCoordinate(userX, userY);
            var nearest = db.Stores.ToList().Select(x => new { Coordinate = new GeoCoordinate(x.LocationX, x.LocationY) , Id = x.Id})
                                   .OrderBy(x => x.Coordinate.GetDistanceTo(coord))
                                   .First().Id;
            return nearest;
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
        }

        public IEnumerable<ProductView> GetProducts() {
            CreateModelAndBindToDbData();
            InitializeConfigurator();

            DecisionBinding d = _decisionsByName["ProductId"];
            var ids = d.Int32FeasibleValues.ToArray();
            return db.Products.Include("Company").Include("Category").Include("Store").Where(x => ids.Contains(x.Id))
                .Select(p => new ProductView () {
                    Name = p.Name,
                    Category = p.Category.Name,
                    Price = p.Price,
                    Company = p.Company.Name,
                    Store = p.Store.Name }).ToList();
        }


        public class Row {
            public int ProductId { get; set; }
            public int Company { get; set; }
            public int Store { get; set; }
            public int Category { get; set; }
            public int Price { get; set; }
        }
    }


    public class UserFilter {

        public UserFilter() {
            selectedCategories = new List<int>();
            selectedStores = new List<int>();
            selectedCompanies = new List<int>();
        }
        public ICollection<int> selectedCategories { get; set; }
        public ICollection<int> selectedStores { get; set; }
        public ICollection<int> selectedCompanies { get; set; }
        public int minPrice { get; set; }
        public int maxPrice { get; set; }

        public double userX { get; set; }
        public double userY { get; set; }
    }

    public class ProductView {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Price { get; set; }
        public string Company { get; set; }
        public string Store { get; set; }
    }
}
