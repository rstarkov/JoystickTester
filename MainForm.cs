using Windows.Gaming.Input;

namespace JoystickTester;

// potential improvements:
// - add spacing between charts
// - show buttons and switches
// - show ID/name
// - show all active joysticks at once
// - autostart and correctly support plug/unplug

public partial class MainForm : Form
{
    private RawGameController _controller;
    private Queue<double>[] _history;
    private HashSet<double>[] _distinct;
    private bool[] _buttons;
    private GameControllerSwitchPosition[] _switches;
    private double[] _axes;
    private Bitmap _img;
    private Font _font = new Font("Segoe UI", 10f);

    public MainForm()
    {
        InitializeComponent();
    }

    private void Form1_Click(object sender, EventArgs e)
    {
        ArcadeStick.ArcadeSticks.ToList();
        FlightStick.FlightSticks.ToList();
        Gamepad.Gamepads.ToList();
        RacingWheel.RacingWheels.ToList();
        _controller = RawGameController.RawGameControllers.Single();
        _history = new Queue<double>[_controller.AxisCount];
        _distinct = new HashSet<double>[_controller.AxisCount];
        for (int i = 0; i < _history.Length; i++)
        {
            _history[i] = new();
            _distinct[i] = new();
        }
        _buttons = new bool[_controller.ButtonCount];
        _switches = new GameControllerSwitchPosition[_controller.SwitchCount];
        _axes = new double[_controller.AxisCount];
        timer1.Enabled = true;
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (_img == null || _img.Width != ClientSize.Width || _img.Height != ClientSize.Height)
            _img = new Bitmap(ClientSize.Width, ClientSize.Height);
        _controller.GetCurrentReading(_buttons, _switches, _axes);
        for (int i = 0; i < _axes.Length; i++)
        {
            _history[i].Enqueue(_axes[i]);
            _distinct[i].Add(_axes[i]);
            while (_history[i].Count > _img.Width)
                _history[i].Dequeue();
        }
        using var g = Graphics.FromImage(_img);
        int height = _img.Height / _axes.Length;
        for (int i = 0; i < _axes.Length; i++)
            g.FillRectangle(i % 2 == 0 ? Brushes.MidnightBlue : Brushes.Indigo, 0, i * height, _img.Width, (i + 1) * height);
        for (int i = 0; i < _axes.Length; i++)
        {
            if (_history[i].Count >= 2)
                g.DrawLines(Pens.White, _history[i].Select((p, x) => new Point(x, height * i + (int)(height * (1 - p)))).ToArray());
            g.DrawString($"Axis {i}: {_axes[i]:0.000} ({_distinct[i].Count} distinct values)", _font, Brushes.White, 10, height * i);
        }
        Invalidate();
    }

    private void Form1_Paint(object sender, PaintEventArgs e)
    {
        if (_img == null) return;
        e.Graphics.DrawImageUnscaled(_img, 0, 0);
    }
}