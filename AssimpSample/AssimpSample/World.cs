// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using System.Drawing.Imaging;
using System.Drawing;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene sceneCastle;
        private AssimpScene sceneArrow;

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
        private float m_sceneDistance = 0.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private float sceneDistance1 = 100.0f;


        private uint[] m_textures = null; //UCITANE TEKSTURE
        private enum TextureObjects { Grass=0};
        
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private string[] m_textureFiles = { ".//textures//grass.jpg" };

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return sceneCastle; }
            set { sceneCastle = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; if (m_xRotation < 4) m_xRotation = 4; if (m_xRotation > 60) m_xRotation = 60; }
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

        public float SceneDistance1 { get => sceneDistance1; set => sceneDistance1 = value; }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            
            this.sceneCastle = new AssimpScene(scenePath, sceneFileName, gl);
            this.sceneArrow = new AssimpScene(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Arrow"), "Ballista_Anim.obj", gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount]; //PRAVIMO NOVI NIZ U KOJI CE SE TEKSUTRE UCITATI
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
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);

            gl.Enable(OpenGL.GL_NORMALIZE);                                         // normalizacija
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);       // != glMaterial(), bolji jer olaksava def. materijala
                                                                                    // na nivou verteksa
            //gl.FrontFace(OpenGL.GL_CCW);
            EnableTextures(gl);
            SetupLighting(gl);      // ukljucivanje svetla
            //gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
            sceneCastle.LoadScene();
            sceneCastle.Initialize();
            sceneArrow.LoadScene();
            sceneArrow.Initialize();
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            // Ocisti sadrzaj kolor bafera i bafera dubine
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(60, (double)m_width / (double)m_height, 1, 20000.0);
            gl.LookAt(0f, 30f, 100f, 0f, 0f, 0, 0f, 1f, 0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            gl.PushMatrix();
            gl.Translate(0.0f, 0.2f, 0.0f);
            DrawFloor(gl);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, 0.3f, 0.0f);
            DrawPath(gl);
            gl.PopMatrix();

            gl.PushMatrix();
            DrawWalls(gl);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, 0.3f, 0.0f);
            sceneCastle.Draw();
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-40.0f, 0.5f, 40.0f);
            gl.Scale(0.3, 0.3, -0.3);
            sceneArrow.Draw();
            gl.PopMatrix();

            gl.PushMatrix();
            DrawTextInfo(gl);
            gl.PopMatrix();

            gl.PopMatrix();
            // Oznaci kraj iscrtavanja
            gl.Flush();
        }


        private void EnableTextures(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE); //STAPANJE = MODULATE (2.3)
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                Bitmap image = new Bitmap(m_textureFiles[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGBA8, imageData.Width, imageData.Height, 0,
                            OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);

                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR); 
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        private void SetupLighting(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);

            float[] white = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] ambient0 = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] diffuse0 = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] specular0 = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambient0);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, diffuse0);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, specular0);

            //float[] spot_direction = { -1.0f, -1.0f, 0.0f };
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_DIRECTION, spot_direction);

            //float[] pos = { 0.5f, 0.5f, 1f, 0.0f };
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pos);
        }


        private void DrawFloor(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            gl.Scale(8.0f, 8.0f, 8.0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Grass]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.1f, 0.3f, 0.1f);
            gl.Normal(0f, 1f, 0f);                                  // normala za podlogu
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-50f, 0f, 50f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(50f, 0f, 50f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(50f, 0f, -50f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(-50f, 0f, -50f);

            gl.End();
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.LoadIdentity();

        }

        private void DrawPath(OpenGL gl)
        {
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.1f, 0.1f, 0.1f);
            gl.Normal(0f, 1f, 0f);                                  // normala za stazu

            gl.Vertex(-5f, 0f, 50f);
            gl.Vertex(5f, 0f, 50f);
            gl.Vertex(5f, 0f, 20f);
            gl.Vertex(-5f, 0f, 20f);

            gl.End();
            gl.LoadIdentity();
        }

        private void DrawWalls(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Color(0.1f, 0.1f, 0.1f);
            gl.Translate(-50.0f, 0.0f, 0.0f);
            gl.Scale(0.1, 20, 50);
            gl.Translate(-1, 1, 0);
            Cube leftWall = new Cube();
            leftWall.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(50.0f, 0.0f, 0.0f);
            gl.Scale(0.1, 20, 50);
            gl.Translate(-1, 1, 0);
            Cube rightWall = new Cube();
            rightWall.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
        }

        // ORTOGONALNA PROJEKCIJA - svi objekti koji su istih dimenzija prikazuju se u istoj velicini bez obzira gde se nalaze --- gluOrtho()
        // PROJEKCIJA U PERSPEKTIVI - objekti koji su dalje od posmatraca su sve manji i tako --- gluPerspetive()

        private void DrawTextInfo(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PushMatrix();
            gl.Viewport(m_width/2, 0, m_width/2, m_height/2);
            gl.LoadIdentity();

            gl.Ortho2D(-10.0f, 20.0f, -10.0f, 10.0f);       //near = 1 i far = -1
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();    
            gl.Color(1f, 0.0f, 0.0f);

            gl.PushMatrix();
            gl.DrawText3D("Verdana bold", 14, 0.0f, 0, "Predmet: Racunarska grafika");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, -1.0f, 0.0f);
            gl.DrawText3D("Verdana bold", 14, 0.0f, 0, "Sk.god: 2020/21.");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, -2.0f, 0.0f);
            gl.DrawText3D("Verdana bold", 14, 0.0f, 0, "Ime: Marija");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, -3.0f, 0.0f);
            gl.DrawText3D("Verdana bold", 14, 0.0f, 0, "Prezime: Milanovic");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, -4.0f, 0.0f);
            gl.DrawText3D("Verdana bold", 14, 0.0f, 0, "Sifra zad: PF1S3.2.");
            gl.PopMatrix();

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.Viewport(0, 0, m_width, m_height);
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

        }



        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)            //definise skaliranje prozora pri promeni velicine prozora od strane klijenta
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);                       // definise mapiranje log.koordinata u fizicke koordinate
            gl.MatrixMode(OpenGL.GL_PROJECTION);                        // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(60f, (double)width / height, 1f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                sceneCastle.Dispose();
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
