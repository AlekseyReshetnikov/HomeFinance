using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;

namespace HomeFinance
{
    class ChartFilter
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public TextField ChartType { get; set; }
        public TextField DebitType { get; set; }
        public Person Person { get; set; }
        public Category Category { get; set; }
        public Notion Notion { get; set; }
        public Asset Asset { get; set; }   // навигационное свойство
    }

    // Универсальный класс - использую и как строки в комбобоксах, так и для отображения результатов в графиках
    class TextField
    {
        public int Tag { get; set; }
        public string Name { get; set; }

        public TextField()
        { }

        public TextField(int tag, string name)
        {
            Tag = tag;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class MainWindow : Window
    {
        public void Filter_Settings()
        {
            CurrentFilter = new ChartFilter();
            // Если есть движение - установим фильтр за последний месяц
            Motion M = db.GetLastMotion();
            if (M != null)
            {
                CurrentFilter.DateFrom = M.Date.AddMonths(-1);
                CurrentFilter.DateTo = M.Date;
                CurrentFilter.Person = M.Person;
            }
            else
            {
                CurrentFilter.DateFrom = DateTime.Now.AddMonths(-1);
                CurrentFilter.DateTo = DateTime.Now;
            }
            
            TextField T = new TextField(1, "Столбиковая диаграмма");
            cmbChart.Items.Add(T);
            cmbChart.Items.Add(new TextField(2, "Секторная диаграмма"));
            CurrentFilter.ChartType = T;

            List<TextField> DebitList = new List<TextField> {
                new TextField(1, "Расход"),
                new TextField(2, "Доход")
            };
            cmbDebitF.ItemsSource = DebitList;
            cmbDebitF.SelectedIndex = 0;

            gridFilter.DataContext = CurrentFilter;
        }

        private void cmbDebitF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnShowChart_Click(object sender, RoutedEventArgs e)
        {
            // TestChart();
            ShowChart();
        }

        private void ShowChart()
        {
            bool Debit = false;
            if (CurrentFilter.DebitType != null)
                Debit = (CurrentFilter.DebitType.Tag == 2);
            var ResList = db.Motions
                .Where(p => p.Date >= CurrentFilter.DateFrom && p.Date <= CurrentFilter.DateTo && p.Debit == Debit)
                .GroupBy(a => a.Category)
                .Select(b => new TextField() { Name = b.Key.Name, Tag = b.Sum(c => c.Summa)});

            Series Ch;
            switch (CurrentFilter.ChartType.Tag)
            {
                case 1:
                    ColumnSeries Ch1 = new ColumnSeries();
                    Ch1.IndependentValuePath = "Name";
                    Ch1.DependentValuePath = "Tag";
                    Ch1.ItemsSource = ResList;
                    Ch = Ch1;
                    break;
                default:
                    PieSeries P = new PieSeries();
                    P.IndependentValuePath = "Name";
                    P.DependentValuePath = "Tag";
                    P.ItemsSource = ResList;
                    Ch = P;
                    break;
            }

            Ch.Title = "Расходы";
            if (Charts.Series.Count > 0)  // С предыдущего раза осталось - удалим
                Charts.Series.RemoveAt(0);
            Charts.Series.Add(Ch);

            //var A = db.Motions.Where(p => 
            //    p.Date >= CurrentFilter.DateFrom && p.Date <= CurrentFilter.DateTo && p.Debit==Debit).Select( 
            //        a => new { Category = a.Category,
            //                   Summa = a.Summa });
        }

        private void TestChart()
        {
            DateTime CurTime = DateTime.Now;
            Random Read = new Random();
            List<ToolkPoint> Chardata = new List<ToolkPoint>()
            {
        new ToolkPoint { Value = Read.Next(5,47), Time = CurTime + TimeSpan.FromSeconds(10) },
        new ToolkPoint { Value = Read.Next(5,47), Time = CurTime + TimeSpan.FromSeconds(20) },
        new ToolkPoint { Value = Read.Next(5,47), Time = CurTime + TimeSpan.FromSeconds(30) },
        new ToolkPoint { Value = Read.Next(5,47), Time = CurTime + TimeSpan.FromSeconds(40) },
        new ToolkPoint { Value = Read.Next(5,47), Time = CurTime + TimeSpan.FromSeconds(50) },
        new ToolkPoint { Value = Read.Next(5,47), Time = CurTime + TimeSpan.FromSeconds(60) }
            };

            //LineSeries Ch = new LineSeries();
            Series Ch;
            int k = Read.Next(1, 4);
            switch (k)
            {
                case 1:
                    // AreaSeries Ch1 = new AreaSeries();
                    ColumnSeries Ch1 = new ColumnSeries();
                    Ch1.IndependentValuePath = "Time";
                    Ch1.DependentValuePath = "Value";
                    Ch1.ItemsSource = Chardata;
                    Ch = Ch1;
                    break;
                case 2:
                    ScatterSeries B = new ScatterSeries();
                    // BarSeries B = new BarSeries();
                    B.IndependentValuePath = "Time";
                    B.DependentValuePath = "Value";
                    B.ItemsSource = Chardata;
                    Ch = B;
                    break;
                case 3:
                    PieSeries P = new PieSeries();
                    P.IndependentValuePath = "Time";
                    P.DependentValuePath = "Value";
                    P.ItemsSource = Chardata;
                    Ch = P;
                    break;
                default:
                    LineSeries Ch2 = new LineSeries();
                    Ch2.IndependentValuePath = "Time";
                    Ch2.DependentValuePath = "Value";
                    Ch2.ItemsSource = Chardata;
                    Ch = Ch2;
                    break;
            }

            //AreaSeries Ch = new AreaSeries();

            Ch.Title = "Расходы";
            if (Charts.Series.Count > 0)  // С предыдущего раза осталось - удалим
                Charts.Series.RemoveAt(0);
            Charts.Series.Add(Ch);

            //< charting:LineSeries Name = "ChartOne" DependentValuePath = "Value" IndependentValuePath = "Time" Title = "График1" />

            //       < charting:BarSeries Name = "ChartTwo" DependentValuePath = "Value" IndependentValuePath = "Time" Title = "График2" />

            //              < charting:AreaSeries Name = "Chart3" DependentValuePath = "Value" IndependentValuePath = "Time" Title = "График3" />

            //                     < charting:PieSeries Name = "Chart4" DependentValuePath = "Value" IndependentValuePath = "Time" Title = "График4" />

            // ChartOne.ItemsSource = Chardata;
            // <charting:LineSeries Name="ChartOne" DependentValuePath="Value" IndependentValuePath="Time" Title="График1"/>
            //< charting:Chart.Axes >

            //                             < charting:LinearAxis Orientation = "Y" Minimum = "0" Maximum = "50" Title = "Значение" />

            //                                    < charting:DateTimeAxis Orientation = "X" Title = "Время" ShowGridLines = "True" />

            //                                     </ charting:Chart.Axes >
            // ChartTwo.ItemsSource = Chardata;
            //Chart3.ItemsSource = Chardata;
            //Chart4.ItemsSource = Chardata;
        }

    }
}