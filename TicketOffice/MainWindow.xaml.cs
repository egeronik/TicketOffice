using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mime;
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
using Microsoft.Win32;
using MongoDB.Bson;
using MongoDB.Driver;


namespace TicketOffice
{
    public class DataObject
    {
        public ObjectId Id { get; set; }
    }
    
    public class Station : DataObject
    {
        public string Name { get; set; } = "";
        public int Railways { get; set; }
        public int Staff{ get; set; }
    }

    public class Post : DataObject
    {
        public string Cashier { get; set; } = "";
        public string Station { get; set; } = "";

    }

    public class Ticket : DataObject
    {
        public string Cashier { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";
    }

    public class Route  : DataObject
    {
        public string Code { get; set; } = "";
        public string Path { get; set; } = "";
    }

    public class Train  : DataObject
    {
        public string Driver { get; set; } = "";
        public int Length { get; set; }
        public string PathCode { get; set; } = "";
    }


    public class DbManager
    {
        private readonly IMongoDatabase _database;

        public DbManager(string databaseUri, List<string> tables)
        {
            var client = new MongoClient(databaseUri);
            _database = client.GetDatabase("TicketOffice");
            var collections = _database.ListCollectionNames().ToList();
            foreach (var name in tables.Where(name => !collections.Contains(name)))
            {
                _database.CreateCollection(name);
            }
            Console.Out.WriteLine(_database);
        }

        public async Task<List<string>> GetCollections()
        {
            using var cursor = await _database.ListCollectionNamesAsync();
            return cursor.ToList();
        }

        public IAsyncCursor<BsonDocument> GetCollection(string collectionName)
        {
            return _database.GetCollection<BsonDocument>(collectionName).FindSync(new BsonDocument());
        }

        public List<Station> GetStations()
        {
            return _database.GetCollection<Station>("Stations").FindSync(new BsonDocument()).ToList();
        }

        public void InsertStation(Station data)
        {
            _database.GetCollection<Station>("Stations").InsertOne(data);
        }

        public void RemoveFromCollection(String collectionName, ObjectId objectId )
        {
            switch (collectionName)
            {
                case "Stations":
                    _database.GetCollection<Station>(collectionName).DeleteOne(a => a.Id == objectId);
                    break;
                case "Posts":
                    _database.GetCollection<Post>(collectionName).DeleteOne(a => a.Id == objectId);
                    break;
                case "Trains":
                    _database.GetCollection<Train>(collectionName).DeleteOne(a => a.Id == objectId);
                    break;
                case "Paths":
                    _database.GetCollection<Route>(collectionName).DeleteOne(a => a.Id == objectId);
                    break;
                case "Tickets":
                    _database.GetCollection<Ticket>(collectionName).DeleteOne(a => a.Id == objectId);
                    break;
                
            }
        }
        
        public List<Post> GetPosts()
        {
            return _database.GetCollection<Post>("Posts").FindSync(new BsonDocument()).ToList();
        }

        public void InsertPost(Post data)
        {
            _database.GetCollection<Post>("Posts").InsertOne(data);
        }
        
        public List<Ticket> GetTickets()
        {
            return _database.GetCollection<Ticket>("Tickets").FindSync(new BsonDocument()).ToList();
        }

        public void InsertTicket(Ticket data)
        {
            _database.GetCollection<Ticket>("Tickets").InsertOne(data);
        }
        
        public List<Train> GetTrains()
        {
            return _database.GetCollection<Train>("Trains").FindSync(new BsonDocument()).ToList();
        }

        public void InsertTrain(Train data)
        {
            _database.GetCollection<Train>("Trains").InsertOne(data);
        }
        
        public List<Route> GetRoutes()
        {
            return _database.GetCollection<Route>("Paths").FindSync(new BsonDocument()).ToList();
        }

        public void InsertRoute(Route data)
        {
            _database.GetCollection<Route>("Paths").InsertOne(data);
        }
    }


    public partial class MainWindow : Window
    {
        private readonly DbManager _dbManager;
        private readonly List<string> _tables = new List<string>() {"Stations","Posts","Tickets","Trains","Paths"};


        public MainWindow()
        {
            InitializeComponent();
            _dbManager = new DbManager("mongodb://localhost:27017", _tables);
            TypeBox.ItemsSource = _tables;
            TypeBox.SelectedIndex = 0;
            UpdateWindow();

        }
        
        
        
        private void TypeBox_OnDropDownClosed(object? sender, EventArgs e)
        {
            if (TypeBox.Text == "") return;
            UpdateWindow();
        }
        
        private void UpdateWindow()
        {
            
            GenerateBottomPanel(TypeBox.Text);
            GenerateTable(TypeBox.Text);
        }

        private void GenerateTable(string typeBoxText)
        {
            switch (typeBoxText)
            {
                case "Stations":
                    Table.ItemsSource = _dbManager.GetStations();
                    
                    break;
                case "Posts":
                    Table.ItemsSource = _dbManager.GetPosts();
                    break;
                case "Tickets":
                    Table.ItemsSource = _dbManager.GetTickets();
                    break;
                case "Trains":
                    Table.ItemsSource = _dbManager.GetTrains();
                    break;
                case "Paths":
                    Table.ItemsSource = _dbManager.GetRoutes();
                    break;
            }
            


            // Console.Out.WriteLine(dict);
        }


