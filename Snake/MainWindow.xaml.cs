using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
    public partial class MainWindow : Window
    {
        private const int TailleCase = 20;
        private List<UIElement> serpent;

        private UIElement pomme;

        private Point direction = new Point(TailleCase, 0);
        private DispatcherTimer timer;
        private Random rand = new Random();
        private int score = 0;
        private int meilleurScore = 0;
        private string fichierScore = "best_score.txt";
        private SoundPlayer sonManger;

        public MainWindow()
        {
            InitializeComponent();
            ChargerMeilleurScore();
            InitialiserJeu();
        }

        private void InitialiserJeu()
        {
            GameCanvas.Children.Clear();
            serpent = new List<UIElement>();
            AjouterSegment(new Point(100, 100)); // Tête du serpent
            PlacerPomme();

            score = 0;
            ScoreLabel.Text = $"Score: {score}";
            BestScoreLabel.Text = $"Meilleur: {meilleurScore}";

            // Supprime tout ancien timer avant d'en créer un nouveau
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= MiseAJour;
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(150);
            timer.Tick += MiseAJour;
            timer.Start();

            this.KeyDown += GestionTouches;
        }


        private void MiseAJour(object sender, EventArgs e)
        {
            Point nouvelleTete = new Point(Canvas.GetLeft(serpent[0]) + direction.X,
                                           Canvas.GetTop(serpent[0]) + direction.Y);

            // Vérifie si le serpent touche le mur ou lui-même
            if (nouvelleTete.X < 0 || nouvelleTete.X >= GameCanvas.ActualWidth ||
                nouvelleTete.Y < 0 || nouvelleTete.Y >= GameCanvas.ActualHeight ||
                CollisionSerpent(nouvelleTete))
            {
                timer.Stop(); // Arrête le timer
                timer.Tick -= MiseAJour; // Retire l'événement pour éviter qu'il ne se répète

                if (score > meilleurScore)
                {
                    meilleurScore = score;
                    File.WriteAllText(fichierScore, meilleurScore.ToString());
                }

                MessageBox.Show($"Game Over !\nScore: {score}");

                InitialiserJeu(); // Relance le jeu
                return;
            }

            AjouterSegment(nouvelleTete);

            if (nouvelleTete.X == Canvas.GetLeft(pomme) && nouvelleTete.Y == Canvas.GetTop(pomme))
            {
                score += 10;
                ScoreLabel.Text = $"Score: {score}";
                PlacerPomme();

                // Augmenter la vitesse progressivement
                if (timer.Interval.TotalMilliseconds > 200)
                {
                    timer.Interval = TimeSpan.FromMilliseconds(timer.Interval.TotalMilliseconds - 5);
                }
            }
            else
            {
                GameCanvas.Children.Remove(serpent[serpent.Count - 1]);
                serpent.RemoveAt(serpent.Count - 1);
            }
        }

        private void AjouterSegment(Point position, bool estTete = false)
        {
            Canvas segment = new Canvas
            {
                Width = TailleCase, // Corps plus fin
                Height = TailleCase
            };

            // Corps : Un ovale fin pour un effet plus naturel
            Ellipse corps = new Ellipse
            {
                Width = TailleCase * 0.8,
                Height = TailleCase * 0.6,
                Fill = Brushes.DarkGreen
            };

            Canvas.SetLeft(corps, TailleCase * 0.1);
            Canvas.SetTop(corps, TailleCase * 0.2);
            segment.Children.Add(corps);

            // Si c'est la tête, on ajoute des détails réalistes
            if (estTete)
            {
                // Forme triangulaire de la tête (Polygone)
                Polygon tete = new Polygon
                {
                    Fill = Brushes.DarkGreen,
                    Points = new PointCollection
            {
                new Point(TailleCase * 0.2, TailleCase * 0.8), // Bas gauche
                new Point(TailleCase * 0.8, TailleCase * 0.8), // Bas droit
                new Point(TailleCase * 0.5, TailleCase * 0.2)  // Pointe en haut
            }
                };
                segment.Children.Add(tete);

                // Yeux (ovales inclinés pour un regard perçant)
                Ellipse oeilGauche = new Ellipse
                {
                    Width = TailleCase * 0.2,
                    Height = TailleCase * 0.3,
                    Fill = Brushes.White
                };
                Ellipse pupilleGauche = new Ellipse
                {
                    Width = TailleCase * 0.1,
                    Height = TailleCase * 0.15,
                    Fill = Brushes.Black
                };

                Ellipse oeilDroit = new Ellipse
                {
                    Width = TailleCase * 0.2,
                    Height = TailleCase * 0.3,
                    Fill = Brushes.White
                };
                Ellipse pupilleDroite = new Ellipse
                {
                    Width = TailleCase * 0.1,
                    Height = TailleCase * 0.15,
                    Fill = Brushes.Black
                };

                // Position des yeux légèrement inclinés
                Canvas.SetLeft(oeilGauche, TailleCase * 0.3);
                Canvas.SetTop(oeilGauche, TailleCase * 0.3);
                Canvas.SetLeft(pupilleGauche, TailleCase * 0.35);
                Canvas.SetTop(pupilleGauche, TailleCase * 0.35);

                Canvas.SetLeft(oeilDroit, TailleCase * 0.55);
                Canvas.SetTop(oeilDroit, TailleCase * 0.3);
                Canvas.SetLeft(pupilleDroite, TailleCase * 0.6);
                Canvas.SetTop(pupilleDroite, TailleCase * 0.35);

                // Langue fine et fourchue
                Line langue1 = new Line
                {
                    X1 = TailleCase * 0.5,
                    Y1 = TailleCase * 0.8,
                    X2 = TailleCase * 0.45,
                    Y2 = TailleCase * 1.1,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1
                };

                Line langue2 = new Line
                {
                    X1 = TailleCase * 0.5,
                    Y1 = TailleCase * 0.8,
                    X2 = TailleCase * 0.55,
                    Y2 = TailleCase * 1.1,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1
                };

                // Ajout des éléments à la tête
                segment.Children.Add(oeilGauche);
                segment.Children.Add(pupilleGauche);
                segment.Children.Add(oeilDroit);
                segment.Children.Add(pupilleDroite);
                segment.Children.Add(langue1);
                segment.Children.Add(langue2);
            }

            Canvas.SetLeft(segment, position.X);
            Canvas.SetTop(segment, position.Y);
            GameCanvas.Children.Insert(0, segment);
            serpent.Insert(0, segment);
        }





        private void PlacerPomme()
        {
            if (pomme != null) GameCanvas.Children.Remove(pomme);

            int taillePixel = TailleCase / 6; // On divise la taille de la pomme en 6x6 pixels
            Grid pommeGrid = new Grid
            {
                Width = TailleCase,
                Height = TailleCase
            };

            // Ajout des colonnes et lignes pour le Grid
            for (int i = 0; i < 6; i++)
            {
                pommeGrid.ColumnDefinitions.Add(new ColumnDefinition());
                pommeGrid.RowDefinitions.Add(new RowDefinition());
            }

            // Matrice pour dessiner une pomme plus arrondie
            int[,] pommeMatrice = new int[,]
            {
        { 0, 0, 2, 2, 0, 0 }, // 0 = vide, 1 = rouge (pomme), 2 = vert (tige)
        { 0, 1, 1, 1, 1, 0 },
        { 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1 },
        { 0, 1, 1, 1, 1, 0 },
        { 0, 0, 1, 1, 0, 0 }
            };

            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    if (pommeMatrice[y, x] != 0) // Si ce n'est pas un espace vide
                    {
                        Rectangle pixel = new Rectangle
                        {
                            Width = taillePixel,
                            Height = taillePixel,
                            Fill = pommeMatrice[y, x] == 1 ? Brushes.Red : Brushes.Green, // Rouge pour la pomme, vert pour la tige
                            RadiusX = 2, // Coins légèrement arrondis pour un meilleur effet
                            RadiusY = 2
                        };

                        Grid.SetRow(pixel, y);
                        Grid.SetColumn(pixel, x);
                        pommeGrid.Children.Add(pixel);
                    }
                }
            }

            Point position;
            do
            {
                position = new Point(
                    rand.Next(0, (int)(GameCanvas.ActualWidth / TailleCase)) * TailleCase,
                    rand.Next(0, (int)(GameCanvas.ActualHeight / TailleCase)) * TailleCase
                );
            } while (CollisionSerpent(position));

            Canvas.SetLeft(pommeGrid, position.X);
            Canvas.SetTop(pommeGrid, position.Y);
            GameCanvas.Children.Add(pommeGrid);

            pomme = pommeGrid; // Stocker la pomme sous forme de Grid
        }


        private bool CollisionSerpent(Point position)
        {
            foreach (var segment in serpent)
            {
                if (Canvas.GetLeft(segment) == position.X && Canvas.GetTop(segment) == position.Y)
                    return true;
            }
            return false;
        }

        private void GestionTouches(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && direction.Y == 0) direction = new Point(0, -TailleCase);
            if (e.Key == Key.Down && direction.Y == 0) direction = new Point(0, TailleCase);
            if (e.Key == Key.Left && direction.X == 0) direction = new Point(-TailleCase, 0);
            if (e.Key == Key.Right && direction.X == 0) direction = new Point(TailleCase, 0);
        }

        private void ChargerMeilleurScore()
        {
            if (File.Exists(fichierScore))
            {
                int.TryParse(File.ReadAllText(fichierScore), out meilleurScore);
            }
        }
    }
}
