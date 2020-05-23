namespace Audion
{
    partial class AudionWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudionWindow));
            this.audionMenu = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newPluginFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openPluginFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.savePluginFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.savePluginAsFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modulesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AudionStatus = new System.Windows.Forms.StatusStrip();
            this.audionToolStrip = new System.Windows.Forms.ToolStrip();
            this.newToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.openToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.pluginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runPluginMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageModuleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.audionMenu.SuspendLayout();
            this.audionToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // audionMenu
            // 
            this.audionMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.modulesMenuItem,
            this.pluginToolStripMenuItem,
            this.helpMenuItem});
            this.audionMenu.Location = new System.Drawing.Point(0, 0);
            this.audionMenu.Name = "audionMenu";
            this.audionMenu.Size = new System.Drawing.Size(584, 24);
            this.audionMenu.TabIndex = 0;
            this.audionMenu.Text = "menuStrip1";
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newPluginFileMenuItem,
            this.openPluginFileMenuItem,
            this.toolStripSeparator,
            this.savePluginFileMenuItem,
            this.savePluginAsFileMenuItem,
            this.toolStripSeparator1,
            this.exitFileMenuItem});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileMenuItem.Text = "&File";
            // 
            // newPluginFileMenuItem
            // 
            this.newPluginFileMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.newPluginFileMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newPluginFileMenuItem.Image")));
            this.newPluginFileMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newPluginFileMenuItem.Name = "newPluginFileMenuItem";
            this.newPluginFileMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newPluginFileMenuItem.Size = new System.Drawing.Size(183, 22);
            this.newPluginFileMenuItem.Text = "&New Plugin";
            this.newPluginFileMenuItem.Click += new System.EventHandler(this.newPluginFileMenuItem_Click);
            // 
            // openPluginFileMenuItem
            // 
            this.openPluginFileMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.openPluginFileMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openPluginFileMenuItem.Image")));
            this.openPluginFileMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openPluginFileMenuItem.Name = "openPluginFileMenuItem";
            this.openPluginFileMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openPluginFileMenuItem.Size = new System.Drawing.Size(183, 22);
            this.openPluginFileMenuItem.Text = "&Open Plugin";
            this.openPluginFileMenuItem.Click += new System.EventHandler(this.openPluginFileMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(180, 6);
            // 
            // savePluginFileMenuItem
            // 
            this.savePluginFileMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.savePluginFileMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("savePluginFileMenuItem.Image")));
            this.savePluginFileMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.savePluginFileMenuItem.Name = "savePluginFileMenuItem";
            this.savePluginFileMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.savePluginFileMenuItem.Size = new System.Drawing.Size(183, 22);
            this.savePluginFileMenuItem.Text = "&Save Plugin";
            this.savePluginFileMenuItem.Click += new System.EventHandler(this.savePluginFileMenuItem_Click);
            // 
            // savePluginAsFileMenuItem
            // 
            this.savePluginAsFileMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.savePluginAsFileMenuItem.Name = "savePluginAsFileMenuItem";
            this.savePluginAsFileMenuItem.Size = new System.Drawing.Size(183, 22);
            this.savePluginAsFileMenuItem.Text = "Save Plugin &As";
            this.savePluginAsFileMenuItem.Click += new System.EventHandler(this.savePluginAsFileMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(180, 6);
            // 
            // exitFileMenuItem
            // 
            this.exitFileMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exitFileMenuItem.Name = "exitFileMenuItem";
            this.exitFileMenuItem.Size = new System.Drawing.Size(183, 22);
            this.exitFileMenuItem.Text = "E&xit";
            this.exitFileMenuItem.Click += new System.EventHandler(this.exitFileMenuItem_Click);
            // 
            // modulesMenuItem
            // 
            this.modulesMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageModuleMenuItem});
            this.modulesMenuItem.Name = "modulesMenuItem";
            this.modulesMenuItem.Size = new System.Drawing.Size(65, 20);
            this.modulesMenuItem.Text = "&Modules";
            // 
            // helpMenuItem
            // 
            this.helpMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutHelpMenuItem});
            this.helpMenuItem.Name = "helpMenuItem";
            this.helpMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpMenuItem.Text = "&Help";
            // 
            // aboutHelpMenuItem
            // 
            this.aboutHelpMenuItem.Name = "aboutHelpMenuItem";
            this.aboutHelpMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutHelpMenuItem.Text = "&About...";
            this.aboutHelpMenuItem.Click += new System.EventHandler(this.aboutHelpMenuItem_Click);
            // 
            // AudionStatus
            // 
            this.AudionStatus.Location = new System.Drawing.Point(0, 339);
            this.AudionStatus.Name = "AudionStatus";
            this.AudionStatus.Size = new System.Drawing.Size(584, 22);
            this.AudionStatus.TabIndex = 1;
            this.AudionStatus.Text = "statusStrip1";
            // 
            // audionToolStrip
            // 
            this.audionToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripButton,
            this.openToolStripButton,
            this.saveToolStripButton});
            this.audionToolStrip.Location = new System.Drawing.Point(0, 24);
            this.audionToolStrip.Name = "audionToolStrip";
            this.audionToolStrip.Size = new System.Drawing.Size(584, 25);
            this.audionToolStrip.TabIndex = 2;
            this.audionToolStrip.Text = "toolStrip1";
            // 
            // newToolStripButton
            // 
            this.newToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripButton.Image")));
            this.newToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripButton.Name = "newToolStripButton";
            this.newToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.newToolStripButton.Text = "&New Unit";
            // 
            // openToolStripButton
            // 
            this.openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripButton.Image")));
            this.openToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripButton.Name = "openToolStripButton";
            this.openToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.openToolStripButton.Text = "&Open Unit";
            // 
            // saveToolStripButton
            // 
            this.saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripButton.Image")));
            this.saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripButton.Name = "saveToolStripButton";
            this.saveToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveToolStripButton.Text = "&Save Unit";
            // 
            // pluginToolStripMenuItem
            // 
            this.pluginToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runPluginMenuItem});
            this.pluginToolStripMenuItem.Name = "pluginToolStripMenuItem";
            this.pluginToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.pluginToolStripMenuItem.Text = "Plugin";
            // 
            // runPluginMenuItem
            // 
            this.runPluginMenuItem.Name = "runPluginMenuItem";
            this.runPluginMenuItem.Size = new System.Drawing.Size(180, 22);
            this.runPluginMenuItem.Text = "&Run";
            this.runPluginMenuItem.Click += new System.EventHandler(this.runPluginMenuItem_Click);
            // 
            // manageModuleMenuItem
            // 
            this.manageModuleMenuItem.Name = "manageModuleMenuItem";
            this.manageModuleMenuItem.Size = new System.Drawing.Size(180, 22);
            this.manageModuleMenuItem.Text = "&Manage...";
            this.manageModuleMenuItem.Click += new System.EventHandler(this.manageModuleMenuItem_Click);
            // 
            // AudionWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.audionToolStrip);
            this.Controls.Add(this.AudionStatus);
            this.Controls.Add(this.audionMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.audionMenu;
            this.Name = "AudionWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Audion";
            this.audionMenu.ResumeLayout(false);
            this.audionMenu.PerformLayout();
            this.audionToolStrip.ResumeLayout(false);
            this.audionToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip audionMenu;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newPluginFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openPluginFileMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem savePluginFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem savePluginAsFileMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modulesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutHelpMenuItem;
        private System.Windows.Forms.StatusStrip AudionStatus;
        private System.Windows.Forms.ToolStrip audionToolStrip;
        private System.Windows.Forms.ToolStripButton newToolStripButton;
        private System.Windows.Forms.ToolStripButton openToolStripButton;
        private System.Windows.Forms.ToolStripButton saveToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem manageModuleMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pluginToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runPluginMenuItem;
    }
}

