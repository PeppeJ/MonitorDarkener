using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorDarkener
{
    public partial class Form1 : Form
    {
        Screen[] screens;
        Screen primary = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            CheckDisplayCount();
            Console.WriteLine(SystemColors.Control);
            Console.WriteLine(primaryDisplay.BackColor);
        }

        private void CheckDisplayCount()
        {
            ResetDarkScreens();
            foreach (Button b in buttonPanel.Controls.OfType<Button>().ToList())
            {
                if (b.Name != primaryDisplay.Name)
                    buttonPanel.Controls.Remove(b);
            }

            Size buttonSize = primaryDisplay.Size;
            Size buttonsOffset = new Size(25, 25);
            screens = Screen.AllScreens;

            foreach (Screen s in screens)
                if (s.Primary)
                {
                    primary = s;
                    break;
                }

            foreach (Screen s in screens)
            {
                if (!s.Primary)
                {
                    Button newButton = new Button();
                    newButton.Name = string.Format("{0}_{1}", "Button", s.DeviceName);

                    bool isRight;
                    bool isDown;
                    Rectangle screenBounds = s.Bounds;
                    isRight = screenBounds.X > 0 ? true : false;
                    isDown = screenBounds.Y > 0 ? true : false;

                    decimal offsetRightAmount = 0;
                    if (screenBounds.X != 0)
                    {
                        offsetRightAmount = (decimal)screenBounds.Width / (decimal)primary.Bounds.Width;
                        offsetRightAmount *= isRight ? 1 : -1;
                        offsetRightAmount = RoundNearestInteger(offsetRightAmount);
                    }

                    decimal offsetDownAmount = 0;
                    if (screenBounds.Y != 0)
                    {
                        offsetDownAmount = (decimal)screenBounds.Width / (decimal)primary.Bounds.Width;
                        offsetDownAmount *= isDown ? 1 : -1;
                        offsetDownAmount = RoundNearestInteger(offsetDownAmount);
                    }

                    string buttonText;
                    buttonText = string.Format("Screen: {2}{0} {1}", (int)offsetRightAmount, (int)offsetDownAmount, Environment.NewLine);
                    newButton.Text = buttonText;

                    newButton.Location = primaryDisplay.Location;
                    Size offset = new Size();
                    offset.Width = buttonSize.Width;
                    offset.Width *= (int)offsetRightAmount != 0 ? (int)offsetRightAmount : 0;
                    offset.Width += offsetRightAmount != 0 ? (offsetRightAmount > 0 ? 1 : -1) : 0;

                    offset.Height = buttonSize.Height;
                    offset.Height *= (int)offsetDownAmount != 0 ? (int)offsetDownAmount : 0;
                    offset.Height += offsetDownAmount != 0 ? (offsetDownAmount > 0 ? 1 : -1) : 0;

                    newButton.Size = buttonSize;

                    Point loc = Point.Add(newButton.Location, offset);
                    newButton.Location = loc;

                    newButton.Click += new System.EventHandler(DarkenScreen);

                    this.buttonPanel.Controls.Add(newButton);
                }
            }
        }

        private decimal RoundNearestInteger(decimal num)
        {
            num = num > 0 ? Math.Floor(num) : Math.Ceiling(num);
            return num;
        }

        public void DarkenScreen(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (button.BackColor != SystemColors.ControlDark)
            {

                string screenOffset = button.Text.Substring("Screen: ".Length);
                screenOffset = screenOffset.Trim(new char[] { '\r', '\n' });

                string[] splitText = screenOffset.Split(' ');
                int offsetRight = int.Parse(splitText[0]);
                int offsetDown = int.Parse(splitText[1]);

                foreach (Screen s in screens)
                {
                    if (button.Name == "Button_" + s.DeviceName)
                    {
                        DarkForm newDarkForm = new DarkForm();
                        newDarkForm.Show();

                        newDarkForm.Location = new Point(s.Bounds.X, s.Bounds.Y);
                        newDarkForm.Size = new Size(s.Bounds.Width, s.Bounds.Height);
                        newDarkForm.Name = "Form_" + button.Name.Replace("Button_", "");

                        newDarkForm.Opacity = transparencySlider.Value / 100f;

                        SetButtonState(true, button);
                        break;
                    }
                }
            }
            else
            {
                foreach (DarkForm d in Application.OpenForms.OfType<DarkForm>().ToList())
                    if (d.Name == "Form_" + button.Name.Replace("Button_", ""))
                    {
                        d.Close();
                        SetButtonState(false, button);
                    }
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            ResetDarkScreens();
        }

        private void ResetDarkScreens()
        {
            foreach (DarkForm d in Application.OpenForms.OfType<DarkForm>().ToList())
            {
                d.Close();
            }
            foreach (Button b in buttonPanel.Controls)
            {
                if (b.Name != primaryDisplay.Name)
                {
                    SetButtonState(false, b);
                }
            }
        }

        private void SetButtonState(bool clicked, Button b)
        {
            if (clicked)
            {
                b.BackColor = SystemColors.ControlDark;
            }
            else
            {
                b.BackColor = SystemColors.Control;
                b.UseVisualStyleBackColor = true;
            }
        }

        private void transparencyChanged(object sender, EventArgs e)
        {
            transparencyValueLabel.Text = string.Format("{0}%", transparencySlider.Value);
            foreach (DarkForm f in Application.OpenForms.OfType<DarkForm>().ToList())
            {
                f.Opacity = (transparencySlider.Value / 100f);
            }
        }

        private void refreshDisplaysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckDisplayCount();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
