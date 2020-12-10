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
        private float m_xRotation = 10.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 100.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

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
            this.sceneCastle = new AssimpScene(scenePath, sceneFileName, gl);
            this.sceneArrow = new AssimpScene(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Arrow"), "Ballista_Anim.obj", gl);
            this.m_width = width;
            this.m_height = height;
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
            // default je CCW
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            //gl.Enable(OpenGL.GL_CULL_FACE);
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

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, 0.0f);
            DrawFloor(gl);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, 0.1f, 0.0f);
            DrawPath(gl);
            gl.PopMatrix();

            gl.PushMatrix();
            DrawWalls(gl);
            gl.PopMatrix();

            sceneCastle.Draw();

            gl.PushMatrix();
            gl.Translate(-40.0f, 0.5f, 40.0f);
            gl.Scale(0.3, 0.3, -0.3);
            sceneArrow.Draw();
            gl.PopMatrix();

            gl.PopMatrix();
            // Oznaci kraj iscrtavanja
            gl.Flush();
        }


        private void DrawFloor(OpenGL gl)
        {
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.1f, 0.3f, 0.1f);
        
            gl.Vertex(-50f, 0f, 50f);
            gl.Vertex(50f, 0f, 50f);
            gl.Vertex(50f, 0f, -50f);
            gl.Vertex(-50f, 0f, -50f);

            gl.End();
            gl.LoadIdentity();      
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        private void DrawPath(OpenGL gl)
        {
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.1f, 0.1f, 0.1f);

            gl.Vertex(-5f, 0f, 50f);
            gl.Vertex(5f, 0f, 50f);
            gl.Vertex(5f, 0f, 20f);
            gl.Vertex(-5f, 0f, 20f);

            gl.End();
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        private void DrawWalls(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Color(0.5f, 0.7f, 0.5f);
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



        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
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
