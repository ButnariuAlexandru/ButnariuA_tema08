using System;

using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_winforms_z02
{
    public partial class Form1 : Form
    {

        //Stări de control cameră.
        private int eyePosX, eyePosY, eyePosZ;


        private Point mousePos;
        private float camDepth;

        //Stări de control mouse.
        private bool statusControlMouse2D, statusControlMouse3D, statusMouseDown;

        //Stări de control axe de coordonate.
        private bool statusControlAxe;

        //Stări de control iluminare.
        private bool lightON;
        private bool lightON_0;
        private bool lightON_1;


        //Stări de control obiecte 3D.
        private string statusCube;

        private string statusCub;
        private bool statusScene;


        //Structuri de stocare a vertexurilor și a listelor de vertexuri.
        private int[,] arrVertex = new int[50, 3];         //Stocam matricea de vertexuri; 3 coloane vor reține informația pentru X, Y, Z. Nr. de linii definește nr. de vertexuri.
        private int nVertex;

        private int[] arrQuadsList = new int[100];        //Lista de vertexuri pentru construirea cubului prin intermediul quadurilor. Ne bazăm pe lista de vertexuri.
        private int nQuadsList;

        private int[] arrTrianglesList = new int[100];    //Lista de vertexuri pentru construirea cubului prin intermediul triunghiurilor. Ne bazăm pe lista de vertexuri.
        private int nTrianglesList;

        //Fișiere de in/out pentru manipularea vertexurilor.
        private string fileVertex = "vertexList.txt";
        private string fileQList = "quadsVertexList.txt";
        private string fileTList = "trianglesVertexList.txt";
        private bool statusFiles;



        //Dim valuesAmbientTemplate0() As Single = {255, 0, 0, 1.0}      //Valori alternative ambientale(lumină colorată)
        //# SET 1
        private float[] valuesAmbientTemplate0 = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
        private float[] valuesDiffuseTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        private float[] valuesSpecularTemplate0 = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
        private float[] valuesPositionTemplate0 = new float[] { 0.0f, 0.0f, 5.0f, 1.0f };
        //# SET 2
        //private float[] valuesAmbientTemplate0 = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
        //private float[] valuesDiffuseTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        //private float[] valuesSpecularTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        //private float[] valuesPositionTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 0.0f };

        private float[] valuesAmbient0 = new float[4];
        private float[] valuesDiffuse0 = new float[4];
        private float[] valuesSpecular0 = new float[4];
        private float[] valuesPosition0 = new float[4];


        // set de valori pentru a doua sursa de lumina
        private float[] valuesAmbientTemplate1 = new float[] { 0.0f, 0.0f, 0.0f, 1.0f }; // Fără componentă ambientală Light1
        private float[] valuesDiffuseTemplate1 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f }; // Culoare difuză albă
        private float[] valuesSpecularTemplate1 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f }; // Culoare speculară albă
        private float[] valuesPositionTemplate1 = new float[] { 10.0f, 100.0f, 10.0f, 1.0f }; // Poziție locală diferită de Sursa 0

        private float[] valuesAmbient1 = new float[4]; // Componenta ambientală
        private float[] valuesDiffuse1 = new float[4]; // Componenta difuză
        private float[] valuesSpecular1 = new float[4]; // Componenta speculară
        private float[] valuesPosition1 = new float[4]; // Poziția


        private bool isDirectionalLight0 = false;
        private float constantAttenuation = 1.0f;
        private float linearAttenuation = 0.0f;
        private float quadraticAttenuation = 0.0f;
        private bool isSmoothShading = true; // GL_SMOOTH vs. GL_FLAT
        private bool isLocalViewer = false; // GL_LIGHT_MODEL_LOCAL_VIEWER

        // Proprietăți Material (k_a, k_d, k_s, Shininess)
        // Valorile implicite (default)
        private float[] materialAmbient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
        private float[] materialDiffuse = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };
        private float[] materialSpecular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        private float materialShininess = 50.0f; // Exponentul specular

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   ON_LOAD
        public Form1() {
            InitializeComponent();

            /// TODO:
            /// În fișierul <Form1.Designer.cs>, la linia 26 înlocuiți conțînutul original cu linia de mai jos:
            ///         this.GlControl1 = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));
            /// Acest mod de inițializare va activa antialiasing-ul (multisampling MSAA la 8x).
            /// ATENTȚIE!
            /// Veți pierde designerul grafic. Aplicația funcționează dar pentru a putea accesa designerul grafic va trebui să reveniți la constructorul
            /// implicit al controlului OpenTK!
        }

        private void Form1_Load(object sender, EventArgs e) {

            SetupValues();
            SetupWindowGUI();
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   SETARI INIȚIALE
        private void SetupValues() {
            eyePosX = 100;
            eyePosY = 100;
            eyePosZ = 50;

            camDepth = 1.04f;

            setLight0Values();

            numericXeye.Value = eyePosX;
            numericYeye.Value = eyePosY;
            numericZeye.Value = eyePosZ;

            setLight0Values();
            setLight1Values();
        }


        private void SetupWindowGUI() {

            setControlMouse2D(false);
            setControlMouse3D(false);

            numericCameraDepth.Value = (int)camDepth;

            setControlAxe(true);

            setCubeStatus("OFF");
            setIlluminationStatus(false);
            setSource0Status(false);

            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();


            //Setari de lumina
            setIlluminationStatus(false);
            setSource0Status(false);
            setSource1Status(false);


            setCubeStatus("OFF");
            setSceneStatus(false); // Am adăugat acest lucru în răspunsul anterior pentru a controla scena
            setIlluminationStatus(false);
            setSource0Status(false);
            setSource1Status(false); // Sursa 1, dacă ați implementat-o

            isSmoothShading = true; // Implicit
            isLocalViewer = false; // Implicit
            isDirectionalLight0 = false; // Implicit Pozițional
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   MANIPULARE VERTEXURI ȘI LISTE DE COORDONATE.
        //Încărcarea coordonatelor vertexurilor și lista de compunere a obiectelor 3D.
        private void loadVertex() {

            //Testăm dacă fișierul există
            try {
                StreamReader fileReader = new StreamReader((fileVertex));
                nVertex = Convert.ToInt32(fileReader.ReadLine().Trim());
                Console.WriteLine("Vertexuri citite: " + nVertex.ToString());

                string tmpStr = "";
                string[] str = new string[3];
                for (int i = 0; i < nVertex; i++) {
                    tmpStr = fileReader.ReadLine();
                    str = tmpStr.Trim().Split(' ');
                    arrVertex[i, 0] = Convert.ToInt32(str[0].Trim());
                    arrVertex[i, 1] = Convert.ToInt32(str[1].Trim());
                    arrVertex[i, 2] = Convert.ToInt32(str[2].Trim());
                }
                fileReader.Close();

            } catch (Exception) {
                statusFiles = false;
                Console.WriteLine("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
                MessageBox.Show("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
            }
        }

        private void loadQList() {

            //Testăm dacă fișierul există
            try {
                StreamReader fileReader = new StreamReader(fileQList);

                int tmp;
                string line;
                nQuadsList = 0;

                while ((line = fileReader.ReadLine()) != null) {
                    tmp = Convert.ToInt32(line.Trim());
                    arrQuadsList[nQuadsList] = tmp;
                    nQuadsList++;
                }

                fileReader.Close();
            } catch (Exception) {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informații vertex <" + fileQList + "> nu exista!");
            }

        }

        //Setăm variabila de stare pentru sursa de lumină 1.
        private void setSource1Status(bool status)
        {
            lightON_1 = status;
        }
        private void loadTList() {

            //Testăm dacă fișierul există
            try {
                StreamReader fileReader = new StreamReader(fileTList);

                int tmp;
                string line;
                nTrianglesList = 0;

                while ((line = fileReader.ReadLine()) != null) {
                    tmp = Convert.ToInt32(line.Trim());
                    arrTrianglesList[nTrianglesList] = tmp;
                    nTrianglesList++;
                }

                fileReader.Close();
            } catch (Exception) {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informații vertex <" + fileTList + "> nu exista!");
            }

        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL CAMERĂ

        //Controlul camerei după axa X cu spinner numeric (un cadran).
        private void numericXeye_ValueChanged(object sender, EventArgs e) {
            eyePosX = (int)numericXeye.Value;
            GlControl1.Invalidate(); //Forțează redesenarea întregului control OpenGL. Modificările vor fi luate în considerare (actualizare).
        }
        //Controlul camerei după axa Y cu spinner numeric (un cadran).
        private void numericYeye_ValueChanged(object sender, EventArgs e) {
            eyePosY = (int)numericYeye.Value;
            GlControl1.Invalidate(); //Forțează redesenarea întregului control OpenGL. Modificările vor fi luate în considerare (actualizare).
        }
        //Controlul camerei după axa Z cu spinner numeric (un cadran).
        private void numericZeye_ValueChanged(object sender, EventArgs e) {
            eyePosZ = (int)numericZeye.Value;
            GlControl1.Invalidate(); //Forțează redesenarea întregului control OpenGL. Modificările vor fi luate în considerare (actualizare).
        }
        //Controlul adâncimii camerei față de (0,0,0).
        private void numericCameraDepth_ValueChanged(object sender, EventArgs e) {
            camDepth = 1 + ((float)numericCameraDepth.Value) * 0.1f;
            GlControl1.Invalidate();
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL MOUSE
        //Setăm variabila de stare pentru rotația în 2D a mouseului.
        private void setControlMouse2D(bool status) {
            if (status == false) {
                statusControlMouse2D = false;
                btnMouseControl2D.Text = "2D mouse control OFF";
            } else {
                statusControlMouse2D = true;
                btnMouseControl2D.Text = "2D mouse control ON";
            }
        }
        //Setăm variabila de stare pentru rotația în 3D a mouseului.
        private void setControlMouse3D(bool status) {
            if (status == false) {
                statusControlMouse3D = false;
                btnMouseControl3D.Text = "3D mouse control OFF";
            } else {
                statusControlMouse3D = true;
                btnMouseControl3D.Text = "3D mouse control ON";
            }
        }

        //Controlul mișcării setului de coordonate cu ajutorul mouseului (în plan 2D/3D)
        private void btnMouseControl2D_Click(object sender, EventArgs e) {
            if (statusControlMouse2D == true) {
                setControlMouse2D(false);
            } else {
                setControlMouse3D(false);
                setControlMouse2D(true);
            }
        }
        private void btnMouseControl3D_Click(object sender, EventArgs e) {
            if (statusControlMouse3D == true) {
                setControlMouse3D(false);
            } else {
                setControlMouse2D(false);
                setControlMouse3D(true);
            }
        }



        //Mișcarea lumii 3D cu ajutorul mouselui (click'n'drag de mouse).
        private void GlControl1_MouseMove(object sender, MouseEventArgs e) {
            if (statusMouseDown == true) {
                mousePos = new Point(e.X, e.Y);
                GlControl1.Invalidate();     //Forțează redesenarea întregului control OpenGL. Modificările vor fi luate în considerare (actualizare).
            }
        }
        private void GlControl1_MouseDown(object sender, MouseEventArgs e) {
            statusMouseDown = true;
        }
        private void GlControl1_MouseUp(object sender, MouseEventArgs e) {
            statusMouseDown = false;
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL ILUMINARE
        //Setăm variabila de stare pentru iluminare.
        private void setIlluminationStatus(bool status) {
            if (status == false) {
                lightON = false;
                btnLights.Text = "Iluminare OFF";
            } else {
                lightON = true;
                btnLights.Text = "Iluminare ON";
            }
        }

        //Activăm/dezactivăm iluminarea.
        private void btnLights_Click(object sender, EventArgs e) {
            if (lightON == false) {
                setIlluminationStatus(true);
            } else {
                setIlluminationStatus(false);
            }
            GlControl1.Invalidate();
        }

        //Identifică numărul maxim de lumini pentru implementarea curentă a OpenGL.
        private void btnLightsNo_Click(object sender, EventArgs e) {
            int nr = GL.GetInteger(GetPName.MaxLights);
            MessageBox.Show("Nr. maxim de luminii pentru aceasta implementare este <" + nr.ToString() + ">.");
        }

        //Setăm variabila de stare pentru sursa de lumină 0.
        private void setSource0Status(bool status) {
            if (status == false) {
                lightON_0 = false;
                btnLight0.Text = "Sursa 0 OFF";
            } else {
                lightON_0 = true;
                btnLight0.Text = "Sursa 0 ON";
            }
        }
        //Adaug o noua sursa de lumina
        private void setLight1Values()
        {
            for (int i = 0; i < valuesAmbientTemplate1.Length; i++)
            {
                valuesAmbient1[i] = valuesAmbientTemplate1[i];
            }
            for (int i = 0; i < valuesDiffuseTemplate1.Length; i++)
            {
                valuesDiffuse1[i] = valuesDiffuseTemplate1[i];
            }
            for (int i = 0; i < valuesSpecularTemplate1.Length; i++)
            {
                valuesSpecular1[i] = valuesSpecularTemplate1[i];
            }
            for (int i = 0; i < valuesPositionTemplate1.Length; i++)
            {
                valuesPosition1[i] = valuesPositionTemplate1[i];
            }
        }

        //Activăm/dezactivăm sursa 0 de iluminare (doar dacă iluminarea este activă).
        private void btnLight0_Click(object sender, EventArgs e) {
            if (lightON == true) {
                if (lightON_0 == false) {
                    setSource0Status(true);
                } else {
                    setSource0Status(false);
                }
                GlControl1.Invalidate();
            }
        }

        //Schimbăm poziția sursei 0 de iluminare după axele XYZ.
        private void setTrackLigh0Default() {
            trackLight0PositionX.Value = (int)valuesPosition0[0];
            trackLight0PositionY.Value = (int)valuesPosition0[1];
            trackLight0PositionZ.Value = (int)valuesPosition0[2];
        }
        private void trackLight0PositionX_Scroll(object sender, EventArgs e) {
            valuesPosition0[0] = trackLight0PositionX.Value;
            GlControl1.Invalidate();
        }
        private void trackLight0PositionY_Scroll(object sender, EventArgs e) {
            valuesPosition0[1] = trackLight0PositionY.Value;
            GlControl1.Invalidate();
        }
        private void trackLight0PositionZ_Scroll(object sender, EventArgs e) {
            valuesPosition0[2] = trackLight0PositionZ.Value;
            GlControl1.Invalidate();
        }

        //Schimbăm culoarea sursei de lumină 0 (ambiental) în domeniul RGB.
        private void setColorAmbientLigh0Default() {
            numericLight0Ambient_Red.Value = (decimal)valuesAmbient0[0];
            numericLight0Ambient_Green.Value = (decimal)valuesAmbient0[1];
            numericLight0Ambient_Blue.Value = (decimal)valuesAmbient0[2];
        }
        private void numericLight0Ambient_Red_ValueChanged(object sender, EventArgs e) {
            valuesAmbient0[0] = (float)numericLight0Ambient_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Ambient_Green_ValueChanged(object sender, EventArgs e) {
            valuesAmbient0[1] = (float)numericLight0Ambient_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Ambient_Blue_ValueChanged(object sender, EventArgs e) {
            valuesAmbient0[2] = (float)numericLight0Ambient_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        //Schimbăm culoarea sursei de lumină 0 (difuză) în domeniul RGB.
        private void setColorDifuseLigh0Default() {
            numericLight0Difuse_Red.Value = (decimal)valuesDiffuse0[0];
            numericLight0Difuse_Green.Value = (decimal)valuesDiffuse0[1];
            numericLight0Difuse_Blue.Value = (decimal)valuesDiffuse0[2];
        }
        private void numericLight0Difuse_Red_ValueChanged(object sender, EventArgs e) {
            valuesDiffuse0[0] = (float)numericLight0Difuse_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Difuse_Green_ValueChanged(object sender, EventArgs e) {
            valuesDiffuse0[1] = (float)numericLight0Difuse_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Difuse_Blue_ValueChanged(object sender, EventArgs e) {
            valuesDiffuse0[2] = (float)numericLight0Difuse_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        //Schimbăm culoarea sursei de lumină 0 (specular) în domeniul RGB.
        private void setColorSpecularLigh0Default() {
            numericLight0Specular_Red.Value = (decimal)valuesSpecular0[0];
            numericLight0Specular_Green.Value = (decimal)valuesSpecular0[1];
            numericLight0Specular_Blue.Value = (decimal)valuesSpecular0[2];
        }
        private void numericLight0Specular_Red_ValueChanged(object sender, EventArgs e) {
            valuesSpecular0[0] = (float)numericLight0Specular_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Specular_Green_ValueChanged(object sender, EventArgs e) {
            valuesSpecular0[1] = (float)numericLight0Specular_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Specular_Blue_ValueChanged(object sender, EventArgs e) {
            valuesSpecular0[2] = (float)numericLight0Specular_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        //Resetare stare sursă de lumină nr. 0.
        private void setLight0Values() {
            for (int i = 0; i < valuesAmbientTemplate0.Length; i++) {
                valuesAmbient0[i] = valuesAmbientTemplate0[i];
            }
            for (int i = 0; i < valuesDiffuseTemplate0.Length; i++) {
                valuesDiffuse0[i] = valuesDiffuseTemplate0[i];
            }
            for (int i = 0; i < valuesPositionTemplate0.Length; i++) {
                valuesPosition0[i] = valuesPositionTemplate0[i];
            }
        }
        private void btnLight0Reset_Click(object sender, EventArgs e) {
            setLight0Values();
            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();
            GlControl1.Invalidate();
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL OBIECTE 3D
        //Setăm variabila de stare pentru afișarea/scunderea sistemului de coordonate.
        private void setControlAxe(bool status) {
            if (status == false) {
                statusControlAxe = false;
                btnShowAxes.Text = "Axe Oxyz OFF";
            } else {
                statusControlAxe = true;
                btnShowAxes.Text = "Axe Oxyz ON";
            }
        }

        //Controlul axelor de coordonate (ON/OFF).
        private void btnShowAxes_Click(object sender, EventArgs e) {
            if (statusControlAxe == true) {
                setControlAxe(false);
            } else {
                setControlAxe(true);
            }
            GlControl1.Invalidate();
        }

        //Setăm variabila de stare pentru desenarea cubului. Valorile acceptabile sunt:
        //TRIANGLES = cubul este desenat, prin triunghiuri.
        //QUADS = cubul este desenat, prin quaduri.
        //OFF (sau orice altceva) = cubul nu este desenat.
        private void setCubeStatus(string status) {
            if (status.Trim().ToUpper().Equals("TRIANGLES")) {
                statusCube = "TRIANGLES";
            } else if (status.Trim().ToUpper().Equals("QUADS")) {
                statusCube = "QUADS";
            } else {
                statusCube = "OFF";
            }
        }
        private void btnCubeQ_Click(object sender, EventArgs e) {
            statusFiles = true;
            loadVertex();
            loadQList();
            setCubeStatus("QUADS");
            GlControl1.Invalidate();
        }
        private void btnCubeT_Click(object sender, EventArgs e) {
            statusFiles = true;
            loadVertex();
            loadTList();
            setCubeStatus("TRIANGLES");
            GlControl1.Invalidate();
        }
        private void btnResetObjects_Click(object sender, EventArgs e) {
            setCubeStatus("OFF");
            GlControl1.Invalidate();
        }
        private void setSceneStatus(bool status)
        {
            statusScene = status;
            // Poți adăuga logica pentru a schimba textul butonului AICI dacă vrei.
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            // Comutăm starea: dacă e OFF, o facem ON, altfel o facem OFF.
            if (statusScene == false)
            {
                setSceneStatus(true);
                // Poți reseta și starea cubului la OFF dacă vrei să vezi doar scena
                // setCubeStatus("OFF");
            }
            else
            {
                setSceneStatus(false);
            }
            GlControl1.Invalidate(); // Forțează redesenarea cu noua stare.

        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   ADMINISTRARE MOD 3D (METODA PRINCIPALĂ)
        private void GlControl1_Paint(object sender, PaintEventArgs e) {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // Culoarea default a mediului.
            GL.ClearColor(Color.Black);

            // Setări preliminară pentru mediul 3D.
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(camDepth, (float)GlControl1.Width / GlControl1.Height, 1, 10000);
            Matrix4 lookat = Matrix4.LookAt(eyePosX, eyePosY, eyePosZ, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref lookat);
            GL.Viewport(0, 0, GlControl1.Width, GlControl1.Height);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            //  Setarea Modelului de Umbrire 
            if (isSmoothShading)
            {
                GL.ShadeModel(ShadingModel.Smooth); // Gouraud Shading
            }
            else
            {
                GL.ShadeModel(ShadingModel.Flat); // Flat Shading
            }

            //  Setarea Proprietăților Materiale 
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, materialAmbient);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, materialDiffuse);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, materialSpecular);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, materialShininess);

            // Setarea Modului de Vizualizare
            if (isLocalViewer)
            {
                GL.LightModel(LightModelParameter.LightModelLocalViewer, 1);
            }
            else
            {
                GL.LightModel(LightModelParameter.LightModelLocalViewer, 0);
            }

            // Pornim iluminarea (daca avem permisiunea să o facem).
            if (lightON == true)
            {
                GL.Enable(EnableCap.Lighting);
            }
            else
            {
                GL.Disable(EnableCap.Lighting);
            }

            // -----------------------------------------------------------------
            // ILUMINARE SURSA 0 (GL_LIGHT0)
            // -----------------------------------------------------------------
            GL.Light(LightName.Light0, LightParameter.Ambient, valuesAmbient0);
            GL.Light(LightName.Light0, LightParameter.Diffuse, valuesDiffuse0);
            GL.Light(LightName.Light0, LightParameter.Specular, valuesSpecular0);

            // PAGINA 8: Poziție Locală (w=1) vs. Direcțională (w=0)
            if (isDirectionalLight0)
            {
                valuesPosition0[3] = 0.0f; // Direcțională (razele sunt paralele)
            }
            else
            {
                valuesPosition0[3] = 1.0f; // Pozițională (razele vin dintr-un punct)
            }
            GL.Light(LightName.Light0, LightParameter.Position, valuesPosition0);

            // PAGINA 10: Atenuare (Attenuation)
            GL.Light(LightName.Light0, LightParameter.ConstantAttenuation, constantAttenuation);
            GL.Light(LightName.Light0, LightParameter.LinearAttenuation, linearAttenuation);
            GL.Light(LightName.Light0, LightParameter.QuadraticAttenuation, quadraticAttenuation);

            // Sursa 0 ON/OFF
            if ((lightON == true) && (lightON_0 == true))
            {
                GL.Enable(EnableCap.Light0);
            }
            else
            {
                GL.Disable(EnableCap.Light0);
            }

            // Sursa 1 (Dacă este implementată)
            if ((lightON == true) && (lightON_1 == true))
            {
                // Trebuie să adaugi GL.Light(...) pentru Light1 AICI, similar cu Light0
                GL.Enable(EnableCap.Light1);
            }
            else
            {
                GL.Disable(EnableCap.Light1);
            }

            // Controlul rotației cu mouse-ul (2D/3D).
            if (statusControlMouse2D == true)
            {
                GL.Rotate(mousePos.X, 0, 1, 0);
            }
            if (statusControlMouse3D == true)
            {
                GL.Rotate(mousePos.X, 0, 1, 1);
            }

            // Descrierea obiectelor 3D
            if (statusControlAxe == true)
            {
                DeseneazaAxe();
            }
            if (statusScene == true)
            { // Asigură-te că DeseneazaScena() este definită
                DeseneazaScena();
            }
            if (statusCube.ToUpper().Equals("QUADS"))
            {
                DeseneazaCubQ();
            }
            else if (statusCube.ToUpper().Equals("TRIANGLES"))
            {
                DeseneazaCubT();
            }

            GlControl1.SwapBuffers();


        }

        private void Sursa1_Click(object sender, EventArgs e)
        {
            if (lightON == true)
            {
                if (lightON_1 == false)
                {
                    setSource1Status(true);
                    Sursa1.Text = "Sursa 1 ON"; // Asigură-te că butonul are Textul actualizat.
                }
                else
                {
                    setSource1Status(false);
                    Sursa1.Text = "Sursa 1 OFF";
                }
                GlControl1.Invalidate();
            }
        }

        private void label33_Click(object sender, EventArgs e)
        {

        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   DESENARE OBIECTE 3D
        //Desenează axe XYZ.
        private void DeseneazaAxe() {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(75, 0, 0);
            GL.End();
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Yellow);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 75, 0);
            GL.End();
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 75);
            GL.End();
        }
        //Desenează cubul - quads.
        private void DeseneazaCubQ() {
            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < nQuadsList; i++) {
                switch (i % 4) {
                    case 0:
                        GL.Color3(Color.Blue);
                        break;
                    case 1:
                        GL.Color3(Color.Red);
                        break;
                    case 2:
                        GL.Color3(Color.Green);
                        break;
                    case 3:
                        GL.Color3(Color.Yellow);
                        break;
                }
                int x = arrQuadsList[i];
                GL.Vertex3(arrVertex[x, 0], arrVertex[x, 1], arrVertex[x, 2]);
            }
            GL.End();
        }
        //Desenează cubul - triunghuri.
        private void DeseneazaCubT() {
            GL.Begin(PrimitiveType.Triangles);
            for (int i = 0; i < nTrianglesList; i++) {
                switch (i % 3) {
                    case 0:
                        GL.Color3(Color.Blue);
                        break;
                    case 1:
                        GL.Color3(Color.Red);
                        break;
                    case 2:
                        GL.Color3(Color.Green);
                        break;
                }
                int x = arrTrianglesList[i];
                GL.Vertex3(arrVertex[x, 0], arrVertex[x, 1], arrVertex[x, 2]);
            }
            GL.End();
        }


        private void DeseneazaScena()
        {
            // Culoarea bej (exemplu: R=245, G=245, B=220, normalizată la 0-1)
            float r = 245f / 255f;
            float g = 245f / 255f;
            float b = 220f / 255f;

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(r, g, b);

            // Definirea colțurilor scenei (un plan mare centrat în (0,0,0) pe planul XZ)
            int size = 150; // Dimensiunea scenei pe axele X și Z

            // Vertex 1: -X, 0, -Z
            GL.Vertex3(-size, 0, -size);
            // Vertex 2: X, 0, -Z
            GL.Vertex3(size, 0, -size);
            // Vertex 3: X, 0, Z
            GL.Vertex3(size, 0, size);
            // Vertex 4: -X, 0, Z
            GL.Vertex3(-size, 0, size);

            GL.End();
        }



        private void cbDirectionalLight_CheckedChanged(object sender, EventArgs e)
        {
            // Asigură-te că checkbox-ul se numește cbDirectionalLight
            isDirectionalLight0 = ((CheckBox)sender).Checked;
            GlControl1.Invalidate();
        }

        // Local Viewer
        private void cbLocalViewer_CheckedChanged(object sender, EventArgs e)
        {
            // Asigură-te că checkbox-ul se numește cbLocalViewer
            isLocalViewer = ((CheckBox)sender).Checked;
            GlControl1.Invalidate();
        }

        // --- MODEL SHADING ---

        // Smooth Shading (GL_SMOOTH)
        private void rbSmoothShading_CheckedChanged(object sender, EventArgs e)
        {
            // Asigură-te că RadioButton-ul se numește rbSmoothShading
            if (((RadioButton)sender).Checked)
            {
                isSmoothShading = true;
                GlControl1.Invalidate();
            }
        }

        // Flat Shading (GL_FLAT)
        private void rbFlatShading_CheckedChanged(object sender, EventArgs e)
        {
            // Asigură-te că RadioButton-ul se numește rbFlatShading
            if (((RadioButton)sender).Checked)
            {
                isSmoothShading = false;
                GlControl1.Invalidate();
            }
        }

        // --- ATENUARE (ATTENUATION) ---

        private void nudConstAtt_ValueChanged(object sender, EventArgs e)
        {
            constantAttenuation = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }

        private void nudLinAtt_ValueChanged(object sender, EventArgs e)
        {
            linearAttenuation = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }

        private void nudQuadAtt_ValueChanged(object sender, EventArgs e)
        {
            quadraticAttenuation = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }


        // --- PROPRIETĂȚI MATERIAL (AMBIENT) ---

        private void nudMatAmbR_ValueChanged(object sender, EventArgs e)
        {
            materialAmbient[0] = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }
        private void nudMatAmbG_ValueChanged(object sender, EventArgs e)
        {
            materialAmbient[1] = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }
        private void nudMatAmbB_ValueChanged(object sender, EventArgs e)
        {
            materialAmbient[2] = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }

        // --- PROPRIETĂȚI MATERIAL (DIFUZ) ---

        private void nudMatDiffR_ValueChanged(object sender, EventArgs e)
        {
            materialDiffuse[0] = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }
        private void nudMatDiffG_ValueChanged(object sender, EventArgs e)
        {
            materialDiffuse[1] = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }
        private void nudMatDiffB_ValueChanged(object sender, EventArgs e)
        {
            materialDiffuse[2] = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }

        // --- PROPRIETĂȚI MATERIAL (SPECULAR) ---

        private void nudMatSpecR_ValueChanged(object sender, EventArgs e)
        {
            materialSpecular[0] = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }
        private void nudMatSpecG_ValueChanged(object sender, EventArgs e)
        {
            materialSpecular[1] = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }
        private void nudMatSpecB_ValueChanged(object sender, EventArgs e)
        {
            materialSpecular[2] = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }

        // --- SHININESS ---
        private void nudShininess_ValueChanged(object sender, EventArgs e)
        {
            materialShininess = (float)((NumericUpDown)sender).Value;
            GlControl1.Invalidate();
        }
    }

}
