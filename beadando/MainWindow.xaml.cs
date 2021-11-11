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
using System.Windows.Media.Animation;
using System.Threading;
using System.ComponentModel;
using System.Windows.Threading;

namespace beadando
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int korongSzama = 12;
        const double korongSzelesseg = 20;
        const double korongMagassag = 40;
        List<int>[] list = new List<int>[3];
        List<Canvas>[] canList = new List<Canvas>[3];
        int aktLista = 0;
        double[] pozicio = new double[3];
        delegate void mozgTorolLista(int f);
        mozgTorolLista[] mozgTorol;
        public double korongSebesseg = 0.01;
        public MainWindow()
        {
            InitializeComponent();
            //a mozgatott elemeket törötljük a listából 
            mozgTorol = new mozgTorolLista[2];
            mozgTorol[0] = new mozgTorolLista(minMozgatas);
            mozgTorol[1] = new mozgTorolLista(maxMozgatas);
            //a korongok listáját 3 listára bontjuk szét(kiinduló állapot~a, segéd állapot~b, és cél állapot ~c)
            list[0] = new List<int>(5);
            list[1] = new List<int>(5);
            list[2] = new List<int>(5);
            //a korongok CANVAS listáját -||-    
            canList[0] = new List<Canvas>();
            canList[1] = new List<Canvas>();
            canList[2] = new List<Canvas>();
            formInicializalas();
        }
        /// <summary>
        /// a kezdőállapot kirajzolása(kezdetben 12 korong) 
        /// </summary>
        void formInicializalas()
        {
            canvas1.Children.Clear();
            foreach (var v in list)
            {
                v.Clear();
            }
            foreach (var v in canList)
            {
                v.Clear();
            }
            for (int i = 0; i < korongSzama; i++) 
            {
                list[0].Add(i);
            }
            aktLista = 0;
            double szelesseg_3 = canvas1.Width / 3;
            //aktuális pozició a formon (a, b vagy c állapot)
            for (int i = 0; i < 3; i++)
            {
                pozicio[i] = i * szelesseg_3;
            }
     
            SolidColorBrush scb = new SolidColorBrush();
            scb.Color = Color.FromArgb(255, 138, 150, 140);
            //korongok kirajzolása
            for (int i = 0; i < korongSzama; i++)
            {
                Canvas cn = new Canvas();
                cn.Width = szelesseg_3 - (korongSzama - i) * korongSzelesseg;
                cn.Height = korongMagassag + 20;
                Rectangle rc = new Rectangle();
                rc.Fill = scb;
                rc.Width = cn.Width - 8;
                rc.Height = korongMagassag / 2;
                Canvas.SetZIndex(cn, korongSzama - i);
                Ellipse el = new Ellipse();
                el.Fill = scb;
                el.Width = cn.Width;
                el.Height = cn.Height / 2;
                Ellipse el1 = new Ellipse();
                el1.Fill = scb;
                el1.Width = cn.Width;
                el1.Height = cn.Height / 2;
                Canvas.SetTop(el1, korongMagassag - korongMagassag / 2);
                //ez a szélességi kereső körfigyelés nélkül
                Canvas.SetTop(rc, korongMagassag - rc.Height - rc.Height / 2 + 10);
                Canvas.SetLeft(rc, 4);
                el.StrokeThickness = 2;
                el.Stroke = Brushes.Black;
                el1.StrokeThickness = 2;
                el1.Stroke = Brushes.Black;
                cn.Children.Add(el1);
                cn.Children.Add(rc);
                cn.Children.Add(el);
                Canvas.SetLeft(cn, (szelesseg_3 - cn.Width) / 2);
                Canvas.SetTop(cn, i * korongMagassag);
                canvas1.Children.Add(cn);
                canList[0].Add(cn);
            }
        }
        /// <summary>
        /// kattintás hatására elkezdi a hanoi problémát megoldani
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            formInicializalas();
            ThreadPool.QueueUserWorkItem(new WaitCallback(start));
            //button1.IsEnabled = false;
        }
        /// <summary>
        /// start indítja el a korongok mozgását
        /// </summary>
        /// <param name="stateInfo"></param>
        private void start(Object stateInfo)
        {
            elemMozgatas(0);
        }
        /// <summary>
        /// a startban értelmezett korongok mozgását jelenti
        /// </summary>
        /// <param name="f"></param>
        private void elemMozgatas(int f)
        {
            if (list[2].Count == korongSzama || list[1].Count == korongSzama)
            {
                return;
            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal,
            new OnAnimateMove(mozgTorol[f]), f);
        }

        delegate void OnAnimateMove(int f);
        
        /// <summary>
        /// legkisebb elem mozgatása
        /// </summary>
        /// <param name="f"></param>
        private void minMozgatas(int f)
        {
            int i = list[aktLista][0];
            list[aktLista].RemoveAt(0);
            Canvas cn = canList[aktLista][0];
            canList[aktLista].RemoveAt(0);

            int tavolsag = aktLista + 1 >= list.Length ? 0 : aktLista + 1;
            list[tavolsag].Insert(0, i);
            canList[tavolsag].Insert(0, cn);

            aktLista++;
            if (aktLista == list.Length)
            {
                aktLista = 0;
            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal,
            new OnAnimate(this.drawAllapot), f, cn, pozicio[tavolsag], (korongSzama - canList[tavolsag].Count) * korongMagassag);
        }
        /// <summary>
        /// legnagyobb elem mozgatása
        /// </summary>
        /// <param name="f"></param>
        private void maxMozgatas(int f)
        {
            int honnan = -1;
            int hova = -1;
            int minErtek = int.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                if (list[i].Count == 0) {
                    hova = i; 
                    continue; }
                if (i == aktLista) {
                    continue; 
                    }
                if (minErtek > list[i][0]) 
                { 
                    minErtek = list[i][0]; 
                    honnan = i; 
                }
            }
            hova = GetIndex(honnan, aktLista);
            int v = list[honnan][0];
            list[hova].Insert(0, v);
            list[honnan].RemoveAt(0);
            Canvas cn = canList[honnan][0];
            canList[honnan].RemoveAt(0);
            canList[hova].Insert(0, cn);
            double y = (korongSzama - canList[hova].Count) * korongMagassag - Canvas.GetTop(cn);
            drawAllapot(f, cn, pozicio[hova], y);
        }
        /// <summary>
        /// indexet ad meg amit rajzoláshoz fontos
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private int GetIndex(int i, int j) 
        {
            return 3 - (i + j); 
        }

        delegate void OnAnimate(int f, Canvas cn, double x, double y);

        static DependencyProperty dprop = DependencyProperty.RegisterAttached(
            "temp", typeof(int), typeof(int));
        static DependencyProperty CanvasProp = DependencyProperty.RegisterAttached(
            "ccc", typeof(Canvas), typeof(Canvas));

        public int sum = 0, kSzamaSeged = 0;
        /// <summary>
        /// aktuálist kirajzolás canvas
        /// </summary>
        /// <param name="f"></param>
        /// <param name="cn"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void drawAllapot(int f, Canvas cn, double x, double y)
        {
            sum += 1;
            korongLepesek.Content = sum.ToString() +". lépés" ;
            Canvas.SetZIndex(cn, Canvas.GetZIndex(cn) * 10);
            DoubleAnimation myAnimation = new DoubleAnimation();
            myAnimation.Duration = new Duration(TimeSpan.FromSeconds(korongSebesseg));
            myAnimation.From = cn.RenderTransformOrigin.X;
            myAnimation.To = x;
            DoubleAnimation day = new DoubleAnimation();
            day.Duration = new Duration(TimeSpan.FromSeconds(korongSebesseg));
            day.From = cn.RenderTransformOrigin.Y;
            day.To = y;
            TranslateTransform tf = new TranslateTransform();
            cn.RenderTransform = tf;
            tf.SetValue(Canvas.LeftProperty, x);
            tf.SetValue(Canvas.TopProperty, y);
            tf.SetValue(dprop, f);
            tf.SetValue(CanvasProp, cn);

            tf.Changed += new EventHandler(tf_Changed);
            tf.BeginAnimation(TranslateTransform.XProperty, myAnimation);
            tf.BeginAnimation(TranslateTransform.YProperty, day);
        }

        void tf_Changed(object sender, EventArgs e)
        {
            TranslateTransform tf = (TranslateTransform)sender;
            double x = (double)tf.GetValue(Canvas.LeftProperty);
            double y = (double)tf.GetValue(Canvas.TopProperty);
            if (tf.X == x && tf.Y == y)
            {
                Canvas cn = (Canvas)tf.GetValue(CanvasProp);
                Canvas.SetZIndex(cn, Canvas.GetZIndex(cn) / 10);
                cn.RenderTransformOrigin = new Point(x, y);
                elemMozgatas(1 - (int)tf.GetValue(dprop));
            }
        }

        private void radBtn3_Checked(object sender, RoutedEventArgs e)
        {
            korongSzama = Convert.ToInt16(lab12.Content);
            sum = 0;
        }

        private void radBtn2_Checked(object sender, RoutedEventArgs e)
        {
            korongSzama = Convert.ToInt16(lab5.Content);
            sum = 0;
        }

        private void radBtn1_Checked_1(object sender, RoutedEventArgs e)
        {
            korongSzama = Convert.ToInt16(lab3.Content);
            sum = 0;
        }

        private void korongSzamaLab_Click(object sender, RoutedEventArgs e)
        {
            korongSebesseg = Convert.ToDouble(textB.Text);
        }       
    }
}