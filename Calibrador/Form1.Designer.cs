namespace Calibrador
{
    partial class Form1
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDesconectar = new System.Windows.Forms.Button();
            this.btnConectar = new System.Windows.Forms.Button();
            this.textIpJiga = new System.Windows.Forms.TextBox();
            this.txtIpDut = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblCorrenteDutN = new System.Windows.Forms.Label();
            this.lblCorrenteDutC = new System.Windows.Forms.Label();
            this.lblCorrenteDutB = new System.Windows.Forms.Label();
            this.lblTensaoDutVN = new System.Windows.Forms.Label();
            this.lblTensaoDutVC = new System.Windows.Forms.Label();
            this.lblTensaoDutVB = new System.Windows.Forms.Label();
            this.lblFpDut = new System.Windows.Forms.Label();
            this.lblCorrenteDut = new System.Windows.Forms.Label();
            this.lblTensaoDut = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblFpJiga = new System.Windows.Forms.Label();
            this.lblCorrenteJiga = new System.Windows.Forms.Label();
            this.lblTensaoJiga = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnCalibrarEscala = new System.Windows.Forms.Button();
            this.cmbEscalaCorrente = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDesconectar);
            this.groupBox1.Controls.Add(this.btnConectar);
            this.groupBox1.Controls.Add(this.textIpJiga);
            this.groupBox1.Controls.Add(this.txtIpDut);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // btnDesconectar
            // 
            resources.ApplyResources(this.btnDesconectar, "btnDesconectar");
            this.btnDesconectar.Name = "btnDesconectar";
            this.btnDesconectar.UseVisualStyleBackColor = true;
            this.btnDesconectar.Click += new System.EventHandler(this.btnDesconectar_Click);
            // 
            // btnConectar
            // 
            resources.ApplyResources(this.btnConectar, "btnConectar");
            this.btnConectar.Name = "btnConectar";
            this.btnConectar.UseVisualStyleBackColor = true;
            this.btnConectar.Click += new System.EventHandler(this.btnConectar_Click);
            // 
            // textIpJiga
            // 
            resources.ApplyResources(this.textIpJiga, "textIpJiga");
            this.textIpJiga.Name = "textIpJiga";
            // 
            // txtIpDut
            // 
            resources.ApplyResources(this.txtIpDut, "txtIpDut");
            this.txtIpDut.Name = "txtIpDut";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblCorrenteDutN);
            this.groupBox2.Controls.Add(this.lblCorrenteDutC);
            this.groupBox2.Controls.Add(this.lblCorrenteDutB);
            this.groupBox2.Controls.Add(this.lblTensaoDutVN);
            this.groupBox2.Controls.Add(this.lblTensaoDutVC);
            this.groupBox2.Controls.Add(this.lblTensaoDutVB);
            this.groupBox2.Controls.Add(this.lblFpDut);
            this.groupBox2.Controls.Add(this.lblCorrenteDut);
            this.groupBox2.Controls.Add(this.lblTensaoDut);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // lblCorrenteDutN
            // 
            resources.ApplyResources(this.lblCorrenteDutN, "lblCorrenteDutN");
            this.lblCorrenteDutN.Name = "lblCorrenteDutN";
            // 
            // lblCorrenteDutC
            // 
            resources.ApplyResources(this.lblCorrenteDutC, "lblCorrenteDutC");
            this.lblCorrenteDutC.Name = "lblCorrenteDutC";
            // 
            // lblCorrenteDutB
            // 
            resources.ApplyResources(this.lblCorrenteDutB, "lblCorrenteDutB");
            this.lblCorrenteDutB.Name = "lblCorrenteDutB";
            // 
            // lblTensaoDutVN
            // 
            resources.ApplyResources(this.lblTensaoDutVN, "lblTensaoDutVN");
            this.lblTensaoDutVN.Name = "lblTensaoDutVN";
            // 
            // lblTensaoDutVC
            // 
            resources.ApplyResources(this.lblTensaoDutVC, "lblTensaoDutVC");
            this.lblTensaoDutVC.Name = "lblTensaoDutVC";
            // 
            // lblTensaoDutVB
            // 
            resources.ApplyResources(this.lblTensaoDutVB, "lblTensaoDutVB");
            this.lblTensaoDutVB.Name = "lblTensaoDutVB";
            // 
            // lblFpDut
            // 
            resources.ApplyResources(this.lblFpDut, "lblFpDut");
            this.lblFpDut.Name = "lblFpDut";
            // 
            // lblCorrenteDut
            // 
            resources.ApplyResources(this.lblCorrenteDut, "lblCorrenteDut");
            this.lblCorrenteDut.Name = "lblCorrenteDut";
            // 
            // lblTensaoDut
            // 
            resources.ApplyResources(this.lblTensaoDut, "lblTensaoDut");
            this.lblTensaoDut.Name = "lblTensaoDut";
          
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblFpJiga);
            this.groupBox3.Controls.Add(this.lblCorrenteJiga);
            this.groupBox3.Controls.Add(this.lblTensaoJiga);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // lblFpJiga
            // 
            resources.ApplyResources(this.lblFpJiga, "lblFpJiga");
            this.lblFpJiga.Name = "lblFpJiga";
            // 
            // lblCorrenteJiga
            // 
            resources.ApplyResources(this.lblCorrenteJiga, "lblCorrenteJiga");
            this.lblCorrenteJiga.Name = "lblCorrenteJiga";
            // 
            // lblTensaoJiga
            // 
            resources.ApplyResources(this.lblTensaoJiga, "lblTensaoJiga");
            this.lblTensaoJiga.Name = "lblTensaoJiga";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnCalibrarEscala);
            this.groupBox4.Controls.Add(this.cmbEscalaCorrente);
            this.groupBox4.Controls.Add(this.label3);
            resources.ApplyResources(this.groupBox4, "groupBox4");
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.TabStop = false;
            // 
            // btnCalibrarEscala
            // 
            resources.ApplyResources(this.btnCalibrarEscala, "btnCalibrarEscala");
            this.btnCalibrarEscala.Name = "btnCalibrarEscala";
            this.btnCalibrarEscala.UseVisualStyleBackColor = true;
            // 
            // cmbEscalaCorrente
            // 
            this.cmbEscalaCorrente.FormattingEnabled = true;
            this.cmbEscalaCorrente.Items.AddRange(new object[] {
            resources.GetString("cmbEscalaCorrente.Items"),
            resources.GetString("cmbEscalaCorrente.Items1"),
            resources.GetString("cmbEscalaCorrente.Items2"),
            resources.GetString("cmbEscalaCorrente.Items3"),
            resources.GetString("cmbEscalaCorrente.Items4")});
            resources.ApplyResources(this.cmbEscalaCorrente, "cmbEscalaCorrente");
            this.cmbEscalaCorrente.Name = "cmbEscalaCorrente";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // rtbLog
            // 
            this.rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.rtbLog, "rtbLog");
            this.rtbLog.Name = "rtbLog";
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textIpJiga;
        private System.Windows.Forms.TextBox txtIpDut;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        //private System.Windows.Forms.Button btnConectar;
        internal System.Windows.Forms.Button btnConectar;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblFpDut;
        private System.Windows.Forms.Label lblCorrenteDut;
        private System.Windows.Forms.Label lblTensaoDut;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblFpJiga;
        private System.Windows.Forms.Label lblCorrenteJiga;
        private System.Windows.Forms.Label lblTensaoJiga;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnCalibrarEscala;
        private System.Windows.Forms.ComboBox cmbEscalaCorrente;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblTensaoDutVN;
        private System.Windows.Forms.Label lblTensaoDutVC;
        private System.Windows.Forms.Label lblTensaoDutVB;
        private System.Windows.Forms.Button btnDesconectar;
        private System.Windows.Forms.Label lblCorrenteDutN;
        private System.Windows.Forms.Label lblCorrenteDutC;
        private System.Windows.Forms.Label lblCorrenteDutB;
    }
}

