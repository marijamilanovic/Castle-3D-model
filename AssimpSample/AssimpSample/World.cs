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
using System.Windows.Threading;

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
        private AssimpScene sceneArrowMachine;
        private AssimpScene sceneArrow;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 10.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 150.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;


        private uint[] m_textures = null; //UCITANE TEKSTURE
        private enum TextureObjects { Grass=0, MetalFence, PavedMud, CastleWalls};
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private string[] m_textureFiles = { ".//textures//grass.jpg", 
                                            ".//textures//metalFence.jpeg", 
                                            ".//textures//pavedMud.jpeg", 
                                            ".//textures//Castle Exterior Texture Bump.jpg" };


        //ONO STO SE MENJA SLAJDERIMA
        private float m_scaleArrow = 1f; //velicina strele
        private int m_leftWall = 0; // rotacija levog
        private int m_rigthWall = 0; // pomeranje desnog

        //ANIMACIJA
        private bool animation = false;
        private DispatcherTimer timer;
        private float worldRotationY = 0;//rotacija kod animacije
        private float worldZ =0;// koristi se kod pomeranja iz zamka do mesta gde se gleda kako strele lete
        private float arrowZ=0, arrowY = 0;//za pomeranje strela
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
            set {if(value>0 && value<90) m_xRotation = value;  }
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
            set { if(value>0) m_sceneDistance = value; }
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

        public int RightWall {
            get { return m_rigthWall; }
            set { m_rigthWall = value; }
        }


        public int LeftWall {
            get { return m_leftWall; }
            set { m_leftWall = value; }
        }


        public float ScaleArrow {
            get { return m_scaleArrow; }
            set { m_scaleArrow = value; }
        }

        public Boolean Animation {
            get { return animation; }
            set { animation = value; }
        }
        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.sceneCastle = new AssimpScene(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models"), "Castle.3DS", gl);
            this.sceneArrow = new AssimpScene(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models"), "flechaventa.obj", gl);
            this.sceneArrowMachine = new AssimpScene(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models"), "Ballista_Anim.obj", gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount]; //pravimo novi niz u koji ce se tekstura ucitati
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

            gl.Enable(OpenGL.GL_NORMALIZE);                                         // definisu se za svako teme objekta; normale su neophodne za proracun osvetljenosti nekog poligona                                
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);    
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);       // != glMaterial(), bolji jer olaksava def. materijala
                                                                                    // na nivou verteksa

                                                                                    // Ambijentalno - uniformno/const, ne dolazi iz nekog pravca (sunce)
                                                                                    // Difuzno - dolazi iz nekog pravca, sv. se prelama i rasipa na povrsini (lampa)
                                                                                    // Spekulatro - dolazi iz pravca, mnogo ostriji ugao refleksije i nema rasipanja (sjaj)
                                                                                    // Emisiona - boja koju materijal isijava
            //gl.FrontFace(OpenGL.GL_CCW);
            EnableTextures(gl);
            SetupLighting(gl);     
            //gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
            sceneCastle.LoadScene();
            sceneCastle.Initialize();
            sceneArrowMachine.LoadScene();
            sceneArrowMachine.Initialize();
            sceneArrow.LoadScene();
            sceneArrow.Initialize();
        }



        public void startAnimation() {
            ///iskluci interfjes
            ///                    
            MainWindow win = (MainWindow)System.Windows.Application.Current.MainWindow;

            win.slider.IsEnabled = win.slider2.IsEnabled = win.slider3.IsEnabled = win.c1.IsEnabled = win.c2.IsEnabled = false;
            worldRotationY = 180; //u pocetku je okrenut za 180 ceo svet 
            worldZ = 40;// pozicija u zamku
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += new EventHandler(animate);//na svakih 30ms poziva ovu metoud
            timer.Start();
            animation = true;
            arrowZ = -25;
            arrowY = 35;
        }

        private void animate(object sender, EventArgs e) {
            if (worldZ != -40)//pomeraj po z osi dok kraj staze ne bude u centru
                worldZ--;
            else if (worldRotationY != 0)//nakon toga rotiraj tako da se vidi zamak
                worldRotationY -= 5;
            else {
                arrowZ+=2;
                arrowY -= 1.19f;
                if (arrowZ > 40) {//kraj
                    timer.Stop();
                    animation = false;
                    worldRotationY = 180;//da se strele ne crtaju
                    MainWindow win = (MainWindow)System.Windows.Application.Current.MainWindow;
                    ///ukljuci interfjes
                    win.slider.IsEnabled = win.slider2.IsEnabled = win.slider3.IsEnabled = win.c1.IsEnabled = win.c2.IsEnabled = true;
                }
            }
        }




        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl) {
            // Ocisti sadrzaj kolor bafera i bafera dubine
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(60, (double)m_width / (double)m_height, 1, 20000.0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.PushMatrix();
            // gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            if (!animation) {
                gl.LookAt(0f, 0f, m_sceneDistance, 0f, 0f, 0, 0f, 1f, 0f);          // pozicioniranje kamere - def.pozicije kamere, smer (tacka) gledanja kamere, orjentacija kamere

                gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
                gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            }
            else {                                              //kamera miruje u centru, gleda u - smeru z ose, a ceo svet se vrti i pomera oko nje
                gl.LookAt(0f, 5f, 0f, 0f,5f, -1, 0f, 1f, 0f);
                gl.Rotate(0, worldRotationY, 0);

                gl.Translate(0, 0, worldZ);                     //a ceo svet se pomera i rotira

                if (worldRotationY == 0) {                      //ako je okrenuto treba strele da se ispaljuju
                    for (int i = 0; i < 11; i++) {
                        gl.PushMatrix();
                        gl.Translate(8+-2*i, arrowY, arrowZ);
                        gl.Scale(m_scaleArrow, m_scaleArrow, m_scaleArrow);
                        gl.Rotate(15, 90, 0);
                        sceneArrow.Draw();
                        gl.PopMatrix();
                    }  
                }
            }

            //Reflektorski sv. izvor - snop sv. zraka; 2 slabljenja (udaljenost od objekta i srediste snopa ka njegovom obodu) -- pr. automobilski far
            float[] spot_direction = { 0.0f, -1.0f, 0.0f };         //na dole sija
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, spot_direction);

            float[] pos = { 0f, 60f, -20f, 1.0f };                  //iznad zamka
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, pos);


            gl.PushMatrix();
            gl.Translate(0.0f, 0.2f, 0.0f);
            DrawFloor(gl);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, 0.3f, 0.0f);
            DrawPath(gl);
            gl.PopMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.CastleWalls]);

            gl.PushMatrix();
            DrawWalls(gl);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -20.0f);
            sceneCastle.Draw();
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, 30.0f, -25.0f);
            gl.Scale(0.3, 0.3, -0.3);
            sceneArrowMachine.Draw();
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
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.GenTextures(m_textureCount, m_textures);                             // kreira N tekstura odjednom
            for (int i = 0; i < m_textureCount; ++i)
            {
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);                // kreirana tekstura se pridruzuje objektu
                Bitmap image = new Bitmap(m_textureFiles[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGBA8, imageData.Width, imageData.Height, 0,
                            OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);              // ucitavanje 2D tekstura, mipmapping level je 0 

                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);      // filtriranje tekstura, kada se tekstura mora smanjiti da bi bila pridruzena objektu
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);      // filtriranje tekstura, kada se tekstura mora povecati kako bi bila pridruzena objektu
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);          // definise da li ce se tekstura ponavljati po s-osi i kojim tekselima ce biti realizovano ponavljanje
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);          // definise da li ce se tekstura ponavljati po t-osi i kojim tekselima ce biti realizovano ponavljanje
                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        private void SetupLighting(OpenGL gl)
        {
            // Tackasti sv. izvor - postojanje sv. zraka koji se rasejavaju na sve strane; cutoff je 180; slabljenje je udaljenost od objekta -- pr. sijalica
            gl.Enable(OpenGL.GL_LIGHTING);                  //ukljuci svetla
            gl.Enable(OpenGL.GL_LIGHT0);                    //ukljuci svetlo 0, bice ono stacinarno

            float[] ambient0 = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] diffuse0 = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] specular0 = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);      //tackasto == cutoff je 180
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambient0);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, diffuse0);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, specular0);

            float[] pos = { -100,80, 0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pos);            //pozicija je uvek ova, trans. ne uticu na nju

            gl.Enable(OpenGL.GL_LIGHT1);                                    //ukljuci svetlo1, reflektor
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 45.0f);       //cutoff je 45

            float[] ambient1 = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] diffuse1 = new float[] { 0.5f, 0.5f, 0.5f, 1.0f };
            float[] specular1 = new float[] { 1f, 1f, 1f, 1.0f };
            
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, ambient1);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, diffuse1);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, specular1);
        }


        private void DrawFloor(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            gl.Scale(8.0f, 8.0f, 8.0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Grass]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.1f, 0.3f, 0.1f);                             // boja se uvek odnosi na tacku -- glShadeModel() moze biti FLAT ili SMOOTH
            gl.Normal(0f, 1f, 0f);                                  // normala za podlogu

            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-80f, 0f, 52f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(80f, 0f, 52f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(80f, 0f, -52f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(-80f, 0f, -52f);

            gl.End();
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

        }

        private void DrawPath(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            gl.Scale(5f, 1.0, 1.0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.PavedMud]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.7f, 0.5f, 0.5f);
            gl.Normal(0f, 1f, 0f);                                  // normala za stazu

            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-5f, 0f, 50f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(5f, 0f, 50f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(5f, 0f, -20f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(-5f, 0f, -20f);

            gl.End();
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
        }

        private void DrawWalls(OpenGL gl)
        {
            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.MetalFence]);
            gl.Color(1f, 1f, 1f);

            gl.Translate(-50.0f, 0.0f, 0.0f);
            gl.Rotate(0, m_leftWall, 0);                                    //rotira se levi
            gl.Scale(0.1, 20, 50);
            gl.Translate(-1, 1, 0);
            Cube leftWall = new Cube();
            //gl.Normal(0f, 1f, 0f);
            leftWall.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(50.0f+m_rigthWall, 0.0f, 0.0f);                    //translira se desni
            gl.Scale(0.1, 20, 50);
            gl.Translate(-1, 1, 0);
            Cube rightWall = new Cube();
            //gl.Normal(0f, 1f, 0f);
            rightWall.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.LoadIdentity();
        }

        // ORTOGONALNA PROJEKCIJA - svi objekti koji su istih dimenzija prikazuju se u istoj velicini bez obzira gde se nalaze --- gluOrtho()
        // PROJEKCIJA U PERSPEKTIVI - objekti koji su dalje od posmatraca su sve manji i tako --- gluPerspetive()

        private void DrawTextInfo(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_LIGHTING);
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

            gl.Disable(OpenGL.GL_LIGHTING);
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
