using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GameHelper.Multiplayer.Packets;
using GameHelper;
using GameHelper;
using GameHelper.Base;
using Microsoft.Xna.Framework;

namespace GameHelper.Gui.Forms
{    
    public partial class frmServerBase : Form
    {

        #region Properties
        Queue<Packet> InputQueue = new Queue<Packet>();
        Queue<Packet> OutputQueue = new Queue<Packet>();
        int iLobbyPort;
        int iBasePort;

        #endregion
        
        #region Constructor
        ServerBase game;
        GameBase bGame;


        public frmServerBase(ServerBase g)
        {
            InitializeComponent();
            iLobbyPort = (int)numLobbyPort.Value;
            iBasePort = (int)numBasePort.Value;
            btnStopServer.Enabled = false;

            game = g;
            game.ClientConnected += new GameHelper.Handlers.IntStringEH(game_ClientConnected);
            bGame = game;
            AddXnaPanel(ref bGame);

            bool autoStart = false;
            bool autoMinimize = false;
            if (autoStart)
                StartServer();

            if (autoMinimize)
                this.WindowState = FormWindowState.Minimized;

            Application.Idle += new EventHandler(Application_Idle);
            this.FormClosing += new FormClosingEventHandler(frmServerBase_FormClosing);
        }
        void Application_Idle(object sender, EventArgs e)
        {
            game.ProcessInput();
        }
        void frmServerBase_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        public frmServerBase()
        {
            InitializeComponent();

            bGame = new GameBase();
            AddXnaPanel(ref bGame);
        }

        void game_ClientConnected(int id, string alias)
        {
            if (InvokeRequired)
            {
                this.Invoke(new GameHelper.Handlers.IntStringEH(game_ClientConnected), new object[] { id, alias });
                return;
            }
            lstClients.Items.Add(alias);
        }

        XnaView.XnaPanel XnaPanelMain;
        private void AddXnaPanel(ref GameBase game)
        {
            // 
            // XnaPanelMain
            // 
            this.XnaPanelMain = new XnaView.XnaPanel(ref game);
            //this.XnaPanelMain.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top                 | System.Windows.Forms.AnchorStyles.Bottom                | System.Windows.Forms.AnchorStyles.Left                     | System.Windows.Forms.AnchorStyles.Right);
            this.XnaPanelMain.Dock = DockStyle.Fill;
            this.XnaPanelMain.Debug = false;
            this.XnaPanelMain.DebugPhysics = false;
            this.XnaPanelMain.DrawingEnabled = true;
            this.XnaPanelMain.Location = new System.Drawing.Point(296, 3);
            this.XnaPanelMain.Name = "XnaPanelMain";
            this.XnaPanelMain.PhysicsEnabled = true;
            this.XnaPanelMain.Size = new System.Drawing.Size(596, 366);
            this.XnaPanelMain.TabIndex = 46;
            this.XnaPanelMain.Text = "XnaPanel";
            this.XnaPanelMain.KeyDown += new KeyEventHandler(XnaPanelMain_KeyDown);
            this.XnaPanelMain.KeyUp += new KeyEventHandler(XnaPanelMain_KeyUp);
            this.XnaPanelMain.MouseEnter += new EventHandler(XnaPanelMain_MouseEnter);
            this.XnaPanelMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlMouseDown);
            this.XnaPanelMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlMouseMove);
            this.XnaPanelMain.PreviewKeyDown += new PreviewKeyDownEventHandler(XnaPanelMain_PreviewKeyDown);
            this.XnaPanelMain.BackColor = game.BackColor;
            this.spMain.Panel2.Controls.Add(this.XnaPanelMain);
            //this.Controls.Add(this.XnaPanelMain);
        }

        void XnaPanelMain_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
        }

        void XnaPanelMain_MouseEnter(object sender, EventArgs e)
        {
            XnaPanelMain.Focus();
        }

        void XnaPanelMain_KeyUp(object sender, KeyEventArgs e)
        {
        }

        void XnaPanelMain_KeyDown(object sender, KeyEventArgs e)
        {
        }
        #endregion
        
        #region Events Handlers

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            StartServer();
        }

        private void StartServer()
        {
            game.ListenOnPort = iLobbyPort;
            game.Start();
            btnStartServer.Enabled = false;
            btnStopServer.Enabled = true;
        }


        private void btnStopServer_Click(object sender, EventArgs e)
        {
            btnStopServer.Enabled = false;
            btnStartServer.Enabled = true;
        }
        
        private void ServerApp_MainFormClosed(object sender, FormClosedEventArgs e)
        {
            game.Stop();
        }

        private void numLobbyPort_ValueChanged(object sender, EventArgs e)
        {
            iLobbyPort = (int)numLobbyPort.Value;
        }

        private void numBasePort_ValueChanged(object sender, EventArgs e)
        {
            iBasePort = (int)numBasePort.Value;
        }

        #endregion


        private void tbStep_Scroll(object sender, EventArgs e)
        {
            float value = tbStep.Value;
            if (value <= 10)
                value /= 10;
            else
            {
                value = value - 10;
                // 15 should be around 5 times
                // 16 should be around 6
                // 18 should be around 9 times
            }
            //1 -> 1/10
            //11 -> 10/10
            //20 -> 20/10
            game.SetSimFactor(value);
        }

        #region Mouse Input
        private void pnlMouseEnter(object sender, EventArgs e)
        {
            XnaPanelMain.Focus();
        }
        int lastx;
        int lasty;
        Point dPos;
        private void pnlMouseMove(object sender, MouseEventArgs e)
        {
            if (e.X == lastx && e.Y == lasty)
                return;

            dPos = new Microsoft.Xna.Framework.Point((lastx - e.X), (lasty - e.Y));
            lastx = e.X;
            lasty = e.Y;
            game.ProcessMouseMove(dPos, e, XnaPanelMain.Bounds);
        }
        private void pnlMouseDown(object sender, MouseEventArgs e)
        {
            game.ProcessMouseDown(sender, e, XnaPanelMain.Bounds);
            //XnaPanelMain.ProcessMouseDown(e, XnaPanelMain.Bounds);
        }
        void XnaPanelMain_MouseWheel(object sender, MouseEventArgs e)
        {
            game.AdjustZoom(e.Delta);
        }
        #endregion
    }
}