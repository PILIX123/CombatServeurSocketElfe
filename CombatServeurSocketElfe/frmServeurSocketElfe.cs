using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CombatServeurSocketElfe.Classes;

namespace CombatServeurSocketElfe
{
    public partial class frmServeurSocketElfe : Form
    {
        Random m_r;
        Nain m_nain;
        Elfe m_elfe;
        TcpListener m_ServerListener;
        Socket m_client;

        public frmServeurSocketElfe()
        {
            InitializeComponent();
            m_r = new Random();
            Reset();
            btnReset.Enabled = false;
            //Démarre un serveur de socket (TcpListener)
            
            m_ServerListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            m_ServerListener.Start();
            lstReception.Items.Add("Serveur démarré !");
            lstReception.Items.Add("PRESSER : << attendre un client >>");
            lstReception.Update();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        void Reset()
        {
            m_nain = new Nain(1, 0, 0);
            picNain.Image = m_nain.Avatar;
            AfficheStatNain();

            m_elfe = new Elfe(m_r.Next(10, 20), m_r.Next(2, 6), m_r.Next(2, 6));
            picElfe.Image = m_elfe.Avatar;
            AfficheStatElfe();
 
            lstReception.Items.Clear();
        }

        void AfficheStatNain()
        {
            lblArmeNain.Text = m_nain.Arme;
            lblForceNain.Text = m_nain.Force.ToString();
            lblVieNain.Text = m_nain.Vie.ToString();

            
            this.Update(); // pour s'assurer de l'affichage via le thread
        }
        void AfficheStatElfe()
        {
            lblVieElfe.Text = m_elfe.Vie.ToString();
            lblForceElfe.Text = m_elfe.Force.ToString();
            lblSortElfe.Text = m_elfe.Sort.ToString();


            this.Update(); // pour s'assurer de l'affichage via le thread
        }
        private void btnReset_Click(object sender, EventArgs e)
        {

            Reset();
        }     

        private void btnAttente_Click(object sender, EventArgs e)
        {
            // Combat par un thread
            ThreadStart threadStart = new ThreadStart(Combat);
            Thread thread = new Thread(threadStart);
            thread.Start();
        }
        public void Combat() 
        {
            // déclarations de variables locales 
            string reponseServeur;
            string receptionClient;
            int nbOctetReception;
            int vie, force;
            string arme;
            string[] splitter;
            byte[] tByteReception = new byte[50];
            ASCIIEncoding textByte = new ASCIIEncoding();
            byte[] tByteEnvoie;
            try
            {
                while (m_nain.Vie>0 || m_elfe.Vie>0)
                {
                    m_client = m_ServerListener.AcceptSocket();
                    lstReception.Items.Add("Client branché");
                    lstReception.Update();
                    Thread.Sleep(500);

                    nbOctetReception = m_client.Receive(tByteReception);
                    receptionClient = Encoding.ASCII.GetString(tByteReception);
                    lstReception.Items.Add("du client: " + receptionClient);
                    lstReception.Update();
                    splitter= receptionClient.Split(';');
                    vie = Int32.Parse(splitter[0]);
                    force = Int32.Parse(splitter[1]);
                    arme = splitter[2];
                    m_nain.Vie = vie;
                    m_nain.Force = force;
                    m_nain.Arme = arme;
                    
                    AfficheStatNain();
                    //faire m_nain frappe
                    m_nain.Frapper(m_elfe);
                    MessageBox.Show("Serveur: Frapper l'elfe");
                    AfficheStatElfe();
                    m_elfe.LancerSort(m_nain);
                    //faire m_elfe lance sort
                    MessageBox.Show("Serveur: Lancer un sort au nain");
                    AfficheStatElfe();
                    AfficheStatNain();
                    //vieNain;forceNain;armeNain|vieElfe;forceElfe;sortElfe
                    reponseServeur = m_nain.Vie.ToString() + ";" + m_nain.Force.ToString() + ";" + m_nain.Arme + "|" + m_elfe.Vie.ToString() + ";" + m_elfe.Force.ToString() +";"+m_elfe.Sort ;
                    lstReception.Items.Add(reponseServeur);
                    lstReception.Update();
                    tByteEnvoie = textByte.GetBytes(reponseServeur);
                    m_client.Send(tByteEnvoie);
                    Thread.Sleep(500);
                    //check gangnant
                    m_client.Close();

                }
                // tous le code de traitement
                if (m_elfe.Vie <= 0)
                {
                    reponseServeur = "Nain gagnant";
                    lstReception.Items.Add(reponseServeur);
                    lstReception.Update();
                }
                else
                {
                    reponseServeur = "Elfe gagnant";
                    lstReception.Items.Add(reponseServeur);
                    lstReception.Update();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            // il faut avoir un objet elfe et un objet nain instanciés
            m_elfe.Vie = 0;
            m_nain.Vie = 0;
            try
            {
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void frmServeurSocketElfe_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnFermer_Click(sender,e);
            try
            {
                // il faut avoir un objet TCPListener existant
                if(m_ServerListener.Pending())
                {
                    m_ServerListener.Stop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }
    }
}
