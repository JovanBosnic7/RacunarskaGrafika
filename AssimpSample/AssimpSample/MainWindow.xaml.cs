using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using SharpGL.SceneGraph;
using Microsoft.Win32;


namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Touareg"), "camera.3ds", (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
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
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F6: this.Close(); break;
                case Key.Up:
                    if (m_world.RotationX - 7.0f < -10.0f)
                        break;
                    else
                    {
                        m_world.RotationX -= 7.0f; break;
                    }
                case Key.Down:
                    if (m_world.RotationX + 7.0f > 40.0f)
                        break;
                    else
                    {
                        m_world.RotationX += 7.0f; break;
                    }
                case Key.Left: 
                    if (m_world.RotationY >= -56) m_world.RotationY -= 7.0f; break;
                case Key.Right: 
                    if (m_world.RotationY <= 56) m_world.RotationY += 7.0f; break;
                case Key.Add: m_world.SceneDistance += 700.0f; break;
                case Key.Subtract: m_world.SceneDistance -= 700.0f; break;
                case Key.M:
                    m_world.stop = 1;
                    if (m_world.m_startAnimation)
                        m_world.m_startAnimation = false;
                    else
                    {
                        m_world.m_startAnimation = true;
                    }
                    widthOfCage.IsEnabled = !m_world.m_startAnimation;
                    cameraRotationSpeedSlider.IsEnabled = !m_world.m_startAnimation;
                    ambientr.IsEnabled = !m_world.m_startAnimation;
                    ambientg.IsEnabled = !m_world.m_startAnimation;
                    ambientb.IsEnabled = !m_world.m_startAnimation;

                    break;

                case Key.F2:
                    OpenFileDialog opfModel = new OpenFileDialog();
                    bool result = (bool)opfModel.ShowDialog();
                    if (result)
                    {

                        try
                        {
                            World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                            m_world.Dispose();
                            m_world = newWorld;
                            m_world.Initialize(openGLControl.OpenGL);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK);
                        }
                    }
                    break;
            }
        }
        private void WidhtOfCage(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                float number = (float)widthOfCage.Value;

                if (number < 160.0f)
                {
                    return;
                }
                else
                {
                    m_world.widthOfCage = number;
                }
            }
        }
        private void cameraSpeed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                m_world.cameraSpeed = (int)cameraRotationSpeedSlider.Value;
            }
        }
        private void ambientColorR(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                m_world.ambientr = (int)ambientr.Value;
            }
        }
        private void ambientColorG(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                m_world.ambientg = (int)ambientg.Value;
            }
        }
        private void ambientColorB(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                m_world.ambientr = (int)ambientb.Value;
            }
        }
    }
}
