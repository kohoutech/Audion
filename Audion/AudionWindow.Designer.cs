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
            this.manageModuleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsPluginMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildPluginMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runPluginMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AudionStatus = new System.Windows.Forms.StatusStrip();
            this.audionToolStrip = new System.Windows.Forms.ToolStrip();
            this.newPluginToolButton = new System.Windows.Forms.ToolStripButton();
            this.openPluginToolButton = new System.Windows.Forms.ToolStripButton();
            this.savePluginToolButton = new System.Windows.Forms.ToolStripButton();
            this.openPatchDialog = new System.Windows.Forms.OpenFileDialog();
            this.savePatchDialog = new System.Windows.Forms.SaveFileDialog();
            this.hostMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsHostMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.audionMenu.SuspendLayout();
            this.audionToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // audionMenu
            // 
            this.audionMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.modulesMenuItem,
            this.pluginMenuItem,
            this.hostMenuItem,
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
            // manageModuleMenuItem
            // 
            this.manageModuleMenuItem.Name = "manageModuleMenuItem";
            this.manageModuleMenuItem.Size = new System.Drawing.Size(180, 22);
            this.manageModuleMenuItem.Text = "&Manage...";
            this.manageModuleMenuItem.Click += new System.EventHandler(this.manageModuleMenuItem_Click);
            // 
            // pluginMenuItem
            // 
            this.pluginMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsPluginMenuItem,
            this.buildPluginMenuItem,
            this.runPluginMenuItem});
            this.pluginMenuItem.Name = "pluginMenuItem";
            this.pluginMenuItem.Size = new System.Drawing.Size(53, 20);
            this.pluginMenuItem.Text = "Plugin";
            // 
            // settingsPluginMenuItem
            // 
            this.settingsPluginMenuItem.Name = "settingsPluginMenuItem";
            this.settingsPluginMenuItem.Size = new System.Drawing.Size(180, 22);
            this.settingsPluginMenuItem.Text = "&Settings";
            this.settingsPluginMenuItem.Click += new System.EventHandler(this.settingsPluginMenuItem_Click);
            // 
            // buildPluginMenuItem
            // 
            this.buildPluginMenuItem.Name = "buildPluginMenuItem";
            this.buildPluginMenuItem.Size = new System.Drawing.Size(180, 22);
            this.buildPluginMenuItem.Text = "&Build";
            this.buildPluginMenuItem.Click += new System.EventHandler(this.buildPluginMenuItem_Click);
            // 
            // runPluginMenuItem
            // 
            this.runPluginMenuItem.Name = "runPluginMenuItem";
            this.runPluginMenuItem.Size = new System.Drawing.Size(180, 22);
            this.runPluginMenuItem.Text = "&Run";
            this.runPluginMenuItem.Click += new System.EventHandler(this.runPluginMenuItem_Click);
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
            this.newPluginToolButton,
            this.openPluginToolButton,
            this.savePluginToolButton});
            this.audionToolStrip.Location = new System.Drawing.Point(0, 24);
            this.audionToolStrip.Name = "audionToolStrip";
            this.audionToolStrip.Size = new System.Drawing.Size(584, 25);
            this.audionToolStrip.TabIndex = 2;
            this.audionToolStrip.Text = "toolStrip1";
            // 
            // newPluginToolButton
            // 
            this.newPluginToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newPluginToolButton.Image = ((System.Drawing.Image)(resources.GetObject("newPluginToolButton.Image")));
            this.newPluginToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newPluginToolButton.Name = "newPluginToolButton";
            this.newPluginToolButton.Size = new System.Drawing.Size(23, 22);
            this.newPluginToolButton.Text = "&New Plugin";
            this.newPluginToolButton.Click += new System.EventHandler(this.newPluginToolButton_Click);
            // 
            // openPluginToolButton
            // 
            this.openPluginToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openPluginToolButton.Image = ((System.Drawing.Image)(resources.GetObject("openPluginToolButton.Image")));
            this.openPluginToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openPluginToolButton.Name = "openPluginToolButton";
            this.openPluginToolButton.Size = new System.Drawing.Size(23, 22);
            this.openPluginToolButton.Text = "&Open Plugin";
            this.openPluginToolButton.Click += new System.EventHandler(this.openPluginToolButton_Click);
            // 
            // savePluginToolButton
            // 
            this.savePluginToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.savePluginToolButton.Image = ((System.Drawing.Image)(resources.GetObject("savePluginToolButton.Image")));
            this.savePluginToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.savePluginToolButton.Name = "savePluginToolButton";
            this.savePluginToolButton.Size = new System.Drawing.Size(23, 22);
            this.savePluginToolButton.Text = "&Save Plugin";
            this.savePluginToolButton.Click += new System.EventHandler(this.savePluginToolButton_Click);
            // 
            // hostMenuItem
            // 
            this.hostMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsHostMenuItem});
            this.hostMenuItem.Name = "hostMenuItem";
            this.hostMenuItem.Size = new System.Drawing.Size(44, 20);
            this.hostMenuItem.Text = "Host";
            // 
            // settingsHostMenuItem
            // 
            this.settingsHostMenuItem.Name = "settingsHostMenuItem";
            this.settingsHostMenuItem.Size = new System.Drawing.Size(180, 22);
            this.settingsHostMenuItem.Text = "&Settings";
            this.settingsHostMenuItem.Click += new System.EventHandler(this.settingsHostMenuItem_Click);
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
            this.Text = "Audion";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AudionWindow_FormClosed);
            this.Load += new System.EventHandler(this.AudionWindow_Load);
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
        private System.Windows.Forms.ToolStripButton newPluginToolButton;
        private System.Windows.Forms.ToolStripButton openPluginToolButton;
        private System.Windows.Forms.ToolStripButton savePluginToolButton;
        private System.Windows.Forms.ToolStripMenuItem manageModuleMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pluginMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runPluginMenuItem;
        private System.Windows.Forms.OpenFileDialog openPatchDialog;
        private System.Windows.Forms.SaveFileDialog savePatchDialog;
        private System.Windows.Forms.ToolStripMenuItem buildPluginMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsPluginMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hostMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsHostMenuItem;
    }
}

