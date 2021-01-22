// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using SharpGL.Enumerations;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = -2500.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;


        private enum TextureObjects { Bricks, Concentrate, Rust };
        private int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private string[] m_textureFiles = { "C:/Users/Jovan/Desktop/IV godina/Racunarska grafika/RA1732017/AssimpSample/AssimpSample/images/bricks.jpg",
            "C:/Users/Jovan/Desktop/IV godina/Racunarska grafika/RA1732017/AssimpSample/AssimpSample/images/stone.jpg",
            "C:/Users/Jovan/Desktop/IV godina/Racunarska grafika/RA1732017/AssimpSample/AssimpSample/images/rust.jpg" };
        private uint[] m_textures = null;
        public Boolean m_startAnimation = false;
        private DispatcherTimer animationTimer;
        public float widthOfCage = 160.0f;
        public float cameraSpeed = 3.0f;
        public int stop = 1;
        public float ambientr = 0.15f;
        public float ambientg = 0.15f;
        public float ambientb = 0.15f;
        public bool redLight = true;
        public float cameraRotation = 90.0f;
        public float doorOpen = -2.5f;
        public float doorRotation = 0.0f;
        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            // testiranje dubine 
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            // sakrivanje nevidljivih povrsina 
            gl.Enable(OpenGL.GL_CULL_FACE_MODE);


            //color tracking mehanizam 
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_NORMALIZE);
            gl.Enable(OpenGL.GL_AUTO_NORMAL);


            // Teksture
            // Enable ce omoguciti rad sa teksurama

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            // Stapanje teksture sa materijalom
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            // Ucitavanje slika i kreiranje tekstura
            gl.GenTextures(m_textureCount, m_textures);

            for (int i = 0; i < m_textureCount; i++)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // Rotiramo sliku zbog koordinatnog sistema OpenGL-a
                image.RotateFlip(RotateFlipType.Rotate90FlipX);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljava providnost slike)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //MipMap linearno filtriranje
                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
                // Podesavanje Wrapinga da bude GL_REPEAT po obema osama
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                image.UnlockBits(imageData);
                image.Dispose();
            }
            m_scene.LoadScene();
            m_scene.Initialize();

            animationTimer = new System.Windows.Threading.DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(1);
            animationTimer.Tick += new EventHandler(UpdateAnimation);

        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            if (m_startAnimation)
            {
                animationTimer.Start();
            }
            else
            {
                animationTimer.Stop();

                cameraRotation = 90.0f;
                doorOpen = -2.5f;
            }

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, m_sceneDistance);
            gl.Rotate(50.0f, -45.0f, 0.0f);

            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            //glLookAt metoda za pozicioniranje kamere
            gl.LookAt(0, -50, -300, 0, 0, -1500, 0, 1, 0);

            //iscrtavanje podloge
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.PushMatrix();
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Color(0.2f, 0.2f, 0.2f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Concentrate]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Translate(-300.0f, 0.0f, 0.0f);
            gl.TexCoord(0, 0);
            gl.Vertex(500.0f, -300.0f, -485.0f);
            gl.TexCoord(1, 0);
            gl.Vertex(-500.0f, -300.0f, -485.0f);
            gl.TexCoord(1, 1);
            gl.Vertex(-500.0f, -300.0f, 500.0f);
            gl.TexCoord(0, 1);
            gl.Vertex(500.0f, -300.0f, 500.0f);
            gl.End();
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);


            //iscrtavanje zidova 
            Cube cube = new Cube();
            //Srednji zid
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Bricks]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Color(0.214f, 0.214f, 0.214f);
            gl.Translate(0.0f, 100.0f, -485.0f);
            gl.Scale(500.0f, 400.0f, 10.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);



            // Desni bocni zid
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Bricks]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Color(0.154f, 0.154f, 0.154f);
            gl.Translate(500.0f, 100.0f, 0.0f);
            gl.Rotate(0.0f, 90.0f, 0.0f);
            gl.Scale(500.0f, 400.0f, 10.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            // Levi bocni zid
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Bricks]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Color(0.114f, 0.114f, 0.114f);
            gl.Translate(-500.0f, 100.0f, 0.0f);
            gl.Rotate(0.0f, -90.0f, 0.0f);
            gl.Scale(500.0f, 400.0f, 10.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);


            //Srednji zid
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Bricks]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Color(0.214f, 0.214f, 0.214f);
            gl.Translate(0.0f, 100.0f, 490.0f);
            gl.Scale(500.0f, 400.0f, 10.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);


            //iscrtavanje kaveza
            Cylinder cylinder = new Cylinder();
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_CULL_FACE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Rust]);
            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Rust]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            cylinder.NormalGeneration = Normals.Smooth;
            cylinder.NormalOrientation = Orientation.Outside;
            gl.Color(0.235f, 0.055f, 0.055f);
            gl.Translate(0.0f, -300.0f, 0.0f);
            cylinder.CreateInContext(gl);
            cylinder.TextureCoords = true;
            gl.Scale(widthOfCage, 300.0f, 300.0f);
            cylinder.TopRadius = 1;
            gl.Rotate(-90.0f, 0.0f, 0.0f);
            gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
            gl.LineWidth(2.5f);
            cylinder.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
            gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Disable(OpenGL.GL_TEXTURE_2D);


            //iscrtavanje vrata kaveza
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Rust]);
            gl.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Filled);
            Cube door = new Cube();
            gl.PushMatrix();
            gl.Scale(1f, 80f, 40f);
            gl.Translate(-widthOfCage, doorOpen, 0f);
            gl.Rotate(0.0f, doorRotation, 0.0f);
            door.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.Disable(OpenGL.GL_TEXTURE_2D);




            //iscrtavanje modela kamere 
            gl.PushMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            gl.Translate(-450.0f, 110.0f, -435.0f);
            gl.Rotate(0f, cameraRotation, 0f);
            gl.Rotate(m_earthRotation, 0f, 1f, 0f);
            gl.Scale(0.05f, 0.05f, 0.05f);
            m_scene.Draw();
            gl.PopMatrix();


            //iscrtavanje svetla iznad kaveza
            gl.PushMatrix();
            Sphere sphere = new Sphere();
            gl.Translate(-350.0f, 200.0f, 150.0f);
            gl.Rotate(0f, cameraRotation, 0f);
            gl.Scale(5.0f, 5.0f, 5.0f);
            gl.Color(255.0f, 255.0f, 0.224f);
            sphere.Radius = 7f;
            sphere.CreateInContext(gl);
            setLight(gl);
            sphere.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();


            //iscrtavanje crvenog svetla kod kamere
            drawRedLight(gl, redLight);


            //iscrtavanje teksta
            gl.PushMatrix();
            gl.Viewport(0, 2 * m_height / 3, m_width / 3, m_height / 3);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.PushMatrix();
            gl.DrawText3D("Helvetica bold", 12, 0, 0, "");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.DrawText(10, 7 * m_height / 8, 255.0f, 0.0f, 255.0f, "Helvetica bold", 12, "Predmet: Racunarska grafika");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.DrawText(10, 7 * m_height / 8 - 50, 255.0f, 0.0f, 255.0f, "Helvetica bold", 12, "Sk.god: 2020/21.");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.DrawText(10, 7 * m_height / 8 - 100, 255.0f, 0.0f, 255.0f, "Helvetica bold", 12, "Ime: Jovan");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.DrawText(10, 7 * m_height / 8 - 150, 255.0f, 0.0f, 255.0f, "Helvetica bold", 12, "Prezime: Bosnic");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.DrawText(10, 7 * m_height / 8 - 200, 255.0f, 0.0f, 255.0f, "Helvetica bold", 12, "Sifra zad: 4.2");
            gl.PopMatrix();
            Resize(gl, m_width, m_height);

            gl.PopMatrix();

            gl.PopMatrix();
            // Oznaci kraj iscrtavanja
            gl.Flush();
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        /// 
  
        public void drawRedLight(OpenGL gl, bool enable)
        {
            if (enable)
            {
                gl.PushMatrix();
                Sphere sfera = new Sphere();
                gl.Translate(-355.0f, 345.0f, -435.0f);
                gl.Scale(1.0f, 1.0f, 1.0f);
                gl.Color(1.0f, 0.0f, 0.0f);
                float[] ambient = { ambientr, ambientg, ambientg, 1.0f }; //setovanje ambijentalne komponente reflektroskog svetlosnog izvora
                gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, ambient);
                gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 25.0f); // cut off 25 
                gl.Enable(OpenGL.GL_LIGHT1);
                sfera.Radius = 7f;
                sfera.CreateInContext(gl);
                sfera.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                gl.PopMatrix();
            }
            else
            {
                gl.Disable(OpenGL.GL_LIGHT1);
            }
        }
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Viewport(0, 0, m_width, m_height);  // projekcija u perspekrivi fov = 45, near = 0.5, far po potrebi
            gl.Perspective(45f, (double)width / height, 0.5f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        private void setLight(OpenGL gl)
        {
        float[] ambient = { 0.2f, 0.2f, 0.2f, 1.0f };
        float[] difuse = { 0.6f, 0.6f, 0.6f, 1.0f };
        // Pridruži komponente svetlosnom izvoru 0
        gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT,ambient);
        gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, difuse);
        // Podesi parametre tackastog svetlosnog izvora
        gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f); 
        // Ukljuci svetlosni izvor
        gl.Enable(OpenGL.GL_LIGHT0);
        gl.Enable(OpenGL.GL_LIGHTING);
        // Pozicioniraj svetlosni izvor
        float[] pozicija = { -40f, 0f, 0f, 1f }; //negativna x-osa, levo od kaveza; kec za reflektivno
        gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pozicija);
        }

        private void UpdateAnimation(object sender, EventArgs e)
        {
           
            if (cameraRotation > 90.0f && cameraRotation <= 130.0f)
            {
                cameraRotation += cameraSpeed;
                stop = stop + 1;        
            }
            else
            {
                cameraRotation -= cameraSpeed;
                stop = stop + 1;
            }
            if (stop > 200)
            {
                doorOpen = -1f;
                cameraRotation = 40.0f;
               
               
            }
        }

    protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
