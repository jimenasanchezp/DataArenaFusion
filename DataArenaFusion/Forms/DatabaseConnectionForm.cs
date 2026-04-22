using System.Drawing;
using DataArenaFusion.Core.Models;
using DataArenaFusion.Core.Services.Database;

namespace DataArenaFusion.Forms
{
    public sealed class DatabaseConnectionForm : Form
    {
        private readonly IDatabaseConnectionService _service;
        private readonly TextBox txtHost;
        private readonly NumericUpDown nudPort;
        private readonly TextBox txtDatabase;
        private readonly TextBox txtUser;
        private readonly TextBox txtPassword;
        private readonly CheckBox chkSsl;
        private readonly Label lblEstado;

        public DatabaseConnectionSettings Settings { get; private set; }

        public DatabaseConnectionForm(DatabaseProvider provider, DatabaseConnectionSettings? initialSettings = null)
        {
            _service = DatabaseConnectionServiceFactory.Create(provider);
            Settings = initialSettings?.Clone() ?? CreateDefaultSettings(provider);

            Text = $"Conexion {_service.DisplayName}";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            Font = new Font("Trebuchet MS", 9F);
            ClientSize = new Size(460, 310);

            var titulo = new Label
            {
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(16, 16),
                Text = $"Configura la conexion para {_service.DisplayName}"
            };

            var panelCampos = new TableLayoutPanel
            {
                Location = new Point(16, 48),
                Size = new Size(424, 182),
                ColumnCount = 2,
                RowCount = 5,
                AutoSize = false
            };
            panelCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            panelCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));

            txtHost = new TextBox { Dock = DockStyle.Fill, Text = Settings.Host };
            nudPort = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 1,
                Maximum = 65535,
                Value = Settings.Port > 0 ? Settings.Port : _service.DefaultPort
            };
            txtDatabase = new TextBox { Dock = DockStyle.Fill, Text = Settings.Database };
            txtUser = new TextBox { Dock = DockStyle.Fill, Text = Settings.Username };
            txtPassword = new TextBox { Dock = DockStyle.Fill, Text = Settings.Password, UseSystemPasswordChar = true };
            chkSsl = new CheckBox
            {
                AutoSize = true,
                Text = "Usar SSL",
                Checked = Settings.UseSsl,
                Dock = DockStyle.Left
            };

            AddRow(panelCampos, 0, "Servidor / IP", txtHost);
            AddRow(panelCampos, 1, "Puerto", nudPort);
            AddRow(panelCampos, 2, "Base de datos", txtDatabase);
            AddRow(panelCampos, 3, "Usuario", txtUser);
            AddRow(panelCampos, 4, "Contrasena", CreatePasswordPanel());

            var botones = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 48,
                Padding = new Padding(0, 0, 16, 8)
            };

            var btnAceptar = new Button
            {
                Text = "Aceptar",
                Width = 90,
                DialogResult = DialogResult.OK
            };
            btnAceptar.Click += (_, _) => GuardarSettings();

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Width = 90,
                DialogResult = DialogResult.Cancel
            };

            var btnProbar = new Button
            {
                Text = "Probar",
                Width = 90
            };
            btnProbar.Click += (_, _) => ProbarConexion();

            botones.Controls.Add(btnAceptar);
            botones.Controls.Add(btnCancelar);
            botones.Controls.Add(btnProbar);

            lblEstado = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Height = 42,
                Padding = new Padding(16, 8, 16, 0),
                ForeColor = Color.FromArgb(94, 129, 172),
                Text = "Completa los datos y prueba la conexion antes de aceptar."
            };

            Controls.Add(botones);
            Controls.Add(lblEstado);
            Controls.Add(panelCampos);
            Controls.Add(titulo);

            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;
        }

        private Panel CreatePasswordPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            txtPassword.Dock = DockStyle.Top;
            chkSsl.Dock = DockStyle.Bottom;
            panel.Controls.Add(chkSsl);
            panel.Controls.Add(txtPassword);
            return panel;
        }

        private static void AddRow(TableLayoutPanel panel, int row, string label, Control input)
        {
            var lbl = new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 0, 0)
            };

            input.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            input.Margin = new Padding(0, 4, 0, 0);

            panel.Controls.Add(lbl, 0, row);
            panel.Controls.Add(input, 1, row);
        }

        private static DatabaseConnectionSettings CreateDefaultSettings(DatabaseProvider provider)
        {
            return new DatabaseConnectionSettings
            {
                Provider = provider,
                Host = "localhost",
                Port = provider == DatabaseProvider.MariaDb ? 3306 : 5432,
                Database = string.Empty,
                Username = string.Empty,
                Password = string.Empty,
                UseSsl = false
            };
        }

        private void GuardarSettings()
        {
            Settings = new DatabaseConnectionSettings
            {
                Provider = Settings.Provider,
                Host = txtHost.Text.Trim(),
                Port = (int)nudPort.Value,
                Database = txtDatabase.Text.Trim(),
                Username = txtUser.Text.Trim(),
                Password = txtPassword.Text,
                UseSsl = chkSsl.Checked
            };
        }

        private void ProbarConexion()
        {
            GuardarSettings();

            if (string.IsNullOrWhiteSpace(Settings.Host) ||
                string.IsNullOrWhiteSpace(Settings.Database) ||
                string.IsNullOrWhiteSpace(Settings.Username))
            {
                lblEstado.Text = "Completa al menos servidor, base de datos y usuario.";
                lblEstado.ForeColor = Color.FromArgb(191, 97, 106);
                return;
            }

            if (_service.TryTestConnection(Settings, out var mensaje))
            {
                lblEstado.Text = mensaje;
                lblEstado.ForeColor = Color.FromArgb(163, 190, 140);
            }
            else
            {
                lblEstado.Text = mensaje;
                lblEstado.ForeColor = Color.FromArgb(191, 97, 106);
            }
        }
    }
}