        private void GenerateBottomPanel(string tableName)
        {
            BottomPanel.Children.Clear();
            switch (tableName)
            {
                case "Stations":
                    BottomPanel.Children.Add(new TextBlock() { Text = "Name" });
                    var nameBox = new TextBox ();
                    BottomPanel.Children.Add(nameBox);
                    BottomPanel.Children.Add(new TextBlock() { Text = "Railway count" });
                    var railways = new TextBox() ;
                    BottomPanel.Children.Add(railways);
                    BottomPanel.Children.Add(new TextBlock() { Text = "Staff count" });
                    var staff = new TextBox();
                    BottomPanel.Children.Add(staff);
                    break;
                case "Posts":
                    BottomPanel.Children.Add(new TextBlock() { Text = "Cashier" });
                    var cashier = new TextBox();
                    BottomPanel.Children.Add(cashier);
                    var stationsList = _dbManager.GetStations().Select(c => c.Name).ToList();
                    var stations = new ComboBox();
                    if (stationsList.Any())
                    {
                        stations.ItemsSource = stationsList;
                    }

                    BottomPanel.Children.Add(stations);

                    break;
                case "Tickets":
                    BottomPanel.Children.Add(new TextBlock() { Text = "Cashier" });
                    var postsList = _dbManager.GetPosts().Select(c => c.Cashier).ToList();
                    var posts = new ComboBox();
                    if (postsList.Any()) posts.ItemsSource = postsList;
                    BottomPanel.Children.Add(posts);
                    var destList = _dbManager.GetStations().Select(c => c.Name).ToList();
                    var from = new ComboBox();
                    var to = new ComboBox();
                    if (destList.Any())
                    {
                        from.ItemsSource = destList;
                        to.ItemsSource = destList;
                    }

                    BottomPanel.Children.Add(new TextBlock() { Text = "From"});
                    BottomPanel.Children.Add(from);
                    BottomPanel.Children.Add(new TextBlock() { Text = "To"});
                    BottomPanel.Children.Add(to);
                    break;
                case "Trains":
                    BottomPanel.Children.Add(new TextBlock() { Text = "Driver" });
                    var driver = new TextBox();
                    BottomPanel.Children.Add(driver);
                    BottomPanel.Children.Add(new TextBlock() { Text = "Length" });
                    var length = new TextBox();
                    BottomPanel.Children.Add(length);
                    BottomPanel.Children.Add(new TextBlock() { Text = "Path" });
                    var pathList = _dbManager.GetRoutes().Select(c => c.Code).ToList();
                    var route = new ComboBox();
                    if (pathList.Any()) route.ItemsSource = pathList;
                    BottomPanel.Children.Add(route);
                    
                    break;
                case "Paths":
                    BottomPanel.Children.Add(new TextBlock() { Text = "Code" });
                    var code = new TextBox();
                    BottomPanel.Children.Add(code);
                    BottomPanel.Children.Add(new TextBlock() { Text = "Path" });
                    var path = new TextBox();
                    BottomPanel.Children.Add(path);
                    break;
            }

                    Button button = new Button() { Content = "Add" };
            button.Click += AddButton;
            BottomPanel.Children.Add(button);
        }
        
        private void AddButton(object sender, RoutedEventArgs e)
        {
            var children = BottomPanel.Children;
            var textBoxes = children.OfType<TextBox>().ToList();
            var comboBoxes = children.OfType<ComboBox>().ToList();
            switch (TypeBox.Text)
            {
                case "Stations":
                    Station station = new Station();
                    station.Name = textBoxes[0].Text;
                    station.Railways = int.TryParse(textBoxes[1].Text, out var railways) ? railways : 0;
                    station.Staff = int.TryParse(textBoxes[2].Text, out var staff) ? staff : 0;
                    _dbManager.InsertStation(station);
                    break;
                case "Posts":
                    Post post = new Post();
                    post.Cashier = textBoxes[0].Text;
                    post.Station = comboBoxes[0].Text;
                    _dbManager.InsertPost(post);
                    break;
                case "Tickets":
                    Ticket ticket = new Ticket();
                    ticket.Cashier = comboBoxes[0].Text;
                    ticket.From = comboBoxes[1].Text;
                    ticket.To = comboBoxes[2].Text;
                    _dbManager.InsertTicket(ticket);
                    break;
                case "Trains":
                    Train train = new Train();
                    train.Driver = textBoxes[0].Text;
                    train.Length = int.TryParse(textBoxes[1].Text, out var length) ? length : 0;
                    train.PathCode = comboBoxes[0].Text;
                    _dbManager.InsertTrain(train);
                    break;
                case "Paths":
                    Route route = new Route();
                    route.Code = textBoxes[0].Text;
                    route.Path = textBoxes[1].Text;
                    _dbManager.InsertRoute(route);
                    break;
            }       
            UpdateWindow();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dbManager.RemoveFromCollection(TypeBox.Text, ((TicketOffice.DataObject)((Button)e.Source).DataContext).Id);
                UpdateWindow();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            
        }
    }
    
}