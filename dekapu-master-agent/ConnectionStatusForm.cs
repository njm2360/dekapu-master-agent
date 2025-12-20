using System.Windows.Forms;

public class ConnectionStatusForm : Form
{
    private readonly Label _label;

    public ConnectionStatusForm()
    {
        Text = "サーバー接続中";
        Width = 300;
        Height = 100;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        TopMost = true;

        _label = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "接続を試みています…"
        };

        Controls.Add(_label);

        FormClosing += OnFormClosing;
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
        }
    }

    public void UpdateStatus(string text)
    {
        if (InvokeRequired)
        {
            Invoke(() => _label.Text = text);
        }
        else
        {
            _label.Text = text;
        }
    }
}
