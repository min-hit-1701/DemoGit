namespace Client
{
    partial class CaroGameForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelBoard = new Panel();
            buttonConnect = new Button();
            textBoxMatchCode = new TextBox();
            SuspendLayout();
            // 
            // panelBoard
            // 
            panelBoard.Location = new Point(12, 12);
            panelBoard.Name = "panelBoard";
            panelBoard.Size = new Size(447, 426);
            panelBoard.TabIndex = 0;
            // 
            // buttonConnect
            // 
            buttonConnect.Location = new Point(475, 355);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new Size(75, 43);
            buttonConnect.TabIndex = 3;
            buttonConnect.Text = "Tham gia";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += ButtonConnect_Click;
            // 
            // textBoxMatchCode
            // 
            textBoxMatchCode.Location = new Point(464, 311);
            textBoxMatchCode.Name = "textBoxMatchCode";
            textBoxMatchCode.Size = new Size(100, 23);
            textBoxMatchCode.TabIndex = 4;
            // 
            // CaroGameForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(576, 450);
            Controls.Add(textBoxMatchCode);
            Controls.Add(buttonConnect);
            Controls.Add(panelBoard);
            Name = "CaroGameForm";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelBoard;
        private TextBox textBoxOpponent;
        private Button buttonConnect;
        private TextBox textBoxMatchCode;
    }
}
