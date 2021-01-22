using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;


namespace AssimpSample {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion Atributi

        #region Konstruktori

        public MainWindow() {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Castle"), "untitled.stl", (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e) {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args) {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args) {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args) {
            //m_world.Resize(args.OpenGL, (int)openGLControl.Width, (int)openGLControl.Height);
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }


        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if(!m_world.Animation)//kad nema animacije 
            switch (e.Key) {

                case Key.F4: this.Close(); break;                                               //izlaz
                case Key.I: m_world.RotationX += 5.0f; break;                               // rotacija oko horizontalne ose
                case Key.K: m_world.RotationX -= 5.0f; break;                               // -||- 
                case Key.J: m_world.RotationY += 5.0f; break;                               // rotacija oko vertikalne ose
                case Key.L: m_world.RotationY -= 5.0f; break;                               // -||-
                case Key.Add: m_world.SceneDistance -= 10.0f; break;                        // priblizavanje
                case Key.Subtract: m_world.SceneDistance += 10.0f; break;                   // udaljavanje
                case Key.V: m_world.startAnimation(); break;                                // animacija

            }
        }
        public void enableSliders() { //ENABLE KAD SE ZAVRSI (IZ WORLD.CS SE POZOVE OVO )
            slider.IsEnabled = true;
            slider2.IsEnabled = true;
            slider3.IsEnabled = true;
            c1.IsEnabled = true;
            c2.IsEnabled = true;

        }

        //POMERANJE DESNOG
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (m_world != null)
                m_world.RightWall = (int)e.NewValue;
        }

        //ROTACIAJ LEVOG
        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (m_world != null)
                m_world.LeftWall = (int)e.NewValue;
        }


        //SKALIRANJE STRELE
        private void slider3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (m_world != null)
                m_world.ScaleArrow = (float)e.NewValue;
        }

        //UKLJUCI ILI ISKLJUCI SVETLO
        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            CheckBox c = (CheckBox)sender;
            if (this.openGLControl != null) {
                if (c.IsChecked == true)
                    openGLControl.OpenGL.Enable(OpenGL.GL_LIGHT0);
                else
                    openGLControl.OpenGL.Disable(OpenGL.GL_LIGHT0);

            }
        }
        private void CheckBox_Checked2(object sender, RoutedEventArgs e) {
            CheckBox c = (CheckBox)sender;
            if (this.openGLControl != null) {
                if (c.IsChecked == true)
                    openGLControl.OpenGL.Enable(OpenGL.GL_LIGHT1);
                else
                    openGLControl.OpenGL.Disable(OpenGL.GL_LIGHT1);
            }
        }
    }
}
